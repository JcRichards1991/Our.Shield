using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;

namespace Our.Shield.Core.Data.Accessors
{
    internal class JournalAccessor : Accessor, IJournalAccessor
    {
        public JournalAccessor(
            IScopeProvider scopeProvider,
            ILogger logger)
            : base(scopeProvider, logger)
        {
        }
    }
}
