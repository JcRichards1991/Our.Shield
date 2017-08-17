using Our.Shield.Core;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Web;

namespace Our.Shield.BackofficeAccess.Models
{
    /// <summary>
    /// 
    /// </summary>
    [AppEditor("/App_Plugins/Shield.BackofficeAccess/Views/BackofficeAccess.html?version=1.0.2")]
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
                    BackendAccessUrl = "umbraco",
                    IpEntries = new IpEntry[0],
                    UnauthorisedAction = Enums.UnauthorisedAction.Redirect,
                    UnauthorisedUrlType = Enums.UnautorisedUrlType.Url
                };
            }
        }

        private readonly string allowKey = Guid.NewGuid().ToString();
        private readonly TimeSpan CacheLength = new TimeSpan(TimeSpan.TicksPerDay);        //   Once per day

        private string UnauthorisedUrl(IJob job, BackofficeAccessConfiguration config)
        {
            return ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(allowKey, () =>
            {
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

                        journalMessage.Message = "Error: Unable to get the unauthorized URL from the specified XPath expression";
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

                            journalMessage.Message = "Error: Unable to get the unauthorized URL from the unauthorized URL content picker. Please ensure the selected page is published and not deleted";
                            break;
                        }

                        journalMessage.Message = "Error: Unable to parse the selected unauthorized URL content picker content. Please ensure a valid content node is selected";
                        break;

                    default:
                        journalMessage.Message = "Error: Unable to determine which method to use to get the unauthorized URL. Please ensure URL, XPath or Content Picker is selected";
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

        private static int ResetterLock = 0;

        private int SoftWatcher(IJob job, Regex regex, int priority, string hardLocation, string softLocation, bool rewrite = true, bool addHardReseterFile = false)
        {
            //Add watch on the soft location
            return job.WatchWebRequests(regex, priority, (count, httpApp) =>
            {
                if (addHardReseterFile && Interlocked.CompareExchange(ref ResetterLock, 0, 1) == 0)
                {
                    var resetter = new HardResetFileHandler();
                    resetter.Delete();

                    var path = HttpRuntime.AppDomainAppPath;

                    resetter.HardLocation = path + hardLocation.Trim('/');
                    resetter.SoftLocation = path + softLocation.Trim('/');
                    resetter.Save();
                }

                //change the Url to point to the hardLocation
                //for the request to work as expected
                var rewritePath = httpApp.Request.Url.AbsolutePath.Length > softLocation.Length
                    ? hardLocation + httpApp.Request.Url.AbsolutePath.Substring(softLocation.Length)
                    : hardLocation;

                //Request is for a physical file, if it's
                //a usercontrol etc, we need to TransferRequest
                //otherwise, it's a css etc. and we need to
                //transmitFile and set the correct mime type
                //otherwise, UmbracoModule will try and return
                //a content item for the request, resulting in
                //a 404 status code
                if (!string.IsNullOrEmpty(httpApp.Context.Request.CurrentExecutionFilePathExtension))
                {
                    //Request is for a usercontrol etc. transfer
                    //the request and leave the watcher
                    if (httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".aspx")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".ascx")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".asmx")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".ashx"))
                    {
                        //add the querystring to pass onto the usercontrol etc.
                        //we can't do it on creating the variable as it
                        //causes issues with transmit file
                        httpApp.Context.Server.TransferRequest(rewritePath + httpApp.Request.Url.Query, true);
                        return WatchCycle.Stop;
                    }

                    //Request is for a css etc. file, transmit
                    //the file and set correct mime type
                    var mimeType = MimeMapping.GetMimeMapping(rewritePath);

                    httpApp.Context.Response.ContentType = mimeType;
                    httpApp.Context.Response.TransmitFile(httpApp.Context.Server.MapPath(rewritePath));
                    httpApp.Context.Response.End();
                    return WatchCycle.Stop;
                }

                //we can add querystring here as the request is not for
                //a physical file and may be needed for any 'sub' requests
                rewritePath += httpApp.Request.Url.Query;

                //We're not a physical usercontrol etc. file, so we can just
                //rewrite the path to the hard location. we need to add the
                //access token to context so we can allow the request to pass
                //through on the watch for the hard location
                httpApp.Context.Items.Add(allowKey, true);

                if (rewrite || regex.IsMatch(softLocation))
                {
                    httpApp.Context.RewritePath(rewritePath);
                    return WatchCycle.Restart;
                }
                httpApp.Context.Response.Redirect(rewritePath);
                return WatchCycle.Stop;
            });
        }

        private bool IsRequestAuthenticated(HttpApplication httpApp)
        {
            var httpContext = new HttpContextWrapper(httpApp.Context);
            var umbAuthTicket = httpContext.GetUmbracoAuthTicket();

            return httpContext.AuthenticateCurrentRequest(umbAuthTicket, true);
        }

        private void AddSoftWatches(IJob job, BackofficeAccessConfiguration config)
        {
            var umbracoLocation = ((BackofficeAccessConfiguration)this.DefaultConfiguration).BackendAccessUrl.EnsureStartsWith('/').EnsureEndsWith('/');
            var hardLocation = ApplicationSettings.UmbracoPath;
            var softLocation = (config.Enable && job.Environment.Enable)
                ? config.BackendAccessUrl.EnsureStartsWith('/').EnsureEndsWith('/')
                : umbracoLocation;

            //Match Umbraco path for badly written Umbraco Packages, that only work with hardcoded /umbraco/backoffice
            if (!softLocation.Equals(umbracoLocation, StringComparison.InvariantCultureIgnoreCase) && !hardLocation.Equals(umbracoLocation, StringComparison.InvariantCultureIgnoreCase))
            {
                SoftWatcher(job,
                    new Regex("^((" + umbracoLocation + "backoffice([\\w-/_]+))|(" + umbracoLocation + "[\\w-/_]+\\.[\\w.]{2,5}))$", RegexOptions.IgnoreCase),
                    15,
                    hardLocation,
                    umbracoLocation);
            }

            if (softLocation.Equals(hardLocation, StringComparison.InvariantCultureIgnoreCase))

            {
                //if the hardlocation doesn't equals /umbraco/
                //we need to add a watch on umbraco in case 
                //another backoffice access app is disabled
                //which will cause /umbraco/ to be accessible
                //even though it shouldn't
                if (!hardLocation.Equals(umbracoLocation, StringComparison.InvariantCultureIgnoreCase))
                {
                    job.WatchWebRequests(new Regex("^(" + umbracoLocation.TrimEnd('/') + "(/)?)$|^(" + umbracoLocation + "[\\w-/_]+\\.[\\w.]{2,5})$", RegexOptions.IgnoreCase), 20, (count, httpApp) =>
                    {
                        //If the requests has an authenticated umbraco user,
                        //we need to redirect the request back to the
                        //hard location - This is most likely due to
                        //clicking a link (i.e. content breadcrumb)
                        //which isn't handle by the angular single page app
                        if (IsRequestAuthenticated(httpApp))
                        {
                            //request has a authenticated user, we want to
                            //redirect the user back to the soft location
                            var rewritePath = httpApp.Request.Url.AbsolutePath.Length > umbracoLocation.Length
                                ? hardLocation + httpApp.Request.Url.AbsolutePath.Substring(umbracoLocation.Length)
                                : hardLocation;

                            httpApp.Context.Response.Redirect(rewritePath, true);
                            return WatchCycle.Stop;
                        }

                        return WatchCycle.Continue;
                    });
                }

                return;
            }


                //A hard save is needed, so we need to hook up
                //the watches to route everything correctly
                }

            //A hard save is needed, so we need to hook up
            //the watches to route everything correctly

            //add watch on the soft location
                SoftWatcher(job,
                new Regex("^((" + softLocation.TrimEnd('/') + "(/)?)|(" + softLocation + "[\\w-/_]+\\.[\\w.]{2,5}))$", RegexOptions.IgnoreCase),
                10,
                hardLocation,
                softLocation,
                config.UnauthorisedAction.Equals(Enums.UnauthorisedAction.Rewrite), true);

                //Add watch on the hard location
                job.WatchWebRequests(new Regex("^((" + hardLocation.TrimEnd('/') + "(/)?)|(" + hardLocation + "[\\w-/]+\\.[\\w.]{2,5}))$", RegexOptions.IgnoreCase),
                    20,
                    (count, httpApp) =>
                    {
                        //Check if request has our access token, if so, we're
                        //rewriting the user to the hard location, so let 
                        //the request continue
                        if ((bool?)httpApp.Context.Items[allowKey] == true)
                        {
                            return WatchCycle.Continue;
                        }

                        //Check if requesting a physical file, as the user may have
                        //logged into umbraco on the custom configured url and is
                        //now requesting the assets (i.e. *.css, *.js) or is running
                        //some action (i.e. /umbraco/dialogs/republish.aspx) which
                        //wouldn't have our access token! Our user IP checking Watch will
                        //handle if the request can gain access to what is being requested
                        if (!string.IsNullOrEmpty(httpApp.Context.Request.CurrentExecutionFilePathExtension))
                        {
                            return WatchCycle.Continue;
                        }

                        //If the requests has an authenticated umbraco user,
                        //we need to redirect the request back to the
                        //softLocation - This is most likely due to
                        //clicking a link (i.e. content breadcrumb)
                        //which isn't handle by the angular single page app
                        if (IsRequestAuthenticated(httpApp))
                        {
                            //request has a authenticated user, we want to
                            //redirect the user back to the soft location
                            var rewritePath = httpApp.Context.Request.Url.AbsolutePath.Length > hardLocation.Length
                                ? softLocation + httpApp.Context.Request.Url.AbsolutePath.Substring(hardLocation.Length)
                                : softLocation;

                            httpApp.Context.Response.Redirect(rewritePath, true);
                            return WatchCycle.Stop;
                        }

                        //if we're disabled, then we just want
                        //to change the status code to 404
                        if (!config.Enable || !job.Environment.Enable)
                        {
                            httpApp.Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            return WatchCycle.Stop;
                        }

                        //We're Enabled, so we need to get the unauthorised Url
                        var url = UnauthorisedUrl(job, config);

                        //Confirm if url is not null, if it is null, we're going to stop
                        //the request, as they don't have our access token anyway
                        if (url == null)
                        {
                            httpApp.Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            return WatchCycle.Stop;
                        }

                        //We have a url, so we need to redirect/rewrite the request
                        //dependant on what is configured
                        if (config.UnauthorisedAction == Enums.UnauthorisedAction.Redirect)
                        {
                            httpApp.Context.Response.Redirect(url, true);
                            return WatchCycle.Stop;
                        }

                        httpApp.Context.RewritePath(url, string.Empty, string.Empty);
                        return WatchCycle.Restart;
                    });
            }
        }

        private IPAddress ConvertToIpv6(string ip)
        {
            if (ip.Equals("127.0.0.1"))
                ip = "::1";

            IPAddress typedIp;
            if(IPAddress.TryParse(ip, out typedIp))
            {
                return typedIp.MapToIPv6();
            }

            return null;
        }

        private void AddHardWatch(IJob job, BackofficeAccessConfiguration config)
        {
            var whiteList = new List<IPAddress>();

            //Convert our IP address(es) to the IpAddress
            //Class, so we're working with something more standard
            foreach (var ipEntry in config.IpEntries)
            {
                var ip = ConvertToIpv6(ipEntry.IpAddress);
                if (ip == null)
                {
                    job.WriteJournal(new JournalMessage($"Error: Invalid IP Address {ipEntry.IpAddress}, unable to add to white-list"));
                    continue;
                }

                whiteList.Add(ip);
            }

            var hardLocationRegex = new Regex("^((" + ApplicationSettings.UmbracoPath.TrimEnd('/') + "(/)?)|(" + ApplicationSettings.UmbracoPath + "[\\w-/]+\\.[\\w.]{2,5}))$", RegexOptions.IgnoreCase);

            //Add watch on the on-disk UmbracoPath location to do the security checking of the user's ip
            job.WatchWebRequests(hardLocationRegex, 1000, (count, httpApp) =>
            {
                //If request has an authenticated user,
                //we've already checked the user's IP
                //address, and therefore we should just
                //allow access without checking IP again
                if (IsRequestAuthenticated(httpApp))
                {
                    return WatchCycle.Continue;
                }

                var userIp = ConvertToIpv6(httpApp.Context.Request.UserHostAddress);

                //check if IP address is not within the white-list;
                if (userIp == null || !whiteList.Any(x => x.Equals(userIp)))
                {
                    //User is coming from a non white-listed IP Address,
                    //so we need to get the unauthorized access url
                    var url = UnauthorisedUrl(job, config);

                    //Confirm if url is not null, if it is null, we're going to stop
                    //the request, as they're coming from a non white-listed IP Address anyway
                    if (url == null)
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

                    //lets log the fact that an unauthorised user tried
                    //to access our configured backoffice access url
                    job.WriteJournal(new JournalMessage($"User with IP Address: {(userIp.ToString().Equals("::1") ? "127.0.0.1" : userIp.ToString())}; tried to access the backoffice access url. Access was denied"));

                    //request isn't for a physical asset file, so redirect/rewrite
                    //the request dependant on what is configured
                    if (config.UnauthorisedAction == Enums.UnauthorisedAction.Redirect)
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public override bool Execute(IJob job, IConfiguration c)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(allowKey);
            job.UnwatchWebRequests();

            ResetterLock = 0;

            var config = c as BackofficeAccessConfiguration;

            //Add our soft watches - this also handles
            //the watches for when we're disabled
            AddSoftWatches(job, config);

            //if we're enabled, we need to add our IP checking watch
            if (config.Enable && job.Environment.Enable && config.IpAddressesRestricted == Enums.IpAddressesRestricted.Restricted)
            {
                //Add our Hard Watch to
                //do the security checking
                //on the user's IP Address
                AddHardWatch(job, config);
            }

            return true;
        }
    }
}
