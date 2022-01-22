using LightInject;
using Newtonsoft.Json;
using NPoco;
using Our.Shield.Core.Data.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<Guid> Write(App app)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                app.LastModifiedDateUtc = DateTime.UtcNow;
                app.Key = Guid.NewGuid();

                var result = await scope.Database.InsertAsync(app);

                scope.Complete();

                return (Guid)result;
            }
        }

        public async Task<IReadOnlyList<App>> Read()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var result = await scope.Database.FetchAsync<App>();

                return result.AsReadOnly();
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

        public async Task<bool> Update(Guid key, Models.IAppConfiguration configuration)
        {
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
                dto.Configuration = JsonConvert.SerializeObject(configuration);
                var result = scope.Database.Update(dto);

                scope.Complete();

                return result == 1;
            }

            throw new NotImplementedException();
        }
    }
}
