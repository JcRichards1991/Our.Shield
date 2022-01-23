using Our.Shield.Core.Data.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Our.Shield.Core.Data.Accessors
{
    /// <summary>
    /// Accessor for reading/writing to the database for apps
    /// </summary>
    public interface IAppAccessor
    {
        /// <summary>
        /// Bulk writes multiple Apps to the database
        /// </summary>
        /// <param name="apps"></param>
        void Write(IEnumerable<App> apps);

        /// <summary>
        /// Reads an App by it's Key
        /// </summary>
        /// <param name="key">The key of the app to get</param>
        /// <returns></returns>
        Task<App> Read(Guid key);

        /// <summary>
        /// Gets an app by the App's Name and Environment's KEy
        /// </summary>
        /// <param name="appName">The App Name for the environment to get</param>
        /// <param name="environmentKey">The key of the environment to get the app for</param>
        /// <returns></returns>
        Task<App> Read(string appName, Guid environmentKey);

        /// <summary>
        /// Gets all the apps associated to an environment
        /// </summary>
        /// <param name="environmentKey">The key of the environment</param>
        /// <returns>List of apps associated to the environment</returns>
        Task<IReadOnlyCollection<IApp>> ReadByEnvironmentKey(Guid environmentKey);

        /// <summary>
        /// Updates the App's Configuration by the App's Key
        /// </summary>
        /// <param name="appId">Id of the app</param>
        /// <param name="key">Key of the app to update</param>
        /// <param name="configuration">The configuration for the App</param>
        /// <returns></returns>
        Task<bool> Update(string appId, Guid key, Models.IAppConfiguration configuration);
    }
}
