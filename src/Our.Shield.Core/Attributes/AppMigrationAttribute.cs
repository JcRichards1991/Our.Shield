using System;

namespace Our.Shield.Core.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AppMigrationAttribute : Attribute
    {
        public Type Migration;

        /// <inheritdoc />
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
