using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Shield.MediaProtection.Models
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
        public IDataTypeDefinition Text =>
            _dataTypeService.GetDataTypeDefinitionById(-88);

        /// <summary>
        /// 
        /// </summary>
        public IDataTypeDefinition Label =>
            _dataTypeService.GetDataTypeDefinitionById(-92);

        /// <summary>
        /// 
        /// </summary>
        public IDataTypeDefinition Upload =>
            _dataTypeService.GetDataTypeDefinitionById(-90);

        /// <summary>
        /// 
        /// </summary>
        public IDataTypeDefinition MediaListView =>
            _dataTypeService.GetDataTypeDefinitionById(Umbraco.Core.Constants.System.DefaultMediaListViewDataTypeId);

        /// <summary>
        /// 
        /// </summary>
        public IDataTypeDefinition Date =>
            _dataTypeService.GetDataTypeDefinitionById(-41);

        /// <summary>
        /// 
        /// </summary>
        public IDataTypeDefinition TrueFalse =>
            _dataTypeService.GetDataTypeDefinitionById(-49);

        /// <summary>
        /// 
        /// </summary>
        public IDataTypeDefinition ImageCropper =>
            _dataTypeService.GetDataTypeDefinitionById(1043);
    }
}
