namespace Our.Shield.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    /// <summary>
    /// Class that conatins each of our executions
    /// </summary>
    internal class Job : IJob
    {
        public int Id { get; set; }
        public IEnvironment Environment { get; set; }
        public string AppId { get; set; }

        internal Type AppType;
        internal Type ConfigType;

        internal DateTime? LastRan;
        internal Task<bool> Task;
        internal CancellationTokenSource CancelToken;

        internal IJob DeepCopy()
        {
            return new Job
            {
                Id = this.Id,
                Environment = this.Environment,
                AppId = this.AppId,
                AppType = this.AppType,
                ConfigType = this.ConfigType,
                LastRan = this.LastRan,
                Task = this.Task,
                CancelToken = this.CancelToken
            };
        }

        public bool WriteConfiguration(IConfiguration config) => Operation.JobService.Instance.WriteConfiguration(this, config);
        public bool WriteJournal(IJournal journal) => Operation.JobService.Instance.WriteJournal(this, journal);
        public IConfiguration ReadConfiguration() => Operation.JobService.Instance.ReadConfiguration(this);
        public IEnumerable<T> ListJournals<T>(int page, int itemsPerPage) where T : IJournal => Operation.JobService.Instance.ListJournals<T>(this, page, itemsPerPage);

        public int WatchWebRequests(Regex regex, 
            int beginRequestPriority, Func<int, HttpApplication, WatchCycle> beginRequest, 
            int endRequestPriority, Func<int, HttpApplication, WatchCycle> endRequest) =>
            Operation.WebRequestHandler.Watch(this, regex, beginRequestPriority, beginRequest, endRequestPriority, endRequest);

        public int WatchWebRequests(Regex regex, 
            int beginRequestPriority, Func<int, HttpApplication, WatchCycle> beginRequest) => 
            Operation.WebRequestHandler.Watch(this, regex, beginRequestPriority, beginRequest, 0, null);

        public int UnwatchWebRequests(Regex regex) => Operation.WebRequestHandler.Unwatch(this, regex);
        public int UnwatchWebRequests() => Operation.WebRequestHandler.Unwatch(this);
        public int UnwatchWebRequests(IApp app) => Operation.WebRequestHandler.Unwatch(app.Id);
    }
}
