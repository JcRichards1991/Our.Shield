using Our.Shield.Core.Components;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Our.Shield.Core.Composers
{
    /// <summary>
    /// <see cref="IUserComposer"/> for the Startup process of the application
    /// </summary>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class StartUpComposer : IUserComposer
    {
        /// <inheritdoc/>
        public void Compose(Composition composition)
        {
            composition.Components().Append<ClearCacheComponent>();
        }
    }
}
