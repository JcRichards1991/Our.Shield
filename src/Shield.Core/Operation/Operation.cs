using System;
using System.Collections.Generic;

namespace Shield.Core.Operation
{
    public abstract class Operation<C, J> : Models.Interfaces.IOperation
        where C : Models.Configuration
        where J : IEnumerable<Models.Journal>
    {
        /// <summary>
        /// Unique identifier of the plugin
        /// </summary>
        public abstract string Id { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool Init()
        {
            return true;
        } 

        public static IDictionary<string, Type> Register
        {
            get
            {
                return Frisk.Register<Operation<C, J>>();
            }
        }

        /// <summary>
        /// Create a derived Position with a particular Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Models.Interfaces.IOperation Create(string id)
        {
            Type derivedType = null;
            if (Register.TryGetValue(id, out derivedType))
            {
                return System.Activator.CreateInstance(derivedType) as Models.Interfaces.IOperation;
            }
            return null;
        }

        /// <summary>
        /// Create a derived Position from a particular type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Models.Interfaces.IOperation Create(Type type)
        {
            return System.Activator.CreateInstance(type) as Models.Interfaces.IOperation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public virtual bool Execute(Models.Configuration config)
        {
            return true;
        }

        /// <summary>
        /// Writes a configuration to the database.
        /// </summary>
        /// <param name="config">
        /// The config to write
        /// </param>
        /// <returns>
        /// Whether or not was successfully written to the database.
        /// </returns>
        public bool WriteConfiguration(Models.Configuration config)
        {
            return Executor.Instance.WriteConfiguration(this.Id, config);
        }

        /// <summary>
        /// Reads a configuration from the database
        /// </summary>
        /// <returns>
        /// The configuration
        /// </returns>
        public Models.Configuration ReadConfiguration()
        {
            return Executor.Instance.ReadConfiguration(this.Id);
        }

        /// <summary>
        /// Writes a journal entry to the database
        /// </summary>
        /// <param name="journal">
        /// the journal to write
        /// </param>
        /// <returns>
        /// Whether or not was successfully written to the database
        /// </returns>
        public bool WriteJournal(Models.Journal journal)
        {
            return Executor.Instance.WriteJournal(this.Id, journal);
        }

        /// <summary>
        /// Gets all Journals from the database for the configuration
        /// </summary>
        /// <param name="page">
        /// The page of results to return
        /// </param>
        /// <param name="itemsPerPage">
        /// the number of items to return
        /// </param>
        /// <returns>
        /// A collection of Journals for the Configuration
        /// </returns>
        public IEnumerable<Models.Journal> ReadJournals(int page, int itemsPerPage)
        {
            return Executor.Instance.ReadJournals(this.Id, page, itemsPerPage);
        }
    }
}
