[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Shield.Core.Operation.WebRequestHandler), nameof(Shield.Core.Operation.WebRequestHandler.Register))]
namespace Shield.Core.Operation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using Models;

    internal class WebRequestHandler : IHttpModule
    {
        private const int watchLockTimeout = 1000;

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
                return b.priority - a.priority;
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
                            appId = job.AppId,
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
                            appId = job.AppId,
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

        public static int Unwatch(IJob job, Regex regex)
        {
            string regy = regex == null ? null : regex.ToString();
            var count = 0;

            if (beginWatchLock.TryEnterWriteLock(watchLockTimeout))
            {
                try
                {
                    count += beginWatchers.RemoveAll(x => x.environment.Id == job.Environment.Id &&
                        x.appId.Equals(job.AppId, StringComparison.InvariantCultureIgnoreCase) && 
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
                        x.appId.Equals(job.AppId, StringComparison.InvariantCultureIgnoreCase) && 
                        ((regy == null && x.regex == null) || (regy != null && x.regex != null && regy.Equals(x.regex.ToString(), StringComparison.InvariantCulture))));
                }
                finally
                {
                    endWatchLock.ExitWriteLock();
                }
            }

            return count;
        }

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

        public void Init(HttpApplication application)
        {
            application.BeginRequest += (new EventHandler(this.Application_BeginRequest));
            application.EndRequest += (new EventHandler(this.Application_EndRequest));
        }

        private void Application_BeginRequest(object source, EventArgs e)
        {
            JobService.Instance.Poll();
            
            string filePath = ((HttpApplication)source).Context.Request.FilePath;
            int count = 0;

            if (beginWatchLock.TryEnterReadLock(watchLockTimeout))
            {
                try
                {
restart:
                    count++;
                    foreach (var watch in beginWatchers)
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
                    beginWatchLock.ExitReadLock();
                }
            }
        }

        private void Application_EndRequest(Object source, EventArgs e)
        {
            string filePath = ((HttpApplication)source).Context.Request.FilePath;
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

        public void Dispose() { }
    }
}
