using Our.Shield.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Net;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Our.Shield.Core.Operation.WebRequestHandler), nameof(Our.Shield.Core.Operation.WebRequestHandler.Register))]
namespace Our.Shield.Core.Operation
{
    /// <summary>
    /// 
    /// </summary>
    internal class WebRequestHandler : IHttpModule
    {
        private const int watchLockTimeout = 1000;
        private const int requestRestartLimit = 100;

        /// <summary>
        /// 
        /// </summary>
        public static void Register()
        {
            // Register our module
            Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(typeof(WebRequestHandler));
        }

        private class Watcher
        {
            public int id;
            public int priority;
            public IEnvironment environment;
            public IList<string> domains;
            public string appId;
            public Regex regex;
            public Func<int, HttpApplication, WatchResponse> request;
        }

        private static ReaderWriterLockSlim beginWatchLock = new ReaderWriterLockSlim();
        private static ReaderWriterLockSlim endWatchLock = new ReaderWriterLockSlim();

        private static List<Watcher> beginWatchers = new List<Watcher>();
        private static List<Watcher> endWatchers = new List<Watcher>();

        private static int requestCount = 0;

        private class WatchComparer : IComparer<Watcher>
        {
            public int Compare(Watcher a, Watcher b)
            {
                var sortOrder = a.environment.SortOrder - b.environment.SortOrder;
                return (sortOrder != 0) ? sortOrder : a.priority - b.priority;
            }
        }

        private static IList<string> Domains(IEnumerable<IDomain> domains)
        {
            if (domains == null || !domains.Any())
            {
                return null;
            }

            var results = new List<string>();

            foreach (var domain in domains)
            {
                var url = new UriBuilder(domain.Name);
                if (url.Scheme == null)
                {
                    url.Scheme = Uri.UriSchemeHttp;

                    var urlHttps = new UriBuilder(domain.Name);
                    urlHttps.Scheme = Uri.UriSchemeHttps;
                    results.Add(urlHttps.ToString());
                }
                results.Add(url.ToString());
            }
            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="regex"></param>
        /// <param name="beginRequestPriority"></param>
        /// <param name="beginRequest"></param>
        /// <param name="endRequestPriority"></param>
        /// <param name="endRequest"></param>
        /// <returns></returns>
        public static int Watch(IJob job, Regex regex, 
            int beginRequestPriority, Func<int, HttpApplication, WatchResponse> beginRequest, 
            int endRequestPriority, Func<int, HttpApplication, WatchResponse> endRequest)
        {
            var count = Interlocked.Increment(ref requestCount);

            if (beginRequest != null)
            {
                if (beginWatchLock.TryEnterWriteLock(watchLockTimeout))
                {
                    try
                    {
                        beginWatchers.Add(new Watcher
                        {
                            id = count,
                            priority = beginRequestPriority,
                            environment = job.Environment,
                            domains = Domains(job.Environment.Domains),
                            appId = job.App.Id,
                            regex = regex,
                            request = beginRequest
                        });

                        beginWatchers.Sort(new WatchComparer());
                    }
                    finally
                    {
                        beginWatchLock.ExitWriteLock();
                    }

                }
            }

            if (endRequest != null)
            {
                if (endWatchLock.TryEnterWriteLock(watchLockTimeout))
                {
                    try
                    {
                        endWatchers.Add(new Watcher
                        {
                            id = count,
                            priority = endRequestPriority,
                            environment = job.Environment,
                            domains = Domains(job.Environment.Domains),
                            appId = job.App.Id,
                            regex = regex,
                            request = endRequest
                        });

                        endWatchers.Sort(new WatchComparer());
                    }
                    finally
                    {
                        endWatchLock.ExitWriteLock();
                    }

                }
            }

            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="regex"></param>
        /// <returns></returns>
        public static int Unwatch(IJob job, Regex regex)
        {
            string regy = regex == null ? null : regex.ToString();
            var count = 0;

            if (beginWatchLock.TryEnterWriteLock(watchLockTimeout))
            {
                try
                {
                    count += beginWatchers.RemoveAll(x => x.environment.Id == job.Environment.Id &&
                        x.appId.Equals(job.App.Id, StringComparison.InvariantCultureIgnoreCase) && 
                        ((regy == null && x.regex == null) || (regy != null && x.regex != null && regy.Equals(x.regex.ToString(), StringComparison.InvariantCulture))));
                }
                finally
                {
                    beginWatchLock.ExitWriteLock();
                }
            }

            if (endWatchLock.TryEnterWriteLock(watchLockTimeout))
            {
                try
                {
                    count += endWatchers.RemoveAll(x => x.environment.Id == job.Environment.Id &&
                        x.appId.Equals(job.App.Id, StringComparison.InvariantCultureIgnoreCase) && 
                        ((regy == null && x.regex == null) || (regy != null && x.regex != null && regy.Equals(x.regex.ToString(), StringComparison.InvariantCulture))));
                }
                finally
                {
                    endWatchLock.ExitWriteLock();
                }
            }

            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public static int Unwatch(IJob job)
        {
            var count = 0;

            if (beginWatchLock.TryEnterWriteLock(watchLockTimeout))
            {
                try
                {
                    count += beginWatchers.RemoveAll(x => x.environment.Id == job.Environment.Id &&
                        x.appId.Equals(job.App.Id, StringComparison.InvariantCultureIgnoreCase));
                }
                finally
                {
                    beginWatchLock.ExitWriteLock();
                }
            }

            if (endWatchLock.TryEnterWriteLock(watchLockTimeout))
            {
                try
                {
                    count += endWatchers.RemoveAll(x => x.environment.Id == job.Environment.Id &&
                        x.appId.Equals(job.App.Id, StringComparison.InvariantCultureIgnoreCase));
                }
                finally
                {
                    endWatchLock.ExitWriteLock();
                }
            }

            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static int Unwatch(string appId)
        {
            var count = 0;

            if (beginWatchLock.TryEnterWriteLock(watchLockTimeout))
            {
                try
                {
                    count += beginWatchers.RemoveAll(x => x.appId.Equals(appId, StringComparison.InvariantCultureIgnoreCase));
                }
                finally
                {
                    beginWatchLock.ExitWriteLock();
                }
            }

            if (endWatchLock.TryEnterWriteLock(watchLockTimeout))
            {
                try
                {
                    count += endWatchers.RemoveAll(x => x.appId.Equals(appId, StringComparison.InvariantCultureIgnoreCase));
                }
                finally
                {
                    endWatchLock.ExitWriteLock();
                }
            }

            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="application"></param>
        public void Init(HttpApplication application)
        {
            application.BeginRequest += (new EventHandler(this.Application_BeginRequest));
            application.EndRequest += (new EventHandler(this.Application_EndRequest));
        }

        private WatchResponse.Cycles ExecuteResponse(Watcher watch, WatchResponse response, HttpApplication application)
        {
            if (response.Transfer == null)
            {
                return response.Cycle;
            }

            if (response.Transfer.TransferType == TransferTypes.PlayDead)
            {
                application.Context.Response.Close();
                return WatchResponse.Cycles.Stop;
            }

            var url = new UmbracoUrlService().Url(response.Transfer.Url);
            if (!string.IsNullOrWhiteSpace(url))
            {
                switch (response.Transfer.TransferType)
                {
                    case TransferTypes.Redirect:
                        application.Context.Response.Redirect(url, true);
                        return WatchResponse.Cycles.Stop;

                    case TransferTypes.Rewrite:
                        application.Context.RewritePath(url, string.Empty, string.Empty);
                        return WatchResponse.Cycles.Restart;
                }
            }

            return WatchResponse.Cycles.Error;
        }

        private void Request(ReaderWriterLockSlim locker, IEnumerable<Watcher> watchers, HttpApplication application)
        {
            if (locker.TryEnterReadLock(watchLockTimeout))
            {
                try
                {
                    if (!watchers.Any())
                    {
                        return;
                    }

                    int count = 0;
restart:
                    if (count++ > requestRestartLimit)
                    {
                        application.Context.Response.StatusCode = 500;
                        application.CompleteRequest();
                    }

                    IEnvironment environment = null;
                    string uri = application.Context.Request.Url.AbsoluteUri;
                    string uriWithoutDomain = null;

                    foreach (var watch in watchers)
                    {
                        if (environment != null && environment.Id != watch.environment.Id)
                        {
                            return;
                        }

                        string filePath = null;
                        if (watch.domains == null)
                        {
                            if (uriWithoutDomain == null)
                            {
                                uriWithoutDomain = application.Context.Request.Url.LocalPath;
                            }
                            filePath = uriWithoutDomain;
                        }
                        else
                        {
                            var domain = watch.domains.FirstOrDefault(x => uri.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));
                            if (domain != null)
                            {
                                filePath = uri.Substring(domain.Length - 1);
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if ((watch.regex == null || watch.regex.IsMatch(filePath)))
                        {
                            if (!watch.environment.ContinueProcessing)
                            {
                                environment = watch.environment;
                            }

                            switch (ExecuteResponse(watch, watch.request(count, application), application))
                            {
                                case WatchResponse.Cycles.Stop:
                                    return;

                                case WatchResponse.Cycles.Restart:
                                    goto restart;

                                case WatchResponse.Cycles.Error:
                                    application.Context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                                    application.CompleteRequest();
                                    break;

                                //  If WatchCycle.Continue we do nothing
                            }
                        }
                    }
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
        }

        private void Application_BeginRequest(object source, EventArgs e)
        {
            JobService.Instance.Poll();
            Request(beginWatchLock, beginWatchers, (HttpApplication)source);
        }

        private void Application_EndRequest(Object source, EventArgs e)
        {
            Request(endWatchLock, endWatchers, (HttpApplication)source);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose() { }
    }
}
