using Our.Shield.Core.Components;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Our.Shield.Core.Composers
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
            composition.Components().Append<ShieldMigrationComponent>();
        }
    }
}
