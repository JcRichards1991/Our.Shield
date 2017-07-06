namespace Our.Shield.Core.Models
{
    using System;
    using System.Collections.Generic;

    public abstract class App<C> : IApp where C : IConfiguration
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
                return Operation.Frisk.Register<App<C>>();
            }
        }

        public virtual IConfiguration DefaultConfiguration
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
        public virtual bool Execute(IJob job, IConfiguration config)
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
        public bool WriteConfiguration(IJob job, IConfiguration config)
        {
            return Operation.JobService.Instance.WriteConfiguration(job, config);
        }

        public bool WriteConfiguration(int jobId, IConfiguration config) => WriteConfiguration(Operation.JobService.Instance.Job(jobId), config);

        /// <summary>
        /// Reads a configuration from the database
        /// </summary>
        /// <returns>
        /// The configuration
        /// </returns>
        public IConfiguration ReadConfiguration(IJob job)
        {
            return Operation.JobService.Instance.ReadConfiguration(job, DefaultConfiguration);
        }

        public IConfiguration ReadConfiguration(int jobId) => ReadConfiguration(Operation.JobService.Instance.Job(jobId));

        /// <summary>
        /// Writes a journal entry to the database
        /// </summary>
        /// <param name="journal">
        /// the journal to write
        /// </param>
        /// <returns>
        /// Whether or not was successfully written to the database
        /// </returns>
        public bool WriteJournal(IJob job, IJournal journal)
        {
            return Operation.JobService.Instance.WriteJournal(job, journal);
        }
        public bool WriteJournal(int jobId, IJournal journal) => WriteJournal(Operation.JobService.Instance.Job(jobId), journal);

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
        public IEnumerable<T> ListJournals<T>(IJob job, int page, int itemsPerPage) where T : IJournal
        {
            return Operation.JobService.Instance.ListJournals<T>(job, page, itemsPerPage);
        }

        public IEnumerable<T> ListJournals<T>(int jobId, int page, int itemsPerPage) where T : IJournal => ListJournals<T>(Operation.JobService.Instance.Job(jobId),
            page, itemsPerPage);

        public override bool Equals(object other)
        {
            var otherApp = other as App<IConfiguration>;
            if (otherApp == null)
            {
                return false;
            }
            return Id == otherApp.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
