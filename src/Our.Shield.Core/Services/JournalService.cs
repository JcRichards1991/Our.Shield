using Our.Shield.Core.Data.Accessors;
using Our.Shield.Core.Enums;
using Our.Shield.Core.Models;
using Our.Shield.Shared;
using System;
using System.Threading.Tasks;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Our.Shield.Core.Services
{
    /// <summary>
    /// Implements <see cref="IJournalService"/>
    /// </summary>
    public class JournalService : IJournalService
    {
        private readonly IJournalAccessor _journalAccessor;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        /// <summary>
        /// Initializes a new instance of <see cref="JournalService"/>
        /// </summary>
        /// <param name="journalAcccessor"></param>
        /// <param name="localizedTextService"></param>
        /// <param name="umbracoContextAccessor"></param>
        public JournalService(
            IJournalAccessor journalAcccessor,
            ILocalizedTextService localizedTextService,
            IUmbracoContextAccessor umbracoContextAccessor)
        {
            GuardClauses.NotNull(journalAcccessor, nameof(journalAcccessor));
            GuardClauses.NotNull(localizedTextService, nameof(localizedTextService));

            _journalAccessor = journalAcccessor;
            _localizedTextService = localizedTextService;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <inheritdoc />
        public async Task WriteEnvironmentJournal(
            string environmentName,
            Guid environmentKey,
            JournalEnvironmentAction action)
        {
            using (var umbContext = _umbracoContextAccessor.UmbracoContext)
            {
                var user = umbContext.Security.CurrentUser;
                var localizedMessage = _localizedTextService.Localize(
                    $"Shield.General/EnvironmentMessage",
                    new[]
                    {
                        user.Name,
                        action.ToString(),
                        environmentName
                    });

                await WriteJournal(
                    localizedMessage,
                    environmentKey);
            }
        }

        /// <inheritdoc />
        public async Task WriteAppJournal(
            Guid appKey,
            Guid environmentKey,
            string localizedTextKey,
            params string[] localizedTextTokens)
        {
            var localizedMessage = _localizedTextService.Localize(
                    localizedTextKey,
                    localizedTextTokens);

            await WriteJournal(
                localizedMessage,
                environmentKey,
                appKey);
        }

        /// <inheritdoc />
        public async Task WriteAppUpdateJournal(string appId, Guid appKey, Guid environmentKey)
        {
            using (var umbContext = _umbracoContextAccessor.UmbracoContext)
            {
                var user = umbContext.Security.CurrentUser;
                var appLocalisedName = _localizedTextService.Localize($"Shield.{appId}", "Name");

                var localizedMessage = _localizedTextService.Localize(
                    $"Shield.General/UpdateAppConfigurationMessage",
                    new[]
                    {
                        user.Name,
                        appLocalisedName
                    });

                await WriteJournal(
                    localizedMessage,
                    environmentKey,
                    appKey);
            }
        }

        private async Task<bool> WriteJournal(
            string message,
            Guid environmentKey,
            Guid? appKey = null) => await _journalAccessor.Write(
                new Journal
                {
                    Message = message,
                    EnvironmentKey = environmentKey,
                    AppKey = appKey
                });
    }
}
