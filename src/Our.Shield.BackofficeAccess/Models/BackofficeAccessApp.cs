namespace Our.Shield.BackofficeAccess.Models
{
    using Core.Models;
    using Core.UI;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using Umbraco.Core;
    using Umbraco.Web;

    /// <summary>
    /// 
    /// </summary>
    [AppEditor("/App_Plugins/Shield.BackofficeAccess/Views/BackofficeAccess.html?version=1.0.0-pre-alpha")]
    public class BackofficeAccessApp : App<BackofficeAccessConfiguration>
    {
        /// <summary>
        /// 
        /// </summary>
        public override string Id => nameof(BackofficeAccess);

        /// <summary>
        /// 
        /// </summary>
        public override string Name => "Backoffice Access";

        /// <summary>
        /// 
        /// </summary>
        public override string Description => "Change the backoffice access URL and/or secure your backoffice URL via IP restrictions";

        /// <summary>
        /// 
        /// </summary>
        public override string Icon => "icon-stop-hand red";

        /// <summary>
        /// 
        /// </summary>
        public override IConfiguration DefaultConfiguration
        {
            get
            {
                return new BackofficeAccessConfiguration
                {
                    BackendAccessUrl = "/umbraco/",
                    IpAddresses = new IpAddress[0],
                    RedirectRewrite = Enums.RedirectRewrite.Redirect,
                    UnauthorisedUrlType = Enums.UnautorisedUrlType.Url
                };
            }
        }

        private int firstExecute = 0;

        private readonly string allowKey = Guid.NewGuid().ToString();
        private readonly TimeSpan CacheLength = new TimeSpan(TimeSpan.TicksPerDay);        //   Once per day

        private string UnauthorisedUrl(IJob job, BackofficeAccessConfiguration config)
        {
            return ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(allowKey, () => {
                string url = null;
                var journalMessage = new JournalMessage();
                var umbContext = UmbracoContext.Current;

                switch (config.UnauthorisedUrlType)
                {
                    case Enums.UnautorisedUrlType.Url:
                        if (!string.IsNullOrEmpty(config.UnauthorisedUrl))
                        {
                            url = config.UnauthorisedUrl;
                            break;
                        }

                        journalMessage.Message = "Error: No Unauthorized URL set in configuration";
                        break;

                    case Enums.UnautorisedUrlType.XPath:
                        var xpathNode = umbContext.ContentCache.GetSingleByXPath(config.UnauthorisedUrlXPath);

                        if(xpathNode != null)
                        {
                            url = xpathNode.Url;
                            break;
                        }

                        journalMessage.Message = "Error: Unable to get the unauthorized URL from the specified XPath expression.";
                        break;

                    case Enums.UnautorisedUrlType.ContentPicker:
                        int id;

                        if(int.TryParse(config.UnauthorisedUrlContentPicker, out id))
                        {
                            var contentPickerNode = umbContext.ContentCache.GetById(id);

                            if (contentPickerNode != null)
                            {
                                url = contentPickerNode.Url;
                                break;
                            }

                            journalMessage.Message = "Error: Unable to get the unauthorized URL from the selected unauthorized URL content picker. Please ensure the selected page is published and not deleted.";
                            break;
                        }

                        journalMessage.Message = "Error: Unable to parse the selected unauthorized URL content picker id to integer. Please ensure a valid content node is selected.";
                        break;

                    default:
                        journalMessage.Message = "Error: Unable to determine which method to use to get the unauthorized URL. Please ensure URL, XPath or Content Picker is selected.";
                        break;
                }

                if (url == null)
                {
                    if(!string.IsNullOrEmpty(journalMessage.Message))
                    {
                        job.WriteJournal(journalMessage);
                    }

                    return null;
                }

                return url;
            }, CacheLength) as string;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public override bool Execute(IJob job, IConfiguration c)
        {
            var config = c as BackofficeAccessConfiguration;

            //Check if we're the first time being ran after an app pool restart.
            if (Interlocked.CompareExchange(ref firstExecute, 1, 0) == 0)
            {
                //TODO: Uncomment out code when method is more 'stable'

                //if (ExecuteFirstTime(job, config))
                //{
                //    //Hard save occurred, the app pool has been
                //    //restart. so no need to continue executing.
                //    return true;
                //}
            }

            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(allowKey);
            job.UnwatchWebRequests();

            //Setup varaibles that are required regardless on app is enabled or disabled
            var currentAppBackendUrl = Umbraco.Core.IO.IOHelper.ResolveUrl(config.BackendAccessUrl.EnsureEndsWith('/'));
            var umbracoPathRegex = new Regex("^((" + ApplicationSettings.UmbracoPath.TrimEnd('/') + "/?)|(" + ApplicationSettings.UmbracoPath + ".*\\.([A-Za-z0-9]){2,5}))$", RegexOptions.IgnoreCase);
            

            //Check if we've been disabled, if so,
            //we might need to add a watch
            if (!config.Enable)
            {
                var defaultConfig = ((BackofficeAccessConfiguration)DefaultConfiguration);

                //Check if the Backend Access Url is equal to the
                //DefaultConfiguration.BackendAccessUrl (/umbraco/).
                if (!ApplicationSettings.UmbracoPath.Equals(defaultConfig.BackendAccessUrl))
                {
                    //We're different, so need to add a watch to rewrite any traffic to the on-disk UmbracoPath location
                    //Otherwise, backoffice won't be accessible from the Default Url (/umbraco/)
                    job.WatchWebRequests(new Regex("^((" + defaultConfig.BackendAccessUrl.TrimEnd('/') + "/?)|(" + defaultConfig.BackendAccessUrl + ".*\\.([A-Za-z0-9]){2,5}))$", RegexOptions.IgnoreCase), 10, (count, httpApp) =>
                    {
                        var rewritePath = (httpApp.Request.Url.AbsolutePath.Length > defaultConfig.BackendAccessUrl.Length) ?
                            ApplicationSettings.UmbracoPath +
                            httpApp.Request.Url.AbsolutePath.Substring(ApplicationSettings.UmbracoPath.Length) :
                            ApplicationSettings.UmbracoPath;

                        //Request is for a physical file, if it's
                        //a usercontrol etc, we need to TransferRequest
                        //otherwise, UmbracoModule will try and return
                        //a content node for the request, instead of the
                        //desired action (i.e. /umbraco/dialogs/republish.aspx)
                        if (!string.IsNullOrEmpty(httpApp.Context.Request.CurrentExecutionFilePathExtension)
                            && (httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".aspx")
                            || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".ascx")
                            || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".asmx")
                            || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".ashx")))
                        {
                            httpApp.Context.Server.TransferRequest(rewritePath, true);
                            return WatchCycle.Continue;
                        }

                        //request is not for a physical file, rewrite
                        //the request and restart the watch cycle
                        httpApp.Context.Items.Add(allowKey, true);
                        httpApp.Context.RewritePath(rewritePath);
                        return WatchCycle.Restart;
                    });

                    var defaultBackendUrlRegex = new Regex("^((" + defaultConfig.BackendAccessUrl.TrimEnd('/') + "/?)|(" + defaultConfig.BackendAccessUrl + ".*(\\.([A-Za-z0-9]){2,5})?))$", RegexOptions.IgnoreCase);

                    //We also need to add a watch to the on-disk UmbracoPath
                    //location so we can disallow any traffic from accessing it
                    //unless the request is being rewritten by us
                    job.WatchWebRequests(umbracoPathRegex, 20, (count, httpApp) =>
                    {
                        // Check if request has our access token, if so, we're
                        //rewriting the user to the on-disk UmbracoPath location,
                        //so let the request continue
                        if ((bool?)httpApp.Context.Items[allowKey] == true)
                        {
                            return WatchCycle.Continue;
                        }

                        //Check if requesting a physical file, as the user may have
                        //logged into umbraco on the custom configured url and is
                        //now loading the assets (i.e. *.css, *.js) or is running
                        //some action (i.e. /umbraco/dialogs/republish.aspx) which
                        //wouldn't have our access token!
                        if (!string.IsNullOrEmpty(httpApp.Context.Request.CurrentExecutionFilePathExtension))
                        {
                            return WatchCycle.Continue;
                        }

                        //If the request has a referrer as the configured backoffice access url,
                        //it's most likely a user clicking a link within the backoffice, which
                        //is pointing to the on-disk UmbracoPath location, so we need to redirect them
                        //to /umbraco/
                        //TODO: figure out a better way of detcting this which is more 'secure' than checking the referrer
                        if (httpApp.Context.Request.UrlReferrer != null && defaultBackendUrlRegex.IsMatch(httpApp.Context.Request.UrlReferrer.AbsolutePath))
                        {
                            var rewritePath = httpApp.Request.Url.AbsolutePath.Length > ApplicationSettings.UmbracoPath.Length
                                ? currentAppBackendUrl + httpApp.Request.Url.AbsolutePath.Substring(ApplicationSettings.UmbracoPath.Length)
                                : currentAppBackendUrl;

                            httpApp.Context.Response.Redirect(rewritePath, true);
                            return WatchCycle.Stop;
                        }

                        //as the request is not being rewritten by us
                        //we need to set the status code to 404 and let
                        //default functionality happen
                        httpApp.Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        return WatchCycle.Stop;
                    });
                }

                //We're the same, no further
                //action is required.
                return true;
            }
            
            var ipv6s = new List<IPAddress>();

            //Convert our IP address(es) to the System.Net.IPAddress
            //Class, so we're working with something more standard
            foreach (var ip in config.IpAddresses)
            {
                if (ip.ipAddress.Equals("127.0.0.1"))
                    ip.ipAddress = "::1";

                IPAddress tempIp;
                if (!IPAddress.TryParse(ip.ipAddress, out tempIp))
                {
                    job.WriteJournal(new JournalMessage($"Error: Unable to cast {ip.ipAddress} to the standard System.Net.IPAddress class; Therefore this is not a valid IP address. This IP address will not be added to the white-listed IP Address list"));
                    continue;
                }

                ipv6s.Add(tempIp.MapToIPv6());
            }
            
            if (currentAppBackendUrl != ApplicationSettings.UmbracoPath)
            {
                var currentBackendUrlRegex = new Regex("^((" + currentAppBackendUrl.TrimEnd('/') + "/?)|(" + currentAppBackendUrl + ".*(\\.([A-Za-z0-9]){2,5})?))$", RegexOptions.IgnoreCase);

                //Add watch on the configured backoffice access url
                job.WatchWebRequests(currentBackendUrlRegex, 10, (count, httpApp) =>
                {

                    var rewritePath = httpApp.Request.Url.AbsolutePath.Length > currentAppBackendUrl.Length
                        ? ApplicationSettings.UmbracoPath + httpApp.Request.Url.AbsolutePath.Substring(currentAppBackendUrl.Length)
                        : ApplicationSettings.UmbracoPath;

                    //Request is for a physical file, if it's
                    //a usercontrol etc, we need to TransferRequest
                    //otherwise, UmbracoModule will try and return
                    //a content node for the request, instead of the
                    //desired action (i.e. /umbraco/dialogs/republish.aspx)
                    if (!string.IsNullOrEmpty(httpApp.Context.Request.CurrentExecutionFilePathExtension)
                        && (httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".aspx")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".ascx")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".asmx")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".ashx")))
                    {
                        httpApp.Context.Server.TransferRequest(rewritePath, true);
                        return WatchCycle.Continue;
                    }

                    //Add access token to context so we can allow the request to
                    //pass through on the watch for the on-disk UmbracoPath location
                    httpApp.Context.Items.Add(allowKey, true);
                    httpApp.Context.RewritePath(rewritePath);
                    return WatchCycle.Restart;
                });

                //Add watch on the on-disk UmbracoPath location
                job.WatchWebRequests(umbracoPathRegex, 30, (count, httpApp) =>
                {
                    //Check if request has our access token, if so, we're
                    //rewriting the user to the on-disk UmbracoPath location,
                    //so let the request continue
                    if ((bool?)httpApp.Context.Items[allowKey] == true)
                    {
                        return WatchCycle.Continue;
                    }

                    //Check if requesting a physical file, as the user may have
                    //logged into umbraco on the custom configured url and is
                    //now loading the assets (i.e. *.css, *.js) or is running
                    //some action (i.e. /umbraco/dialogs/republish.aspx) which
                    //wouldn't have our access token! Our user IP checking Watch will
                    //handle if the request can gain access to what is being requested
                    if (!string.IsNullOrEmpty(httpApp.Context.Request.CurrentExecutionFilePathExtension))
                    {
                        return WatchCycle.Continue;
                    }

                    //If the request has a referrer as the configured backoffice access url,
                    //it's most likely a user clicking a link within the backoffice, which
                    //is pointing to the on-disk UmbracoPath location, so we need to redirect them
                    //to what's configured
                    //TODO: figure out a better way of detcting this which is more 'secure' than checking the referrer
                    if (httpApp.Context.Request.UrlReferrer != null && currentBackendUrlRegex.IsMatch(httpApp.Context.Request.UrlReferrer.AbsolutePath))
                    {
                        var rewritePath = httpApp.Request.Url.AbsolutePath.Length > ApplicationSettings.UmbracoPath.Length
                            ? currentAppBackendUrl + httpApp.Request.Url.AbsolutePath.Substring(ApplicationSettings.UmbracoPath.Length)
                            : currentAppBackendUrl;

                        httpApp.Context.Response.Redirect(rewritePath, true);
                        return WatchCycle.Stop;
                    }
                    
                    //Request isn't being rewritten by us, we need to
                    //get the unauthroised access url,
                    var url = UnauthorisedUrl(job, config);

                    //Confirm if url is not null, if it is null, we're going to stop
                    //the request, as they don't have our access token anyway
                    if (url == null)
                    {
                        return WatchCycle.Stop;
                    }

                    //We have a url, so we need to redirect/rewrite the request
                    if (config.RedirectRewrite == Enums.RedirectRewrite.Redirect)
                    {
                        httpApp.Context.Response.Redirect(url, true);
                        return WatchCycle.Stop;
                    }

                    httpApp.Context.RewritePath(url);
                    return WatchCycle.Restart;
                });
            }

            //Add watch on the on-disk UmbracoPath location to do the security checking of the user's ip
            job.WatchWebRequests(umbracoPathRegex, 1000, (count, httpApp) =>
            {
                var userIp = GetUserIp(httpApp);

                //check if IP address is not within the white-list;
                if (userIp == null || !ipv6s.Any(x => x.Equals(userIp)))
                {
                    //User is coming from a non white-listed IP Address,
                    //so we need to get the unauthroised access url
                    var url = UnauthorisedUrl(job, config);

                    //Confirm if url is not null, if it is null, we're going to stop
                    //the request, as they're coming from a non white-listed IP Address anyway
                    if(url == null)
                    {
                        return WatchCycle.Stop;
                    }

                    //Requesting a physical asset file, we're going to set the
                    //status code as 404 and let default functionality happen
                    if (!string.IsNullOrEmpty(httpApp.Context.Request.CurrentExecutionFilePathExtension) &&
                    (httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".css")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".map")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".js")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".png")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".jpg")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".jpeg")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".gif")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".woff")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".woff2")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".ttf")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".otf")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".eot")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".svg")))
                    {
                        httpApp.Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        return WatchCycle.Stop;
                    }

                    //lets log the fact that an unauthorised user try
                    //to access our configured backoffice access url
                    job.WriteJournal(new JournalMessage($"User with IP Address: {userIp.MapToIPv4()}; tried to acces the backoffice access url. Access was denied"));

                    //request isn't for a physical asset file, so redirect/rewrite
                    //the request dependant on what is configured
                    if (config.RedirectRewrite == Enums.RedirectRewrite.Redirect)
                    {
                        httpApp.Context.Response.Redirect(url, true);
                        return WatchCycle.Stop;
                    }

                    httpApp.Context.RewritePath(url);
                    return WatchCycle.Restart;
                }

                //User's IP is white-listed, allow request to continue
                return WatchCycle.Continue;
            });

            return true;
        }

        private ReaderWriterLockSlim webConfigLocker = new ReaderWriterLockSlim();

        private bool ExecuteFirstTime(IJob job, BackofficeAccessConfiguration config)
        {
            //Check if we're enabled & the configured
            //backoffice url is equal to the on-disk UmbracoPath
            if (config.Enable && new Regex($"^{ config.BackendAccessUrl.Trim('~').TrimEnd('/') }(/){{0,1}}$").IsMatch(ApplicationSettings.UmbracoPath))
            {
                //we're enalbed and the same
                //no need for a hard save
                return false;
            }

            //Check if disabled and on-disk UmbracoPath location is /umbraco/
            if (!config.Enable && ApplicationSettings.UmbracoPath.Equals(((BackofficeAccessConfiguration)DefaultConfiguration).BackendAccessUrl))
            {
                //we're disabled, and
                //on-disk UmbracoPath is /umbraco/
                //so no need for a hard save
                return false;
            }

            //We need to do a hard save, so try
            //and enter an UpgradeableReadLock
            if (!webConfigLocker.TryEnterUpgradeableReadLock(1))
            {
                //Unable to access readlock, possibly another
                //thread has beaten us here, so no need to continue
                return true;
            }

            //We have the UpgradeableReadLock
            //let's confirm everything is in place
            //and if so, we'll update things,
            //restarting the app pool as a result
            try
            {
                var path = HttpRuntime.AppDomainAppPath;
                if (!Directory.Exists(path + ApplicationSettings.UmbracoPath.TrimEnd('/')))
                {
                    job.WriteJournal(new JournalMessage($"Unable to Rename and/or move directory from: { ApplicationSettings.UmbracoPath } to: { config.BackendAccessUrl }\nThe directory {ApplicationSettings.UmbracoPath} cannot be found"));
                    return false;
                }

                var webConfig = WebConfigurationManager.OpenWebConfiguration("~");
                var appSettings = (AppSettingsSection)webConfig.GetSection("appSettings");

                var umbracoPath = appSettings.Settings["umbracoPath"];
                var umbracoReservedPaths = appSettings.Settings["umbracoReservedPaths"];

                var regex = new Regex($"^({ umbracoPath.Value.Trim('~').TrimEnd('/') }(/){{0,1}})$");

                if (!regex.IsMatch(ApplicationSettings.UmbracoPath) && !regex.IsMatch(umbracoReservedPaths.Value))
                {
                    job.WriteJournal(new JournalMessage($"Unable to make necessary changes to the web.config, appSetting keys: umbracoPath & umbracoReservedPaths doesn't contain the expected values"));
                    return false;
                }

                Directory.Move(path + ApplicationSettings.UmbracoPath.Trim('/'), path + config.BackendAccessUrl.Trim('/'));

                umbracoReservedPaths.Value = umbracoReservedPaths.Value.Replace(umbracoPath.Value.TrimEnd('/'), config.BackendAccessUrl);
                umbracoPath.Value = $"~{config.BackendAccessUrl.TrimEnd('/')}";
                webConfig.Save();
            }
            catch(Exception ex)
            {
                job.WriteJournal(new JournalMessage($"Unexpected error occurred, exception:\n{ex.Message}"));
                return false;
            }
            finally
            {
                webConfigLocker.ExitUpgradeableReadLock();
            }

            if (HttpContext.Current != null)
            {
                var response = HttpContext.Current.Response;
                response.Clear();
                response.Redirect(HttpContext.Current.Request.Url.ToString(), true);
            }

            HttpRuntime.UnloadAppDomain();

            return true;
        }

        private IPAddress GetUserIp(HttpApplication app)
        {
            var ip = app.Context.Request.UserHostAddress;

            if (ip.Equals("127.0.0.1"))
                ip = "::1";

            IPAddress tempIp;

            if(IPAddress.TryParse(ip, out tempIp))
            {
                return tempIp.MapToIPv6();
            }

            return null;
        }
    }
}
