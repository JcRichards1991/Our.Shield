using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

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

        int WatchWebRequests(Regex regex, 
            int beginRequestPriority, Func<int, HttpApplication, WatchCycle> beginRequest, 
            int endRequestPriority, Func<int, HttpApplication, WatchCycle> endRequest);

        int WatchWebRequests(Regex regex, 
            int beginRequestPriority, Func<int, HttpApplication, WatchCycle> beginRequest);

        int UnwatchWebRequests(Regex regex);

        int UnwatchWebRequests();

        int UnwatchWebRequests(IApp app);


    }
}
