using Our.Shield.Core;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.Helpers;
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

namespace Our.Shield.BackofficeAccess.Models
{
    /// <summary>
    /// 
    /// </summary>
    [AppEditor("/App_Plugins/Shield.BackofficeAccess/Views/BackofficeAccess.html?version=1.0.4")]
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
        public override string Description => "Change the backoffice access URL and/or secure your backoffice access URL via IP restrictions";

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
                    IpAddressesAccess = Enums.IpAddressesAccess.Unrestricted,
                    IpEntries = new IpEntry[0],
                    Unauthorized = new TransferUrl
                    {
                        TransferType = TransferTypes.Redirect,
                        Url = new UmbracoUrl
                        {
                            Type = UmbracoUrlTypes.Url,
                            Value = string.Empty
                        }
                    }
                };
            }
        }

        private readonly string AllowKey = Guid.NewGuid().ToString();
        private readonly TimeSpan CacheLength = new TimeSpan(TimeSpan.TicksPerDay);        //   Once per day

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
                        return new WatchResponse(WatchResponse.Cycles.Stop);
                    }

                    //Request is for a css etc. file, transmit
                    //the file and set correct mime type
                    var mimeType = MimeMapping.GetMimeMapping(rewritePath);

                    httpApp.Context.Response.ContentType = mimeType;
                    httpApp.Context.Response.TransmitFile(httpApp.Context.Server.MapPath(rewritePath));
                    httpApp.Context.Response.End();
                    return new WatchResponse(WatchResponse.Cycles.Stop);
                }

                //we can add querystring here as the request is not for
                //a physical file and may be needed for any 'sub' requests
                rewritePath += httpApp.Request.Url.Query;

                //We're not a physical usercontrol etc. file, so we can just
                //rewrite the path to the hard location. we need to add the
                //access token to context so we can allow the request to pass
                //through on the watch for the hard location
                httpApp.Context.Items.Add(AllowKey, true);

                if (rewrite || regex.IsMatch(softLocation))
                {
                    httpApp.Context.RewritePath(rewritePath);
                    return new WatchResponse(WatchResponse.Cycles.Restart);
                }
                httpApp.Context.Response.Redirect(rewritePath);
                return new WatchResponse(WatchResponse.Cycles.Stop);
            });
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
                        if (AccessHelper.IsRequestAuthenticatedUmbracoUser(httpApp))
                        {
                            //request has a authenticated user, we want to
                            //redirect the user back to the soft location
                            return new WatchResponse(new TransferUrl
                            {
                                TransferType = TransferTypes.Redirect,
                                Url = new UmbracoUrl
                                {
                                    Type = UmbracoUrlTypes.Url,
                                    Value = httpApp.Request.Url.AbsolutePath.Length > umbracoLocation.Length
                                        ? hardLocation + httpApp.Request.Url.AbsolutePath.Substring(umbracoLocation.Length)
                                        : hardLocation
                                }
                            });
                        }

                        return new WatchResponse(WatchResponse.Cycles.Continue);
                    });
                }

                return;
            }

            //A hard save is needed, so we need to hook up
            //the watches to route everything correctly

            //add watch on the soft location
            SoftWatcher(job,
                new Regex("^((" + softLocation.TrimEnd('/') + "(/)?)|(" + softLocation + "[\\w-/_]+\\.[\\w.]{2,5}))$", RegexOptions.IgnoreCase),
                10,
                hardLocation,
                softLocation,
                config.Unauthorized.TransferType.Equals(TransferTypes.Rewrite),
                true);

            //Add watch on the hard location
            job.WatchWebRequests(new Regex("^((" + hardLocation.TrimEnd('/') + "(/)?)|(" + hardLocation + "[\\w-/]+\\.[\\w.]{2,5}))$", RegexOptions.IgnoreCase), 20, (count, httpApp) =>
            {
                //Check if request has our access token, if so, we're
                //rewriting the user to the hard location, so let 
                //the request continue
                if ((bool?)httpApp.Context.Items[AllowKey] == true)
                {
                    return new WatchResponse(WatchResponse.Cycles.Continue);
                }

                //Check if requesting a physical file, as the user may have
                //logged into umbraco on the custom configured url and is
                //now requesting the assets (i.e. *.css, *.js) or is running
                //some action (i.e. /umbraco/dialogs/republish.aspx) which
                //wouldn't have our access token! Our user IP checking Watch will
                //handle if the request can gain access to what is being requested
                if (!string.IsNullOrEmpty(httpApp.Context.Request.CurrentExecutionFilePathExtension))
                {
                    return new WatchResponse(WatchResponse.Cycles.Continue);
                }

                //If the requests has an authenticated umbraco user,
                //we need to redirect the request back to the
                //softLocation - This is most likely due to
                //clicking a link (i.e. content breadcrumb)
                //which isn't handle by the angular single page app
                if (AccessHelper.IsRequestAuthenticatedUmbracoUser(httpApp))
                {
                    //request has a authenticated user, we want to
                    //redirect the user back to the soft location
                    return new WatchResponse(new TransferUrl
                    {
                        TransferType = TransferTypes.Redirect,
                        Url = new UmbracoUrl
                        {
                            Type = UmbracoUrlTypes.Url,
                            Value = httpApp.Context.Request.Url.AbsolutePath.Length > hardLocation.Length
                                ? softLocation + httpApp.Context.Request.Url.AbsolutePath.Substring(hardLocation.Length)
                                : softLocation
                        }
                    });
                }

                //if we're disabled, then we just want
                //to change the status code to 404
                if (!config.Enable || !job.Environment.Enable)
                {
                    httpApp.Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return new WatchResponse(WatchResponse.Cycles.Stop);
                }

                //We're Enabled, so we need to transfer to the unauthorised Url
                return new WatchResponse(config.Unauthorized);
            });
        }

        private void AddHardWatch(IJob job, BackofficeAccessConfiguration config)
        {
            var hardLocationRegex = new Regex("^((" + ApplicationSettings.UmbracoPath.TrimEnd('/') + "(/)?)|(" + ApplicationSettings.UmbracoPath + "[\\w-/]+\\.[\\w.]{2,5}))$", RegexOptions.IgnoreCase);

            if (config.IpAddressesAccess == Enums.IpAddressesAccess.Unrestricted)
            {
                job.WatchWebRequests(hardLocationRegex, 1000, (count, httpApp) =>
                {
                    return new WatchResponse(WatchResponse.Cycles.Continue);
                });

                return;
            }

            var whiteList = new List<IPAddress>();
            
            //Convert our IP address(es) to the IpAddress
            //Class, so we're working with something more standard
            foreach (var ipEntry in config.IpEntries)
            {
                var ip = AccessHelper.ConvertToIpv6(ipEntry.IpAddress);
                if (ip == null)
                {
                    job.WriteJournal(new JournalMessage($"Error: Invalid IP Address {ipEntry.IpAddress}, unable to add to white-list"));
                    continue;
                }

                whiteList.Add(ip);
            }

            //Add watch on the on-disk UmbracoPath location to do the security checking of the user's ip
            job.WatchWebRequests(hardLocationRegex, 1000, (count, httpApp) =>
            {
                //If request has an authenticated user,
                //we've already checked the user's IP
                //address, and therefore we should just
                //allow access without checking IP again
                if (AccessHelper.IsRequestAuthenticatedUmbracoUser(httpApp))
                {
                    return new WatchResponse(WatchResponse.Cycles.Continue);
                }

                var userIp = AccessHelper.ConvertToIpv6(httpApp.Context.Request.UserHostAddress);

                //check if IP address is not within the white-list;
                if (userIp == null || !whiteList.Any(x => x.Equals(userIp)))
                {
                    //User is coming from a non white-listed IP Address,
                    //so we need to get the unauthorized access url
                    var url = new UmbracoUrlService().Url(config.Unauthorized.Url);

                    //Confirm if url is not null, if it is null, we're going to stop
                    //the request, as they're coming from a non white-listed IP Address anyway
                    if (url == null)
                    {
                        return new WatchResponse(WatchResponse.Cycles.Stop);
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
                        return new WatchResponse(WatchResponse.Cycles.Stop);
                    }

                    //lets log the fact that an unauthorised user tried
                    //to access our configured backoffice access url
                    job.WriteJournal(new JournalMessage($"User with IP Address: {(userIp.ToString().Equals("::1") ? "127.0.0.1" : userIp.ToString())}; tried to access the backoffice access url. Access was denied"));

                    //request isn't for a physical asset file, so redirect/rewrite
                    //the request dependant on what is configured
                    return new WatchResponse(config.Unauthorized);
                }

                //User's IP is white-listed, allow request to continue
                return new WatchResponse(WatchResponse.Cycles.Continue);
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
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(AllowKey);
            job.UnwatchWebRequests();

            ResetterLock = 0;

            var config = c as BackofficeAccessConfiguration;

            //Add our soft watches - this also handles
            //the watches for when we're disabled
            AddSoftWatches(job, config);

            //if we're enabled, we need to add our IP checking watch
            if (config.Enable && job.Environment.Enable)
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
