using System;
using System.Linq;
using System.Text.RegularExpressions;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.Helpers;
using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using Umbraco.Core;

namespace Our.Shield.SiteMaintenance.Models
{
    [AppEditor("/App_Plugins/Shield.SiteMaintenance/Views/FrontendAccess.html?version=1.0.4")]
    public class SiteMaintenanceApp : App<SiteMaintenanceConfiguration>
    {
        /// <summary>
        /// 
        /// </summary>
        public override string Description => "Display a \"Maintenance\" or \"Under Construction\" page to all unauthorised traffic";

        /// <summary>
        /// 
        /// </summary>
        public override string Icon => "icon-combination-lock blue";

        /// <summary>
        /// 
        /// </summary>
        public override string Id => nameof(SiteMaintenance);

        /// <summary>
        /// 
        /// </summary>
        public override string Name => "Site Maintenance";
        
        /// <summary>
        /// </summary>
        public override IConfiguration DefaultConfiguration => new SiteMaintenanceConfiguration
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

        private readonly string _allowKey = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public override bool Execute(IJob job, IConfiguration c)
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

            foreach (var error in new IpAccessControlService().InitIpAccessControl(config.IpAccessRules))
            {
                job.WriteJournal(new JournalMessage($"Error: Invalid IP Address {error}, unable to add to exception list"));
            }

            job.WatchWebRequests(PipeLineStages.AuthenticateRequest, regex, 5000, (count, httpApp) =>
            {
                if ((config.UmbracoUserEnable && !AccessHelper.IsRequestAuthenticatedUmbracoUser(httpApp))
                    || !new IpAccessControlService().IsValid(config.IpAccessRules, httpApp.Context.Request.UserHostAddress))
                {
                    return new WatchResponse(config.Unauthorized);
                }

                return new WatchResponse(WatchResponse.Cycles.Continue);
            });

            return true;
        }
    }
}
