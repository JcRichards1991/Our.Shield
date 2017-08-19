using Our.Shield.Core.Attributes;
using Our.Shield.Core.Models;

namespace Our.Shield.ScraperDefense.Models
{
    [AppEditor("/App_Plugins/Shield.ScraperDefense/Views/ScraperDefense.html?version=1.0.3")]
    public class ScraperDefenseApp : App<ScraperDefenseConfiguration>
    {
        public override string Description => "Protect your site from being scraped by 3rd party applications/websites";

        public override string Icon => "";

        public override string Id => nameof(ScraperDefense);

        public override string Name => "Scraper Defense";

        public override IConfiguration DefaultConfiguration
        {
            get
            {
                return new ScraperDefenseConfiguration();
            }
        }

        public override bool Execute(IJob job, IConfiguration c)
        {
            job.UnwatchWebRequests();

            if(!c.Enable)
            {
                return true;
            }

            return true;
        }
    }
}
