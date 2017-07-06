namespace Our.Shield.Core.UI
{
    using ClientDependency.Core;
    using System;

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class AppAssetAttribute : System.Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public ClientDependencyType AssetType;

        /// <summary>
        /// 
        /// </summary>
        public string FilePath;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="filePath"></param>
        public AppAssetAttribute(ClientDependencyType assetType, string filePath)
        {
            AssetType = assetType;
            FilePath = filePath;
        }
    }
}
