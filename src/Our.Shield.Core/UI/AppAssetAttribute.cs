namespace Our.Shield.Core.UI
{
    using ClientDependency.Core;
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class AppAssetAttribute : System.Attribute
    {
        public ClientDependencyType AssetType;
        public string FilePath;
    
        public AppAssetAttribute(ClientDependencyType assetType, string filePath)
        {
            AssetType = assetType;
            FilePath = filePath;
        }
    }
}
