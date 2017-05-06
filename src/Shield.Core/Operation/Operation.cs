using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Core.Operation
{
    public abstract class Operation<C> : IOperation
        where C : Configuration
    {
        /// <summary>
        /// Unique identifier of the plugin
        /// </summary>
        public abstract string Id { get; }


        public virtual bool Init()
        {
            return true;
        } 

        public static IDictionary<string, Type> Register
        {
            get
            {
                return Frisk.Register<Operation<C>>();
            }
        }

        /// <summary>
        /// Create a derived Position with a particular Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IOperation Create(string id)
        {
            Type derivedType = null;
            if (Register.TryGetValue(id, out derivedType))
            {
                return System.Activator.CreateInstance(derivedType) as IOperation;
            }
            return null;
        }

        /// <summary>
        /// Create a derived Position from a particular type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IOperation Create(Type type)
        {
            return System.Activator.CreateInstance(type) as IOperation;
        }

        public virtual bool Execute(Configuration config)
        {
            return true;
        }

        public bool Write(bool enable, Configuration config)
        {
            return Executor.Instance.Save(this.Id, enable, config);
        }
    }
}
