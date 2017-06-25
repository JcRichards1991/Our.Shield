[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Shield.Core.Operation.Fortress), nameof(Shield.Core.Operation.Fortress.Register))]
namespace Shield.Core.Operation
{
    using System.Web;
    using System;
    using System.Linq;
    using System.Threading;
    using Umbraco.Web;
    using System.Text.RegularExpressions;
    using System.Collections.Generic;
    using System.Collections;

    public class Fortress : IHttpModule
    {
        private const int watchLockTimeout = 1000;

        public static void Register()
        {
            // Register our module
            Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(typeof(Fortress));
        }

        public enum Cycle
        {
            Stop,
            Continue,
            Restart,
            Error
        }

        private class Watcher
        {
            public int id;
            public int priority;
            public Persistance.Data.Dto.Environment environment;
            public IList<Tuple<string, Persistance.Data.Dto.Domain>> domains;
            public IApp app;
            public Regex regex;
            public Func<int, HttpApplication, Cycle> request;
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

        private Tuple<int, string> Domains(IEnumerable<Persistance.Data.Dto.Domain> domains)
        {
            var results = new List<Tuple<string, Persistance.Data.Dto.Domain>>();

            foreach (var domain in domains)
            {
                string url = null;
                if (domain.UmbracoDomainId == null)
                {
                    url = domain.Name;
                }
                else
                {



        public static int Watch(Persistance.Data.Dto.Environment environment, IApp app, Regex regex, 
            int beginRequestPriority, Func<int, HttpApplication, Cycle> beginRequest, 
            int endRequestPriority, Func<int, HttpApplication, Cycle> endRequest)
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
                            environment = environment,
                            domains = Domains(environment.Domains),
                            app = app,
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
                            environment = environment,
                            domains = Domains(environment.Domains),
                            app = app,
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

        public static int Watch(Persistance.Data.Dto.Environment environment, IApp app, Regex regex, 
            int beginRequestPriority, Func<int, HttpApplication, Cycle> beginRequest)
        {
            return Watch(environment, app, regex, beginRequestPriority, beginRequest, 0, null);
        }

        public static int Unwatch(Persistance.Data.Dto.Environment environment, IApp app, Regex regex)
        {
            string regy = regex == null ? null : regex.ToString();
            var count = 0;

            if (beginWatchLock.TryEnterWriteLock(watchLockTimeout))
            {
                try
                {
                    count += beginWatchers.RemoveAll(x => x.environment.Id == environment.Id &&
                        x.app.Id.Equals(app.Id, StringComparison.InvariantCultureIgnoreCase) && 
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
                    count += endWatchers.RemoveAll(x => x.environment.Id == environment.Id &&
                        x.app.Id.Equals(app.Id, StringComparison.InvariantCultureIgnoreCase) && 
                        ((regy == null && x.regex == null) || (regy != null && x.regex != null && regy.Equals(x.regex.ToString(), StringComparison.InvariantCulture))));
                }
                finally
                {
                    endWatchLock.ExitWriteLock();
                }
            }

            return count;
        }

        public static int Unwatch(IApp app)
        {
            var count = 0;

            if (beginWatchLock.TryEnterWriteLock(watchLockTimeout))
            {
                try
                {
                    count += beginWatchers.RemoveAll(x => x.app.Id.Equals(app.Id, StringComparison.InvariantCultureIgnoreCase));
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
                    count += endWatchers.RemoveAll(x => x.app.Id.Equals(app.Id, StringComparison.InvariantCultureIgnoreCase));
                }
                finally
                {
                    endWatchLock.ExitWriteLock();
                }
            }

            return count;
        }

        public static int UnwatchAll(string appId)
        {
            var count = 0;

            if (beginWatchLock.TryEnterWriteLock(watchLockTimeout))
            {
                try
                {
                    count += beginWatchers.RemoveAll(x => x.app.Id.Equals(appId, StringComparison.InvariantCultureIgnoreCase));
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
                    count += endWatchers.RemoveAll(x => x.app.Id.Equals(appId, StringComparison.InvariantCultureIgnoreCase));
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
            Executor.Instance.Poll();
            
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
                        //  First see if the domain matches
                        watch.environment.Domains



                        if (watch.regex == null || watch.regex.IsMatch(filePath))
                        {
                            switch (watch.request(count, (HttpApplication)source))
                            {
                                case Cycle.Stop:
                                    return;

                                case Cycle.Restart:
                                    goto restart;

                                case Cycle.Error:
                                    ((HttpApplication)source).Context.Response.StatusCode = 500;
                                    ((HttpApplication)source).CompleteRequest();
                                    break;

                                //  If Cycle.Continue we do nothing
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
                        if (watch.regex == null || watch.regex.IsMatch(filePath))
                        {
                            switch (watch.request(count, (HttpApplication)source))
                            {
                                case Cycle.Stop:
                                    return;

                                case Cycle.Restart:
                                    goto restart;

                                case Cycle.Error:
                                    ((HttpApplication)source).Context.Response.StatusCode = 500;
                                    ((HttpApplication)source).CompleteRequest();
                                    break;

                                //  If Cycle.Continue we do nothing
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
