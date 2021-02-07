using LightInject;
using NPoco;
using Our.Shield.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;

namespace Our.Shield.Core.Data.Accessors
{
    /// <summary>
    /// Implements <see cref="IEnvironmentAccessor"/>
    /// </summary>
    public class EnvironmentAccessor : Accessor, IEnvironmentAccessor
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EnvironmentAccessor"/> class
        /// </summary>
        /// <param name="scopeProvider"><see cref="IScopeProvider"/></param>
        /// <param name="mapper"><see cref="IMapper"/></param>
        public EnvironmentAccessor(
            IScopeProvider scopeProvider,
            [Inject(nameof(Shield))] AutoMapper.IMapper mapper)
            : base(scopeProvider, mapper)
        {
        }

        /// <inheritdoc />
        public async Task<Guid> Create(IEnvironment environment)
        {
            ((Models.Environment)environment).Key = Guid.NewGuid();

            var dto = Mapper.Map<Dtos.Environment>(environment);

            dto.LastModifiedDateUtc = DateTime.UtcNow;

            using (var scope = ScopeProvider.CreateScope())
            {
                var result = await scope.Database.InsertAsync(dto);

                scope.Complete();

                return (Guid)result;
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Dtos.Environment>> Read()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var dtos = await scope.Database.FetchAsync<Dtos.Environment>();

                return dtos
                    .OrderBy(x => x.SortOrder)
                    .ToList()
                    .AsReadOnly();
            }
        }

        /// <inheritdoc />
        public async Task<Dtos.Environment> Read(Guid key)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var sql = new Sql<ISqlContext>(scope.Database.SqlContext)
                    .Where<Dtos.Environment>(x => x.Key == key);

                var dto = await scope.Database.SingleAsync<Dtos.Environment>(sql);

                return dto;
            }
        }

        /// <inheritdoc />
        public async Task<bool> Update(IEnvironment environment)
        {
            var dto = Mapper.Map<Dtos.Environment>(environment);

            dto.LastModifiedDateUtc = DateTime.UtcNow;

            using (var scope = ScopeProvider.CreateScope())
            {
                return await scope.Database.UpdateAsync(dto) == 0;
            }
        }

        /// <inheritdoc />
        public async Task<bool> Delete(IEnvironment environment)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var result = await scope.Database.DeleteAsync(environment);

                scope.Complete();

                return result == 0;
            }
        }

        /// <inheritdoc />
        public async Task<bool> Delete(Guid key)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var sql = new Sql<ISqlContext>(scope.Database.SqlContext)
                    .Delete<Dtos.Environment>()
                    .Where<Dtos.Environment>(x => x.Key == key);

                await scope.Database.ExecuteAsync(sql);

                return true;
            }
        }
    }
}
