namespace Our.Shield.Core.Models
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class AppMigrationAttribute : Attribute
    {
        public Type Migration;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="migration"></param>
        public AppMigrationAttribute(Type migration)
        {
            Migration = migration;
        }
    }
}
