using AutoMapper;
using Our.Shield.Shared;
using Umbraco.Core.Scoping;

namespace Our.Shield.Core.Data.Accessors
{
    /// <summary>
    /// Base Data Accessor
    /// </summary>
    public abstract class Accessor
    {
        /// <summary>
        /// Scope Provider
        /// </summary>
        protected readonly IScopeProvider ScopeProvider;

        /// <summary>
        /// Mapper
        /// </summary>
        protected readonly IMapper Mapper;

        /// <summary>
        /// Initializes a new instance of <see cref="Accessor"/> class
        /// </summary>
        /// <param name="scopeProvider"><see cref="IScopeProvider"/></param>
        /// <param name="mapper"></param>
        public Accessor(
            IScopeProvider scopeProvider,
            IMapper mapper)
        {
            GuardClauses.NotNull(scopeProvider, nameof(scopeProvider));
            GuardClauses.NotNull(mapper, nameof(mapper));

            ScopeProvider = scopeProvider;
            Mapper = mapper;
        }
    }
}
