namespace Our.Shield.Core.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AppEditorAttribute : System.Attribute
    {
        public string FilePath;
    
        public AppEditorAttribute(string filePath)
        {
            FilePath = filePath;
        }
    }
}
