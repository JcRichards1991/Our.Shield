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

    /// <summary>
    /// 
    /// </summary>
    [AppEditor("/App_Plugins/Shield.MediaProtection/Views/MediaProtection.html?version=1.0.0")]
    [AppMigration(typeof(MediaProtectionMigration))]
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

        /// <summary>
        /// 
        /// </summary>
        public override string Id => nameof(MediaProtection);

        /// <summary>
        /// 
        /// </summary>
        public override string Name => "Media Protection";

        /// <summary>
        /// 
        /// </summary>
        public override string Description => "Secure your media by stopping unauthorised access";

        /// <summary>
        /// 
        /// </summary>
        public override string Icon => "icon-picture red";

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public override bool Execute(IJob job, IConfiguration c)
        {
            AddMediaTypes();
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
                job.WatchWebRequests(new Regex(mediaFolder, RegexOptions.IgnoreCase), 50, (count, httpApp) =>
                {
                    var referrer = httpApp.Request.UrlReferrer;
                    if (referrer == null || String.IsNullOrWhiteSpace(referrer.Host) ||
                        referrer.Host.Equals(httpApp.Request.Url.Host, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //  This media is being accessed directly, 
                        //  or from a browser that doesn't pass referrer info,
                        //  or from our own domain
                        //  so allow access
                        return WatchCycle.Continue;
                    }

                    job.WriteJournal(new JournalMessage($"Access was denied, {referrer.Host} is trying to hotlink your media assets"));

                    //  Someone is trying to hotlink our media
                    httpApp.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                    httpApp.Response.End();
                    return WatchCycle.Stop;
                });

            }

            if (config.EnableMembersOnlyMedia)
            {
                job.WatchWebRequests(new Regex(mediaFolder, RegexOptions.IgnoreCase), 100, (count, httpApp) =>
                {
                    var httpContext = new HttpContextWrapper(httpApp.Context);
                    var umbAuthTicket = httpContext.GetUmbracoAuthTicket();

                    //  If we have logged in as a backend user, then allow all access
                    if (httpContext.AuthenticateCurrentRequest(umbAuthTicket, true))
                    {
                        return WatchCycle.Continue;
                    }

                    var filename = httpApp.Request.Url.LocalPath;
                    int mediaId = 0;
                    string mediaName = string.Empty;
                   
                    var secureMedia = ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(CacheKey + "F" + filename, () =>
                    {
                        IMediaService mediaService = ApplicationContext.Current.Services.MediaService;
                        IMedia media = mediaService.GetMediaByPath(filename);

                        mediaId = media.Id;
                        mediaName = media.Name;

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
                            (httpApp.Context.User != null && httpApp.Context.User.Identity.IsAuthenticated))
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

                        if (httpApp.Context.User.Identity.IsAuthenticated)
                        {
                            //  TODO: Need to handle when security is member group based
                            return WatchCycle.Continue;
                        }
                    }

                    job.WriteJournal(new JournalMessage($"An unauthenticated member tried to access media: {mediaName}; media id: {mediaId}; unauthenticated member IP Address: {httpApp.Context.Request.UserHostAddress}"));

                    //  You need to be logged in or be a member of the correct member group to see this media
                    httpApp.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                    httpApp.Response.End();
                    return WatchCycle.Stop;

                });
            }

            return true;
        }

        private void MediaService_Saved(IMediaService sender, Umbraco.Core.Events.SaveEventArgs<IMedia> e)
        {
            //throw new NotImplementedException();
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
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.TrueFalseAlias,
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.ImageCropper, Umbraco.Core.Constants.Conventions.Media.File)
                {
                    Name = "Upload Image",
                    Description = string.Empty,
                    SortOrder = 1,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.UploadFieldAlias,
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.Label, Umbraco.Core.Constants.Conventions.Media.Width)
                {
                    Name = "Width",
                    Description = string.Empty,
                    SortOrder = 2,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.NoEditAlias,
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.Label, Umbraco.Core.Constants.Conventions.Media.Height)
                {
                    Name = "Height",
                    Description = string.Empty,
                    SortOrder = 3,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.NoEditAlias,
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.Label, Umbraco.Core.Constants.Conventions.Media.Bytes)
                {
                    Name = "Size",
                    Description = string.Empty,
                    SortOrder = 4,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.NoEditAlias,
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.Label, Umbraco.Core.Constants.Conventions.Media.Extension)
                {
                    Name = "Type",
                    Description = string.Empty,
                    SortOrder = 5,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.NoEditAlias,
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
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.TrueFalseAlias,
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.Upload, Umbraco.Core.Constants.Conventions.Media.File)
                {
                    Name = "Upload file",
                    Description = string.Empty,
                    SortOrder = 1,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.UploadFieldAlias,
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.Label, Umbraco.Core.Constants.Conventions.Media.Extension)
                {
                    Name = "Type",
                    Description = string.Empty,
                    SortOrder = 2,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.NoEditAlias,
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.Label, Umbraco.Core.Constants.Conventions.Media.Bytes)
                {
                    Name = "Size",
                    Description = string.Empty,
                    SortOrder = 3,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.NoEditAlias,
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
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.TrueFalseAlias,
                }, "Contents");

            mediaType.AddPropertyType(
                new PropertyType(umbracoDataType.MediaListView, "contents")
                {
                    Name = "Contents",
                    Description = string.Empty,
                    SortOrder = 1,
                    PropertyEditorAlias = Umbraco.Core.Constants.PropertyEditors.ListViewAlias,
                }, "Contents");

            return mediaType;
        }
        
        private void AddMediaTypes()
        {
            if (!((MediaProtectionMigration) Migrations["1.0.0"]).AddMediaTypes)
            {
                return;
            }
            ((MediaProtectionMigration) Migrations["1.0.0"]).AddMediaTypes = false;

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
            if (contentTypeService.GetMediaType(folder.Alias) == null)
            {
                var allowedContentTypes = new List<ContentTypeSort>();
                allowedContentTypes.Add(new ContentTypeSort(new Lazy<int>(() => doesFileExist.Id), 0, doesFileExist.Alias));
                allowedContentTypes.Add(new ContentTypeSort(new Lazy<int>(() => doesImageExist.Id), 0, doesImageExist.Alias));
                allowedContentTypes.Add(new ContentTypeSort(new Lazy<int>(() => folder.Id), 0, folder.Alias));

                var umbFolder = contentTypeService.GetMediaType(Umbraco.Core.Constants.Conventions.MediaTypes.Folder);
                if (umbFolder != null)
                {
                    allowedContentTypes.Add(new ContentTypeSort(new Lazy<int>(() => umbFolder.Id), 3, umbFolder.Alias));
                }

                var umbImage = contentTypeService.GetMediaType(Umbraco.Core.Constants.Conventions.MediaTypes.Image);
                if (umbImage != null)
                {
                    allowedContentTypes.Add(new ContentTypeSort(new Lazy<int>(() => umbImage.Id), 3, umbImage.Alias));
                }
                
                var umbFile = contentTypeService.GetMediaType(Umbraco.Core.Constants.Conventions.MediaTypes.File);
                if (umbFile != null)
                {
                    allowedContentTypes.Add(new ContentTypeSort(new Lazy<int>(() => umbFile.Id), 3, umbFile.Alias));
                }
                folder.AllowedContentTypes = allowedContentTypes;

                contentTypeService.Save(folder);
            }
        }
    }
}
