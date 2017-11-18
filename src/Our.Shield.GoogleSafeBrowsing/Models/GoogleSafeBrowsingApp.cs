using Our.Shield.Core.Attributes;
using Our.Shield.Core.Models;
using Umbraco.Core.Services;

namespace Our.Shield.GoogleSafeBrowsing.Models
{
    [AppEditor("/App_Plugins/Shield.GoogleSafeBrowsing/Views/GoogleSafeBrowsing.html?version=1.0.4")]
    public class GoogleSafeBrowsingApp : App<GoogleSafeBrowsingConfiguration>
    {
        /// <inheritdoc />
        public override string Description => "Stops the content editors from linking to dangerous domains";

        /// <inheritdoc />
        public override string Icon => "icon-alert red";

        /// <inheritdoc />
        public override string Id => nameof(GoogleSafeBrowsing);

        /// <inheritdoc />
        public override string Name => "Google Safe Browsing";

        /// <inheritdoc />
        public override IConfiguration DefaultConfiguration => new GoogleSafeBrowsingConfiguration();

        /// <inheritdoc />
        public override bool Execute(IJob job, IConfiguration c)
        {
            if (!(c is GoogleSafeBrowsingConfiguration config))
            {
                job.WriteJournal(new JournalMessage("Error: Config passed into Google safe Browsing was not of the correct type"));
                return false;
            }

            ContentService.Saving += (sender, e) =>
            {
                if (!config.Enable)
                    return;
            };

            ContentService.Publishing += (sender, e) =>
            {
                if (!config.Enable)
                    return;
            };

            return true;
        }
    }
}
