using System;

namespace Our.Shield.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class AppTabAttribute : Attribute
    {
        public readonly string FilePath;
        public readonly int SortOrder;
        public readonly string Caption;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="caption">The text for the tab</param>
        /// <param name="filePath">File path of the html file</param>
        /// <param name="sortOrder">The sort order of the tab</param>
        protected AppTabAttribute(string caption, int sortOrder, string filePath)
        {
            Caption = caption;
            SortOrder = sortOrder;
            FilePath = filePath;
        }
    }
}
