using Our.Shield.MediaProtection.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Migrations;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core;

namespace Our.Shield.MediaProtection.Migrations
{
    public class InstallMigration : MigrationBase
    {
        private const string MemberOnlyAlias = "umbracoMemberOnly";

        private readonly IMediaTypeService _mediaTypeService;

        private readonly UmbracoDataTypes _umbDataTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallMigration"/> class
        /// </summary>
        /// <param name="context"></param>
        /// <param name="mediaTypeService"><see cref="IMediaTypeService"/></param>
        /// <param name="umbDataTypes"><see cref="UmbracoDataTypes"/></param>
        public InstallMigration(
            IMigrationContext context,
            IMediaTypeService mediaTypeService,
            UmbracoDataTypes umbDataTypes)
            : base(context)
        {
            _mediaTypeService = mediaTypeService;
            _umbDataTypes = umbDataTypes;
        }

        public override void Migrate()
        {
            //  File
            var secureFile = SecureFile();
            var doesFileExist = _mediaTypeService.Get(secureFile.Alias);

            if (doesFileExist == null)
            {
                _mediaTypeService.Save(secureFile);
                doesFileExist = _mediaTypeService.Get(secureFile.Alias);
            }

            //  Image
            var secureImage = SecureImage();
            var doesImageExist = _mediaTypeService.Get(secureImage.Alias);

            if (doesImageExist == null)
            {
                _mediaTypeService.Save(secureImage);
                doesImageExist = _mediaTypeService.Get(secureImage.Alias);
            }

            //  Folder
            var folder = SecureFolder();
            if (_mediaTypeService.Get(folder.Alias) != null)
            {
                return;
            }

            var allowedContentTypes = new List<ContentTypeSort>
            {
                new ContentTypeSort(new Lazy<int>(() => doesFileExist.Id), 0, doesFileExist.Alias),
                new ContentTypeSort(new Lazy<int>(() => doesImageExist.Id), 0, doesImageExist.Alias),
                new ContentTypeSort(new Lazy<int>(() => folder.Id), 0, folder.Alias)
            };

            var umbFolder = _mediaTypeService.Get(Constants.Conventions.MediaTypes.Folder);
            if (umbFolder != null)
            {
                allowedContentTypes.Add(new ContentTypeSort(new Lazy<int>(() => umbFolder.Id), 3, umbFolder.Alias));
            }

            var umbImage = _mediaTypeService.Get(Constants.Conventions.MediaTypes.Image);
            if (umbImage != null)
            {
                allowedContentTypes.Add(new ContentTypeSort(new Lazy<int>(() => umbImage.Id), 3, umbImage.Alias));
            }

            var umbFile = _mediaTypeService.Get(Constants.Conventions.MediaTypes.File);
            if (umbFile != null)
            {
                allowedContentTypes.Add(new ContentTypeSort(new Lazy<int>(() => umbFile.Id), 3, umbFile.Alias));
            }

            folder.AllowedContentTypes = allowedContentTypes;

            _mediaTypeService.Save(folder);
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

            mediaType.AddPropertyType(
                new PropertyType(_umbDataTypes.TrueFalse, MemberOnlyAlias)
                {
                    Name = "Member Only",
                    Description = "Only members who have logged in can view this image",
                    SortOrder = 0,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.Boolean
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(_umbDataTypes.ImageCropper, Constants.Conventions.Media.File)
                {
                    Name = "Upload Image",
                    Description = string.Empty,
                    SortOrder = 1,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.UploadField
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(_umbDataTypes.Label, Constants.Conventions.Media.Width)
                {
                    Name = "Width",
                    Description = "in pixels",
                    SortOrder = 2,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.Label
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(_umbDataTypes.Label, Constants.Conventions.Media.Height)
                {
                    Name = "Height",
                    Description = "in pixels",
                    SortOrder = 3,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.Label
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(_umbDataTypes.Label, Constants.Conventions.Media.Bytes)
                {
                    Name = "Size",
                    Description = "in bytes",
                    SortOrder = 4,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.Label
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(_umbDataTypes.Label, Constants.Conventions.Media.Extension)
                {
                    Name = "Type",
                    Description = string.Empty,
                    SortOrder = 5,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.Label
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

            mediaType.AddPropertyType(
                new PropertyType(_umbDataTypes.TrueFalse, MemberOnlyAlias)
                {
                    Name = "Member Only",
                    Description = "Only members who have logged in can view this image",
                    SortOrder = 0,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.Boolean
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(_umbDataTypes.Upload, Constants.Conventions.Media.File)
                {
                    Name = "Upload file",
                    Description = string.Empty,
                    SortOrder = 1,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.UploadField
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(_umbDataTypes.Label, Constants.Conventions.Media.Extension)
                {
                    Name = "Type",
                    Description = string.Empty,
                    SortOrder = 2,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.Label,
                }, "Image");

            mediaType.AddPropertyType(
                new PropertyType(_umbDataTypes.Label, Constants.Conventions.Media.Bytes)
                {
                    Name = "Size",
                    Description = string.Empty,
                    SortOrder = 3,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.Label
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

            mediaType.AddPropertyType(
                new PropertyType(_umbDataTypes.TrueFalse, MemberOnlyAlias)
                {
                    Name = "Member Only",
                    Description = "Only members who have logged in can view media stored within this folder",
                    SortOrder = 0,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.Boolean
                }, "Contents");

            mediaType.AddPropertyType(
                new PropertyType(_umbDataTypes.MediaListView, "contents")
                {
                    Name = "Contents",
                    Description = string.Empty,
                    SortOrder = 1,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.ListView
                }, "Contents");

            return mediaType;
        }
    }
}
