using Our.Shield.Core.Operation;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using static Our.Shield.Core.Operation.WebRequestHandler;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// job interface
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// The Job Id
        /// </summary>
        int Id { get; }

        /// <summary>
        /// The Job Environment
        /// </summary>
        IEnvironment Environment { get; }

        /// <summary>
        /// The Job App Id
        /// </summary>
        IApp App { get; }

        /// <summary>
        /// Writes a Apps configuration to the database
        /// </summary>
        /// <param name="config">The configuration to write</param>
        /// <returns>True, if successfully written the config to the database; Otherwise, False</returns>
        bool WriteConfiguration(IConfiguration config);

        /// <summary>
        /// Writes a journal to the database
        /// </summary>
        /// <param name="journal">The journal to write</param>
        /// <returns>True, if successfully written the journal to the database; Otherwise, False</returns>
        bool WriteJournal(IJournal journal);

        /// <summary>
        /// Reads the configuration from the database
        /// </summary>
        /// <returns>The configuration for the App</returns>
        IConfiguration ReadConfiguration();

        /// <summary>
        /// Reads a list of Journals fro the database
        /// </summary>
        /// <typeparam name="T">The Type of Journal to return</typeparam>
        /// <param name="page">The page of results to return</param>
        /// <param name="itemsPerPage">The number of items to return per page</param>
        /// <returns>Collection of Journals of the desired type</returns>
        IEnumerable<T> ListJournals<T>(int page, int itemsPerPage, out int totalPages) where T : IJournal;

        /// <summary>
        /// Adds a Web Request to WebRequestsHandler collection
        /// </summary>
        /// <param name="regex">The Regex use to match for requests</param>
        /// <param name="beginRequestPriority">The priority of the begin request watch</param>
        /// <param name="beginRequest">The function to call when the Regex matches a request</param>
        /// <param name="endRequestPriority">The priority of the end request watch</param>
        /// <param name="endRequest">The function to call when the regex matches a request</param>
        /// <returns></returns>
        int WatchWebRequests(PipeLineStages stage, Regex regex, int priority, 
            Func<int, HttpApplication, WatchResponse> request);

        /// <summary>
        /// Removes a Web Requests from the WebRequestHandler collection
        /// </summary>
        /// <param name="regex">The regex of the corresponding Web Request to remove</param>
        /// <returns></returns>
        int UnwatchWebRequests(PipeLineStages stage, Regex regex);

        /// <summary>
        /// Removes all Web Requests from the WebRequestHandler collection created by this job
        /// </summary>
        /// <returns></returns>
        int UnwatchWebRequests(PipeLineStages stage);

        int UnwatchWebRequests();

        /// <summary>
        /// Removes all Web Requests from the WebRequestsHandler collection for the given App
        /// </summary>
        /// <param name="app">The App of the corresponding Web Requests to remove</param>
        /// <returns></returns>
        int UnwatchWebRequests(IApp app);

        int ExceptionWebRequest(Regex regex);
        int ExceptionWebRequest(UmbracoUrl url);

        int UnexceptionWebRequest(Regex regex);
        int UnexceptionWebRequest(UmbracoUrl url);

        int UnexceptionWebRequest();
    }
}
