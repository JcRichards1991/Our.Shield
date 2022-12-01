using Our.Shield.MediaProtection.Components;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Our.Shield.MediaProtection.Composers
{
    /// <summary>
    /// Initializes Shield's Boot Up requirements
    /// </summary>
    [RuntimeLevel(MinLevel = RuntimeLevel.Boot, MaxLevel = RuntimeLevel.Run)]
    public class BootUpComposer : IUserComposer
    {
        /// <inheritdoc />
        public void Compose(Composition composition)
        {
            composition.Components().Append<MigrationComponent>();
        }
    }
}
