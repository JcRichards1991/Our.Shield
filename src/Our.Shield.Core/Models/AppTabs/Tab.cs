using Our.Shield.Core.Attributes;

namespace Our.Shield.Core.Models.AppTabs
{
    /// <summary>
    /// Base implementation for <see cref="ITab"/>
    /// </summary>
    internal class Tab : ITab
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Tab"/>
        /// </summary>
        /// <param name="attr"></param>
        public Tab(AppTabAttribute attr)
        {
            View = attr.FilePath;
            Name = attr.Caption;
            Id = attr.SortOrder;
            Active = attr.SortOrder == 0;
            Icon = attr.Icon;
        }

        /// <inheritdoc />
        public int Id { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public bool Active { get; set; }

        /// <inheritdoc />
        public string View { get; set; }

        public string Icon { get; set; }
    }
}
