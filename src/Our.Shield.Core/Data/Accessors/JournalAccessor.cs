using LightInject;
using Our.Shield.Core.Models;
using System;
using System.Threading.Tasks;
using Umbraco.Core.Scoping;

namespace Our.Shield.Core.Data.Accessors
{
    /// <summary>
    /// Implements <see cref="IJournalAccessor"/>
    /// </summary>
    internal class JournalAccessor : Accessor, IJournalAccessor
    {
        /// <summary>
        /// Initializes a new instance of <see cref="JournalAccessor"/>
        /// </summary>
        /// <param name="scopeProvider"></param>
        /// <param name="mapper"></param>
        public JournalAccessor(
            IScopeProvider scopeProvider,
            [Inject(nameof(Shield))] AutoMapper.IMapper mapper)
            : base(scopeProvider, mapper)
        {
        }

        /// <inheritdoc />
        public async Task<bool> Write(IJournal journal)
        {
            var dto = Mapper.Map<Dtos.Journal>(journal);

            using (var scope = ScopeProvider.CreateScope())
            {
                var result = await scope.Database.InsertAsync(dto);

                scope.Complete();

                return (Guid)result != Guid.Empty;
            }
        }
    }
}
