using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shield.Core.Persistance.Business;

namespace Shield.Core.Models
{
    /// <summary>
    /// Class that conatins each of our executions
    /// </summary>
    public class Job : IJob
    {
        public int Id { get; set; }
        public IEnvironment Environment { get; set; }
        public string AppId { get; set; }

        internal Type AppType;
        internal Type ConfigType;
        internal IConfiguration DefaultConfig;

        internal IJob DeepCopy()
        {
            return new Job
            {
                Id = this.Id,
                AppId = this.AppId,
                Environment = this.Environment
            };
        }

        internal DateTime? LastRan;
        internal Task<bool> Task;
        internal CancellationTokenSource CancelToken;

        public bool WriteConfiguration(IConfiguration config) => Operation.JobService.Instance.WriteConfiguration(this, config);
        public bool WriteJournal(IJournal journal) => Operation.JobService.Instance.WriteJournal(this, journal);
        public IConfiguration ReadConfiguration() => Operation.JobService.Instance.ReadConfiguration(this);
        public IEnumerable<T> ListJournals<T>(int page, int itemsPerPage) where T : IJournal => Operation.JobService.Instance.ListJournals<T>(this, page, itemsPerPage);
    }
}
