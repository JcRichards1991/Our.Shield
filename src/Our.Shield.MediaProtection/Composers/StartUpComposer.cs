using Our.Shield.MediaProtection.Helpers;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Our.Shield.MediaProtection.Composers
{
    /// <summary>
    /// Initializes Media Protect Start Up requirements
    /// </summary>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run, MaxLevel = RuntimeLevel.Run)]
    public class StartUpComposer : IUserComposer
    {
        /// <inheritdoc />
        public void Compose(Composition composition)
        {
            composition.Register<UmbracoDataTypes>();
        }
    }
}
