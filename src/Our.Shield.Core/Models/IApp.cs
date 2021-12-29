using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Umbraco.Core.Migrations;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// Definition of an App to plugin to Our.Shield custom umbraco section
    /// </summary>
    public interface IApp : IFrisk
    {
        /// <summary>
        /// The Unique key of the App
        /// </summary>
        Guid Key { get; set; }

        /// <summary>
        /// CSS class of icon
        /// </summary>
        [JsonProperty("icon")]
        string Icon { get; }

        /// <summary>
        /// The default configuration for the App
        /// </summary>
        [JsonIgnore]
        IAppConfiguration DefaultConfiguration { get; }

        /// <summary>
        /// The Migrations for the App
        /// </summary>
        [JsonIgnore]
        IDictionary<string, IMigration> Migrations { get; set; }

        /// <summary>
        /// The initialize method for the App
        /// </summary>
        /// <returns>True if successfully initialized; Otherwise, False</returns>
        bool Init();

        /// <summary>
        /// Execute the config of a derived App
        /// </summary>
        /// <param name="job">The job to execute the config</param>
        /// <param name="config">The current config to execute</param>
        /// <returns>True, if successfully executed; Otherwise, False</returns>
        bool Execute(IJob job, IAppConfiguration config);

        /// <summary>
        /// Writes a configuration to the database
        /// </summary>
        /// <param name="job">The job to write the configuration for</param>
        /// <param name="config">The config to write</param>
        /// <returns>Whether or not was successfully written to the database</returns>
        bool WriteConfiguration(IJob job, IAppConfiguration config);

        /// <summary>
        /// Writes a configuration to the database.
        /// </summary>
        /// <param name="jobId">The job Id to write the configuration for</param>
        /// <param name="config">The config to write</param>
        /// <returns>Whether or not was successfully written to the database</returns>
        bool WriteConfiguration(int jobId, IAppConfiguration config);

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
        IAppConfiguration ReadConfiguration(IJob job);

        /// <summary>
        /// Reads a configuration from the database
        /// </summary>
        /// <param name="jobId">The job id to get the configuration for</param>
        /// <returns>The configuration</returns>
        IAppConfiguration ReadConfiguration(int jobId);

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
    }
}
