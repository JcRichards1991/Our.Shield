using Our.Shield.Core.Attributes;
using Our.Shield.Core.Helpers;
using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using Our.Shield.Core.Services;
using Our.Shield.Core.Settings;
using System;
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
    [AppEditor("/App_Plugins/Shield.BackofficeAccess/Views/BackofficeAccess.html?version=1.1.0")]
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
        private static int _reSetterLock;

        private void SoftWatcher(IJob job,
            Regex regex,
            int priority,
            string onDiscUmbracoLocation,
            string virtualUmbracoLocation,
            bool rewrite)
        {
            job.WatchWebRequests(PipeLineStages.AuthenticateRequest, regex, priority, (count, httpApp) =>
            {
                var path = httpApp.Request.Url.AbsolutePath.EnsureEndsWith('/');
                var rewritePath = onDiscUmbracoLocation + path.Substring(virtualUmbracoLocation.Length);
                
                if (!string.IsNullOrEmpty(httpApp.Context.Request.CurrentExecutionFilePathExtension))
                {
                    if (httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".aspx")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".ascx")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".asmx")
                        || httpApp.Context.Request.CurrentExecutionFilePathExtension.Equals(".ashx"))
                    {
                        return new WatchResponse(new TransferUrl
                        {
                            TransferType = TransferTypes.TransferRequest,
                            Url = new UmbracoUrl
                            {
                                Type = UmbracoUrlTypes.Url,
                                Value = rewritePath + httpApp.Request.Url.Query
                            }
                        });
                    }

                    return new WatchResponse(new TransferUrl
                    {
                        TransferType = TransferTypes.TransmitFile,
                        Url = new UmbracoUrl
                        {
                            Type = UmbracoUrlTypes.Url,
                            Value = rewritePath
                        }
                    });
                }
                
                httpApp.Context.Items.Add(_allowKey, true);
                rewritePath += httpApp.Request.Url.Query;

                if (rewrite)
                {
                    return new WatchResponse(new TransferUrl
                    {
                        TransferType = TransferTypes.Rewrite,
                        Url = new UmbracoUrl
                        {
                            Type = UmbracoUrlTypes.Url,
                            Value = rewritePath
                        }
                    });
                }

                return new WatchResponse(new TransferUrl
                {
                    TransferType = TransferTypes.Redirect,
                    Url = new UmbracoUrl
                    {
                        Type = UmbracoUrlTypes.Url,
                        Value = rewritePath
                    }
                });
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

            _reSetterLock = 0;

            if (!(c is BackofficeAccessConfiguration config))
            {
                job.WriteJournal(new JournalMessage("Error: Config passed into Backoffice Access was not of the correct type"));
                return false;
            }

            var defaultUmbracoLocation = ((BackofficeAccessConfiguration)DefaultConfiguration).BackendAccessUrl.EnsureStartsWith('/').EnsureEndsWith('/');
            var onDiscUmbracoLocation = Configuration.UmbracoPath.EnsureStartsWith('/').EnsureEndsWith('/');
            var virtualUmbracoLocation = config.Enable && job.Environment.Enable
                ? config.BackendAccessUrl.EnsureStartsWith('/').EnsureEndsWith('/')
                : defaultUmbracoLocation;

            var defaultUmbracoRegex = job.PathToRegex(defaultUmbracoLocation);
            var onDiscUmbracoRegex = job.PathToRegex(onDiscUmbracoLocation);
            var virtualUmbracoRegex = job.PathToRegex(virtualUmbracoLocation);

            job.IgnoreWebRequest(defaultUmbracoRegex);
            job.IgnoreWebRequest(onDiscUmbracoRegex);
            job.IgnoreWebRequest(virtualUmbracoRegex);
            
            if (!virtualUmbracoLocation.Equals(defaultUmbracoLocation, StringComparison.InvariantCultureIgnoreCase)
                && !onDiscUmbracoLocation.Equals(defaultUmbracoLocation, StringComparison.InvariantCultureIgnoreCase))
            {
                SoftWatcher(job,
                    new Regex("^(" + defaultUmbracoLocation + "backoffice([\\w-/_]+))", RegexOptions.IgnoreCase),
                    20000,
                    onDiscUmbracoLocation,
                    virtualUmbracoLocation,
                    false);
            }

            if (config.Enable && job.Environment.Enable && config.Unauthorized.TransferType != TransferTypes.PlayDead)
            {
                job.ExceptionWebRequest(config.Unauthorized.Url);
            }

            if (!virtualUmbracoLocation.Equals(onDiscUmbracoLocation, StringComparison.InvariantCultureIgnoreCase))
            {
                if (Interlocked.CompareExchange(ref _reSetterLock, 0, 1) == 0)
                {
                    var path = HttpRuntime.AppDomainAppPath;
                    var onDiscPath = path + onDiscUmbracoLocation.Trim('/');
                    var virtualPath = path + virtualUmbracoLocation.Trim('/');

                    var reSetter = new HardResetFileHandler();

                    if (reSetter.HardLocation != onDiscPath || reSetter.SoftLocation != virtualPath)
                    {
                        reSetter.Delete();

                        reSetter.HardLocation = onDiscPath;
                        reSetter.SoftLocation = virtualPath;

                        reSetter.Save();
                    }
                }

                SoftWatcher(job,
                    virtualUmbracoRegex,
                    20100,
                    onDiscUmbracoLocation,
                    virtualUmbracoLocation,
                    true);

                job.WatchWebRequests(PipeLineStages.AuthenticateRequest, new Regex($"^(({onDiscUmbracoLocation})|({onDiscUmbracoLocation.TrimEnd('/')}))$"), 20200, (count, httpApp) =>
                {
                    if ((bool?)httpApp.Context.Items[_allowKey] == true
                        || !string.IsNullOrEmpty(httpApp.Context.Request.CurrentExecutionFilePathExtension)
                        || AccessHelper.IsRequestAuthenticatedUmbracoUser(httpApp))
                    {
                        return new WatchResponse(WatchResponse.Cycles.Continue);
                    }

                    httpApp.Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return new WatchResponse(WatchResponse.Cycles.Stop);
                });
            }
            
            if (!config.Enable || !job.Environment.Enable)
            {
                return true;
            }

            foreach (var error in _ipAccessControlService.InitIpAccessControl(config.IpAccessRules))
            {
                job.WriteJournal(new JournalMessage($"Error: Invalid IP Address {error}, unable to add to exception list"));
            }

            job.WatchWebRequests(PipeLineStages.AuthenticateRequest, onDiscUmbracoRegex, 20300, (count, httpApp) =>
            {
                if (AccessHelper.IsRequestAuthenticatedUmbracoUser(httpApp)
                    || _ipAccessControlService.IsValid(config.IpAccessRules, httpApp.Context.Request))
                {
                    return new WatchResponse(WatchResponse.Cycles.Continue);
                }

                job.WriteJournal(new JournalMessage($"User with IP Address: {httpApp.Context.Request.UserHostAddress}; tried to access the backoffice access url. Access was denied"));

                return new WatchResponse(config.Unauthorized);
            });

            return true;
        }
    }
}