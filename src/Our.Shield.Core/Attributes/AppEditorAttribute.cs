using System;

namespace Our.Shield.Core.Attributes
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AppEditorAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string FilePath;
        
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
