namespace Our.Shield.MediaProtection.Models
{
    using Core.Models;
    using Core.UI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Web;
    using Umbraco.Core;
    using Umbraco.Core.Models;
    using Umbraco.Core.Security;
    using Umbraco.Core.Services;

    [AppEditor("/App_Plugins/Shield.MediaProtection/Views/MediaProtection.html?v=1.0.1")]
    public class MediaProtectionApp : App<MediaProtectionConfiguration>
    {
        /// <summary>
        /// Alias that denotes whether a media item only allowed to be accessed by members or not
        /// </summary>
        private const string MemberOnlyAlias = "umbracoMemberOnly";

        /// <summary>
        /// Media cache length
        /// </summary>
        private readonly TimeSpan CacheLength = new TimeSpan(TimeSpan.TicksPerSecond * 60 * 15);        //   = 15 minutes

        /// <summary>
        /// Unique cache key
        /// </summary>
        private const string CacheKey = ",e&yL2maXa?CVfWy";

        public override string Id => nameof(MediaProtection);

        public override string Name => "Media Protection";

        public override string Description => "Secure your media by stopping unauthorised access";

        public override string Icon => "icon-picture red";


        public override IConfiguration DefaultConfiguration
        {
            get
            {
                return new MediaProtectionConfiguration
                {
                    EnableHotLinkingProtection = true,
                    EnableMembersOnlyMedia = true
                };
            }
        }

        private static List<int> Ids = new List<int>();

        public override bool Execute(IJob job, IConfiguration c)
        {
            var config = c as MediaProtectionConfiguration;

            job.UnwatchWebRequests();
            Umbraco.Core.Services.MediaService.Saved += MediaService_Saved;


            if (!config.Enable)
            {
                return true;
            }

            Umbraco.Core.Services.MediaService.Saved += MediaService_Saved;


            var mediaFolder = VirtualPathUtility.ToAbsolute(new Uri(Umbraco.Core.IO.SystemDirectories.Media, UriKind.Relative).ToString()) + "/";

            if (config.EnableHotLinkingProtection)
            {
                job.WatchWebRequests(new Regex(mediaFolder), 50, (count, app) =>
                {
                    var referrer = app.Request.UrlReferrer;
                    if (referrer == null || String.IsNullOrWhiteSpace(referrer.Host) ||
                        referrer.Host.Equals(app.Request.Url.Host, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //  This media is being accessed directly, 
                        //  or from a browser that doesn't pass referrer info,
                        //  or from our own domain
                        //  so allow access
                        return WatchCycle.Continue;
                    }

                    //  Someone is trying to hotlink our media
                    app.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                    app.Response.End();
                    return WatchCycle.Stop;
                }, 0, null);

            }

            if (config.EnableMembersOnlyMedia)
            {
                job.WatchWebRequests(new Regex(mediaFolder), 100, (count, app) =>
                {
                    //  If we have logged in as a backend user, then allow all access
                    if (new System.Web.HttpContextWrapper(System.Web.HttpContext.Current).GetUmbracoAuthTicket() != null)
                    {
                        return WatchCycle.Continue;
                    }

                    var filename = app.Request.Url.LocalPath;
                   
                    var secureMedia = ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(CacheKey + "F" + filename, () =>
                    {
                        IMediaService mediaService = ApplicationContext.Current.Services.MediaService;
                        IMedia media = mediaService.GetMediaByPath(filename);
                        var pathIdKeys = new List<string>();
                        object accessRights = null;

                        //  We traverse up the ancestors until we either hit the root, or find our access rights 
                        //  from either our special alias value or a cache value from a previous request
                        while (media != null)
                        {
                            var cacheIdKey = CacheKey + "I" + media.Id.ToString();
                            if (media.Id != Umbraco.Core.Constants.System.Root)
                            {
                                accessRights = ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(cacheIdKey);
                                if (accessRights != null)
                                {
                                    break;
                                }

                                pathIdKeys.Add(cacheIdKey);
                                if (!media.HasProperty(MemberOnlyAlias))
                                {
                                    accessRights = false;
                                }
                                else
                                {
                                    var memberOnly = media.GetValue(MemberOnlyAlias);
                                    if (memberOnly != null)
                                    {
                                        if (memberOnly is bool)
                                        {
                                            accessRights = (int) memberOnly;
                                            break;
                                        }
                                        
                                        if (memberOnly is int)
                                        {
                                            //  Is a boolean value denoting whether you need to be logged in or not to access
                                            accessRights = (((int) memberOnly)  == 0) ? false : true;
                                            break;
                                        }
                            
                                        if (memberOnly is string)
                                        {
                                            //  Is a MNTP that states which member groups can access this media item
                                            accessRights = (int[]) ((string)memberOnly).Split(',').Select(x => int.Parse(x)).ToArray();
                                            break;
                                        }
                                    }
                                }
                            }

                            media = media.Parent();
                        }

                        if (accessRights == null)
                        {
                            accessRights = false;
                        }

                        //  Give our descendants the same access rights as us
                        foreach (var key in pathIdKeys)
                        {
                            ApplicationContext.Current.ApplicationCache.RuntimeCache.InsertCacheItem(key, () => { return accessRights;}, CacheLength );
                        }
                        return accessRights;

                    }, CacheLength);

                    if (secureMedia is bool)
                    {
                        if ((bool) secureMedia == false || 
                            (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated))
                        {
                            //  They are allowed to view this media
                            return WatchCycle.Continue;
                        }
                    }

                    if (secureMedia is int[])
                    {
                        if (((int[]) secureMedia).Length == 0)
                        {
                            //  They are allowed to view this media
                            return WatchCycle.Continue;
                        }

                        if (HttpContext.Current.User.Identity.IsAuthenticated)
                        {
                            //  TODO: Need to handle when security is member group based
                            return WatchCycle.Continue;
                        }
                    }

                    //  You need to be logged in or be a member of the correct member group to see this media
                    app.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                    app.Response.End();
                    return WatchCycle.Stop;

                }, 0, null);
            }

            return true;
        }

        private void MediaService_Saved(IMediaService sender, Umbraco.Core.Events.SaveEventArgs<IMedia> e)
        {
            throw new NotImplementedException();
        }
    }
}
