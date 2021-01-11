using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;

namespace Our.Shield.Core.Data.Accessors
{
    internal abstract class Accessor
    {
        protected readonly IScopeProvider ScopeProvider;

        protected readonly ILogger Logger;

        protected Accessor(IScopeProvider scopeProvider, ILogger logger)
        {
            ScopeProvider = scopeProvider;
            Logger = logger;
        }
    }
}
