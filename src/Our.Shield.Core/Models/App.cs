namespace Our.Shield.Core.Models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Semver;
    using Umbraco.Core.Logging;
    using Umbraco.Core.Persistence.Migrations;
    using Umbraco.Core.Persistence.SqlSyntax;

    /// <summary>
    /// Definition of an App to plugin to Our.Shield custom umbraco section
    /// </summary>
    /// <typeparam name="C"></typeparam>
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
        /// The initialise method for the App
        /// </summary>
        /// <returns>
        /// True if successfully initialised; Otherwise, false
        /// </returns>
        public virtual bool Init()
        {
            return true;
        } 

        /// <summary>
        /// Dictionary of the registered App Id, and it's type
        /// </summary>
        public static IDictionary<string, Type> Register
        {
            get
            {
                return Operation.Frisk.Register<App<C>>();
            }
        }

        /// <summary>
        /// The default configuration for the App
        /// </summary>
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
        /// <param name="id">
        /// The id of the App to create
        /// </param>
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
        /// <param name="type">
        /// The type of App to create
        /// </param>
        /// <returns></returns>
        public static IApp Create(Type type)
        {
            return Activator.CreateInstance(type) as IApp;
        }

        /// <summary>
        /// Execute the config of a derived app
        /// </summary>
        /// <param name="job">
        /// The job to to execute the config
        /// </param>
        /// <param name="config">
        /// The current config to execute
        /// </param>
        /// <returns>
        /// True, if successfully executed; Otherwise, False
        /// </returns>
        public virtual bool Execute(IJob job, IConfiguration config)
        {
            return true;
        }

        /// <summary>
        /// Writes a configuration to the database.
        /// </summary>
        /// <param name="job">
        /// The job to write the configuration for
        /// </param>
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

        /// <summary>
        /// Writes a configuration to the database.
        /// </summary>
        /// <param name="jobId">
        /// The job Id to write the configuration for
        /// </param>
        /// <param name="config">
        /// The config to write
        /// </param>
        /// <returns>
        /// Whether or not was successfully written to the database.
        /// </returns>
        public bool WriteConfiguration(int jobId, IConfiguration config) => WriteConfiguration(Operation.JobService.Instance.Job(jobId), config);

        /// <summary>
        /// Reads a configuration from the database
        /// </summary>
        /// <param name="job">
        /// The job to get the configuration for
        /// </param>
        /// <returns>
        /// The configuration
        /// </returns>
        public IConfiguration ReadConfiguration(IJob job)
        {
            return Operation.JobService.Instance.ReadConfiguration(job, DefaultConfiguration);
        }

        /// <summary>
        /// Reads a configuration from the database
        /// </summary>
        /// <param name="jobId">
        /// The job id to get the configuration for
        /// </param>
        /// <returns>
        /// The configuration
        /// </returns>
        public IConfiguration ReadConfiguration(int jobId) => ReadConfiguration(Operation.JobService.Instance.Job(jobId));

        /// <summary>
        /// Writes a journal entry to the database
        /// </summary>
        /// <param name="job">
        /// The job for writing the journal for
        /// </param>
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

        /// <summary>
        /// Writes a journal entry to the database
        /// </summary>
        /// <param name="jobId">
        /// The job id for writing the journal for
        /// </param>
        /// <param name="journal">
        /// the journal to write
        /// </param>
        /// <returns>
        /// Whether or not was successfully written to the database
        /// </returns>
        public bool WriteJournal(int jobId, IJournal journal) => WriteJournal(Operation.JobService.Instance.Job(jobId), journal);

        /// <summary>
        /// Gets all Journals from the database for the given Job
        /// </summary>
        /// <typeparam name="T">
        /// The type of journals to return
        /// </typeparam>
        /// <param name="job">
        /// The Job of journals to return
        /// </param>
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

        /// <summary>
        /// Gets all Journals from the database for the given Job
        /// </summary>
        /// <typeparam name="T">
        /// The type of journals to return
        /// </typeparam>
        /// <param name="jobId">
        /// The id of the Job for journals to be return
        /// </param>
        /// <param name="page">
        /// The page of results to return
        /// </param>
        /// <param name="itemsPerPage">
        /// the number of items to return
        /// </param>
        /// <returns>
        /// A collection of Journals for the Configuration
        /// </returns>
        public IEnumerable<T> ListJournals<T>(int jobId, int page, int itemsPerPage) where T : IJournal => ListJournals<T>(Operation.JobService.Instance.Job(jobId),
            page, itemsPerPage);

        /// <summary>
        /// Checks whether or not two Apps are the same
        /// </summary>
        /// <param name="other"></param>
        /// <returns>
        /// True if equals; Otherwise False
        /// </returns>
        public override bool Equals(object other)
        {
            var otherApp = other as App<IConfiguration>;
            if (otherApp == null)
            {
                return false;
            }
            return Id == otherApp.Id;
        }

        /// <summary>
        /// Gets the HashCode for the App
        /// </summary>
        /// <returns>
        /// The Id's HashCode
        /// </returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        [JsonIgnore]
        private IDictionary<string, IMigration> _migrations;

        [JsonIgnore]
        public IDictionary<string, IMigration> Migrations
        {
            get
            {
                return _migrations;
            }
            set
            {
                _migrations = value;
            }
        }
    }
}
