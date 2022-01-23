using LightInject;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NPoco;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.ContractResolvers;
using Our.Shield.Core.Data.Dtos;
using Our.Shield.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;

namespace Our.Shield.Core.Data.Accessors
{
    internal class AppAccessor : Accessor, IAppAccessor
    {
        public AppAccessor(
            IScopeProvider scopeProvider,
            [Inject(nameof(Shield))] AutoMapper.IMapper mapper)
            : base(scopeProvider, mapper)
        {
        }

        public void Write(IEnumerable<App> apps)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.Database.InsertBulk(apps);

                scope.Complete();
            }
        }

        public async Task<IReadOnlyCollection<IApp>> ReadByEnvironmentKey(Guid environmentKey)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var sql = new Sql<ISqlContext>(scope.SqlContext)
                        .Where<App>(x => x.EnvironmentKey == environmentKey);

                return await scope.Database.FetchAsync<App>(sql);
            }
        }

        public async Task<App> Read(Guid key)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var sql = new Sql<ISqlContext>(scope.SqlContext)
                        .Where<App>(x => x.Key == key);

                return await scope.Database.SingleOrDefaultAsync<App>(sql);
            }
        }

        public async Task<App> Read(string appName, Guid environmentKey)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var sql = new Sql<ISqlContext>(scope.SqlContext)
                        .Where<App>(x => x.AppId == appName && x.EnvironmentKey == environmentKey);

                return await scope.Database.SingleOrDefaultAsync<App>(sql);
            }
        }

        public async Task<bool> Update(string appId, Guid key, Models.IAppConfiguration configuration)
        {
            await SetSingleEnvironmentValues(appId, key, configuration);

            using (var scope = ScopeProvider.CreateScope())
            {
                var sql = new Sql<ISqlContext>(scope.SqlContext)
                    .Where<App>(x => x.Key == key);

                var dto = await scope.Database.SingleOrDefaultAsync<App>(sql);

                if (dto == null)
                {
                    scope.Complete();
                    return false;
                }

                dto.LastModifiedDateUtc = DateTime.UtcNow;
                dto.Enabled = configuration.Enabled;
                dto.Configuration = JsonConvert.SerializeObject(
                    configuration,
                    Formatting.None,
                    new JsonSerializerSettings
                    {
                        ContractResolver = new AppConfigurationShouldSerializeContractResolver()
                    });

                var result = scope.Database.Update(dto);

                scope.Complete();

                return result == 1;
            }
        }

        private async Task SetSingleEnvironmentValues(string appId, Guid ignoreKey, Models.IAppConfiguration configuration)
        {
            var configurationType = configuration.GetType();
            var properties = configurationType.GetProperties();

            if (properties.All(x => x.GetCustomAttribute<SingleEnvironmentAttribute>() == null))
            {
                return;
            }

            var singleEnvProperties = properties
                .Where(x => x.GetCustomAttribute<SingleEnvironmentAttribute>() != null)
                .ToArray();

            using (var scope = ScopeProvider.CreateScope())
            {
                var sql = new Sql<ISqlContext>(scope.SqlContext)
                        .Where<App>(x => x.Key != ignoreKey)
                        .Where<App>(x => x.AppId == appId);

                var dtos = await scope.Database.FetchAsync<App>(sql);

                if (dtos.None())
                {
                    scope.Complete();

                    return;
                }

                foreach (var dto in dtos)
                {

                    if (!(JsonConvert.DeserializeObject(dto.Configuration, configurationType) is Models.IAppConfiguration recordConfiguration))
                    {
                        continue;
                    }

                    foreach (var property in singleEnvProperties)
                    {
                        property.SetValue(recordConfiguration, property.GetValue(configuration));
                    }

                    dto.Configuration = JsonConvert.SerializeObject(
                        recordConfiguration,
                        Formatting.None,
                        new JsonSerializerSettings
                        {
                            ContractResolver = new AppConfigurationShouldSerializeContractResolver()
                        });

                    dto.LastModifiedDateUtc = DateTime.UtcNow;

                    scope.Database.Update(dto);
                }

                scope.Complete();
            }
        }
    }
}
