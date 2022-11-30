using Our.Shield.Core.Attributes;
using Our.Shield.Core.Enums;
using Our.Shield.Core.Models;
using Our.Shield.Core.Services;
using Our.Shield.Shared.Extensions;
using System;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Our.Shield.Swagger.Models
{
    [AppEditor("/App_Plugins/Shield.Swagger/Views/Swagger.html?version=2.0.0")]
    public class SwaggerApp : App<SwaggerConfiguration>
    {
        private readonly string _allowKey = Guid.NewGuid().ToString();

        private readonly IIpAccessControlService _ipAccessControlService;

        public SwaggerApp(
            IUmbracoContextAccessor umbContextAccessor,
            ILocalizedTextService localizedTextService,
            ILogger logger,
            IIpAccessControlService ipAccessControlService)
            : base(umbContextAccessor, localizedTextService, logger)
        {
            _ipAccessControlService = ipAccessControlService;
        }

        /// <inheritdoc />
        public override string Id => nameof(Swagger);

        /// <inheritdoc />
        public override string Icon => "icon-gps orange";

        /// <inheritdoc />
        public override IAppConfiguration DefaultConfiguration => new SwaggerConfiguration
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

        /// <inheritdoc />
        public override bool Execute(IJob job, IAppConfiguration c)
        {
            if (!(c is SwaggerConfiguration config))
            {
                return false;
            }

            job.UnwatchWebRequests();
            job.UnexceptionWebRequest();
            job.UnignoreWebRequest();

            var regex = job.PathToRegex("swagger");
            job.IgnoreWebRequest(regex);

            if (!c.Enabled || !job.Environment.Enabled)
            {
                job.WatchWebRequests(PipeLineStages.AuthenticateRequest, regex, 500000, (count, httpApp) => new WatchResponse(Cycle.Error));

                return true;
            }

            var ipAddressesInvalid = _ipAccessControlService.InitIpAccessControl(config.IpAccessControl);
            if (ipAddressesInvalid.HasValues())
            {
                using (var umbContext = UmbContextAccessor.UmbracoContext)
                {
                    var localizedAppName = LocalizedTextService.Localize($"{nameof(Shield)}.{nameof(Swagger)}", "Name");
                    var localizedMessage = LocalizedTextService.Localize(
                    $"{nameof(Shield)}.General_InvalidIpControlRules",
                    new[]
                    {
                        string.Join(", ", ipAddressesInvalid),
                        localizedAppName,
                        job.Environment.Name,
                    });

                    Logger.Warn<SwaggerApp>(
                        localizedMessage + "App Key: {AppKey}; Environment Key: {EnvironmentKey}",
                        job.App.Key,
                        job.Environment.Key);
                }
            }

            if (config.TransferUrlControl.TransferType != TransferType.PlayDead)
            {
                job.ExceptionWebRequest(config.TransferUrlControl.Url);
            }

            job.WatchWebRequests(PipeLineStages.AuthenticateRequest, regex, 500250, (count, httpApp) =>
            {
                if (_ipAccessControlService.IsValid(config.IpAccessControl, httpApp.Context.Request))
                {
                    httpApp.Context.Items.Add(_allowKey, true);
                }

                return new WatchResponse(Cycle.Continue);
            });

            job.WatchWebRequests(PipeLineStages.AuthenticateRequest, regex, 500500, (count, httpApp) =>
            {
                if ((bool?)httpApp.Context.Items[_allowKey] == true
                  || (config.UmbracoUserEnable && _ipAccessControlService.IsRequestAuthenticatedUmbracoUser(httpApp)))
                {
                    return new WatchResponse(Cycle.Continue);
                }

                using (var umbContext = UmbContextAccessor.UmbracoContext)
                {
                    var localizedAppName = LocalizedTextService.Localize($"{nameof(Shield)}.{nameof(Swagger)}", "Name");
                    var localizedMessage = LocalizedTextService.Localize(
                    $"{nameof(Shield)}.{nameof(Swagger)}_DeniedAccess",
                    new[]
                    {
                        httpApp.Context.Request.UserHostAddress,
                        job.Environment.Name
                    });

                    Logger.Warn<SwaggerApp>(
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