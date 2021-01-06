using Umbraco.Core.Migrations;

namespace Our.Shield.MediaProtection.Models
{
    [Migration("1.0.0", 1, nameof(Shield) + nameof(MediaProtection))]
    internal class MediaProtectionMigration : MigrationBase
    {
        public bool AddMediaTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaProtectionMigration"/> class
        /// </summary>
        /// <param name="migrationContext"></param>
        public MediaProtectionMigration(IMigrationContext migrationContext) : base(migrationContext)
        {
        }

        /// <inheritdoc />
        public override void Migrate()
        {
            AddMediaTypes = true;
        }
    }
}
