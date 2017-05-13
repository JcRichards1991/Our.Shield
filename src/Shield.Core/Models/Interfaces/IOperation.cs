namespace Shield.Core.Models.Interfaces
{
    using System.Collections.Generic;

    public interface IOperation : IFrisk
    {
        bool Init();

        bool Execute(Configuration config);

        bool WriteConfiguration(Configuration config);

        bool WriteJournal(Journal journal);

        Configuration ReadConfiguration();

        IEnumerable<Journal> ReadJournals(int page, int itemsPerPage);

        Configuration DefaultConfiguration { get; }
    }
}
