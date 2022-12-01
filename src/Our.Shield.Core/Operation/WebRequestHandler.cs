using Our.Shield.Core.Enums;
using Our.Shield.Core.Models;
using Our.Shield.Core.Operation.Comparers;
using Our.Shield.Core.Operation.Models;
using Our.Shield.Core.Services;
using Our.Shield.Shared.Extensions;
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
    /// <summary>
    /// Shield's HttpModule to handle requests to check for security
    /// </summary>
    internal class WebRequestHandler : IHttpModule
    {
        private const int RequestRestartLimit = 100;

        private static readonly int PipeLineStagesLength = Enum.GetNames(typeof(PipeLineStage)).Length;

        private static readonly Locker EnvironLock = new Locker();

        private static readonly SortedDictionary<int, Environ> Environs = new SortedDictionary<int, Environ>();

        private static readonly bool[] EnvironHasWatches = new bool[PipeLineStagesLength];

        private static readonly Locker UrlExceptionLock = new Locker();

        private static readonly List<UrlException> UrlExceptions = new List<UrlException>();

        private static readonly Locker UrlIgnoresLock = new Locker();

        private static readonly List<UrlException> UrlIgnores = new List<UrlException>();

        private static int _requestCount;

        /// <summary>
        /// Registers the HttpModule to the Request Pipeline
        /// </summary>
        public static void Register()
        {
            // Register our module
            Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(typeof(WebRequestHandler));
        }

        /// <inheritdoc />
        public void Init(HttpApplication application)
        {
            application.BeginRequest += Application_BeginRequest;
            application.AuthenticateRequest += Application_AuthenticateRequest;
            application.ResolveRequestCache += Application_ResolveRequestCache;
            application.UpdateRequestCache += Application_UpdateRequestCache;
            application.EndRequest += Application_EndRequest;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        /// <summary>
        /// Adds a watch to check for security on a given RegEx match
        /// </summary>
        /// <param name="job"></param>
        /// <param name="stage"></param>
        /// <param name="regex"></param>
        /// <param name="priority"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static int Watch(IJob job, PipeLineStage stage, Regex regex, int priority, Func<int, HttpApplication, WatchResponse> request)
        {
            Environ environ = null;
            if (EnvironLock.Write(() =>
            {
                if (!Environs.TryGetValue(job.Environment.SortOrder, out environ))
                {
                    Environs.Add(job.Environment.SortOrder, environ = new Environ(job.Environment, PipeLineStagesLength));
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
        /// Removes all watches for a given job that match the RegEx
        /// </summary>
        /// <param name="job"></param>
        /// <param name="stage"></param>
        /// <param name="regex"></param>
        /// <returns></returns>
        public static int Unwatch(IJob job, PipeLineStage stage, Regex regex = null)
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
                    return environ.Watchers[(int)stage]
                        .RemoveAll(x => x.AppId.Equals(job.App.Id, StringComparison.InvariantCultureIgnoreCase)
                            && ((regy == null && x.Regex == null) || (regy != null && x.Regex != null
                            && regy.Equals(x.Regex.ToString(), StringComparison.InvariantCulture))));
                });
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static int Unwatch(string appId) => Unwatch(null, appId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environmentKey"></param>
        /// <param name="appId"></param>
        /// <param name="stage"></param>
        /// <param name="regex"></param>
        /// <returns></returns>
        public static int Unwatch(Guid? environmentKey = null, string appId = null, PipeLineStage? stage = null, Regex regex = null)
        {
            var watchRemovedCounter = 0;
            var deleteEnvirons = new List<int>();

            // ReSharper disable once InvertIf
            if (EnvironLock.Read(() =>
            {
                var regy = regex?.ToString();
                foreach (var environ in Environs.Where(x => environmentKey == null || x.Value.Key == environmentKey))
                {
                    var watchRemainCounter = 0;
                    foreach (var objectStage in Enum.GetValues(typeof(PipeLineStage)))
                    {
                        var currentStage = (PipeLineStage)objectStage;
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
                    EnvironmentKey = job.Environment.Key,
                    AppId = job.App.Id,
                    Regex = regex,
                    Url = url
                });
                return count;
            });
        }

        public static int Unexception(IJob job, Regex regex = null) => Unexception(job.Environment.Key, job.App.Id, regex);

        public static int Unexception(IJob job, UmbracoUrl url = null) => Unexception(job.Environment.Key, job.App.Id, null, url);

        public static int Unexception(IJob job) => Unexception(job.Environment.Key, job.App.Id);

        public static int Unexception(string appId = null, Regex regex = null) => Unexception(null, appId, regex);

        public static int Unexception(Guid? environmentKey = null, string appId = null, Regex regex = null, UmbracoUrl url = null)
        {
            return UrlExceptionLock.Write(() =>
            {
                var regy = regex?.ToString();

                return UrlExceptions.RemoveAll(x =>
                    (environmentKey == null || x.EnvironmentKey == environmentKey) && (appId == null || x.AppId == appId) &&
                    ((regex == null || x.Regex.ToString() == regy) || (url == null || x.Url.Equals(url))));
            });
        }

        /// <summary>
        /// Create an exception Url, that won't be redirected. Usually a service or error page that you want displayed, regardless or what watches want
        /// </summary>
        /// <param name="job">The job that created this Exception</param>
        /// <param name="regex">The url / url rule that can be shown, try and match with or without trailing slash</param>
        /// <returns>A Unique id for this Exception, or -1 if we failed to create it</returns>
        public static int Ignore(IJob job, Regex regex = null)
        {
            return UrlIgnoresLock.Write(() =>
            {
                var count = Interlocked.Increment(ref _requestCount);
                UrlIgnores.Add(new UrlException
                {
                    EnvironmentKey = job.Environment.Key,
                    AppId = job.App.Id,
                    Regex = regex
                });
                return count;
            });
        }

        public static int Unignore(IJob job, Regex regex = null) => Unignore(job.Environment.Key, job.App.Id, regex);

        public static int Unignore(IJob job) => Unignore(job.Environment.Key, job.App.Id);

        public static int Unignore(string appId = null, Regex regex = null) => Unignore(null, appId, regex);

        public static int Unignore(Guid? environmentKey = null, string appId = null, Regex regex = null)
        {
            return UrlIgnoresLock.Write(() =>
            {
                var regy = regex?.ToString();

                return UrlIgnores.RemoveAll(x =>
                    (environmentKey == null || x.EnvironmentKey == environmentKey)
                    && (appId == null || x.AppId == appId) &&
                    (regex == null || x.Regex.ToString() == regy));
            });
        }

        private void Application_BeginRequest(object source, EventArgs e)
        {
            Request(PipeLineStage.BeginRequest, (HttpApplication)source);
        }

        private void Application_AuthenticateRequest(object source, EventArgs e)
        {
            Request(PipeLineStage.AuthenticateRequest, (HttpApplication)source);
        }

        private void Application_ResolveRequestCache(object source, EventArgs e)
        {
            Request(PipeLineStage.ResolveRequestCache, (HttpApplication)source);
        }

        private void Application_UpdateRequestCache(object source, EventArgs e)
        {
            Request(PipeLineStage.UpdateRequestCache, (HttpApplication)source);
        }

        private void Application_EndRequest(object source, EventArgs e)
        {
            Request(PipeLineStage.EndRequest, (HttpApplication)source);
        }

        private void Request(PipeLineStage stage, HttpApplication application)
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
                if (ProcessRequest(PipeLineStage.BeginRequest, count, application) && stage == PipeLineStage.BeginRequest)
                {
                    return;
                }

                if (stage != PipeLineStage.BeginRequest)
                {
                    if (ProcessRequest(PipeLineStage.AuthenticateRequest, count, application) && stage == PipeLineStage.AuthenticateRequest)
                    {
                        return;
                    }

                    if (stage != PipeLineStage.AuthenticateRequest)
                    {
                        if (ProcessRequest(PipeLineStage.ResolveRequestCache, count, application) && stage == PipeLineStage.ResolveRequestCache)
                        {
                            return;
                        }

                        if (stage != PipeLineStage.ResolveRequestCache)
                        {
                            if (ProcessRequest(PipeLineStage.UpdateRequestCache, count, application) && stage == PipeLineStage.UpdateRequestCache)
                            {
                                return;
                            }

                            if (stage != PipeLineStage.UpdateRequestCache)
                            {
                                if (ProcessRequest(PipeLineStage.EndRequest, count, application) && stage == PipeLineStage.EndRequest)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            //  To many redirects
            application.Context.Response.StatusCode = 500;
            application.CompleteRequest();

            return;
        }

        private bool ProcessRequest(PipeLineStage stage, int count, HttpApplication application)
        {
            if (EnvironHasWatches[(int)stage] == false)
            {
                return true;
            }

            return EnvironLock.Read(() =>
            {
                if (Environs.None())
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
                            return UrlIgnores.Where(x => x.EnvironmentKey == environ.Value.Key && x.Regex.IsMatch(filePath)).Select(x => x.AppId);
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
                                watchResponse.Cycle = ExecuteTransfer(environ.Value.Key, watchResponse, application);
                            }

                            switch (watchResponse.Cycle)
                            {
                                case Cycle.Kill:
#if TRACE
                                    Debug.WriteLine(debug + "Kill");
#endif
                                    application.Response.End();
                                    //application.CompleteRequest();
                                    return true;

                                case Cycle.Stop:
#if TRACE
                                    Debug.WriteLine(debug + "Stop");
#endif
                                    return true;

                                case Cycle.Restart:
#if TRACE
                                    Debug.WriteLine(debug + "Restart");
#endif
                                    return false;

                                case Cycle.Error:
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
            return UrlExceptionLock
                .Write<bool?>(() =>
                {
                    foreach (var exception in UrlExceptions.Where(x => x.CalculatedUrl == false && x.Url != null))
                    {
                        if (!UrlProcess(exception))
                        {
                            return false;
                        }
                    }
                    return true;
                })
                .Value;
        }

        private bool IgnoresProcess()
        {
            return UrlIgnoresLock
                .Write<bool?>(() =>
                {
                    foreach (var ignore in UrlIgnores.Where(x => x.CalculatedUrl == false && x.Url != null))
                    {
                        if (!UrlProcess(ignore))
                        {
                            return false;
                        }
                    }
                    return true;
                })
                .Value;
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
                        //  if connection or keep-alive. ignore
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

            HttpWebResponse result;
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

        private Cycle ExecuteTransfer(Guid environmentKey, WatchResponse response, HttpApplication application)
        {
            if (response.Transfer.TransferType == TransferType.PlayDead)
            {
                application.Context.Response.Close();
                return Cycle.Stop;
            }

            if (!ExceptionsProcess() || !IgnoresProcess())
            {
                return Cycle.Error;
            }

            if (UrlExceptionLock.Read(() =>
            {
                var requestUrl = application.Context.Request.Url;

                foreach (var exception in UrlExceptions.Where(x => x.EnvironmentKey == environmentKey))
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
                return Cycle.Continue;
            }

            var urlService = new UmbracoUrlService();
            var url = urlService.Url(response.Transfer.Url);

            switch (response.Transfer.TransferType)
            {
                case TransferType.Redirect:
                    application.Context.Response.Redirect(url, true);

                    return Cycle.Stop;

                case TransferType.TransferRequest:
                    application.Server.TransferRequest(url, true);

                    return Cycle.Stop;

                case TransferType.TransmitFile:
                    // Request is for a CSS etc. file, transmit the file and set correct mime type
                    var mimeType = MimeMapping.GetMimeMapping(url);

                    application.Response.ContentType = mimeType;
                    application.Response.TransmitFile(application.Server.MapPath(url));
                    return Cycle.Kill;

                case TransferType.Rewrite:
                    if (urlService.IsUmbracoUrl(response.Transfer.Url))
                    {
                        RewritePage(application, url);
                        return Cycle.Kill;
                    }

                    application.Context.RewritePath(url);

                    return Cycle.Restart;
            }

            return Cycle.Error;
        }
    }
}
