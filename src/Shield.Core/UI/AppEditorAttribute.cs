using System;

namespace Shield.Core.UI
{
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
