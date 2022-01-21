using Our.Shield.Core.Attributes;
using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using Our.Shield.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Security;

namespace Our.Shield.MediaProtection.Models
{
    [AppEditor("/App_Plugins/Shield.MediaProtection/Views/MediaProtection.html?version=1.1.0")]
    [AppJournal]
    [AppMigration(typeof(MediaProtectionMigration))]
    public class MediaProtectionApp : App<MediaProtectionConfiguration>
    {
        private readonly IAppCache _appCache;

        public MediaProtectionApp(
            ILocalizedTextService localizedTextService,
            IAppCache appCache)
            : base(localizedTextService)
        {
            _appCache = appCache;
        }

        /// <summary>
        /// Alias that denotes whether a media item only allowed to be accessed by members or not
        /// </summary>
        private const string MemberOnlyAlias = "umbracoMemberOnly";

        /// <summary>
        /// Media cache length
        /// </summary>
        private readonly TimeSpan _cacheLength = new TimeSpan(TimeSpan.TicksPerSecond * 30);

        /// <summary>
        /// Unique cache key
        /// </summary>
        private const string CacheKey = ",e&yL2maXa?CVfWy";

        /// <inheritdoc />
        public override string Id => nameof(MediaProtection);

        /// <inheritdoc />
        public override string Name => LocalizedTextService.Localize("Shield.MediaProtection.General/Name", CultureInfo.CurrentCulture);

        /// <inheritdoc />
        public override string Description => LocalizedTextService.Localize("Shield.MediaProtection.General/Description", CultureInfo.CurrentCulture);

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
            AddMediaTypes();
            job.UnwatchWebRequests();
            job.UnignoreWebRequest();

            if (!(c is MediaProtectionConfiguration config))
            {
                job.WriteJournal(new JournalMessage("Error: Config passed into Media Protection was not of the correct type"));

                return false;
            }

            if (!config.Enable || !job.Environment.Enable)
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
                        var uriBuilder = new UriBuilder(domain.Name);
                        domains.Add(uriBuilder.Host);
                    }
                    catch (Exception)
                    {
                        // Swallow
                    }
                }

                var regex = new Regex("^(" + string.Join("|", config.HotLinkingProtectedDirectories) + ")", RegexOptions.IgnoreCase);
                job.IgnoreWebRequest(regex);

                job.WatchWebRequests(PipeLineStages.AuthenticateRequest, regex, 30000, (count, httpApp) =>
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
                        return new WatchResponse(WatchResponse.Cycles.Continue);
                    }

                    job.WriteJournal(new JournalMessage($"Access was denied, {httpApp.Context.Request.UserHostAddress} from {referrer.Host} was trying to hotlink your media assets"));

                    //  Someone is trying to hotlink our media
                    httpApp.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    httpApp.Response.End();
                    return new WatchResponse(WatchResponse.Cycles.Stop);
                });
            }

            if (!config.EnableMembersOnlyMedia)
            {
                return true;
            }

            job.WatchWebRequests(PipeLineStages.AuthenticateRequest, mediaRegex, 30100, (count, httpApp) =>
            {
                var httpContext = new HttpContextWrapper(httpApp.Context);
                var umbAuthTicket = httpContext.GetUmbracoAuthTicket();

                //  If we have logged in as a backend user, then allow all access
                if (httpContext.AuthenticateCurrentRequest(umbAuthTicket, true))
                {
                    return new WatchResponse(WatchResponse.Cycles.Continue);
                }

                var filename = httpApp.Request.Url.LocalPath;

                var secureMedia = _appCache.GetCacheItem(CacheKey + "F" + filename, () =>
                {
                    var umbracoContext = Umbraco.Web.UmbracoContext.Current;

                    if (umbracoContext == null)
                    {
                        var newHttpContext = new HttpContextWrapper(HttpContext.Current);
                        umbracoContext = UmbracoContext.EnsureContext(
                            newHttpContext,
                            ApplicationContext.Current,
                            new Umbraco.Web.Security.WebSecurity(newHttpContext, ApplicationContext.Current),
                            UmbracoConfig.For.UmbracoSettings(),
                            UrlProviderResolver.Current.Providers,
                            true);
                    }

                    var mediaService = new UmbracoMediaService(umbracoContext);
                    var mediaId = mediaService.Id(filename);

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
                        accessRights = ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(cacheIdKey);
                        if (accessRights != null)
                        {
                            break;
                        }

                        pathIdKeys.Add(cacheIdKey);
                        var memberOnly = mediaService.Value((int)traverseId, MemberOnlyAlias);
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

                        traverseId = mediaService.ParentId((int)traverseId);
                    }

                    if (accessRights == null)
                    {
                        accessRights = false;
                    }

                    //  Give our descendants the same access rights as us
                    foreach (var key in pathIdKeys)
                    {
                        ApplicationContext.Current.ApplicationCache.RuntimeCache.InsertCacheItem(key, () => accessRights, _cacheLength);
                    }
                    return accessRights;

                }, _cacheLength);

                if (secureMedia is bool b)
                {
                    if (b == false ||
                        (httpApp.Context.User != null && httpApp.Context.User.Identity.IsAuthenticated))
                    {
                        //  They are allowed to view this media
                        return new WatchResponse(WatchResponse.Cycles.Continue);
                    }
                }

                if (secureMedia is int[] ints)
                {
                    if (ints.Length == 0)
                    {
                        //  They are allowed to view this media
                        return new WatchResponse(WatchResponse.Cycles.Continue);
                    }

                    if (httpApp.Context.User.Identity.IsAuthenticated)
                    {
                        //  TODO: Need to handle when security is member group based
                        return new WatchResponse(WatchResponse.Cycles.Continue);
                    }
                }
                job.WriteJournal(new JournalMessage($"An unauthenticated member tried to access {filename} with IP Address: {httpApp.Context.Request.UserHostAddress}"));

                //  You need to be logged in or be a member of the correct member group to see this media
                httpApp.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                httpApp.Response.End();
                return new WatchResponse(WatchResponse.Cycles.Stop);

            });

            return true;
        }

        private MediaType SecureImage()
        {
            var mediaType = new MediaType(-1)
            {
                Alias = "secureImage",
                Name = "Secure Image",
                Description = "Only members who have logged in can view this image",
                Icon = "icon-umb-media color-red",
                Thumbnail = "doc.png",
                SortOrder = 20,
                CreatorId = 0,
                Trashed = false,
                IsContainer = false,
                AllowedAsRoot = true,
                AllowedContentTypes = Enumerable.Empty<ContentTypeSort>()
            };

            var umbracoDataType = new UmbracoDataTypes(ApplicationContext.Current.Services.DataTypeService);

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.TrueFalse, MemberOnlyAlias)
                {
                    Name = "Member Only",
                    Description = "Only members who have logged in can view this image",
                    SortOrder = 0,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.Aliases.Boolean
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.ImageCropper, Umbraco.Core.Constants.Conventions.Media.File)
                {
                    Name = "Upload Image",
                    Description = string.Empty,
                    SortOrder = 1,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.Aliases.UploadField
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.Label, Umbraco.Core.Constants.Conventions.Media.Width)
                {
                    Name = "Width",
                    Description = string.Empty,
                    SortOrder = 2,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.Aliases.Label
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.Label, Umbraco.Core.Constants.Conventions.Media.Height)
                {
                    Name = "Height",
                    Description = string.Empty,
                    SortOrder = 3,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.Aliases.Label
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.Label, Umbraco.Core.Constants.Conventions.Media.Bytes)
                {
                    Name = "Size",
                    Description = string.Empty,
                    SortOrder = 4,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.Aliases.Label
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.Label, Umbraco.Core.Constants.Conventions.Media.Extension)
                {
                    Name = "Type",
                    Description = string.Empty,
                    SortOrder = 5,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.Aliases.Label
                }, "Image");

            return mediaType;
        }

        private MediaType SecureFile()
        {
            var mediaType = new MediaType(-1)
            {
                Alias = "secureFile",
                Name = "Secure File",
                Description = "Only members who have logged in can view this file",
                Icon = "icon-lock color-red",
                Thumbnail = "doc.png",
                SortOrder = 21,
                CreatorId = 0,
                Trashed = false,
                IsContainer = false,
                AllowedAsRoot = true,
                AllowedContentTypes = Enumerable.Empty<ContentTypeSort>()
            };

            var umbracoDataType = new UmbracoDataTypes(ApplicationContext.Current.Services.DataTypeService);

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.TrueFalse, MemberOnlyAlias)
                {
                    Name = "Member Only",
                    Description = "Only members who have logged in can view this image",
                    SortOrder = 0,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.Aliases.Boolean
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.Upload, Umbraco.Core.Constants.Conventions.Media.File)
                {
                    Name = "Upload file",
                    Description = string.Empty,
                    SortOrder = 1,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.Aliases.UploadField
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.Label, Umbraco.Core.Constants.Conventions.Media.Extension)
                {
                    Name = "Type",
                    Description = string.Empty,
                    SortOrder = 2,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.Aliases.Label,
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.Label, Umbraco.Core.Constants.Conventions.Media.Bytes)
                {
                    Name = "Size",
                    Description = string.Empty,
                    SortOrder = 3,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.Aliases.Label
                }, "Image");

            return mediaType;
        }

        private MediaType SecureFolder()
        {
            var mediaType = new MediaType(-1)
            {
                Alias = "secureFolder",
                Name = "Secure Folder",
                Description = "Only members who have logged in can access media stored within this folder",
                Icon = "icon-combination-lock color-red",
                Thumbnail = "doc.png",
                SortOrder = 22,
                CreatorId = 0,
                Trashed = false,
                IsContainer = false,
                AllowedAsRoot = true,
                AllowedContentTypes = Enumerable.Empty<ContentTypeSort>()
            };

            var umbracoDataType = new UmbracoDataTypes(ApplicationContext.Current.Services.DataTypeService);

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.TrueFalse, MemberOnlyAlias)
                {
                    Name = "Member Only",
                    Description = "Only members who have logged in can view media stored within this folder",
                    SortOrder = 0,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.Aliases.Boolean
                }, "Contents");

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.MediaListView, "contents")
                {
                    Name = "Contents",
                    Description = string.Empty,
                    SortOrder = 1,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.Aliases.ListView
                }, "Contents");

            return mediaType;
        }

        private void AddMediaTypes()
        {
            if (Migrations == null || !((MediaProtectionMigration)Migrations["1.0.0"]).AddMediaTypes)
            {
                return;
            }

            ((MediaProtectionMigration)Migrations["1.0.0"]).AddMediaTypes = false;

            var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;

            //  File
            var secureFile = SecureFile();
            var doesFileExist = contentTypeService.GetMediaType(secureFile.Alias);

            if (doesFileExist == null)
            {
                contentTypeService.Save(secureFile);
                doesFileExist = contentTypeService.GetMediaType(secureFile.Alias);
            }

            //  Image
            var secureImage = SecureImage();
            var doesImageExist = contentTypeService.GetMediaType(secureImage.Alias);

            if (doesImageExist == null)
            {
                contentTypeService.Save(secureImage);
                doesImageExist = contentTypeService.GetMediaType(secureImage.Alias);
            }

            //  Folder
            var folder = SecureFolder();
            if (contentTypeService.GetMediaType(folder.Alias) != null)
                return;

            var allowedContentTypes = new List<ContentTypeSort>
            {
                new ContentTypeSort(new Lazy<int>(() => doesFileExist.Id), 0, doesFileExist.Alias),
                new ContentTypeSort(new Lazy<int>(() => doesImageExist.Id), 0, doesImageExist.Alias),
                new ContentTypeSort(new Lazy<int>(() => folder.Id), 0, folder.Alias)
            };

            var umbFolder = contentTypeService.GetMediaType(Constants.Conventions.MediaTypes.Folder);
            if (umbFolder != null)
            {
                allowedContentTypes.Add(new ContentTypeSort(new Lazy<int>(() => umbFolder.Id), 3, umbFolder.Alias));
            }

            var umbImage = contentTypeService.GetMediaType(Constants.Conventions.MediaTypes.Image);
            if (umbImage != null)
            {
                allowedContentTypes.Add(new ContentTypeSort(new Lazy<int>(() => umbImage.Id), 3, umbImage.Alias));
            }

            var umbFile = contentTypeService.GetMediaType(Constants.Conventions.MediaTypes.File);
            if (umbFile != null)
            {
                allowedContentTypes.Add(new ContentTypeSort(new Lazy<int>(() => umbFile.Id), 3, umbFile.Alias));
            }
            folder.AllowedContentTypes = allowedContentTypes;

            contentTypeService.Save(folder);
        }
    }
}
