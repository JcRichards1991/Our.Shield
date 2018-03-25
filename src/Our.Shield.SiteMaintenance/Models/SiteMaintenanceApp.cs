using Our.Shield.Core.Attributes;
using Our.Shield.Core.Helpers;
using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using Our.Shield.Core.Services;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core;

namespace Our.Shield.SiteMaintenance.Models
{
    [AppEditor("/App_Plugins/Shield.SiteMaintenance/Views/SiteMaintenance.html?version=1.0.6")]
    [AppJournal]
    public class SiteMaintenanceApp : App<SiteMaintenanceConfiguration>
    {
        /// <inheritdoc />
        public override string Id => nameof(SiteMaintenance);

        /// <inheritdoc />
        public override string Name =>
            ApplicationContext.Current.Services.TextService.Localize("Shield.SiteMaintenance.General/Name", CultureInfo.CurrentCulture);

        /// <inheritdoc />
        public override string Description =>
            ApplicationContext.Current.Services.TextService.Localize("Shield.SiteMaintenance.General/Description", CultureInfo.CurrentCulture);

        /// <inheritdoc />
        public override string Icon =>
            "icon-combination-lock blue";

        /// <inheritdoc />
        public override IAppConfiguration DefaultConfiguration =>
            new SiteMaintenanceConfiguration
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
        public SiteMaintenanceApp()
        {
            _ipAccessControlService = new IpAccessControlService();
        }

        private readonly string _allowKey = Guid.NewGuid().ToString();
        
        /// <inheritdoc />
        public override bool Execute(IJob job, IAppConfiguration c)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(_allowKey);
            job.UnwatchWebRequests();

            if (!c.Enable || !job.Environment.Enable)
            {
                return true;
            }

            var config = c as SiteMaintenanceConfiguration
                ?? (SiteMaintenanceConfiguration)DefaultConfiguration;

            var hardUmbracoLocation = ApplicationSettings.UmbracoPath;
            var regex = new Regex("^/$|^(/(?!" + hardUmbracoLocation.Trim('/') + ")[\\w-/_]+?)$", RegexOptions.IgnoreCase);

            foreach (var error in _ipAccessControlService.InitIpAccessControl(config.IpAccessRules))
            {
                job.WriteJournal(new JournalMessage($"Error: Invalid IP Address {error}, unable to add to exception list"));
            }

            job.WatchWebRequests(PipeLineStages.AuthenticateRequest, regex, 5000, (count, httpApp) =>
            {
                if (config.UmbracoUserEnable && !AccessHelper.IsRequestAuthenticatedUmbracoUser(httpApp)
                    || !_ipAccessControlService.IsValid(config.IpAccessRules, httpApp.Context.Request))
                {
                    return new WatchResponse(config.Unauthorized);
                }

                return new WatchResponse(WatchResponse.Cycles.Continue);
            });

            return true;
        }
    }
}
