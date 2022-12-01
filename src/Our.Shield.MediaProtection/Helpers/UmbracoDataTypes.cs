using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Shield.MediaProtection.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public class UmbracoDataTypes
    {
        private readonly IDataTypeService _dataTypeService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTypeService"></param>
        public UmbracoDataTypes(IDataTypeService dataTypeService)
        {
            _dataTypeService = dataTypeService;
        }

        /// <summary>
        /// 
        /// </summary>
        public IDataType Text =>
            _dataTypeService.GetDataType(Umbraco.Core.Constants.DataTypes .Textbox);

        /// <summary>
        /// 
        /// </summary>
        public IDataType Label =>
            _dataTypeService.GetDataType(Umbraco.Core.Constants.DataTypes.LabelString);

        /// <summary>
        /// 
        /// </summary>
        public IDataType Upload =>
            _dataTypeService.GetDataType(Umbraco.Core.Constants.DataTypes.Upload);

        /// <summary>
        /// 
        /// </summary>
        public IDataType MediaListView =>
            _dataTypeService.GetDataType(Umbraco.Core.Constants.DataTypes.DefaultMediaListView);

        /// <summary>
        /// 
        /// </summary>
        public IDataType Date =>
            _dataTypeService.GetDataType(Umbraco.Core.Constants.DataTypes.DateTime);

        /// <summary>
        /// 
        /// </summary>
        public IDataType TrueFalse =>
            _dataTypeService.GetDataType(Umbraco.Core.Constants.DataTypes.Boolean);

        /// <summary>
        /// 
        /// </summary>
        public IDataType ImageCropper =>
            _dataTypeService.GetDataType(Umbraco.Core.Constants.DataTypes.ImageCropper);
    }
}
