using Our.Shield.Core.Attributes;
using Our.Shield.Core.Models;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;

namespace Our.Shield.GoogleSafeBrowsing.Models
{
    [AppEditor("/App_Plugins/Shield.GoogleSafeBrowsing/Views/GoogleSafeBrowsing.html?version=1.0.3")]
    public class GoogleSafeBrowsingApp : App<GoogleSafeBrowsingConfiguration>
    {
        /// <summary>
        /// 
        /// </summary>
        public override string Description => "Stops the content editors from linking to dangerous domains";

        /// <summary>
        /// 
        /// </summary>
        public override string Icon => "icon-alert red";

        /// <summary>
        /// 
        /// </summary>
        public override string Id => nameof(GoogleSafeBrowsing);

        /// <summary>
        /// 
        /// </summary>
        public override string Name => "Google Safe Browsing";

        /// <summary>
        /// 
        /// </summary>
        public override IConfiguration DefaultConfiguration
        {
            get
            {
                return new GoogleSafeBrowsingConfiguration();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public override bool Execute(IJob job, IConfiguration c)
        {
            ContentService.Saving -= ContentService_Saving;
            ContentService.Publishing -= ContentService_Publishing;

            if (!c.Enable)
            {
                return true;
            }

            ContentService.Saving += ContentService_Saving;
            ContentService.Publishing += ContentService_Publishing;

            return true;
        }

        private void ContentService_Publishing(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            
        }

        private void ContentService_Saving(IContentService sender, SaveEventArgs<IContent> e)
        {
            
        }
    }
}
