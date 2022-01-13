using System;

namespace Our.Shield.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AppJournalAttribute : AppTabAttribute
    {
        /// <inheritdoc />
        public AppJournalAttribute(
            int sortOrder = 1)
            : base("Journal", sortOrder, "/App_Plugins/Shield/Backoffice/Dashboards/Journal.html?version=2.0.0", "icon-message")
        {
        }
    }
}
