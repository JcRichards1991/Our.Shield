using Our.Shield.BackofficeAccess.Models;
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
using Umbraco.Web;

namespace Our.Shield.EnvironmentLocker.Models
{
    [AppEditor("/App_Plugins/Shield.EnvironmentLocker/Views/EnvironmentLocker.html?version=1.0.2")]
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
        public override string Id => nameof(EnvironmentLocker);

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
                string url = null;
                var journalMessage = new JournalMessage();
                var umbContext = UmbracoContext.Current;

                switch (config.UnauthorisedUrlType)
                {
                    case Enums.UnautorisedUrlType.Url:
                        if (!string.IsNullOrEmpty(config.UnauthorisedUrl))
                        {
                            url = config.UnauthorisedUrl;
                            break;
                        }

                        journalMessage.Message = "Error: No Unauthorized URL set in configuration";
                        break;

                    case Enums.UnautorisedUrlType.XPath:
                        var xpathNode = umbContext.ContentCache.GetSingleByXPath(config.UnauthorisedUrlXPath);

                        if (xpathNode != null)
                        {
                            url = xpathNode.Url;
                            break;
                        }

                        journalMessage.Message = "Error: Unable to get the unauthorized URL from the specified XPath expression";
                        break;

                    case Enums.UnautorisedUrlType.ContentPicker:
                        int id;

                        if (int.TryParse(config.UnauthorisedUrlContentPicker, out id))
                        {
                            var contentPickerNode = umbContext.ContentCache.GetById(id);

                            if (contentPickerNode != null)
                            {
                                url = contentPickerNode.Url;
                                break;
                            }

                            journalMessage.Message = "Error: Unable to get the unauthorized URL from the unauthorized URL content picker. Please ensure the selected page is published and not deleted";
                            break;
                        }

                        journalMessage.Message = "Error: Unable to parse the selected unauthorized URL content picker content. Please ensure a valid content node is selected";
                        break;

                    default:
                        journalMessage.Message = "Error: Unable to determine which method to use to get the unauthorized URL. Please ensure URL, XPath or Content Picker is selected";
                        break;
                }

                if (url == null)
                {
                    if (!string.IsNullOrEmpty(journalMessage.Message))
                    {
                        job.WriteJournal(journalMessage);
                    }

                    return null;
                }

                return url;
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

                    var url = UnauthorisedUrl(job, config);

                    if (url == null)
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
