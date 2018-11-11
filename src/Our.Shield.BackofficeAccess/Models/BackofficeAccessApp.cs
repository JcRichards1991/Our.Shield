using Our.Shield.Core.Attributes;
using Our.Shield.Core.Helpers;
using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using Our.Shield.Core.Services;
using Our.Shield.Core.Settings;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Umbraco.Core;

namespace Our.Shield.BackofficeAccess.Models
{
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    [AppEditor("/App_Plugins/Shield.BackofficeAccess/Views/BackofficeAccess.html?version=1.0.6")]
    [AppJournal]
    [AppMigration(typeof(Persistence.Migrations.Migration104))]
    public class BackofficeAccessApp : App<BackofficeAccessConfiguration>
    {
        /// <inheritdoc />
        public override string Id => nameof(BackofficeAccess);

        /// <inheritdoc />
        public override string Name =>
            ApplicationContext.Current.Services.TextService.Localize("Shield.BackofficeAccess.General/Name", CultureInfo.CurrentCulture);

        /// <inheritdoc />
        public override string Description => ApplicationContext.Current.Services.TextService.Localize("Shield.BackofficeAccess.General/Description", CultureInfo.CurrentCulture);

        /// <inheritdoc />
        public override string Icon => "icon-stop-hand red";

        /// <inheritdoc />
        public override IAppConfiguration DefaultConfiguration => new BackofficeAccessConfiguration
        {
            BackendAccessUrl = "umbraco",
            IpAccessRules = new IpAccessControl
            {
                AccessType = IpAccessControl.AccessTypes.AllowAll,
                Exceptions = Enumerable.Empty<IpAccessControl.Entry>()
            },
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

        private readonly IpAccessControlService _ipAccessControlService;
        public BackofficeAccessApp()
        {
            _ipAccessControlService = new IpAccessControlService();
        }

        private readonly string _allowKey = Guid.NewGuid().ToString();
        private static int _resetterLock;

        private void SoftWatcher(IJob job, Regex regex, int priority, string hardLocation, string softLocation, bool rewrite = true, bool addHardReseterFile = false)
        {
            //Add watch on the soft location
            job.WatchWebRequests(PipeLineStages.AuthenticateRequest, regex, priority, (count, httpApp) =>
            {
                if (addHardReseterFile && Interlocked.CompareExchange(ref _resetterLock, 0, 1) == 0)
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
                httpApp.Context.Items.Add(_allowKey, true);

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
            var umbracoLocation = ((BackofficeAccessConfiguration)DefaultConfiguration).BackendAccessUrl.EnsureStartsWith('/').EnsureEndsWith('/');
            var hardLocation = Configuration.UmbracoPath;
            var softLocation = (config.Enable && job.Environment.Enable)
                ? config.BackendAccessUrl.EnsureStartsWith('/').EnsureEndsWith('/')
                : umbracoLocation;

#if TRACE
            Debug.WriteLine($"AddSoftWatches({job.Environment.Name}): hardLocation = {hardLocation}, softLocation = {softLocation}");
#endif

            //Match Umbraco path for badly written Umbraco Packages, that only work with hardcoded /umbraco/backoffice
            if (!softLocation.Equals(umbracoLocation, StringComparison.InvariantCultureIgnoreCase) && !hardLocation.Equals(umbracoLocation, StringComparison.InvariantCultureIgnoreCase))
            {
                SoftWatcher(job,
                    new Regex("^((" + umbracoLocation + "backoffice([\\w-/_]+))|(" + umbracoLocation + "[\\w-/_]+\\.[\\w.]{2,5}))$", RegexOptions.IgnoreCase),
                    20015,
                    hardLocation,
                    umbracoLocation);
            }

            if (softLocation.Equals(hardLocation, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            //A hard save is needed, so we need to hook up
            //the watches to route everything correctly

            //add watch on the soft location
            SoftWatcher(job,
                new Regex("^((" + softLocation.TrimEnd('/') + "(/)?)|(" + softLocation + "[\\w-/_]+\\.[\\w.]{2,5}))$", RegexOptions.IgnoreCase),
                20010,
                hardLocation,
                softLocation,
                config.Unauthorized.TransferType.Equals(TransferTypes.Rewrite),
                true);

            if (config.Enable && job.Environment.Enable)
            {
                job.ExceptionWebRequest(config.Unauthorized.Url);
            }

            //Add watch on the hard location
            job.WatchWebRequests(PipeLineStages.AuthenticateRequest, new Regex("^((" + hardLocation.TrimEnd('/') + "(/)?)|(" + hardLocation + "[\\w-/]+\\.[\\w.]{2,5}))$", RegexOptions.IgnoreCase), 20020, (count, httpApp) =>
            {
                //Check if request has our access token, if so, we're
                //rewriting the user to the hard location, so let 
                //the request continue
                if ((bool?)httpApp.Context.Items[_allowKey] == true
                    || !string.IsNullOrEmpty(httpApp.Context.Request.CurrentExecutionFilePathExtension))
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
            var hardLocationRegex = new Regex("^((" + Configuration.UmbracoPath.TrimEnd('/') + "(/)?)|(" + Configuration.UmbracoPath + "[\\w-/]+\\.[\\w.]{2,5}))$", RegexOptions.IgnoreCase);

            foreach (var error in _ipAccessControlService.InitIpAccessControl(config.IpAccessRules))
            {
                job.WriteJournal(new JournalMessage($"Error: Invalid IP Address {error}, unable to add to exception list"));
            }

            //Add watch on the on-disk UmbracoPath location to do the security checking of the user's ip
            job.ExceptionWebRequest(config.Unauthorized.Url);
            job.WatchWebRequests(PipeLineStages.AuthenticateRequest, hardLocationRegex, 21000, (count, httpApp) =>
            {
                if (AccessHelper.IsRequestAuthenticatedUmbracoUser(httpApp))
                {
                    return new WatchResponse(WatchResponse.Cycles.Continue);
                }

                if (_ipAccessControlService.IsValid(config.IpAccessRules, httpApp.Context.Request))
                    return new WatchResponse(WatchResponse.Cycles.Continue);

                var url = new UmbracoUrlService().Url(config.Unauthorized.Url);

                if (url == null)
                {
                    return new WatchResponse(WatchResponse.Cycles.Stop);
                }

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

                job.WriteJournal(new JournalMessage($"User with IP Address: {httpApp.Context.Request.UserHostAddress}; tried to access the backoffice access url. Access was denied"));

                return new WatchResponse(config.Unauthorized);
            });
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="job"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public override bool Execute(IJob job, IAppConfiguration c)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(_allowKey);
            job.UnwatchWebRequests();
            job.UnexceptionWebRequest();

            _resetterLock = 0;

            if (!(c is BackofficeAccessConfiguration config))
            {
                job.WriteJournal(new JournalMessage("Error: Config passed into Backoffice Access was not of the correct type"));
                return false;
            }

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
