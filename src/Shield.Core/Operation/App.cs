namespace Shield.Core.Operation
{
    using System;
    using System.Collections.Generic;

    public abstract class App<C> : IApp where C : Persistance.Serialization.Configuration
    {
        /// <summary>
        /// Unique identifier of the plugin
        /// </summary>
        public abstract string Id { get; }

        /// <summary>
        /// Name of the plugin
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Description of the plugin
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Css class of icon
        /// </summary>
        public abstract string Icon { get; }


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
                return Frisk.Register<App<C>>();
            }
        }

        public virtual Persistance.Serialization.Configuration DefaultConfiguration
        {
            get
            {
                return default(C);
            }
        }

        /// <summary>
        /// Create a derived App with a particular Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IApp Create(string id)
        {
            Type derivedType = null;
            if (Register.TryGetValue(id, out derivedType))
            {
                return Activator.CreateInstance(derivedType) as IApp;
            }
            return null;
        }

        /// <summary>
        /// Create a derived App from a particular type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IApp Create(Type type)
        {
            return Activator.CreateInstance(type) as IApp;
        }

        /// <summary>
        /// Execute the config of a derived app
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public virtual bool Execute(Persistance.Serialization.Configuration config)
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
        public bool WriteConfiguration(Persistance.Serialization.Configuration config)
        {
            return Operation.Executor.Instance.WriteConfiguration(this.Id, config);
        }

        /// <summary>
        /// Reads a configuration from the database
        /// </summary>
        /// <returns>
        /// The configuration
        /// </returns>
        public Persistance.Serialization.Configuration ReadConfiguration()
        {
            return Operation.Executor.Instance.ReadConfiguration(this.Id, DefaultConfiguration);
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
        public bool WriteJournal(Persistance.Serialization.Journal journal)
        {
            return Operation.Executor.Instance.WriteJournal(this.Id, journal);
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
        public IEnumerable<Persistance.Serialization.Journal> ReadJournals(int page, int itemsPerPage)
        {
            return Operation.Executor.Instance.ReadJournals(this.Id, page, itemsPerPage);
        }
    }
}
