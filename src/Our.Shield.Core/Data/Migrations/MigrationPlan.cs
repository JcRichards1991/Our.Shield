using Our.Shield.Core.Data.Migrations.Install;

namespace Our.Shield.Core.Data.Migrations
{
    /// <summary>
    /// Shield's Migration plan from creating the tables to upgrading from one version to another
    /// </summary>
    public class MigrationPlan : Umbraco.Core.Migrations.MigrationPlan
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationPlan"/> class.
        /// </summary>
        public MigrationPlan() : base(Constants.App.Name)
        {
            // Fresh install on umbraco v8
            From(string.Empty).To<InstallMigration>("2.0.0");

            //  Version X.X.X
            //From("2.0.0");
            //To<NextVersionMigration>("X.X.X");

            // Version Y.Y.Y
            //To<FutureVersionMigration>("Y.Y.Y");
        }
    }
}
