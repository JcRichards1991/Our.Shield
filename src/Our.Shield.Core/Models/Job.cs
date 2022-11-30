using Our.Shield.Core.Enums;
using Our.Shield.Core.Operation;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Our.Shield.Core.Models
{
    /// <inheritdoc />
    /// <summary>
    /// Class that contains each of our executions
    /// </summary>
    public class Job : IJob
    {
        /// <inheritdoc />
        public IEnvironment Environment { get; set; }

        /// <inheritdoc />
        public IApp App { get; set; }

        /// <summary>
        /// The unique key of the job
        /// </summary>
        public Guid Key { get; internal set; }

        internal Type ConfigType;

        internal DateTime? LastRan;
        internal Task<bool> Task;
        internal CancellationTokenSource CancelToken;

        internal Job DeepCopy(IApp app)
        {
            return new Job
            {
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
        public int WatchWebRequests(
            PipeLineStages stage,
            Regex regex,
            int priority,
            Func<int, HttpApplication, WatchResponse> request) => WebRequestHandler.Watch(this, stage, regex, priority, request);

        /// <inheritdoc />
        public int UnwatchWebRequests(PipeLineStages stage, Regex regex) => WebRequestHandler.Unwatch(this, stage, regex);

        /// <inheritdoc />
        public int UnwatchWebRequests(PipeLineStages stage) => WebRequestHandler.Unwatch(this, stage);

        /// <inheritdoc />
        public int UnwatchWebRequests() => WebRequestHandler.Unwatch(Environment.Key, App.Id);

        /// <inheritdoc />
        public int UnwatchWebRequests(IApp app) => WebRequestHandler.Unwatch(app.Id);

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

        public int IgnoreWebRequest(Regex regex) => WebRequestHandler.Ignore(this, regex);

        public Regex PathToRegex(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (path[0] == '/')
            {
                path = path.Substring(1);
            }

            if (path[path.Length - 1] == '/')
            {
                path = path.Substring(0, path.Length - 1);
            }

            return new Regex(@"^/" + path + "$|/" + path + "/", RegexOptions.IgnoreCase);
        }

        public int IgnoreWebRequest(string path) => WebRequestHandler.Ignore(this, PathToRegex(path));

        /// <inheritdoc />
        public int UnignoreWebRequest(Regex regex) => WebRequestHandler.Unignore(this, regex);

        public int UnignoreWebRequest(string path) => WebRequestHandler.Unignore(this, PathToRegex(path));

        /// <inheritdoc />
        public int UnignoreWebRequest() => WebRequestHandler.Unignore(this);
    }
}
