using System.Collections.Generic;

namespace Shield.Core.Models.Interfaces
{
    public interface IOperation : Operation.IFrisk
    {
        bool Init();

        bool Execute(Configuration config);

        bool WriteConfiguration(Configuration config);

        bool WriteJournal(Journal journal);

        Configuration ReadConfiguration();

        IEnumerable<Journal> ReadJournals(int page, int itemsPerPage);
    }
}
