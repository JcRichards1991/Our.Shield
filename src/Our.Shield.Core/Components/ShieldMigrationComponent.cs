using Our.Shield.Core.Data.Migrations;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;

namespace Our.Shield.Core.Components
{
    /// <summary>
    /// Initializes Shield's Migrations
    /// </summary>
    public class ShieldMigrationComponent : IComponent
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IMigrationBuilder _migrationBuilder;
        private readonly IKeyValueService _keyValueService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initalizes a new instance of <see cref="ShieldMigrationComponent"/> class
        /// </summary>
        /// <param name="scopeProvider"></param>
        /// <param name="migrationBuilder"></param>
        /// <param name="keyValueService"></param>
        /// <param name="logger"></param>
        public ShieldMigrationComponent(
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
            var upgrader = new Upgrader(new ShieldMigrationPlan());

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
