using Our.Shield.Core;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Security;

namespace Our.Shield.FrontendLocker.Models
{
    [AppEditor("/App_Plugins/Shield.FrontendLocker/Views/FrontendLocker.html?version=1.0.2")]
    public class FrontendLockerApp : App<FrontendLockerConfiguration>
    {
        /// <summary>
        /// 
        /// </summary>
        public override string Description => "Locks down the frontend to be viewed only by users that have authenticated as an Umbraco User";

        /// <summary>
        /// 
        /// </summary>
        public override string Icon => "icon-combination-lock red";

        /// <summary>
        /// 
        /// </summary>
        public override string Id => nameof(FrontendLocker);

        /// <summary>
        /// 
        /// </summary>
        public override string Name => "Frontend Locker";

        /// <summary>
        /// 
        /// </summary>
        public override IConfiguration DefaultConfiguration
        {
            get
            {
                return new FrontendLockerConfiguration
                {
                    UnauthorisedAction = Enums.UnauthorisedAction.Redirect,
                    UnauthorisedUrl = "/403"
                };
            }
        }

        private readonly string allowKey = Guid.NewGuid().ToString();

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

            var config = c as FrontendLockerConfiguration;
            var hardUmbracoLocation = ApplicationSettings.UmbracoPath;
            var regex = new Regex("^($|(/(?!(umbraco|" + config.UnauthorisedUrl.Trim('/') + "|" + hardUmbracoLocation.Trim('/') + "))([\\w-/_]+)?)$)", RegexOptions.IgnoreCase);

            job.WatchWebRequests(regex, 75, (count, httpApp) =>
            {
                if ((bool?)httpApp.Context.Items[allowKey] == true)
                {
                    return WatchCycle.Continue;
                }

                var httpContext = new HttpContextWrapper(httpApp.Context);
                var umbAuthTicket = httpContext.GetUmbracoAuthTicket();

                if(!httpContext.AuthenticateCurrentRequest(umbAuthTicket, true))
                {
                    httpApp.Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                    if (config.UnauthorisedUrl == null)
                    {
                        return WatchCycle.Stop;
                    }

                    if (config.UnauthorisedAction == Enums.UnauthorisedAction.Rewrite)
                    {
                        httpApp.Context.RewritePath(config.UnauthorisedUrl);
                        return WatchCycle.Restart;
                    }

                    httpApp.Context.Response.Redirect(config.UnauthorisedUrl, true);
                    return WatchCycle.Stop;
                }

                httpApp.Context.Items.Add(allowKey, true);

                return WatchCycle.Continue;
            });

            return true;
        }
    }
}
