using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// Environment interface 
    /// </summary>
    public interface IEnvironment
    {
        /// <summary>
        /// The Id of the Environment
        /// </summary>
        [JsonProperty("id")]
        int Id { get; }

        /// <summary>
        /// The Name of the Environment
        /// </summary>
        [JsonProperty("name")]
        string Name { get; }

        /// <summary>
        /// The Icon of the Environment
        /// </summary>
        [JsonProperty("icon")]
        string Icon { get; }

        /// <summary>
        /// The Domains for the Environment
        /// </summary>
        [JsonProperty("domains")]
        IEnumerable<IDomain> Domains { get; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("sortOrder")]
        int SortOrder { get; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("enable")]
        bool Enable { get; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("continueProcessing")]
        bool ContinueProcessing { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="type"></param>
        /// <param name="totalPages"></param>
        /// <returns></returns>
        IEnumerable<IJournal> JournalListing(int page, int itemsPerPage, Type type, out int totalPages);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="page"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="totalPages"></param>
        /// <returns></returns>
        IEnumerable<T> JournalListing<T>(int page, int itemsPerPage, out int totalPages) where T : IJournal;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool Write();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool Delete();

    }
}
