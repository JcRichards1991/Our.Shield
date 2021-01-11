﻿using Our.Shield.Core.Data.Dtos;
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
        /// Initializes a new intace of <see cref="InstallMigration"/> class
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
                .Table<App>()
                .Do();

            Create
                .Table<Journal>()
                .Do();

            Insert
                .IntoTable(nameof(Shield) + "Environments")
                .Row(
                new Dtos.Environment
                {
                    Key = Guid.NewGuid(),
                    LastModifiedDateUtc = DateTime.UtcNow,
                    Name = "Default",
                    Icon = "icon-cog",
                    SortOrder = int.MaxValue,
                    Enabled = true,
                    ContinueProcessing = true,
                    Domains = "[]"
                })
                .Do();
        }
    }
}
