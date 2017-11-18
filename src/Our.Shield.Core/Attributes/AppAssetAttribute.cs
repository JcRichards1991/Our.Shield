using ClientDependency.Core;
using System;

namespace Our.Shield.Core.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AppAssetAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public ClientDependencyType AssetType;

        /// <summary>
        /// 
        /// </summary>
        public string FilePath;
        
        /// <inheritdoc />
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
