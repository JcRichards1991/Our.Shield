using Our.Shield.Core.Attributes;
using Our.Shield.Core.Enums;
using Our.Shield.Core.Models;
using Our.Shield.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Security;

namespace Our.Shield.MediaProtection.Models
{
    [AppEditor("/App_Plugins/Shield.MediaProtection/Views/MediaProtection.html?version=1.1.0")]
    public class MediaProtectionApp : App<MediaProtectionConfiguration>
    {
        private readonly AppCaches _appCaches;

        private const string MemberOnlyAlias = "umbracoMemberOnly";

        private readonly TimeSpan _cacheLength = new TimeSpan(TimeSpan.TicksPerSecond * 30);

        private const string CacheKey = ",e&yL2maXa?CVfWy";

        private readonly IUmbracoMediaService _umbMediaService;

        public MediaProtectionApp(
            IUmbracoContextAccessor umbContextAccessor,
            ILocalizedTextService localizedTextService,
            ILogger logger,
            IUmbracoMediaService umbMediaService,
            AppCaches appCaches)
            : base(umbContextAccessor, localizedTextService, logger)
        {
            _umbMediaService = umbMediaService;
            _appCaches = appCaches;
        }

        /// <inheritdoc />
        public override string Id => nameof(MediaProtection);

        /// <inheritdoc />
        public override string Icon => "icon-picture red";

        /// <inheritdoc />
        public override IAppConfiguration DefaultConfiguration =>
            new MediaProtectionConfiguration
            {
                EnableHotLinkingProtection = true,
                EnableMembersOnlyMedia = true,
                HotLinkingProtectedDirectories = new string[0]
            };

        /// <inheritdoc />
        public override bool Execute(IJob job, IAppConfiguration c)
        {
            if (!(c is MediaProtectionConfiguration config))
            {
                return false;
            }


            job.UnwatchWebRequests();
            job.UnignoreWebRequest();

            if (!config.Enabled || !job.Environment.Enabled)
            {
                return true;
            }

            var mediaRegex = job.PathToRegex(VirtualPathUtility.ToAbsolute(new Uri(Umbraco.Core.IO.SystemDirectories.Media, UriKind.Relative).ToString()));
            job.IgnoreWebRequest(mediaRegex);

            if (config.EnableHotLinkingProtection)
            {
                var domains = new List<string>();
                foreach (var domain in job.Environment.Domains)
                {
                    try
                    {
                        var uriBuilder = new UriBuilder(domain.FullyQualifiedUrl);
                        domains.Add(uriBuilder.Host);
                    }
                    catch (Exception)
                    {
                        // Swallow
                    }
                }

                var regex = new Regex("^(" + string.Join("|", config.HotLinkingProtectedDirectories) + ")", RegexOptions.IgnoreCase);
                job.IgnoreWebRequest(regex);

                job.WatchWebRequests(PipeLineStage.AuthenticateRequest, regex, 30000, (count, httpApp) =>
                {
                    var referrer = httpApp.Request.UrlReferrer;
                    if (referrer == null || String.IsNullOrWhiteSpace(referrer.Host) ||
                        referrer.Host.Equals(httpApp.Request.Url.Host, StringComparison.InvariantCultureIgnoreCase) ||
                        domains.Any(x => x.Equals(referrer.Host, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        //  This media is being accessed directly,
                        //  or from a browser that doesn't pass referrer info,
                        //  or from our own domain
                        //  so allow access
                        return new WatchResponse(Cycle.Continue);
                    }

                    using (var umbContext = UmbContextAccessor.UmbracoContext)
                    {
                        var localizedAppName = LocalizedTextService.Localize($"{nameof(Shield)}.{nameof(MediaProtection)}", "Name");
                        var localizedMessage = LocalizedTextService.Localize(
                            $"{nameof(Shield)}.{nameof(MediaProtection)}.DeniedAccess_Hotlink",
                            new[]
                            {
                                httpApp.Context.Request.UserHostAddress,
                                referrer.Host,
                                job.Environment.Name
                            });

                        Logger.Warn<MediaProtectionApp>(
                            localizedMessage + "App Key: {AppKey}; Environment Key: {EnvironmentKey}",
                            job.App.Key,
                            job.Environment.Key);
                    }

                    httpApp.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    httpApp.Response.End();

                    return new WatchResponse(Cycle.Stop);
                });
            }

            if (!config.EnableMembersOnlyMedia)
            {
                return true;
            }

            job.WatchWebRequests(PipeLineStage.AuthenticateRequest, mediaRegex, 30100, (count, httpApp) =>
            {
                var httpContext = new HttpContextWrapper(httpApp.Context);
                var umbAuthTicket = httpContext.GetUmbracoAuthTicket();

                //  If we have logged in as a backend user, then allow all access
                if (httpContext.AuthenticateCurrentRequest(umbAuthTicket, true))
                {
                    return new WatchResponse(Cycle.Continue);
                }

                var filename = httpApp.Request.Url.LocalPath;

                var secureMedia = _appCaches.RuntimeCache.GetCacheItem(CacheKey + "F" + filename, () =>
                {
                    var mediaId = _umbMediaService.Id(filename);

                    if (mediaId == null)
                    {
                        return false;
                    }

                    var pathIdKeys = new List<string>();
                    object accessRights = null;

                    var traverseId = mediaId;

                    //  We traverse up the ancestors until we either hit the root, or find our access rights
                    //  from either our special alias value or a cache value from a previous request
                    while (traverseId != null)
                    {
                        var cacheIdKey = CacheKey + "I" + traverseId;
                        accessRights = _appCaches.RuntimeCache.GetCacheItem<object>(cacheIdKey);
                        if (accessRights != null)
                        {
                            break;
                        }

                        pathIdKeys.Add(cacheIdKey);
                        var memberOnly = _umbMediaService.Value((int)traverseId, MemberOnlyAlias);
                        if (memberOnly == null)
                        {
                            accessRights = false;
                        }
                        else
                        {
                            switch (memberOnly)
                            {
                                case bool _:
                                    accessRights = (bool)memberOnly ? 1 : 0;
                                    break;

                                case int _:
                                    accessRights = (int)memberOnly != 0;
                                    break;

                                case string _:
                                    //  Is a MNTP that states which member groups can access this media item
                                    try
                                    {
                                        accessRights = ((string)memberOnly).Split(',').Select(int.Parse).ToArray();
                                    }
                                    catch (Exception)
                                    {
                                        //  Swallow conversion issues, and continue processing
                                    }
                                    break;
                            }
                            break;
                        }

                        traverseId = _umbMediaService.ParentId((int)traverseId);
                    }

                    if (accessRights == null)
                    {
                        accessRights = false;
                    }

                    //  Give our descendants the same access rights as us
                    foreach (var key in pathIdKeys)
                    {
                        _appCaches.RuntimeCache.InsertCacheItem(key, () => accessRights, _cacheLength);
                    }
                    return accessRights;

                }, _cacheLength);

                if (secureMedia is bool b)
                {
                    if (!b ||
                        (httpApp.Context.User != null && httpApp.Context.User.Identity.IsAuthenticated))
                    {
                        //  They are allowed to view this media
                        return new WatchResponse(Cycle.Continue);
                    }
                }

                if (secureMedia is int[] ints)
                {
                    if (ints.Length == 0)
                    {
                        //  They are allowed to view this media
                        return new WatchResponse(Cycle.Continue);
                    }

                    if (httpApp.Context.User.Identity.IsAuthenticated)
                    {
                        //  TODO: Need to handle when security is member group based
                        return new WatchResponse(Cycle.Continue);
                    }
                }

                using (var umbContext = UmbContextAccessor.UmbracoContext)
                {
                    var localizedMessage = LocalizedTextService.Localize(
                        $"{nameof(Shield)}.{nameof(MediaProtection)}.DeniedAccess_ProtectedMedia",
                        new[]
                        {
                            filename,
                            httpApp.Context.Request.UserHostAddress,
                            job.Environment.Name
                        });

                    Logger.Warn<MediaProtectionApp>(
                        localizedMessage + "App: {App}; App Key: {AppKey}; Environment Key: {EnvironmentKey}",
                        LocalizedTextService.Localize($"{nameof(Shield)}.{nameof(MediaProtection)}", "Name"),
                        job.App.Key,
                        job.Environment.Key);
                }

                //  You need to be logged in or be a member of the correct member group to see this media
                httpApp.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                httpApp.Response.End();

                return new WatchResponse(Cycle.Stop);
            });

            return true;
        }
    }
}
