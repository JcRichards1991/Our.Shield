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

namespace Our.Shield.Core.Persistence.Data.Migrations
{
    public static class MigrationExtensions
    {
        private static string TableNameAttribute<T>() where T : new()
        {
            var type = typeof(T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            if (tableNameAttribute == null)
                throw new Exception(
                    $"The Type '{type.Name}' does not contain a TableNameAttribute, which is used to find the name of the table to drop. The operation could not be completed.");

            return tableNameAttribute.Value;
        }

        public static IAlterTableSyntax Table<T>(this IAlterSyntaxBuilder alter) where T : new()
        {
            
            return alter.Table(TableNameAttribute<T>());
        }

        public static void FromTable<T>(this IDeleteColumnFromTableSyntax alter) where T : new()
        {
            alter.FromTable(TableNameAttribute<T>());
        }

        public static IDeleteIndexOnColumnSyntax OnTable<T>(this IDeleteIndexForTableSyntax alter) where T : new()
        {
            return alter.OnTable(TableNameAttribute<T>());
        }

        public static ICreateIndexOnColumnSyntax OnTable<T>(this ICreateIndexForTableSyntax alter) where T : new()
        {
            return alter.OnTable(TableNameAttribute<T>());
        }

        public static ICreateForeignKeyForeignColumnSyntax FromTable<T>(this ICreateForeignKeyFromTableSyntax alter) where T : new()
        {
            return alter.FromTable(TableNameAttribute<T>());
        }
        public static IDeleteForeignKeyForeignColumnSyntax FromTable<T>(this IDeleteForeignKeyFromTableSyntax alter) where T : new()
        {
            return alter.FromTable(TableNameAttribute<T>());
        }

        public static ICreateForeignKeyPrimaryColumnSyntax ToTable<T>(this ICreateForeignKeyToTableSyntax alter) where T : new()
        {
            return alter.ToTable(TableNameAttribute<T>());
        }

        public static void OnTable<T>(this IDeleteForeignKeyOnTableSyntax alter) where T : new()
        {
            alter.OnTable(TableNameAttribute<T>());
        }

        public static bool TableExist<T>(this DatabaseSchemaHelper schemaHelper) where T : new()
        {
            return schemaHelper.TableExist(TableNameAttribute<T>());
        }
    }
}
