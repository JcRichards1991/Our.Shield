namespace Our.Shield.Core.Models
{
    using System;
    using Semver;
    using Umbraco.Core.Persistence.Migrations;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class AppMigrationAttribute : Attribute
    {
        public Type Migration;

        public AppMigrationAttribute(Type migration)
        {
            Migration = migration;
        }
    }
}
