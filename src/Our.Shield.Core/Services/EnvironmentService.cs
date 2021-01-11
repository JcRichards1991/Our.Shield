using Our.Shield.Core.Data.Accessors;
using Our.Shield.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core.Logging;

namespace Our.Shield.Core.Services
{
    /// <summary>
    /// Implentation of <see cref="IEnvironmentService"/>
    /// </summary>
    public class EnvironmentService : IEnvironmentService
    {
        private readonly IEnvironmentAccessor _dataAccessor;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentService"/> class
        /// </summary>
        /// <param name="environmentAccessor"></param>
        /// <param name="logger"></param>
        public EnvironmentService(
            IEnvironmentAccessor environmentAccessor,
            ILogger logger)
        {
            _dataAccessor = environmentAccessor;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<bool> Upsert(IEnvironment environment)
        {
            if (!await UpsertDatabase(environment))
            {
                return false;
            }

            return UpsertSystem();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<IEnvironment>> Get()
        {
            var envs = await _dataAccessor.Read();

            if (envs.Any())
            {
                return envs
                    .Select(x => new Models.Environment(x))
                    .ToList()
                    .AsReadOnly();
            }

            return new List<IEnvironment>();
        }

        /// <inheritdoc />
        public async Task<IEnvironment> Get(Guid key)
        {
            var env = await _dataAccessor.Read(key);

            if (env != null)
            {
                return new Models.Environment(env);
            }

            _logger.Warn<EnvironmentService>("No environment found in database with key: {Key}", key);

            return default(IEnvironment);
        }

        /// <inheritdoc />
        public async Task<bool> Delete(IEnvironment environment)
        {
            //if (!JobService.Instance.Unregister(environment) || !DbContext.Instance.Environment.Delete(environment.Id))
            //{
            //    return false;
            //}

            //var environments = DbContext.Instance.Environment.Read().Select(x => new Models.Environment(x));
            //var oldEnvironments = JobService.Instance.Environments.Keys;

            //foreach (var newEnv in environments)
            //{
            //    if (oldEnvironments.Any(x => x.Id.Equals(newEnv.Id) && !x.SortOrder.Equals(newEnv.SortOrder)))
            //    {
            //        JobService.Instance.Unregister(newEnv);
            //        JobService.Instance.Register(newEnv);
            //    }
            //}

            var env = new Data.Dtos.Environment(environment);

            return await _dataAccessor.Delete(env);
        }

        private async Task<bool> UpsertDatabase(IEnvironment environment)
        {
            var env = new Data.Dtos.Environment(environment);

            if (environment.Key == Guid.Empty)
            {
                return await _dataAccessor.Create(env);
            }

            return await _dataAccessor.Update(env);
        }

        private bool UpsertSystem()
        {
            //if (!JobService.Instance.Environments.Any(x => x.Key.Id.Equals(environment.Id)))
            //{
            //    //created new environment, we need to register it
            //    JobService.Instance.Register(environment);

            //    var environments = DbContext.Instance.Environment.Read().Select(x => new Models.Environment(x));
            //    var oldEnvironments = JobService.Instance.Environments.Keys;

            //    foreach (var newEnv in environments)
            //    {
            //        if (!oldEnvironments.Any(x => x.Id.Equals(newEnv.Id) && !x.SortOrder.Equals(newEnv.SortOrder)))
            //            continue;

            //        JobService.Instance.Unregister(newEnv);
            //        JobService.Instance.Register(newEnv);
            //    }
            //}
            //else
            //{
            //    //Environment has changed, we need to unregister it
            //    //and then re-register it with the new changes
            //    if (!JobService.Instance.Unregister(environment))
            //    {
            //        return false;
            //    }

            //    JobService.Instance.Register(environment);
            //}

            return true;
        }
    }
}
