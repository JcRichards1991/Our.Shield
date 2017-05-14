namespace Shield.Core.Models
{
    using System;
    using System.Collections.Generic;

    public abstract class TreeNode : IFrisk
    {
        public abstract string Name { get; }

        public abstract string Id { get; }

        public abstract bool HasChildNodes { get; }

        public abstract string Icon { get; }

        public abstract string RoutePath { get; }

        public abstract string ParentId { get; }

        public abstract string ConfigurationId { get; }

        public static IDictionary<string, Type> Register
        {
            get
            {
                return Frisk.Register<TreeNode>();
            }
        }

        /// <summary>
        /// Create a derived Position with a particular Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static TreeNode Create(string id)
        {
            Type derivedType = null;
            if (Register.TryGetValue(id, out derivedType))
            {
                return Activator.CreateInstance(derivedType) as TreeNode;
            }
            return null;
        }

        /// <summary>
        /// Create a derived Position from a particular type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TreeNode Create(Type type)
        {
            return Activator.CreateInstance(type) as TreeNode;
        }
    }
}
