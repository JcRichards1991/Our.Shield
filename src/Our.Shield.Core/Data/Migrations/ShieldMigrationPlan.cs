using Our.Shield.Core.Data.Migrations.Install;
using Umbraco.Core.Migrations;

namespace Our.Shield.Core.Data.Migrations
{
    /// <summary>
    /// Shield's Migration plan from creating the tables to upgrading from one version to another
    /// </summary>
    public class ShieldMigrationPlan : MigrationPlan
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ShieldMigrationPlan"/> class.
        /// </summary>
        public ShieldMigrationPlan() : base(Constants.App.Name)
        {
            //  Upgrading from compatible umbraco v7
            From("1.0.7").To<Shield2_0_0Migration>("2.0.0");

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
