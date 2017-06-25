namespace Shield.Core.Operation
{
    using System.Collections.Generic;

    public interface IApp : IFrisk
    {

        /// <summary>
        /// Name of the plugin
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of the plugin
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Css class of icon
        /// </summary>
        string Icon { get; }

        bool Init();

        bool Execute(Persistance.Serialization.Configuration config);

        bool WriteConfiguration(Persistance.Serialization.Configuration config);

        bool WriteJournal(Persistance.Serialization.Journal journal);

        Persistance.Serialization.Configuration ReadConfiguration();

        IEnumerable<Persistance.Serialization.Journal> ReadJournals(int page, int itemsPerPage);

        Persistance.Serialization.Configuration DefaultConfiguration { get; }
    }
}
