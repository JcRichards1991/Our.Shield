using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Shield.MediaProtection.Models
{
    public class UmbracoDataTypes
    {
        private readonly IDataTypeService _dataTypeService;

        public UmbracoDataTypes(IDataTypeService dataTypeService)
        {
            _dataTypeService = dataTypeService;
        }

        public IDataTypeDefinition Text
        {
            get
            {
                return _dataTypeService.GetDataTypeDefinitionById(-88);
            }
        }

        public IDataTypeDefinition Label
        {
            get
            {
                return _dataTypeService.GetDataTypeDefinitionById(-92);
            }
        }

        public IDataTypeDefinition Upload
        {
            get
            {
                return _dataTypeService.GetDataTypeDefinitionById(-90);
            }
        }

        public IDataTypeDefinition MediaListView
        {
            get
            {
                return _dataTypeService.GetDataTypeDefinitionById(Umbraco.Core.Constants.System.DefaultMediaListViewDataTypeId);
            }
        }

        public IDataTypeDefinition Date
        {
            get
            {
                return _dataTypeService.GetDataTypeDefinitionById(-41);
            }
        }

        public IDataTypeDefinition TrueFalse
        {
            get
            {
                return _dataTypeService.GetDataTypeDefinitionById(-49);
            }
        }

        public IDataTypeDefinition ImageCropper
        {
            get
            {
                return _dataTypeService.GetDataTypeDefinitionById(1043);
            }
        }
    }
}
