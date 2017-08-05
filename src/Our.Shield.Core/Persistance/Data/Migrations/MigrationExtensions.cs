using System;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Table;
using Umbraco.Core.Persistence.Migrations.Syntax.Create.ForeignKey;
using Umbraco.Core.Persistence.Migrations.Syntax.Create.Index;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.ForeignKey;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Index;

namespace Our.Shield.Core.Persistance.Data.Migrations
{
    public static class MigrationExtensions
    {
        private static string tableNameAttribute<T>() where T : new()
        {
            Type type = typeof(T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            if (tableNameAttribute == null)
                throw new Exception(
                    string.Format(
                        "The Type '{0}' does not contain a TableNameAttribute, which is used to find the name of the table to drop. The operation could not be completed.",
                        type.Name));

            return tableNameAttribute.Value;
        }

        public static IAlterTableSyntax Table<T>(this IAlterSyntaxBuilder alter) where T : new()
        {
            
            return alter.Table(tableNameAttribute<T>());
        }

        public static void FromTable<T>(this IDeleteColumnFromTableSyntax alter) where T : new()
        {
            alter.FromTable(tableNameAttribute<T>());
        }

        public static IDeleteIndexOnColumnSyntax OnTable<T>(this IDeleteIndexForTableSyntax alter) where T : new()
        {
            return alter.OnTable(tableNameAttribute<T>());
        }

        public static ICreateIndexOnColumnSyntax OnTable<T>(this ICreateIndexForTableSyntax alter) where T : new()
        {
            return alter.OnTable(tableNameAttribute<T>());
        }

        public static ICreateForeignKeyForeignColumnSyntax FromTable<T>(this ICreateForeignKeyFromTableSyntax alter) where T : new()
        {
            return alter.FromTable(tableNameAttribute<T>());
        }
        public static IDeleteForeignKeyForeignColumnSyntax FromTable<T>(this IDeleteForeignKeyFromTableSyntax alter) where T : new()
        {
            return alter.FromTable(tableNameAttribute<T>());
        }

        public static ICreateForeignKeyPrimaryColumnSyntax ToTable<T>(this ICreateForeignKeyToTableSyntax alter) where T : new()
        {
            return alter.ToTable(tableNameAttribute<T>());
        }

        public static void OnTable<T>(this IDeleteForeignKeyOnTableSyntax alter) where T : new()
        {
            alter.OnTable(tableNameAttribute<T>());
        }
    }
}
