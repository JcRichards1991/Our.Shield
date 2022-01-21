using Our.Shield.Core.Attributes;
using Our.Shield.Core.Enums;
using Our.Shield.Core.Helpers;
using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using Our.Shield.Core.Services;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Services;

namespace Our.Shield.FrontendAccess.Models
{
    [AppEditor("/App_Plugins/Shield.FrontendAccess/Views/FrontendAccess.html?version=2.0.0")]
    [AppJournal]
    public class FrontendAccessApp : App<FrontendAccessConfiguration>
    {
        private readonly IpAccessControlService _ipAccessControlService;

        public FrontendAccessApp(
            ILocalizedTextService localizedTextService,
            IpAccessControlService ipAccessControlService) : base(localizedTextService)
        {
            _ipAccessControlService = ipAccessControlService;
        }

        /// <inheritdoc />
        public override string Id => nameof(FrontendAccess);

        /// <inheritdoc />
        public override string Name => LocalizedTextService.Localize("Shield.FrontendAccess.General/Name", CultureInfo.CurrentCulture);

        /// <inheritdoc />
        public override string Description => LocalizedTextService.Localize("Shield.FrontendAccess.General/Description", CultureInfo.CurrentCulture);

        /// <inheritdoc />
        public override string Icon => "icon-combination-lock red";

        /// <inheritdoc />
        public override IAppConfiguration DefaultConfiguration => new FrontendAccessConfiguration
        {
            UmbracoUserEnable = true,
            IpAccessRules = new IpAccessControl
            {
                AccessType = IpAccessControl.AccessTypes.AllowAll,
                Exceptions = Enumerable.Empty<IpAccessControl.Entry>()
            },
            Unauthorized = new TransferUrl
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
            job.UnwatchWebRequests();
            job.UnexceptionWebRequest();

            if (!(c is FrontendAccessConfiguration config))
            {
                job.WriteJournal(new JournalMessage("Error: Config passed into Frontend Access was not of the correct type"));
                return false;
            }

            if (!c.Enable || !job.Environment.Enabled)
            {
                return true;
            }

            foreach (var error in _ipAccessControlService.InitIpAccessControl(config.IpAccessRules))
            {
                job.WriteJournal(new JournalMessage($"Error: Invalid IP Address {error}, unable to add to exception list"));
            }

            if (config.Unauthorized.TransferType != TransferType.PlayDead)
            {
                job.ExceptionWebRequest(config.Unauthorized.Url);
            }

            var regex = new Regex(@"^/([a-z0-9-_~&\+%/])*(\?([^\?])*)?$", RegexOptions.IgnoreCase);

            job.WatchWebRequests(PipeLineStages.AuthenticateRequest, regex, 400000, (count, httpApp) =>
            {
                if (_ipAccessControlService.IsValid(config.IpAccessRules, httpApp.Context.Request))
                {
                    httpApp.Context.Items.Add(_allowKey, true);
                }
                return new WatchResponse(Cycle.Continue);
            });

            job.WatchWebRequests(PipeLineStages.AuthenticateRequest, regex, 400500, (count, httpApp) =>
            {
                if ((bool?)httpApp.Context.Items[_allowKey] == true
                    || (config.UmbracoUserEnable && AccessHelper.IsRequestAuthenticatedUmbracoUser(httpApp)))
                {
                    return new WatchResponse(Cycle.Continue);
                }

                var url = new UmbracoUrlService().Url(config.Unauthorized.Url);
                if (url == null)
                {
                    return new WatchResponse(Cycle.Error);
                }

                if (httpApp.Context.Request.Url.LocalPath.Equals(url))
                {
                    return new WatchResponse(Cycle.Continue);
                }

                job.WriteJournal(new JournalMessage($"User with IP Address: {httpApp.Context.Request.UserHostAddress}; tried to access Page: {httpApp.Context.Request.Url}. Access was denied"));
                return new WatchResponse(config.Unauthorized);
            });

            return true;
        }
    }
}
