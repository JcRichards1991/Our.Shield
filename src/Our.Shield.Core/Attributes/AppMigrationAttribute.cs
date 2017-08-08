using System;

namespace Our.Shield.Core.Attributes
{
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
