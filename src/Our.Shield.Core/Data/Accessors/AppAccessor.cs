using LightInject;
using NPoco;
using Our.Shield.Core.Data.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public async Task<bool> Create(App app)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                app.LastModifiedDateUtc = DateTime.UtcNow;
                app.Key = Guid.NewGuid();

                return await scope.Database.InsertAsync(app) != null;
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

        public async Task<App> Read(Guid key)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return await scope.Database.SingleAsync<App>(new Sql().Where("[Key] = {0}", key));
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
