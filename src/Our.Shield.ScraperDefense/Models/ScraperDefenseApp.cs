using Our.Shield.Core.Attributes;
using Our.Shield.Core.Models;
using System.Globalization;
using Umbraco.Core;

namespace Our.Shield.ScraperDefense.Models
{
    [AppEditor("/App_Plugins/Shield.ScraperDefense/Views/ScraperDefense.html?version=1.0.6")]
    [AppJournal]
    public class ScraperDefenseApp : App<ScraperDefenseConfiguration>
    {
        /// <inheritdoc />
        public override string Id => nameof(ScraperDefense);

        /// <inheritdoc />
        public override string Name =>
            ApplicationContext.Current.Services.TextService.Localize("Shield.ScraperDefense.General/Name", CultureInfo.CurrentCulture);

        /// <inheritdoc />
        public override string Description =>
            ApplicationContext.Current.Services.TextService.Localize("Shield.ScraperDefense.General/Description", CultureInfo.CurrentCulture);

        /// <inheritdoc />
        public override string Icon => "";

        /// <inheritdoc />
        public override IAppConfiguration DefaultConfiguration => new ScraperDefenseConfiguration();

        /// <inheritdoc />
        public override bool Execute(IJob job, IAppConfiguration c)
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
