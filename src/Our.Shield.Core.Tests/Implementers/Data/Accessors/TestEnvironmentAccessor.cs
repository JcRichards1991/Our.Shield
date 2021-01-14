using Our.Shield.Core.Data.Accessors;
using Our.Shield.Shared;
using Our.Shield.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Our.Shield.Core.Tests.Implementers.Data.Accessors
{
    internal class TestEnvironmentAccessor : IEnvironmentAccessor
    {
        private static List<Core.Data.Dtos.Environment> _environments;

        static TestEnvironmentAccessor()
        {
            _environments = new List<Core.Data.Dtos.Environment>();
        }

        ~TestEnvironmentAccessor()
        {
            _environments = null;
        }

        public async Task<bool> Create(Core.Data.Dtos.Environment environment)
        {
            return await Task.Run(() =>
            {
                GuardClauses.NotNull(environment, nameof(environment));

                var index = _environments.IndexOf(x => x.Key == environment.Key);

                if (index != -1)
                {
                    return false;
                }

                environment.Key = Guid.NewGuid();

                _environments.Add(environment);

                return true;
            });
        }

        public async Task<bool> Delete(Guid key)
        {
            return await Task.Run(async () => await Delete(_environments.FirstOrDefault(x => x.Key == key)));
        }

        public async Task<bool> Delete(Core.Data.Dtos.Environment environment)
        {
            return await Task.Run(() =>
            {
                GuardClauses.NotNull(environment, nameof(environment));

                var index = _environments.IndexOf(x => x.Key == environment.Key);

                if (index == -1)
                {
                    return false;
                }

                _environments.RemoveAt(index);

                return true;
            });
        }

        public async Task<IReadOnlyList<Core.Data.Dtos.Environment>> Read()
        {
            return await Task.Run(() => _environments.AsReadOnly());
        }

        public async Task<Core.Data.Dtos.Environment> Read(Guid key)
        {
            return await Task.Run(() => _environments.FirstOrDefault(x => x.Key == key));
        }

        public async Task<bool> Update(Core.Data.Dtos.Environment environment)
        {
            return await Task.Run(() =>
            {
                GuardClauses.NotNull(environment, nameof(environment));

                var index = _environments.IndexOf(x => x.Key == environment.Key);

                if (index == -1)
                {
                    return false;
                }

                _environments[index] = environment;

                return true;
            });
        }
    }
}
