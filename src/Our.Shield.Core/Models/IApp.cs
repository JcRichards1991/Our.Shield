using Newtonsoft.Json;
using System.Collections.Generic;
using Umbraco.Core.Persistence.Migrations;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// 
    /// </summary>
    public interface IApp : IFrisk
    {
        /// <summary>
        /// Name of the plugin
        /// </summary>
        [JsonProperty("name")]
        string Name { get; }

        /// <summary>
        /// Description of the plugin
        /// </summary>
        [JsonProperty("description")]
        string Description { get; }

        /// <summary>
        /// Css class of icon
        /// </summary>
        [JsonProperty("icon")]
        string Icon { get; }

        /// <summary>
        /// The initialise method for the App
        /// </summary>
        /// <returns>True if successfully initialised; Otherwise, False</returns>
        bool Init();

        /// <summary>
        /// Execute the config of a derived app
        /// </summary>
        /// <param name="job">The job to to execute the config</param>
        /// <param name="config">The current config to execute</param>
        /// <returns>True, if successfully executed; Otherwise, False</returns>
        bool Execute(IJob job, IConfiguration config);

        /// <summary>
        /// Writes a configuration to the database
        /// </summary>
        /// <param name="job">The job to write the configuration for</param>
        /// <param name="config">The config to write</param>
        /// <returns>Whether or not was successfully written to the database</returns>
        bool WriteConfiguration(IJob job, IConfiguration config);

        /// <summary>
        /// Writes a configuration to the database.
        /// </summary>
        /// <param name="jobId">The job Id to write the configuration for</param>
        /// <param name="config">The config to write</param>
        /// <returns>Whether or not was successfully written to the database</returns>
        bool WriteConfiguration(int jobId, IConfiguration config);

        /// <summary>
        /// Writes a journal entry to the database
        /// </summary>
        /// <param name="job">The job for writing the journal for</param>
        /// <param name="journal">The journal to write</param>
        /// <returns>Whether or not was successfully written to the database</returns>
        bool WriteJournal(IJob job, IJournal journal);

        /// <summary>
        /// Writes a journal entry to the database
        /// </summary>
        /// <param name="jobId">The job id for writing the journal for</param>
        /// <param name="journal">The journal to write</param>
        /// <returns>Whether or not was successfully written to the database</returns>
        bool WriteJournal(int jobId, IJournal journal);

        /// <summary>
        /// Reads a configuration from the database
        /// </summary>
        /// <param name="job">The job to get the configuration for</param>
        /// <returns>The configuration</returns>
        IConfiguration ReadConfiguration(IJob job);

        /// <summary>
        /// Reads a configuration from the database
        /// </summary>
        /// <param name="jobId">The job id to get the configuration for</param>
        /// <returns>The configuration</returns>
        IConfiguration ReadConfiguration(int jobId);

        /// <summary>
        /// Gets all Journals from the database for the given Job
        /// </summary>
        /// <typeparam name="T">The type of journals to return</typeparam>
        /// <param name="job">The Job of journals to return</param>
        /// <param name="page">The page of results to return</param>
        /// <param name="itemsPerPage">The number of items to return</param>
        /// <param name="totalPages"></param>
        /// <returns>A collection of Journals for the Configuration</returns>
        IEnumerable<T> ListJournals<T>(IJob job, int page, int itemsPerPage, out int totalPages) where T : IJournal;

        /// <summary>
        /// Gets all Journals from the database for the given Job
        /// </summary>
        /// <typeparam name="T">The type of journals to return</typeparam>
        /// <param name="jobId">The id of the Job for journals to be return</param>
        /// <param name="page">The page of results to return</param>
        /// <param name="itemsPerPage">The number of items to return</param>
        /// <param name="totalPages"></param>
        /// <returns>A collection of Journals for the Configuration</returns>
        IEnumerable<T> ListJournals<T>(int jobId, int page, int itemsPerPage, out int totalPages) where T : IJournal;

        /// <summary>
        /// The default configuration for the App
        /// </summary>
        [JsonIgnore]
        IConfiguration DefaultConfiguration { get; }

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        IDictionary<string, IMigration> Migrations { get; set; }
    }
}
