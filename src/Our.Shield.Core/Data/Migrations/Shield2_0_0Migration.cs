using System;
using Umbraco.Core.Migrations;

namespace Our.Shield.Core.Data.Migrations
{
    /// <summary>
    /// Shield's 2.0.0 Migration to upgrade the tables in the database to version 2.0.0
    /// </summary>
    public class Shield2_0_0Migration : MigrationBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Shield2_0_0Migration"/> class
        /// </summary>
        /// <param name="context"></param>
        public Shield2_0_0Migration(IMigrationContext context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public override void Migrate()
        {
            throw new NotImplementedException();
        }
    }
}
