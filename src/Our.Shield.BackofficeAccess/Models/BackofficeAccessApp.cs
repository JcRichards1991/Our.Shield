﻿namespace Our.Shield.BackofficeAccess.Models
{
    using Core.Models;
    using Core.UI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Web;
    using Umbraco.Core;
    using Umbraco.Core.Security;
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

        private int SoftWatcher(IJob job, Regex regex, int priority, string hardLocation, string softLocation, bool rewrite = true)
        {
            //Add watch on the soft location
            return job.WatchWebRequests(regex, priority, (count, httpApp) =>
            {
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

                if (rewrite)
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
            var umbracoLocation = ((BackofficeAccessConfiguration)this.DefaultConfiguration).BackendAccessUrl;
            var hardLocation = ApplicationSettings.UmbracoPath;
            var softLocation = (config.Enable) 
                ? config.BackendAccessUrl.EnsureEndsWith('/') 
                : umbracoLocation;

            //Match Umbraco path for badly written Umbraco Packages, that only work with hardcoded /umbraco/backoffice
            if (!softLocation.Equals(umbracoLocation, StringComparison.InvariantCultureIgnoreCase) && !hardLocation.Equals(umbracoLocation, StringComparison.InvariantCultureIgnoreCase))
            {
                SoftWatcher(job,
                    new Regex("^" + umbracoLocation + "backoffice/", RegexOptions.IgnoreCase),
                    15,
                    hardLocation,
                    umbracoLocation,
                    true);
            }

            var resetter = new HardResetFileHandler();

            //if the softLocation and the hardLocation
            //are the same we don't need to add any watches
            //we can exit the function
            if (softLocation.Equals(hardLocation, StringComparison.InvariantCultureIgnoreCase))
            {
                resetter.Delete();
                return;
            }

            var path = HttpRuntime.AppDomainAppPath;
            
            resetter.HardLocation = path + hardLocation.Trim('/');
            resetter.SoftLocation = path + softLocation.Trim('/');
            resetter.EnvironmentId = job.Environment.Id;
            resetter.Save();

            //A hard save has occurred so we need
            //to make sure backoffice is accessible
            SoftWatcher(job,
                new Regex("^((" + softLocation.TrimEnd('/') + ")(/?)|(" + softLocation + ".*\\.([A-Za-z0-9]){2,5}))$", RegexOptions.IgnoreCase),
                10,
                hardLocation,
                softLocation,
                true);

            var hardLocationRegex = new Regex("^((" + hardLocation.TrimEnd('/') + ")(/?)|(" + hardLocation + ".*\\.([A-Za-z0-9]){2,5}))$", RegexOptions.IgnoreCase);

            //Add watch on the hard location
            job.WatchWebRequests(hardLocationRegex, 20, (count, httpApp) =>
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
                if (!config.Enable)
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
                    return WatchCycle.Stop;
                }

                //We have a url, so we need to redirect/rewrite the request
                //dependant on what is configured
                if (config.RedirectRewrite == Enums.RedirectRewrite.Redirect)
                {
                    httpApp.Context.Response.Redirect(url, true);
                    return WatchCycle.Stop;
                }

                httpApp.Context.RewritePath(url);
                return WatchCycle.Restart;
            });
        }

        private IPAddress GetUserIp(HttpApplication app)
        {
            var ip = app.Context.Request.UserHostAddress;

            if (ip.Equals("127.0.0.1"))
                ip = "::1";

            IPAddress tempIp;

            if (IPAddress.TryParse(ip, out tempIp))
            {
                return tempIp.MapToIPv6();
            }

            return null;
        }

        private void AddHardWatch(IJob job, BackofficeAccessConfiguration config)
        {
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

            var hardLocationRegex = new Regex("^((" + ApplicationSettings.UmbracoPath.TrimEnd('/') + ")(/?)|(" + ApplicationSettings.UmbracoPath + ".*\\.([A-Za-z0-9]){2,5}))$", RegexOptions.IgnoreCase);

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

                var userIp = GetUserIp(httpApp);

                //check if IP address is not within the white-list;
                if (userIp == null || !ipv6s.Any(x => x.Equals(userIp)))
                {
                    //User is coming from a non white-listed IP Address,
                    //so we need to get the unauthroised access url
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

            var config = c as BackofficeAccessConfiguration;

            //Add our soft watches - this also handles
            //the watches for when we're disabled
            AddSoftWatches(job, config);

            //if we're enabled, we need to add our IP checking watch
            if (config.Enable)
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
