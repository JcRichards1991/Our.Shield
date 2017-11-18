using System;

namespace Our.Shield.Core.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AppEditorAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string FilePath;
        
        /// <inheritdoc />
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        public AppEditorAttribute(string filePath)
        {
            FilePath = filePath;
        }
    }
}
