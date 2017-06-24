namespace Shield.Core.Operation
{
    using System.Collections.Generic;

    public interface IOperation : IFrisk
    {
        bool Init();

        bool Execute(Persistance.Serialization.Configuration config);

        bool WriteConfiguration(Persistance.Serialization.Configuration config);

        bool WriteJournal(Persistance.Serialization.Journal journal);

        Persistance.Serialization.Configuration ReadConfiguration();

        IEnumerable<Persistance.Serialization.Journal> ReadJournals(int page, int itemsPerPage);

        Persistance.Serialization.Configuration DefaultConfiguration { get; }
    }
}
