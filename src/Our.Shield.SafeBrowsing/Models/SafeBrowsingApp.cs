using Our.Shield.Core.Attributes;
using Our.Shield.Core.Models;

namespace Our.Shield.SafeBrowsing.Models
{
    [AppEditor("/App_Plugins/Shield.BackofficeAccess/Views/SafeBrowsing.html?version=1.0.2")]
    public class SafeBrowsingApp : App<SafeBrowsingConfiguration>
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
        public override string Id => nameof(SafeBrowsing);

        /// <summary>
        /// 
        /// </summary>
        public override string Name => "Safe Browsing";

        /// <summary>
        /// 
        /// </summary>
        public override IConfiguration DefaultConfiguration => new SafeBrowsingConfiguration();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public override bool Execute(IJob job, IConfiguration c)
        {
            if(!c.Enable)
            {
                return true;
            }

            var config = c as SafeBrowsingConfiguration;

            return true;
        }
    }
}
