using System;

namespace Our.Shield.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomAppTabAttribute : AppTabAttribute
    {
        /// <inheritdoc />
        /// <summary>
        /// Shows an additional custom tab for the custom app
        /// </summary>
        /// <param name="caption">The text to appear for the tab caption</param>
        /// <param name="sortOrder">The order in which this tab should be ordered</param>
        /// <param name="filePath">The location to the view for the tab's content</param>
        public CustomAppTabAttribute(string caption, int sortOrder, string filePath) : base(caption, sortOrder, filePath)
        {
        }
    }
}
