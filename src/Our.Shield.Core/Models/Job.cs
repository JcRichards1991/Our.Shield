using Our.Shield.Core.Operation;
using Our.Shield.Core.Services;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Our.Shield.Core.Models
{
    /// <inheritdoc />
    /// <summary>
    /// Class that conatins each of our executions
    /// </summary>
    internal class Job : IJob
    {
        /// <inheritdoc />
        public int Id { get; set; }

        /// <inheritdoc />
        public IEnvironment Environment { get; set; }

        /// <inheritdoc />
        public IApp App { get; set; }

        public Guid Key { get; internal set; }

        internal Type ConfigType;

        internal DateTime? LastRan;
        internal Task<bool> Task;
        internal CancellationTokenSource CancelToken;

        internal IJob DeepCopy()
        {
            var app = App<IAppConfiguration>.Create(App.Id);
            app.Migrations = App.Migrations;

            return new Job
            {
                Id = Id,
                Key = Key,
                Environment = Environment,
                App = app,
                ConfigType = ConfigType,
                LastRan = LastRan,
                Task = Task,
                CancelToken = CancelToken
            };
        }

        /// <inheritdoc />
        public bool WriteConfiguration(IAppConfiguration config) =>
            JobService.Instance.WriteConfiguration(this, config);

        /// <inheritdoc />
        public bool WriteJournal(IJournal journal) =>
            JobService.Instance.WriteJournal(this, journal);

        /// <inheritdoc />
        public IAppConfiguration ReadConfiguration() =>
            JobService.Instance.ReadConfiguration(this);

        /// <inheritdoc />
        public IEnumerable<T> ListJournals<T>(int page, int itemsPerPage, out int totalPages) where T : IJournal =>
            JobService.Instance.ListJournals<T>(this, page, itemsPerPage, out totalPages);

        /// <inheritdoc />
        public int WatchWebRequests(PipeLineStages stage, Regex regex, 
            int priority, Func<int, HttpApplication, WatchResponse> request) =>
            WebRequestHandler.Watch(this, stage, regex, priority, request);

        /// <inheritdoc />
        public int UnwatchWebRequests(PipeLineStages stage, Regex regex) =>
            WebRequestHandler.Unwatch(this, stage, regex);

        /// <inheritdoc />
        public int UnwatchWebRequests(PipeLineStages stage) =>
            WebRequestHandler.Unwatch(this, stage);

        /// <inheritdoc />
        public int UnwatchWebRequests() =>
            WebRequestHandler.Unwatch(Environment.Id, App.Id);

        /// <inheritdoc />
        public int UnwatchWebRequests(IApp app) =>
            WebRequestHandler.Unwatch(app.Id);

        /// <inheritdoc />
        public int ExceptionWebRequest(Regex regex) => WebRequestHandler.Exception(this, regex);

        /// <inheritdoc />
        public int ExceptionWebRequest(UmbracoUrl url) => WebRequestHandler.Exception(this, null, url);

        /// <inheritdoc />
        public int UnexceptionWebRequest(Regex regex) => WebRequestHandler.Unexception(this, regex);

        /// <inheritdoc />
        public int UnexceptionWebRequest(UmbracoUrl url) => WebRequestHandler.Unexception(this, url);

        /// <inheritdoc />
        public int UnexceptionWebRequest() => WebRequestHandler.Unexception(this);

    }
}
