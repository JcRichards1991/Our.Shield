﻿using Newtonsoft.Json.Linq;
using System;
using Umbraco.Core.Migrations;

namespace Our.Shield.Core.Data.Migrations.Install
{
    /// <summary>
    /// Shield's Create Migration to create the required tables in the database
    /// </summary>
    public class InstallMigration : MigrationBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="InstallMigration"/> class
        /// </summary>
        /// <param name="context"></param>
        public InstallMigration(IMigrationContext context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public override void Migrate()
        {
            Create
                .Table<Dtos.Environment>()
                .Do();

            Create
                .Table<Dtos.App>()
                .Do();

            Create
                .Table<Dtos.Journal>()
                .Do();

            Insert
                .IntoTable(nameof(Shield) + "Environments")
                .Row(new Dtos.Environment
                {
                    Key = Guid.NewGuid(),
                    LastModifiedDateUtc = DateTime.UtcNow,
                    Name = "Default",
                    Icon = "icon-globe",
                    SortOrder = int.MaxValue,
                    Enabled = true,
                    ContinueProcessing = true,
                    Domains = new JArray().ToString()
                })
                .Do();
        }
    }
}
