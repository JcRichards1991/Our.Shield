using Our.Shield.Core.Services;
using Umbraco.Core.Composing;

namespace Our.Shield.Core.Components
{
    internal class StartupComponent : IComponent
    {
        private readonly IJobService _jobService;

        public StartupComponent(IJobService jobService)
        {
            _jobService = jobService;
        }

        public void Initialize()
        {
            _jobService.Init();
        }

        public void Terminate()
        {
        }
    }
}
