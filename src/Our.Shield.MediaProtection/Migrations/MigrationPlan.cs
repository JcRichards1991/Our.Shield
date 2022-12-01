namespace Our.Shield.MediaProtection.Migrations
{
    /// <summary>
    /// Media Protection Migration plan from creating the tables to upgrading from one version to another
    /// </summary>
    public class MigrationPlan : Umbraco.Core.Migrations.MigrationPlan
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationPlan"/> class.
        /// </summary>
        public MigrationPlan() : base($"{nameof(Shield)}.{nameof(MediaProtection)}")
        {
            // Fresh install on umbraco v8
            From(string.Empty).To<InstallMigration>("2.0.0");
        }
    }
}
