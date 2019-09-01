using System;

namespace Our.Shield.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AppJournalAttribute : AppTabAttribute
    {
        /// <inheritdoc />
        public AppJournalAttribute(string caption = "Journal", int sortOrder = 1, string filePath = "/App_Plugins/Shield/Backoffice/Dashboards/Journal.html?version=1.1.0") : base(caption, sortOrder, filePath)
        {
        }
    }
}
