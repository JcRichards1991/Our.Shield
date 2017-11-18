using Our.Shield.Core.Attributes;
using Our.Shield.Core.Models;

namespace Our.Shield.ScraperDefense.Models
{
    [AppEditor("/App_Plugins/Shield.ScraperDefense/Views/ScraperDefense.html?version=1.0.4")]
    public class ScraperDefenseApp : App<ScraperDefenseConfiguration>
    {
        /// <inheritdoc />
        public override string Description => "Protect your site from being scraped by 3rd party applications/websites";

        /// <inheritdoc />
        public override string Icon => "";

        /// <inheritdoc />
        public override string Id => nameof(ScraperDefense);

        /// <inheritdoc />
        public override string Name => "Scraper Defense";

        /// <inheritdoc />
        public override IConfiguration DefaultConfiguration => new ScraperDefenseConfiguration();

        /// <inheritdoc />
        public override bool Execute(IJob job, IConfiguration c)
        {
            job.UnwatchWebRequests();

            if (!(c is ScraperDefenseConfiguration config))
            {
                job.WriteJournal(new JournalMessage("Error: Config passed into Scraper Defense was not of the correct type"));
                return false;
            }

            if (!config.Enable || !job.Environment.Enable)
            {
                return true;
            }

            return true;
        }
    }
}
