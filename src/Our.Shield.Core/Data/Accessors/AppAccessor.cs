using NPoco;
using Our.Shield.Core.Data.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;

namespace Our.Shield.Core.Data.Accessors
{
    internal class AppAccessor : Accessor, IAppAccessor
    {
        public AppAccessor(IScopeProvider scopeProvider, ILogger logger) : base(scopeProvider, logger)
        {
        }

        public async Task<bool> Create(App app)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                try
                {
                    app.LastModifiedDateUtc = DateTime.UtcNow;
                    app.Key = Guid.NewGuid();

                    await scope.Database.InsertAsync(app);

                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error<AppAccessor>(ex);
                }
                finally
                {
                    scope.Complete();
                }
            }

            return false;
        }

        public async Task<IReadOnlyList<App>> Read()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                try
                {
                    var result = await scope.Database.FetchAsync<App>();

                    return result.AsReadOnly();
                }
                catch (Exception ex)
                {
                    Logger.Error<AppAccessor>(ex);
                }
                finally
                {
                    scope.Complete();
                }
            }

            return new List<App>();
        }

        public async Task<App> Read(Guid key)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                try
                {
                    return await scope.Database.SingleAsync<App>(new Sql().Where("[Key] = {0}", key));
                }
                catch(Exception ex)
                {
                    Logger.Error<AppAccessor>(ex);
                }
                finally
                {
                    scope.Complete();
                }
            }

            return null;
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

        public async Task<bool> Delete(Guid key)
        {
            var app = await Read(key);

            if (app != null)
            {
                return await Delete(app);
            }

            return false;
        }

        public async Task<bool> Delete(App app)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                return await scope.Database.DeleteAsync(app) == 0;
            }
        }
    }
}
