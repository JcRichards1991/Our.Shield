[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Our.Shield.Core.Operation.WebRequestHandler), nameof(Our.Shield.Core.Operation.WebRequestHandler.Register))]
namespace Our.Shield.Core.Operation
{
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;

    /// <summary>
    /// 
    /// </summary>
    internal class WebRequestHandler : IHttpModule
    {
        private const int watchLockTimeout = 1000;

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
            public Func<int, HttpApplication, WatchCycle> request;
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
                return a.priority - b.priority;
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
            int beginRequestPriority, Func<int, HttpApplication, WatchCycle> beginRequest, 
            int endRequestPriority, Func<int, HttpApplication, WatchCycle> endRequest)
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

        private void Application_BeginRequest(object source, EventArgs e)
        {
            JobService.Instance.Poll();
restart:
            string uri = ((HttpApplication)source).Context.Request.Url.AbsoluteUri;
            string uriWithoutDomain = null;

#if DEBUG
            //  Ignore when debugging
            if (uri.EndsWith("/umbraco/backoffice/UmbracoApi/Authentication/GetRemainingTimeoutSeconds") ||
                uri.EndsWith("/umbraco/ping.aspx"))
            {
                return;
            }
#endif

            int count = 0;

            if (beginWatchLock.TryEnterReadLock(watchLockTimeout))
            {
                try
                {
                    count++;
                    foreach (var watch in beginWatchers)
                    {
                        string filePath = null;
                        if (watch.domains == null)
                        {
                            if (uriWithoutDomain == null)
                            {
                                uriWithoutDomain = ((HttpApplication)source).Context.Request.Url.LocalPath;
                            }
                            filePath = uriWithoutDomain;
                        }
                        else
                        {
                            var domain = watch.domains.FirstOrDefault(x => uri.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));
                            if (domain != null)
                            {
                                filePath = uri.Substring(domain.Length + 1);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        if ((watch.regex == null || watch.regex.IsMatch(filePath)))
                        {
                            switch (watch.request(count, (HttpApplication)source))
                            {
                                case WatchCycle.Stop:
                                    return;

                                case WatchCycle.Restart:
                                    goto restart;

                                case WatchCycle.Error:
                                    ((HttpApplication)source).Context.Response.StatusCode = 500;
                                    ((HttpApplication)source).CompleteRequest();
                                    break;

                                //  If WatchCycle.Continue we do nothing
                            }
                        }
                    }
                }
                finally
                {
                    beginWatchLock.ExitReadLock();
                }
            }
        }

        private void Application_EndRequest(Object source, EventArgs e)
        {
            string filePath = ((HttpApplication)source).Context.Request.FilePath;

            if (filePath == "/umbraco/backoffice/UmbracoApi/Authentication/GetRemainingTimeoutSeconds")
            {
                return;
            }

            int count = 0;

            if (endWatchLock.TryEnterReadLock(watchLockTimeout))
            {
                try
                {
restart:
                    count++;
                    foreach (var watch in endWatchers)
                    {
                        if ((watch.regex == null || watch.regex.IsMatch(filePath)) &&
                            (watch.domains == null || watch.domains.Any(x => filePath.StartsWith(x, StringComparison.InvariantCultureIgnoreCase))))
                        {
                            switch (watch.request(count, (HttpApplication)source))
                            {
                                case WatchCycle.Stop:
                                    return;

                                case WatchCycle.Restart:
                                    goto restart;

                                case WatchCycle.Error:
                                    ((HttpApplication)source).Context.Response.StatusCode = 500;
                                    ((HttpApplication)source).CompleteRequest();
                                    break;

                                //  If WatchCycle.Continue we do nothing
                            }
                        }
                    }
                }
                finally
                {
                    endWatchLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose() { }
    }
}
