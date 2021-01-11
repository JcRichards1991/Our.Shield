using Our.Shield.Shared;
using Umbraco.Core.Logging;
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
        /// Logger
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of <see cref="Accessor"/> class
        /// </summary>
        /// <param name="scopeProvider"></param>
        /// <param name="logger"></param>
        public Accessor(IScopeProvider scopeProvider, ILogger logger)
        {
            GuardClauses.NotNull(scopeProvider, nameof(scopeProvider));
            GuardClauses.NotNull(logger, nameof(logger));

            ScopeProvider = scopeProvider;
            Logger = logger;
        }
    }
}
