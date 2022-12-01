using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;

namespace Our.Shield.MediaProtection.Components
{
    /// <summary>
    /// Initializes Shield's Migrations
    /// </summary>
    public class MigrationComponent : IComponent
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IMigrationBuilder _migrationBuilder;
        private readonly IKeyValueService _keyValueService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="MigrationComponent"/> class
        /// </summary>
        /// <param name="scopeProvider"></param>
        /// <param name="migrationBuilder"></param>
        /// <param name="keyValueService"></param>
        /// <param name="logger"></param>
        public MigrationComponent(
            IScopeProvider scopeProvider,
            IMigrationBuilder migrationBuilder,
            IKeyValueService keyValueService,
            ILogger logger)
        {
            _scopeProvider = scopeProvider;
            _migrationBuilder = migrationBuilder;
            _keyValueService = keyValueService;
            _logger = logger;
        }

        /// <inheritdoc />
        public void Initialize()
        {
            var upgrader = new Upgrader(new Migrations.MigrationPlan());

            upgrader.Execute(
                _scopeProvider,
                _migrationBuilder,
                _keyValueService,
                _logger);
        }

        /// <inheritdoc />
        public void Terminate()
        {
        }
    }
}
