using Our.Shield.Core.Models;
using System;

namespace Our.Shield.Core.Services
{
    public interface IJobService
    {
        bool ExecuteApp(Guid key, IAppConfiguration configuration);

        bool Register(IEnvironment environment, IApp app, IAppConfiguration configuration);

        bool Unregister(IEnvironment environment);
    }
}
