using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Our.Shield.MediaProtection.Models
{
    /// <summary>
    /// Handles Creating/Editing the Configuration table.
    /// </summary>
    [Migration("1.0.0", 1, nameof(Shield) + nameof(MediaProtection))]
    internal class MediaProtectionMigration : MigrationBase
    {
        public bool AddMediaTypes;

        /// <summary>
        /// Default constructor for the Configuration Migration
        /// </summary>
        /// <param name="sqlSyntax">The SQL Syntax</param>
        /// <param name="logger">The Logger</param>
        public MediaProtectionMigration(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        /// <summary>
        /// Sets the AddMediaTypes Flag
        /// </summary>
        public override void Up()
        {
            AddMediaTypes = true;
        }

        /// <summary>
        /// Placeholder Method
        /// </summary>
        public override void Down()
        {
        }
    }
}
