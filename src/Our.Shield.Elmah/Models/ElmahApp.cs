using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.Helpers;
using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using Umbraco.Core;

namespace Our.Shield.Elmah.Models
{
    [AppEditor("/App_Plugins/Shield.lmah/Views/Elmah.html?version=1.0.4")]
    public class ElmahApp : App<ElmahConfiguration>
    {
        private readonly string AllowKey = Guid.NewGuid().ToString();

        private readonly TimeSpan CacheLength = new TimeSpan(TimeSpan.TicksPerDay);

        /// <summary>
        /// </summary>
        public override string Description => "Lock down access to Elmah reporting page to only be viewed by Authenticated Umbraco Users and/or secure via IP restrictions";

        /// <summary>
        /// </summary>
        public override string Icon => "icon-combination-lock red";

        /// <summary>
        /// </summary>
        public override string Id => nameof(Elmah);

        /// <summary>
        /// </summary>
        public override string Name => Id;

        public override string[] Tabs => new [] {"Configuration", "Reporting", "Journal"};

        /// <summary>
        /// </summary>
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

        private bool IsRequestAllowed(HttpApplication httpApp, string url)
        {
            if (httpApp.Context.Request.Url.AbsolutePath.Equals(url) || (bool?) httpApp.Context.Items[AllowKey] == true)
                return true;

            return false;
        }

        public override bool Execute(IJob job, IConfiguration c)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(AllowKey);
            job.UnwatchWebRequests();

            if (!c.Enable || !job.Environment.Enable)
                return true;

            var config = c as ElmahConfiguration;
            var hardUmbracoLocation = ApplicationSettings.UmbracoPath;
            var regex = new Regex("^/$|^(/(?!" + hardUmbracoLocation.Trim('/') + ")[\\w-/_]+?)$",
                RegexOptions.IgnoreCase);

            foreach (var error in new IpAccessControlService().InitIpAccessControl(config.IpAccessRules))
                job.WriteJournal(
                    new JournalMessage($"Error: Invalid IP Address {error}, unable to add to exception list"));

            job.WatchWebRequests(PipeLineStages.AuthenticateRequest, regex, 400000, (count, httpApp) =>
            {
                if (config.UmbracoUserEnable && !AccessHelper.IsRequestAuthenticatedUmbracoUser(httpApp)
                    || !new IpAccessControlService().IsValid(config.IpAccessRules,
                        httpApp.Context.Request.UserHostAddress))
                    return new WatchResponse(config.Unauthorized);

                return new WatchResponse(WatchResponse.Cycles.Continue);
            });

            return true;
        }
    }
}