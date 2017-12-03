using System;
using System.Linq;
using System.Text.RegularExpressions;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.Helpers;
using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using Our.Shield.Elmah.Attributes;
using Umbraco.Core;
using System.Globalization;

namespace Our.Shield.Elmah.Models
{
    [ReportingTab]
    [AppEditor("/App_Plugins/Shield.Elmah/Views/Elmah.html?version=1.0.4", sortOrder: 1)]
    [AppJournal(sortOrder: 2)]
    public class ElmahApp : App<ElmahConfiguration>
    {
        private readonly string _allowKey = Guid.NewGuid().ToString();

        /// <inheritdoc />
        public override string Id => nameof(Elmah);

        /// <inheritdoc />
        public override string Name =>
            ApplicationContext.Current.Services.TextService.Localize("Shield.Elmah.General/Name", CultureInfo.CurrentCulture);

        /// <inheritdoc />
        public override string Description => ApplicationContext.Current.Services.TextService.Localize("Shield.Elmah.General/Description", CultureInfo.CurrentCulture);

        /// <inheritdoc />
        public override string Icon => "icon-combination-lock red";
        /// <inheritdoc />
        public override IConfiguration DefaultConfiguration => new ElmahConfiguration
        {
            UmbracoUserEnable = true,
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
        public ElmahApp()
        {
            _ipAccessControlService = new IpAccessControlService();
        }

        public override bool Execute(IJob job, IConfiguration c)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(_allowKey);
            job.UnwatchWebRequests();

            if (!c.Enable || !job.Environment.Enable)
                return true;

            if (!(c is ElmahConfiguration config))
            {
                job.WriteJournal(new JournalMessage("Error: Config passed into Elmah was not of the correct type"));
                return false;
            }

            foreach (var error in new IpAccessControlService().InitIpAccessControl(config.IpAccessRules))
                job.WriteJournal(
                    new JournalMessage($"Error: Invalid IP Address {error}, unable to add to exception list"));

            job.WatchWebRequests(PipeLineStages.AuthenticateRequest, new Regex("^/elmah.axd$", RegexOptions.IgnoreCase), 400000, (count, httpApp) =>
            {
                if (config.UmbracoUserEnable && !AccessHelper.IsRequestAuthenticatedUmbracoUser(httpApp)
                    || !_ipAccessControlService.IsValid(config.IpAccessRules,
                        httpApp.Context.Request.UserHostAddress))
                    return new WatchResponse(config.Unauthorized);

                return new WatchResponse(WatchResponse.Cycles.Continue);
            });

            return true;
        }
    }
}