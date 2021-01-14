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
            composition.Register<IEnvironmentAccessor, EnvironmentAccessor>();
            composition.Register<IAppAccessor, AppAccessor>();
            composition.Register<IJournalAccessor, JournalAccessor>();
        }

        private void RegisterServices(Composition composition)
        {
            composition.Register<IEnvironmentService, EnvironmentService>();
        }
    }
}
