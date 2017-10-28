using Our.Shield.Core;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.Helpers;
using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core;
using System.Linq;

namespace Our.Shield.FrontendAccess.Models
{
    [AppEditor("/App_Plugins/Shield.FrontendAccess/Views/FrontendAccess.html?version=1.0.4")]
    public class FrontendAccessApp : App<FrontendAccessConfiguration>
    {
        /// <summary>
        /// 
        /// </summary>
        public override string Description => "Lock down the frontend to only be viewed by Umbraco Authenticated Users and/or secure the frontend via IP restrictions";

        /// <summary>
        /// 
        /// </summary>
        public override string Icon => "icon-combination-lock red";

        /// <summary>
        /// 
        /// </summary>
        public override string Id => nameof(FrontendAccess);

        /// <summary>
        /// 
        /// </summary>
        public override string Name => "Frontend Access";

        /// <summary>
        /// 
        /// </summary>
        public override IConfiguration DefaultConfiguration
        {
            get
            {
                return new FrontendAccessConfiguration
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
            }
        }

        private readonly string AllowKey = Guid.NewGuid().ToString();

        private readonly TimeSpan CacheLength = new TimeSpan(TimeSpan.TicksPerDay);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public override bool Execute(IJob job, IConfiguration c)
        {
            job.UnwatchWebRequests();
            job.UnexceptionWebRequest();

            if (!c.Enable || !job.Environment.Enable)
            {
                return true;
            }

            var config = c as FrontendAccessConfiguration;
            var hardUmbracoLocation = ApplicationSettings.UmbracoPath;
            var regex = new Regex("^/$|^(/(?!" + hardUmbracoLocation.Trim('/') + ")[\\w-/_]+?)$", RegexOptions.IgnoreCase);

            foreach (var error in new IpAccessControlService().InitIpAccessControl(config.IpAccessRules))
            {
                job.WriteJournal(new JournalMessage($"Error: Invalid IP Address {error}, unable to add to exception list"));
            }
			job.ExceptionWebRequest(config.Unauthorized.Url);
            job.WatchWebRequests(PipeLineStages.AuthenticateRequest, regex, 10000, (count, httpApp) =>
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
