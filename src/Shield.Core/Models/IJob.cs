using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Core.Models
{
    public interface IJob
    {
        int Id { get; }

        IEnvironment Environment { get; }

        string AppId { get; }

        bool WriteConfiguration(IConfiguration config);

        bool WriteJournal(IJournal journal);

        IConfiguration ReadConfiguration();

        IEnumerable<T> ListJournals<T>(int page, int itemsPerPage) where T : IJournal;

    }
}
