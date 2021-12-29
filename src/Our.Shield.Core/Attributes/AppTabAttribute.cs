using System;

namespace Our.Shield.Core.Attributes
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class AppTabAttribute : Attribute
    {
        internal readonly string FilePath;
        internal readonly int SortOrder;
        internal readonly string Caption;

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
