using System;

namespace Our.Shield.Core.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Attribute to inform Our.Shield.Core where to find the view to handle the configuration of an app
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AppEditorAttribute : AppTabAttribute
    {
        public string AppView { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="filePath">location of the app's configuration html file</param>
        /// <param name="caption"></param>
        /// <param name="sortOrder"></param>
        public AppEditorAttribute(string filePath, string caption = "Configuration", int sortOrder = 0) : base(caption, sortOrder, "/App_Plugins/Shield/Backoffice/Views/EditApp.html?version=1.0.7")
        {
            AppView = filePath;
        }
    }
}
