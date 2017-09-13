using Our.Shield.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Net;
using Umbraco.Core.Logging;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Our.Shield.Core.Operation.WebRequestHandler), nameof(Our.Shield.Core.Operation.WebRequestHandler.Register))]
namespace Our.Shield.Core.Operation
{
    public enum PipeLineStages
    {
        BeginRequest = 0,
        AuthenticateRequest = 1,
        ResolveRequestCache = 2,
        UpdateRequestCache = 3,
        EndRequest = 4
    }

    /// <summary>
    /// 
    /// </summary>
    internal class WebRequestHandler : IHttpModule
    {
#if DEBUG
        private const int watchLockTimeout = 1000000;
#else
        private const int watchLockTimeout = 1000;
#endif

        private const int requestRestartLimit = 100;

        /// <summary>
        /// 
        /// </summary>
        public static void Register()
        {
            // Register our module
            Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(typeof(WebRequestHandler));
        }

        private static readonly int PipeLineStagesLength = Enum.GetNames(typeof(PipeLineStages)).Length;

        private class Environ
        {
            public int Id;
            public int SortOrder;
            public bool ContinueProcessing;
            public List<string> Domains;
            public ReaderWriterLockSlim[] WatchLocks;
            public List<Watcher>[] Watchers;

            public Environ(IEnvironment environment)
            {
                Id = environment.Id;
                SortOrder = environment.SortOrder;
                ContinueProcessing = environment.ContinueProcessing;
                Domains = Domains(environment.Domains);
                WatchLocks = new ReaderWriterLockSlim[PipeLineStagesLength];
                Watchers = new List<Watcher>[PipeLineStagesLength];
                for (var index = 0; index != PipeLineStagesLength; index++)
                {
                    WatchLocks[index] = new ReaderWriterLockSlim();
                    Watchers[index] = new List<Watcher>();
                }

            }
        }

        private class Watcher
        {
            public int Id;
            public int Priority;
            public string AppId;
            public Regex Regex;
            public Func<int, HttpApplication, WatchResponse> Request;
        }

        private class UrlException
        {
            public int Id;
            public int EnvironmentId;
            public string AppId;
            public Regex Regex;
			public UmbracoUrl Url;
        }

        private static ReaderWriterLockSlim EnvironLock = new ReaderWriterLockSlim();
        private static SortedDictionary<int, Environ> Environs = new SortedDictionary<int, Environ>();

        private static ReaderWriterLockSlim UrlExceptionLock = new ReaderWriterLockSlim();
        private static List<UrlException> UrlExceptions = new List<UrlException>();

        private static int requestCount = 0;

        private class WatchComparer : IComparer<Watcher>
        {
            public int Compare(Watcher a, Watcher b)
            {
                return a.Priority - b.Priority;
            }
        }

        private static List<string> Domains(IEnumerable<IDomain> domains)
        {
            if (domains == null || !domains.Any())
            {
                return null;
            }

            var results = new List<string>();

            foreach (var domain in domains)
            {
                UriBuilder url = null;
                try
                {
                    url = new UriBuilder(domain.Name);
                }
                catch (Exception ex)
                {
                    LogHelper.Error<WebRequestHandler>($"{domain.Name} is not a valid domain", ex);
                    continue;
                }
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
        public static int Watch(IJob job, PipeLineStages stage, Regex regex, int priority, Func<int, HttpApplication, WatchResponse> request)
        {
            if (!EnvironLock.TryEnterWriteLock(watchLockTimeout))
            {
				LogHelper.Error<WebRequestHandler>("Unable to create Watch, can\'t get write lock", null);
				return -1;
			}

            Environ environ;
            try
            {
                if (!Environs.TryGetValue(job.Environment.SortOrder, out environ))
                {
                    Environs.Add(job.Environment.SortOrder, environ = new Environ(job.Environment));
                }
            }
            finally
            {
                EnvironLock.ExitWriteLock();
            }

            if (!EnvironLock.TryEnterReadLock(watchLockTimeout))
            {
				LogHelper.Error<WebRequestHandler>("Unable to create Watch, can\'t get read lock", null);
				return -1;
			}
            try
            {
                if (!environ.WatchLocks[(int) stage].TryEnterWriteLock(watchLockTimeout))
                {
					LogHelper.Error<WebRequestHandler>("Unable to create Watch, can\'t get write lock", null);
					return -1;
				}
                try
                {
                    var watchList = environ.Watchers[(int) stage];
					var count = Interlocked.Increment(ref requestCount);

                    watchList.Add(new Watcher
                    {
                        Id = count,
                        Priority = priority,
                        AppId = job.App.Id,
                        Regex = regex,
                        Request = request
                    });
                    watchList.Sort(new WatchComparer());
					return count;
                }
                finally
                {
                    environ.WatchLocks[(int) stage].ExitWriteLock();
                }
            }
            finally
            {
                EnvironLock.ExitReadLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="regex"></param>
        /// <returns></returns>
        public static int Unwatch(IJob job, PipeLineStages stage, Regex regex = null)
        {
            if (EnvironLock.TryEnterReadLock(watchLockTimeout))
            {
				LogHelper.Error<WebRequestHandler>("Unable to remove Watch, can\'t get read lock", null);
				return -1;
			}
            try
            {
                Environ environ;
                if (!Environs.TryGetValue(job.Environment.SortOrder, out environ))
                {
                    return 0;
                }

                if (!environ.WatchLocks[(int) stage].TryEnterWriteLock(watchLockTimeout))
                {
					LogHelper.Error<WebRequestHandler>("Unable to remove Watch, can\'t get write lock", null);
					return -1;
				}

                try
                {
			        string regy = regex == null ? null : regex.ToString();
                    return environ.Watchers[(int) stage].RemoveAll(x =>
                        x.AppId.Equals(job.App.Id, StringComparison.InvariantCultureIgnoreCase) && 
                        ((regy == null && x.Regex == null) || 
                        (regy != null && x.Regex != null && regy.Equals(x.Regex.ToString(), StringComparison.InvariantCulture))));
                }
                finally
                {
                    environ.WatchLocks[(int) stage].ExitWriteLock();
                }
            }
            finally
            {
                EnvironLock.ExitReadLock();
            }
        }

        public static int Unwatch(string appId) => Unwatch(null, appId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static int Unwatch(int? environmentId = null, string appId = null, PipeLineStages? stage = null, Regex regex = null)
        {
            var count = 0;
            var deleteEnvirons = new List<int>();

            if (EnvironLock.TryEnterReadLock(watchLockTimeout))
            {
				LogHelper.Error<WebRequestHandler>("Unable to remove Watch, can\'t get read lock", null);
				return 0;
			}

            try
            {
				string regy = regex == null ? null : regex.ToString();
                foreach (var environ in Environs.Where(x => environmentId == null || x.Value.Id == environmentId))
                {
                    int watchCount = 0;
                    foreach (var objectStage in Enum.GetValues(typeof(PipeLineStages)))
                    {
						var currentStage = (PipeLineStages) objectStage; 
						if (stage != null && currentStage != stage)
						{
							continue;
						}
                        if (environ.Value.WatchLocks[(int) currentStage].TryEnterWriteLock(watchLockTimeout))
                        {
							LogHelper.Error<WebRequestHandler>("Unable to remove Watch, can\'t get write lock", null);
							return 0;
						}
                        try
                        {
                            count += environ.Value.Watchers[(int) currentStage].RemoveAll(x =>
                                (appId == null ||
								x.AppId.Equals(appId, StringComparison.InvariantCultureIgnoreCase)) && 
								((regy == null && x.Regex == null) || 
								(regy != null && x.Regex != null && regy.Equals(x.Regex.ToString(), StringComparison.InvariantCulture))));
                            watchCount += environ.Value.Watchers[(int) currentStage].Count();
                        }
                        finally
                        {
                            environ.Value.WatchLocks[(int) currentStage].ExitWriteLock();
                        }
                    }

                    if (watchCount == 0)
                    {
                        deleteEnvirons.Add(environ.Value.SortOrder);
                    }
                }
            }
            finally
            {
                EnvironLock.ExitReadLock();
            }

            if (!deleteEnvirons.Any())
            {
				return count;
			}

            if (!EnvironLock.TryEnterWriteLock(watchLockTimeout))
            {
				LogHelper.Error<WebRequestHandler>("Unable to remove Watch, can\'t get write lock", null);
				return 0;
			}

            try
            {
                foreach (var sortOrder in deleteEnvirons)
                {
                    Environs.Remove(sortOrder);
                }
				return count;
            }
            finally
            {
                EnvironLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="application"></param>
        public void Init(HttpApplication application)
        {
            application.BeginRequest += (new EventHandler(this.Application_BeginRequest));
            application.AuthenticateRequest += (new EventHandler(this.Application_AuthenticateRequest));
            application.ResolveRequestCache += (new EventHandler(this.Application_ResolveRequestCache));
            application.UpdateRequestCache += (new EventHandler(this.Application_UpdateRequestCache));
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
                string urlSlash = string.Copy(url);
                if (urlSlash[urlSlash.Length - 1] != '/')
                {
                    urlSlash += "/";
                }

                var slash = string.Copy(application.Context.Request.Url.AbsoluteUri);
                if (slash[slash.Length - 1] != '/')
                {
                    slash += "/";
                }

                if (slash.Equals(urlSlash))
                {
                    return WatchResponse.Cycles.Continue;
                }

                slash = string.Copy(application.Context.Request.Url.PathAndQuery);
                if (slash[slash.Length - 1] != '/')
                {
                    slash += "/";
                }

                if (slash.Equals(urlSlash))
                {
                    return WatchResponse.Cycles.Continue;
                }

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

        private void Request(PipeLineStages stage, HttpApplication application)
        {
            if (!EnvironLock.TryEnterReadLock(watchLockTimeout))
            {
                //  Problem Houston
                application.Context.Response.StatusCode = 500;
                application.CompleteRequest();
                return;
            }

            try
            {
                if (!Environs.Any())
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

                string uri = application.Context.Request.Url.AbsoluteUri;
                string uriWithoutDomain = null;

                foreach (var environ in Environs)
                {
                    string filePath = null;
                    if (environ.Value.Domains == null)
                    {
                        if (uriWithoutDomain == null)
                        {
                            uriWithoutDomain = application.Context.Request.Url.LocalPath;
                        }
                        filePath = uriWithoutDomain;
                    }
                    else
                    {
                        var domain = environ.Value.Domains.FirstOrDefault(x => uri.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));
                        if (domain != null)
                        {
                            filePath = uri.Substring(domain.Length - 1);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (environ.Value.WatchLocks[(int) stage].TryEnterReadLock(watchLockTimeout))
                    {
                        try
                        {
                            foreach (var watch in environ.Value.Watchers[(int) stage])
                            {
                                if ((watch.Regex == null || watch.Regex.IsMatch(filePath)))
                                {
                                    switch (ExecuteResponse(watch, watch.Request(count, application), application))
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
                            environ.Value.WatchLocks[(int) stage].ExitReadLock();
                        }
                    }

                    if (!environ.Value.ContinueProcessing)
                    {
                        break;
                    }
                }
            }
            finally
            {
                EnvironLock.ExitReadLock();
            }
        }

        private void Application_BeginRequest(object source, EventArgs e)
        {
            JobService.Instance.Poll();
            Request(PipeLineStages.BeginRequest, (HttpApplication)source);
        }

        private void Application_AuthenticateRequest(Object source, EventArgs e)
        {
            Request(PipeLineStages.AuthenticateRequest, (HttpApplication)source);
        }

        private void Application_ResolveRequestCache(Object source, EventArgs e)
        {
            Request(PipeLineStages.ResolveRequestCache, (HttpApplication)source);
        }

        private void Application_UpdateRequestCache(Object source, EventArgs e)
        {
            Request(PipeLineStages.UpdateRequestCache, (HttpApplication)source);
        }

        private void Application_EndRequest(Object source, EventArgs e)
        {
            Request(PipeLineStages.EndRequest, (HttpApplication)source);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// Create an exception Url, that won't be redirected. Usually a service or error page that you want displayed, regardless or what watches want
        /// </summary>
        /// <param name="job">The job that created this Exception</param>
        /// <param name="regex">The url / url rule that can be shown, try and match with or without trailing slash</param>
        /// <returns>A Unique id for this Exception, or -1 if we failed to create it</returns>
        public static int Exception(IJob job, Regex regex = null, UmbracoUrl url = null)
        {
            if (!UrlExceptionLock.TryEnterWriteLock(watchLockTimeout))
            {
				LogHelper.Error<WebRequestHandler>("Unable to create UrlException, can\'t get write lock", null);
				return -1;
			}
			try
			{
		        var count = Interlocked.Increment(ref requestCount);
				UrlExceptions.Add(new UrlException
				{
					Id = count,
					EnvironmentId = job.Environment.Id,
					AppId = job.App.Id,
					Regex = regex,
					Url = url
				});
				return count;
			}
			finally
			{
				UrlExceptionLock.ExitWriteLock();
			}
		}

		public static int Unexception(IJob job, Regex regex = null) => Unexception(job.Environment.Id, job.App.Id, regex);
		public static int Unexception(IJob job, UmbracoUrl url = null) => Unexception(job.Environment.Id, job.App.Id, null, url);
		public static int Unexception(IJob job) => Unexception(job.Environment.Id, job.App.Id, null);
		public static int Unexception(string appId = null, Regex regex = null) => Unexception(null, appId, regex);

		public static int Unexception(int? environmentId = null, string appId = null, Regex regex = null, UmbracoUrl url = null)
		{
            if (!UrlExceptionLock.TryEnterWriteLock(watchLockTimeout))
            {
				LogHelper.Error<WebRequestHandler>("Unable to create UrlException, can\'t get write lock", null);
				return -1;
			}
			try
			{
	            string regy = regex == null ? null : regex.ToString();

				return UrlExceptions.RemoveAll(x => 
					(environmentId == null || x.EnvironmentId == environmentId) && 
					(appId == null || x.AppId == appId) ||
					(regex == null || x.Regex.ToString() == regy) ||
					(url == null || x.Url.Equals(url)));			
			}
			finally
			{
				UrlExceptionLock.ExitWriteLock();
			}
		}
    }
}
