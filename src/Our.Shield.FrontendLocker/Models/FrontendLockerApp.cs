using Our.Shield.BackofficeAccess.Models;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Web;

namespace Our.Shield.FrontendLocker.Models
{
    [AppEditor("/App_Plugins/Shield.FrontendLocker/Views/FrontendLocker.html?version=1.0.2")]
    public class EnvironmentLockerApp : App<EnvironmentLockerConfiguration>
    {
        /// <summary>
        /// 
        /// </summary>
        public override string Description => "Stops your content editors from linking to dangerous domains";

        /// <summary>
        /// 
        /// </summary>
        public override string Icon => "";

        /// <summary>
        /// 
        /// </summary>
        public override string Id => nameof(FrontendLocker);

        /// <summary>
        /// 
        /// </summary>
        public override string Name => "Environment Locker";

        /// <summary>
        /// 
        /// </summary>
        public override IConfiguration DefaultConfiguration => new EnvironmentLockerConfiguration();

        private readonly string allowKey = Guid.NewGuid().ToString();
        private readonly TimeSpan CacheLength = new TimeSpan(TimeSpan.TicksPerDay);        //   Once per day

        private string UnauthorisedUrl(IJob job, EnvironmentLockerConfiguration config)
        {
            return ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(allowKey, () =>
            {
                if (string.IsNullOrEmpty(config.UnauthorisedUrl))
                {
                    job.WriteJournal(new JournalMessage("Error: No Unauthorized URL set in configuration"));
                    return null;
                }

                return config.UnauthorisedUrl;
            }, CacheLength) as string;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public override bool Execute(IJob job, IConfiguration c)
        {
            job.UnwatchWebRequests();

            if (!c.Enable || !job.Environment.Enable)
            {
                return true;
            }

            var config = c as EnvironmentLockerConfiguration;
            var backofficeAccessConfig = job.ReadConfiguration(1, nameof(BackofficeAccess)) as BackofficeAccessConfiguration;

            job.WatchWebRequests(new Regex("^/(?!(umbraco|" + backofficeAccessConfig.BackendAccessUrl.Trim('/') + "))[\\w-_/]+)$"), 75, (count, httpApp) => {
                var httpContext = new HttpContextWrapper(httpApp.Context);
                var umbAuthTicket = httpContext.GetUmbracoAuthTicket();

                if(!httpContext.AuthenticateCurrentRequest(umbAuthTicket, true))
                {
                    httpApp.Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                    if (UnauthorisedUrl(job, config) == null)
                    {
                        return WatchCycle.Stop;
                    }

                    return WatchCycle.Stop;
                }

                return WatchCycle.Continue;
            });

            return true;
        }
    }
}
