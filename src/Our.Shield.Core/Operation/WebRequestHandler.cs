using Our.Shield.Core.Models;
using Our.Shield.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

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

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    internal class WebRequestHandler : IHttpModule
    {
        private const int RequestRestartLimit = 100;

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
            public readonly int Id;
            public readonly string Name;
            public readonly int SortOrder;
            public readonly bool ContinueProcessing;
            public readonly List<string> Domains;
            public readonly Locker[] WatchLocks;
            public readonly List<Watcher>[] Watchers;

            public Environ(IEnvironment environment)
            {
                Id = environment.Id;
                Name = string.Copy(environment.Name);
                SortOrder = environment.SortOrder;
                ContinueProcessing = environment.ContinueProcessing;
                Domains = Domains(environment.Domains);
                WatchLocks = new Locker[PipeLineStagesLength];
                Watchers = new List<Watcher>[PipeLineStagesLength];
                for (var index = 0; index != PipeLineStagesLength; index++)
                {
                    WatchLocks[index] = new Locker();
                    Watchers[index] = new List<Watcher>();
                }
            }
        }

        private class Watcher
        {
            public int Priority;
            public string AppId;
            public Regex Regex;
            public Func<int, HttpApplication, WatchResponse> Request;
        }

        private class UrlException
        {
            public int EnvironmentId;
            public string AppId;
            public Regex Regex;
            public UmbracoUrl Url;
            public bool CalculatedUrl;
            public string CalculatedUrlWithoutSlash;
            public string CalculatedUrlWithSlash;
        }

        private static readonly Locker EnvironLock = new Locker();
        private static readonly SortedDictionary<int, Environ> Environs = new SortedDictionary<int, Environ>();
        private static readonly bool[] EnvironHasWatches = new bool[PipeLineStagesLength];

        private static readonly Locker UrlExceptionLock = new Locker();
        private static readonly List<UrlException> UrlExceptions = new List<UrlException>();
        private static readonly Locker UrlIgnoresLock = new Locker();
        private static readonly List<UrlException> UrlIgnores = new List<UrlException>();

        private static int _requestCount;

        private class WatchComparer : IComparer<Watcher>
        {
            public int Compare(Watcher a, Watcher b)
            {
                // ReSharper disable once PossibleNullReferenceException
                return a.Priority - b.Priority;
            }
        }

        private static List<string> Domains(IEnumerable<IDomain> domains)
        {
            var domainsArrary = domains.ToArray();
            if (!domainsArrary.Any())
            {
                return null;
            }

            var results = new List<string>();

            foreach (var domain in domainsArrary)
            {
                UriBuilder urlwithPort, urlWithoutPort;
                try
                {
                    urlwithPort = urlWithoutPort = new UriBuilder(domain.Name);

                }
                catch (Exception ex)
                {
                    Serilog.Log.Error(ex, "{DomainName} is not a valid domain", domain.Name);
                    continue;
                }

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (urlwithPort.Scheme == null)
                // ReSharper disable once HeuristicUnreachableCode
                {
                    // ReSharper disable once HeuristicUnreachableCode
                    urlwithPort.Scheme = Uri.UriSchemeHttp;
                    UriBuilder urlHttpsWithoutPort;
                    var urlHttpsWithPort = urlHttpsWithoutPort = new UriBuilder(domain.Name)
                    {
                        Scheme = Uri.UriSchemeHttps
                    };

                    results.Add(urlHttpsWithoutPort.ToString().Replace($":{urlHttpsWithPort.Port}", string.Empty));
                    results.Add(urlHttpsWithPort.ToString());
                }
                results.Add(urlWithoutPort.ToString().Replace($":{urlwithPort.Port}", string.Empty));
                results.Add(urlwithPort.ToString());
            }
            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="stage"></param>
        /// <param name="regex"></param>
        /// <param name="priority"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static int Watch(IJob job, PipeLineStages stage, Regex regex, int priority, Func<int, HttpApplication, WatchResponse> request)
        {
            Environ environ = null;
            if (EnvironLock.Write(() =>
            {
                if (!Environs.TryGetValue(job.Environment.SortOrder, out environ))
                {
                    Environs.Add(job.Environment.SortOrder, environ = new Environ(job.Environment));
                }
            }))
            {
                return EnvironLock.Read(() =>
                {
                    return environ.WatchLocks[(int)stage].Write(() =>
                   {
                       var watchList = environ.Watchers[(int)stage];
                       var count = Interlocked.Increment(ref _requestCount);
                       EnvironHasWatches[(int)stage] = true;
                       watchList.Add(new Watcher
                       {
                           Priority = priority,
                           AppId = job.App.Id,
                           Regex = regex,
                           Request = request
                       });
                       watchList.Sort(new WatchComparer());
                       return count;
                   });
                });
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="stage"></param>
        /// <param name="regex"></param>
        /// <returns></returns>
        public static int Unwatch(IJob job, PipeLineStages stage, Regex regex = null)
        {
            return EnvironLock.Read(() =>
            {
                if (!Environs.TryGetValue(job.Environment.SortOrder, out var environ))
                {
                    return 0;
                }

                return environ.WatchLocks[(int)stage].Write(() =>
               {
                   var regy = regex?.ToString();
                   return environ.Watchers[(int)stage].RemoveAll(x =>
                       x.AppId.Equals(job.App.Id, StringComparison.InvariantCultureIgnoreCase) &&
                       (regy == null && x.Regex == null ||
                       regy != null && x.Regex != null && regy.Equals(x.Regex.ToString(), StringComparison.InvariantCulture)));
               });
            });
        }

        public static int Unwatch(string appId) => Unwatch(null, appId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environmentId"></param>
        /// <param name="appId"></param>
        /// <param name="stage"></param>
        /// <param name="regex"></param>
        /// <returns></returns>
        public static int Unwatch(int? environmentId = null, string appId = null, PipeLineStages? stage = null, Regex regex = null)
        {
            var watchRemovedCounter = 0;
            var deleteEnvirons = new List<int>();

            // ReSharper disable once InvertIf
            if (EnvironLock.Read(() =>
            {
                var regy = regex?.ToString();
                foreach (var environ in Environs.Where(x => environmentId == null || x.Value.Id == environmentId))
                {
                    var watchRemainCounter = 0;
                    foreach (var objectStage in Enum.GetValues(typeof(PipeLineStages)))
                    {
                        var currentStage = (PipeLineStages)objectStage;
                        if (stage != null && currentStage != stage)
                        {
                            continue;
                        }

                        watchRemainCounter += environ.Value.WatchLocks[(int)currentStage].Write(() =>
                       {
                           watchRemovedCounter += environ.Value.Watchers[(int)currentStage].RemoveAll(x =>
                               (appId == null ||
                               x.AppId.Equals(appId, StringComparison.InvariantCultureIgnoreCase)) &&
                               (regy == null ||
                               (regy != null && x.Regex != null && regy.Equals(x.Regex.ToString(), StringComparison.InvariantCulture))));
                           return environ.Value.Watchers[(int)currentStage].Count;
                       });
                    }

                    if (watchRemainCounter == 0)
                    {
                        deleteEnvirons.Add(environ.Value.SortOrder);
                    }
                }
            }))
            {
                if (!deleteEnvirons.Any())
                {
                    return watchRemovedCounter;
                }

                // ReSharper disable once ImplicitlyCapturedClosure
                return EnvironLock.Write(() =>
                {
                    foreach (var sortOrder in deleteEnvirons)
                    {
                        Environs.Remove(sortOrder);
                    }
                    return watchRemovedCounter;
                });
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="application"></param>
        public void Init(HttpApplication application)
        {
            application.BeginRequest += Application_BeginRequest;
            application.AuthenticateRequest += Application_AuthenticateRequest;
            application.ResolveRequestCache += Application_ResolveRequestCache;
            application.UpdateRequestCache += Application_UpdateRequestCache;
            application.EndRequest += Application_EndRequest;
        }

        private bool UrlProcess(UrlException exception)
        {
            var url = new UmbracoUrlService().Url(exception.Url);
            if (url == null)
            {
                return false;
            }
            exception.CalculatedUrl = true;
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }
            var query = url.IndexOf('?');
            if (query == 0)
            {
                exception.CalculatedUrlWithoutSlash = url;
                exception.CalculatedUrlWithSlash = "/" + url;
                return true;
            }
            var point = (query == -1 ? url.Length : query) - 1;
            var builder = new StringBuilder(url.Length);
            if (url[point] == '/')
            {
                exception.CalculatedUrlWithSlash = url;

                for (var i = 0; i != url.Length; i++)
                {
                    if (i != point)
                    {
                        builder.Append(url[i]);
                    }
                }
                exception.CalculatedUrlWithoutSlash = builder.ToString();
                return true;
            }
            exception.CalculatedUrlWithoutSlash = url;
            for (var i = 0; i != url.Length; i++)
            {
                builder.Append(url[i]);
                if (i == point)
                {
                    builder.Append('/');
                }
            }
            exception.CalculatedUrlWithSlash = builder.ToString();
            return true;
        }

        private bool ExceptionsProcess()
        {
            return UrlExceptionLock.Write<bool?>(() =>
            {
                foreach (var exception in UrlExceptions.Where(x => x.CalculatedUrl == false && x.Url != null))
                {
                    if (!UrlProcess(exception))
                    {
                        return false;
                    }
                }
                return true;
            }).Value;
        }

        private bool IgnoresProcess()
        {
            return UrlIgnoresLock.Write<bool?>(() =>
            {
                foreach (var ignore in UrlIgnores.Where(x => x.CalculatedUrl == false && x.Url != null))
                {
                    if (!UrlProcess(ignore))
                    {
                        return false;
                    }
                }
                return true;
            }).Value;
        }

        private void RewritePage(HttpApplication application, string url)
        {
            var client = WebRequest.Create(application.Request.Url.GetLeftPart(UriPartial.Authority) + url) as HttpWebRequest;
            client.Method = application.Request.HttpMethod;

            //  Add Headers from current request to web request
            foreach (var key in application.Request.Headers.AllKeys)
            {
                var value = application.Request.Headers[key];
                switch (key.ToLower())
                {
                    case "connection":
                        if (value.Equals("keep-alive", StringComparison.OrdinalIgnoreCase))
                        {
                            client.KeepAlive = true;
                        }
                        else
                        {
                            client.Connection = value;
                        }
                        break;

                    case "accept":
                        client.Accept = value;
                        break;

                    case "user-agent":
                        client.UserAgent = value;
                        break;

                    case "host":
                        continue;

                    case "content-type":
                        client.ContentType = value;
                        break;

                    case "referer":
                        client.Referer = value;
                        break;

                    default:
                        client.Headers.Set(key, value);
                        break;
                }
            }

            HttpWebResponse result = null;
            try
            {
                result = client.GetResponse() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                result = ex.Response as HttpWebResponse;
            }

            //  clear current content and headers
            application.Response.ClearContent();
            application.Response.ClearHeaders();

            foreach (var key in result.Headers.AllKeys)
            {
                var value = result.Headers[key];
                switch (key.ToLower())
                {
                    case "content-type":
                        application.Response.ContentType = value;
                        break;

                    default:
                        application.Response.AppendHeader(key, value);
                        break;
                }
            }

            result.GetResponseStream().CopyTo(application.Response.OutputStream);
            application.Response.StatusCode = (int)result.StatusCode;
            application.Response.Flush();
            application.Response.Close();
        }

        // ReSharper disable once UnusedParameter.Local
        private WatchResponse.Cycles ExecuteTransfer(int environmentId, Watcher watch, WatchResponse response, HttpApplication application)
        {
            if (response.Transfer.TransferType == TransferTypes.PlayDead)
            {
                application.Context.Response.Close();
                return WatchResponse.Cycles.Stop;
            }

            if (!ExceptionsProcess())
            {
                return WatchResponse.Cycles.Error;
            }

            if (UrlExceptionLock.Read(() =>
            {
                var requestUrl = application.Context.Request.Url;

                foreach (var exception in UrlExceptions.Where(x => x.EnvironmentId == environmentId))
                {
                    if (exception.Regex != null && (exception.Regex.IsMatch(requestUrl.PathAndQuery) ||
                        exception.Regex.IsMatch(requestUrl.AbsoluteUri)))
                    {
                        return true;
                    }

                    if (exception.CalculatedUrl &&
                        (exception.CalculatedUrlWithoutSlash.Equals(requestUrl.PathAndQuery, StringComparison.InvariantCultureIgnoreCase) ||
                        exception.CalculatedUrlWithSlash.Equals(requestUrl.PathAndQuery, StringComparison.InvariantCultureIgnoreCase) ||
                        exception.CalculatedUrlWithoutSlash.Equals(requestUrl.AbsoluteUri, StringComparison.InvariantCultureIgnoreCase) ||
                        exception.CalculatedUrlWithSlash.Equals(requestUrl.AbsoluteUri, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        return true;
                    }
                }
                return false;
            }))
            {
                return WatchResponse.Cycles.Continue;
            }

            var urlService = new UmbracoUrlService();
            var url = urlService.Url(response.Transfer.Url);

            switch (response.Transfer.TransferType)
            {
                case TransferTypes.Redirect:
                    application.Context.Response.Redirect(url, true);
                    return WatchResponse.Cycles.Stop;

                case TransferTypes.TransferRequest:
                    application.Server.TransferRequest(url, true);
                    return WatchResponse.Cycles.Stop;

                case TransferTypes.TransmitFile:
                    // Request is for a css etc. file, transmit the file and set correct mime type
                    var mimeType = MimeMapping.GetMimeMapping(url);

                    application.Response.ContentType = mimeType;
                    application.Response.TransmitFile(application.Server.MapPath(url));
                    return WatchResponse.Cycles.Kill;

                case TransferTypes.Rewrite:
                    if (urlService.IsUmbracoUrl(response.Transfer.Url))
                    {
                        RewritePage(application, url);
                        return WatchResponse.Cycles.Kill;
                    }
                    application.Context.RewritePath(url);
                    return WatchResponse.Cycles.Restart;
            }

            return WatchResponse.Cycles.Error;
        }

        private bool ProcessRequest(PipeLineStages stage, int count, HttpApplication application)
        {
            if (EnvironHasWatches[(int)stage] == false)
            {
                return true;
            }

            return EnvironLock.Read(() =>
            {
                if (!Environs.Any())
                {
                    return true;
                }

#if TRACE
                Debug.WriteLine(stage.ToString() + " : " + application.Request.Url.AbsoluteUri);
#endif

                var uri = application.Context.Request.Url.AbsoluteUri;
                string uriWithoutDomain = null;

                foreach (var environ in Environs)
                {
                    string filePath;
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

                    if (environ.Value.WatchLocks[(int)stage].Read<bool?>(() =>
                    {
                        var ignores = UrlIgnoresLock.Read(() =>
                        {
                            return UrlIgnores.Where(x => x.EnvironmentId == environ.Value.Id && x.Regex.IsMatch(filePath)).Select(x => x.AppId);
                        });

                        foreach (var watch in environ.Value.Watchers[(int)stage])
                        {
                            if ((watch.Regex != null && !watch.Regex.IsMatch(filePath)) || ignores.Any(x => x != watch.AppId))
                            {
                                continue;
                            }

#if TRACE
                            var debug = $"{uri}: Watcher({environ.Value.Name}, {watch.AppId}, {watch.Priority}, {watch.Regex}) ";
#endif
                            var watchResponse = watch.Request(count, application);

                            if (watchResponse.Transfer != null)
                            {
#if TRACE
                                debug += "by transfer then ";
#endif
                                watchResponse.Cycle = ExecuteTransfer(environ.Value.Id, watch, watchResponse, application);
                            }

                            switch (watchResponse.Cycle)
                            {
                                case WatchResponse.Cycles.Kill:
#if TRACE
                                    Debug.WriteLine(debug + "Kill");
#endif
                                    application.Response.End();
                                    //application.CompleteRequest();
                                    return true;

                                case WatchResponse.Cycles.Stop:
#if TRACE
                                    Debug.WriteLine(debug + "Stop");
#endif
                                    return true;

                                case WatchResponse.Cycles.Restart:
#if TRACE
                                    Debug.WriteLine(debug + "Restart");
#endif
                                    return false;

                                case WatchResponse.Cycles.Error:
#if TRACE
                                    Debug.WriteLine(debug + "Error");
#endif
                                    application.Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                    application.CompleteRequest();
                                    break;

#if TRACE
                                default:
                                    Debug.WriteLine(debug + "Continue");
                                    break;
#endif
                                    //  If WatchCycle.Continue we do nothing
                            }
                        }
                        return true;
                    }) == false)
                    {
                        return false;
                    }

                    if (!environ.Value.ContinueProcessing)
                    {
                        break;
                    }
                }
                return true;
            });
        }

        void Request(PipeLineStages stage, HttpApplication application)
        {
            if (application.Context.Request.Url.AbsolutePath == "/umbraco/ping.aspx" || application.Context.Request.Url.AbsolutePath == "/umbraco/backoffice/UmbracoApi/Authentication/GetRemainingTimeoutSeconds")
            {
                return;
            }

            int count = 0;

            if (ProcessRequest(stage, count, application))
            {
                return;
            }

            while (++count != RequestRestartLimit)
            {
                if (ProcessRequest(PipeLineStages.BeginRequest, count, application) && stage == PipeLineStages.BeginRequest)
                {
                    return;
                }

                if (stage != PipeLineStages.BeginRequest)
                {
                    if (ProcessRequest(PipeLineStages.AuthenticateRequest, count, application) && stage == PipeLineStages.AuthenticateRequest)
                    {
                        return;
                    }

                    if (stage != PipeLineStages.AuthenticateRequest)
                    {
                        if (ProcessRequest(PipeLineStages.ResolveRequestCache, count, application) && stage == PipeLineStages.ResolveRequestCache)
                        {
                            return;
                        }

                        if (stage != PipeLineStages.ResolveRequestCache)
                        {
                            if (ProcessRequest(PipeLineStages.UpdateRequestCache, count, application) && stage == PipeLineStages.UpdateRequestCache)
                            {
                                return;
                            }

                            if (stage != PipeLineStages.UpdateRequestCache)
                            {
                                if (ProcessRequest(PipeLineStages.EndRequest, count, application) && stage == PipeLineStages.EndRequest)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            //	To many redirects
            application.Context.Response.StatusCode = 500;
            application.CompleteRequest();
            return;
        }

        private void Application_BeginRequest(object source, EventArgs e)
        {
            Request(PipeLineStages.BeginRequest, (HttpApplication)source);
        }

        private void Application_AuthenticateRequest(object source, EventArgs e)
        {
            Request(PipeLineStages.AuthenticateRequest, (HttpApplication)source);
        }

        private void Application_ResolveRequestCache(object source, EventArgs e)
        {
            Request(PipeLineStages.ResolveRequestCache, (HttpApplication)source);
        }

        private void Application_UpdateRequestCache(object source, EventArgs e)
        {
            Request(PipeLineStages.UpdateRequestCache, (HttpApplication)source);
        }

        private void Application_EndRequest(object source, EventArgs e)
        {
            Request(PipeLineStages.EndRequest, (HttpApplication)source);
        }

        /// <inheritdoc />
        public void Dispose() { }

        /// <summary>
        /// Create an exception Url, that won't be redirected. Usually a service or error page that you want displayed, regardless or what watches want
        /// </summary>
        /// <param name="job">The job that created this Exception</param>
        /// <param name="regex">The url / url rule that can be shown, try and match with or without trailing slash</param>
        /// <param name="url"></param>
        /// <returns>A Unique id for this Exception, or -1 if we failed to create it</returns>
        public static int Exception(IJob job, Regex regex = null, UmbracoUrl url = null)
        {
            return UrlExceptionLock.Write(() =>
            {
                var count = Interlocked.Increment(ref _requestCount);
                UrlExceptions.Add(new UrlException
                {
                    EnvironmentId = job.Environment.Id,
                    AppId = job.App.Id,
                    Regex = regex,
                    Url = url
                });
                return count;
            });
        }

        // ReSharper disable once MethodOverloadWithOptionalParameter
        public static int Unexception(IJob job, Regex regex = null) => Unexception(job.Environment.Id, job.App.Id, regex);
        // ReSharper disable once MethodOverloadWithOptionalParameter
        public static int Unexception(IJob job, UmbracoUrl url = null) => Unexception(job.Environment.Id, job.App.Id, null, url);
        public static int Unexception(IJob job) => Unexception(job.Environment.Id, job.App.Id);
        public static int Unexception(string appId = null, Regex regex = null) => Unexception(null, appId, regex);

        public static int Unexception(int? environmentId = null, string appId = null, Regex regex = null, UmbracoUrl url = null)
        {
            return UrlExceptionLock.Write(() =>
            {
                var regy = regex?.ToString();

                return UrlExceptions.RemoveAll(x =>
                    (environmentId == null || x.EnvironmentId == environmentId) && (appId == null || x.AppId == appId) &&
                    ((regex == null || x.Regex.ToString() == regy) || (url == null || x.Url.Equals(url))));
            });
        }

        /// <summary>
        /// Create an exception Url, that won't be redirected. Usually a service or error page that you want displayed, regardless or what watches want
        /// </summary>
        /// <param name="job">The job that created this Exception</param>
        /// <param name="regex">The url / url rule that can be shown, try and match with or without trailing slash</param>
        /// <param name="url"></param>
        /// <returns>A Unique id for this Exception, or -1 if we failed to create it</returns>
        public static int Ignore(IJob job, Regex regex = null)
        {
            return UrlIgnoresLock.Write(() =>
            {
                var count = Interlocked.Increment(ref _requestCount);
                UrlIgnores.Add(new UrlException
                {
                    EnvironmentId = job.Environment.Id,
                    AppId = job.App.Id,
                    Regex = regex
                });
                return count;
            });
        }

        // ReSharper disable once MethodOverloadWithOptionalParameter
        public static int Unignore(IJob job, Regex regex = null) => Unignore(job.Environment.Id, job.App.Id, regex);
        // ReSharper disable once MethodOverloadWithOptionalParameter
        public static int Unignore(IJob job) => Unignore(job.Environment.Id, job.App.Id);
        public static int Unignore(string appId = null, Regex regex = null) => Unignore(null, appId, regex);

        public static int Unignore(int? environmentId = null, string appId = null, Regex regex = null)
        {
            return UrlIgnoresLock.Write(() =>
            {
                var regy = regex?.ToString();

                return UrlIgnores.RemoveAll(x =>
                    (environmentId == null || x.EnvironmentId == environmentId)
                    && (appId == null || x.AppId == appId) &&
                    (regex == null || x.Regex.ToString() == regy));
            });
        }

    }
}
