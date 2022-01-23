using Our.Shield.Core.Services;
using Umbraco.Core.Composing;

namespace Our.Shield.Core.Components
{
    internal class StartupComponent : IComponent
    {
        private readonly IShieldService _shieldService;

        public StartupComponent(IShieldService shieldService)
        {
            _shieldService = shieldService;
        }

        public async void Initialize()
        {
            await _shieldService.Init();
        }

        public void Terminate()
        {
        }
    }
}
