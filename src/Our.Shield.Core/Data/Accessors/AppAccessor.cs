using LightInject;
using NPoco;
using Our.Shield.Core.Data.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Core.Scoping;
using System.Linq;
using Umbraco.Core.Persistence;

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

        public async Task<Guid> Create(App app)
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

        public async Task<IReadOnlyList<App>> ReadByEnvironment(Guid environmentKey)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var sql = new Sql<ISqlContext>(scope.Database.SqlContext)
                        .Where<App>(x => x.EnvironmentKey == environmentKey);

                return await scope.Database.FetchAsync<App>(sql);
            }
        }

        public async Task<App> Read(Guid key)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var sql = new Sql<ISqlContext>(scope.Database.SqlContext)
                        .Where<App>(x => x.Key == key);

                return await scope.Database.SingleOrDefaultAsync<App>(sql);
            }
        }

        public async Task<App> Read(string appName, Guid environmentKey)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var sql = new Sql<ISqlContext>(scope.Database.SqlContext)
                        .Where<App>(x => x.AppId == appName && x.EnvironmentKey == environmentKey);

                return await scope.Database.SingleOrDefaultAsync<App>(sql);
            }
        }

        public async Task<bool> Update(App app)
        {
            app.LastModifiedDateUtc = DateTime.UtcNow;

            using (var scope = ScopeProvider.CreateScope())
            {
                await scope.Database.UpdateAsync(app, a => app);
            }

            throw new NotImplementedException();
        }
    }
}
