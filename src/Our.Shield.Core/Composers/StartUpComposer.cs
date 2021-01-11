using Our.Shield.Core.Components;
using Our.Shield.Core.Data.Accessors;
using Our.Shield.Core.Services;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Our.Shield.Core.Composers
{
    /// <summary>
    /// Initializes Shield's Start Up requirements
    /// </summary>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class StartUpComposer : IUserComposer
    {
        /// <inheritdoc/>
        public void Compose(Composition composition)
        {
            RegisterComponents(composition);
            RegisterDataAccessors(composition);
            RegisterServices(composition);
        }

        private void RegisterComponents(Composition composition)
        {
            composition.Components().Append<ClearCacheComponent>();
        }

        private void RegisterDataAccessors(Composition composition)
        {
            composition.RegisterFor<IEnvironmentAccessor, EnvironmentAccessor>(Lifetime.Singleton);
            composition.RegisterFor<IAppAccessor, AppAccessor>(Lifetime.Singleton);
            composition.RegisterFor<IJournalAccessor, JournalAccessor>(Lifetime.Singleton);
        }

        private void RegisterServices(Composition composition)
        {
            composition.RegisterFor<IEnvironmentService, EnvironmentService>(Lifetime.Singleton);
        }
    }
}
