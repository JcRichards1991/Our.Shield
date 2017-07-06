namespace Our.Shield.Core.UI
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AppEditorAttribute : System.Attribute
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
