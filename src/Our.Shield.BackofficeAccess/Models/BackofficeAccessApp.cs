using Our.Shield.Core.Attributes;
using Our.Shield.Core.Enums;
using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using Our.Shield.Core.Services;
using Our.Shield.Core.Settings;
using Our.Shield.Shared.Extensions;
using System;
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
    [AppEditor("/App_Plugins/Shield.BackofficeAccess/Views/BackofficeAccess.html?version=2.0.0")]
    [AppJournal]
    public class BackofficeAccessApp : App<BackofficeAccessConfiguration>
    {
        private readonly string _allowKey = Guid.NewGuid().ToString();
        private static int _reSetterLock;

        private readonly IIpAccessControlService _ipAccessControlService;

        /// <summary>
        /// Initializes a new instance of <see cref="BackofficeAccessApp"/>
        /// </summary>
        /// <param name="journalService"><see cref="IJournalService"/></param>
        /// <param name="ipAccessControlService"><see cref="IpAccessControlService"/></param>
        public BackofficeAccessApp(
            IJournalService journalService,
            IIpAccessControlService ipAccessControlService)
            : base(journalService)
        {
            _ipAccessControlService = ipAccessControlService;
        }

        /// <summary>
        /// App Id
        /// </summary>
        public override string Id => nameof(BackofficeAccess);

        /// <inheritdoc />
        public override string Icon => "icon-stop-hand red";

        /// <inheritdoc />
        public override IAppConfiguration DefaultConfiguration => new BackofficeAccessConfiguration
        {
            BackendAccessUrl = "umbraco",
            ExcludeUrls = Enumerable.Empty<string>(),
            IpAccessControl = new IpAccessControl
            {
                AccessType = AccessTypes.AllowAll,
                IpAccessRules = Enumerable.Empty<IpAccessRule>()
            },
            TransferUrlControl = new TransferUrlControl
            {
                TransferType = TransferType.Redirect,
                Url = new UmbracoUrl
                {
                    Type = UmbracoUrlType.Url,
                    Value = string.Empty
                }
            }
        };

        /// <inheritdoc />
        public override bool Execute(IJob job, IAppConfiguration c)
        {
            _reSetterLock = 0;

            if (!(c is BackofficeAccessConfiguration config))
            {
                return false;
            }

            var defaultUmbracoLocation = ((BackofficeAccessConfiguration)DefaultConfiguration).BackendAccessUrl.EnsureStartsWith('/').EnsureEndsWith('/');
            var onDiscUmbracoLocation = ShieldConfiguration.UmbracoPath.EnsureStartsWith('/').EnsureEndsWith('/');
            var virtualUmbracoLocation = config.Enabled && job.Environment.Enabled
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

            if (config.Enabled && job.Environment.Enabled && config.TransferUrlControl.TransferType != TransferType.PlayDead)
            {
                job.ExceptionWebRequest(config.TransferUrlControl.Url);
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
                        || _ipAccessControlService.IsRequestAuthenticatedUmbracoUser(httpApp))
                    {
                        return new WatchResponse(Cycle.Continue);
                    }

                    httpApp.Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                    return new WatchResponse(Cycle.Stop);
                });
            }

            if (!config.Enabled || !job.Environment.Enabled)
            {
                return true;
            }

            var ipAddressesInvalid = _ipAccessControlService.InitIpAccessControl(config.IpAccessControl);
            if (ipAddressesInvalid.HasValues())
            {
                JournalService.WriteAppJournal(
                    job.App.Key,
                    job.Environment.Key,
                    "Shield.BackofficeAccess.General_InvalidIpControlRules",
                    string.Join(", ", ipAddressesInvalid));
            }

            job.WatchWebRequests(PipeLineStages.AuthenticateRequest, onDiscUmbracoRegex, 20300, (count, httpApp) =>
            {
                if (_ipAccessControlService.IsRequestAuthenticatedUmbracoUser(httpApp)
                    || _ipAccessControlService.IsValid(config.IpAccessControl, httpApp.Context.Request)
                    || config.ExcludeUrls.Any(x => httpApp.Request.Url.AbsolutePath.StartsWith(x.EnsureStartsWith('/'), StringComparison.OrdinalIgnoreCase)))
                {
                    return new WatchResponse(Cycle.Continue);
                }

                JournalService.WriteAppJournal(
                    job.App.Key,
                    job.Environment.Key,
                    "Shield.BackofficeAccess.General_DeniedAccess",
                    httpApp.Context.Request.UserHostAddress);

                return new WatchResponse(config.TransferUrlControl);
            });

            return true;
        }

        private void SoftWatcher(
            IJob job,
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
                        return new WatchResponse(new TransferUrlControl
                        {
                            TransferType = TransferType.TransferRequest,
                            Url = new UmbracoUrl
                            {
                                Type = UmbracoUrlType.Url,
                                Value = rewritePath + httpApp.Request.Url.Query
                            }
                        });
                    }

                    return new WatchResponse(new TransferUrlControl
                    {
                        TransferType = TransferType.TransmitFile,
                        Url = new UmbracoUrl
                        {
                            Type = UmbracoUrlType.Url,
                            Value = rewritePath
                        }
                    });
                }

                httpApp.Context.Items.Add(_allowKey, true);
                rewritePath += httpApp.Request.Url.Query;

                return new WatchResponse(new TransferUrlControl
                {
                    TransferType = rewrite ? TransferType.Rewrite : TransferType.Redirect,
                    Url = new UmbracoUrl
                    {
                        Type = UmbracoUrlType.Url,
                        Value = rewritePath
                    }
                });
            });
        }
    }
}