using Our.Shield.Core.Attributes;
using Our.Shield.Core.Enums;
using Our.Shield.Core.Models;
using Our.Shield.Core.Services;
using Our.Shield.Shared.Extensions;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Our.Shield.FrontendAccess.Models
{
    /// <summary>
    /// 
    /// </summary>
    [AppEditor("/App_Plugins/Shield.FrontendAccess/Views/FrontendAccess.html?version=2.0.0")]
    public class FrontendAccessApp : App<FrontendAccessConfiguration>
    {
        private readonly IIpAccessControlService _ipAccessControlService;

        /// <summary>
        /// Initializes a new instance of <see cref="FrontendAccessApp"/>
        /// </summary>
        /// <param name="umbContextAccessor"><see cref="IUmbracoContextAccessor"/></param>
        /// <param name="localizedTextService"><see cref="ILocalizedTextService"/></param>
        /// <param name="logger"><see cref="ILogger"/></param>
        /// <param name="ipAccessControlService"><see cref="IIpAccessControlService"/></param>
        public FrontendAccessApp(
            IUmbracoContextAccessor umbContextAccessor,
            ILocalizedTextService localizedTextService,
            ILogger logger,
            IIpAccessControlService ipAccessControlService)
            : base(umbContextAccessor, localizedTextService, logger)
        {
            _ipAccessControlService = ipAccessControlService;
        }

        /// <inheritdoc />
        public override string Id => nameof(FrontendAccess);

        /// <inheritdoc />
        public override string Icon => "icon-combination-lock red";

        /// <inheritdoc />
        public override IAppConfiguration DefaultConfiguration => new FrontendAccessConfiguration
        {
            UmbracoUserEnable = true,
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

        private readonly string _allowKey = Guid.NewGuid().ToString();

        /// <inheritdoc />
        public override bool Execute(IJob job, IAppConfiguration c)
        {
            if (!(c is FrontendAccessConfiguration config))
            {
                return false;
            }

            job.UnwatchWebRequests();
            job.UnexceptionWebRequest();

            if (!c.Enabled || !job.Environment.Enabled)
            {
                return true;
            }

            var ipAddressesInvalid = _ipAccessControlService.InitIpAccessControl(config.IpAccessControl);
            if (ipAddressesInvalid.HasValues())
            {
                using (var umbContext = UmbContextAccessor.UmbracoContext)
                {
                    var localizedAppName = LocalizedTextService.Localize($"{nameof(Shield)}.{nameof(FrontendAccess)}", "Name");
                    var localizedMessage = LocalizedTextService.Localize(
                    $"{nameof(Shield)}.General_InvalidIpControlRules",
                    new[]
                    {
                        string.Join(", ", ipAddressesInvalid),
                        localizedAppName,
                        job.Environment.Name,
                    });

                    Logger.Warn<FrontendAccessApp>(
                        localizedMessage + "App Key: {AppKey}; Environment Key: {EnvironmentKey}",
                        job.App.Key,
                        job.Environment.Key);
                }
            }

            if (config.TransferUrlControl.TransferType != TransferType.PlayDead)
            {
                job.ExceptionWebRequest(config.TransferUrlControl.Url);
            }

            var regex = new Regex(@"^/([a-z0-9-_~&\+%/])*(\?([^\?])*)?$", RegexOptions.IgnoreCase);

            job.WatchWebRequests(PipeLineStages.AuthenticateRequest, regex, 400000, (count, httpApp) =>
            {
                if (_ipAccessControlService.IsValid(config.IpAccessControl, httpApp.Context.Request))
                {
                    httpApp.Context.Items.Add(_allowKey, true);
                }
                return new WatchResponse(Cycle.Continue);
            });

            job.WatchWebRequests(PipeLineStages.AuthenticateRequest, regex, 400500, (count, httpApp) =>
            {
                if ((bool?)httpApp.Context.Items[_allowKey] == true
                    || (config.UmbracoUserEnable && _ipAccessControlService.IsRequestAuthenticatedUmbracoUser(httpApp)))
                {
                    return new WatchResponse(Cycle.Continue);
                }

                using (var umbContext = UmbContextAccessor.UmbracoContext)
                {
                    var localizedAppName = LocalizedTextService.Localize($"{nameof(Shield)}.{nameof(FrontendAccess)}", "Name");
                    var localizedMessage = LocalizedTextService.Localize(
                    $"{nameof(Shield)}.{nameof(FrontendAccess)}_DeniedAccess",
                    new[]
                    {
                        httpApp.Context.Request.UserHostAddress,
                        job.Environment.Name
                    });

                    Logger.Warn<FrontendAccessApp>(
                        localizedMessage + "App Key: {AppKey}; Environment Key: {EnvironmentKey}",
                        job.App.Key,
                        job.Environment.Key);
                }

                return new WatchResponse(config.TransferUrlControl);
            });

            return true;
        }
    }
}
