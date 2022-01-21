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
        internal string AppView { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="filePath">location of the app's configuration HTML file</param>
        /// <param name="caption">The Text to display for the tab</param>
        /// <param name="sortOrder">The order this tab should appear</param>
        public AppEditorAttribute(
            string filePath,
            int sortOrder = 0)
            : base("Configuration", sortOrder, "/App_Plugins/Shield/Backoffice/Views/EditApp.html?version=1.1.0", "icon-settings")
        {
            AppView = filePath;
        }
    }
}
