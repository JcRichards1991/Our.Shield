using Our.Shield.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Our.Shield.Core.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEnvironmentService
    {
        /// <summary>
        /// Updates or inserts the environment into the system and updates the database
        /// </summary>
        /// <param name="environment">The environment to write</param>
        /// <returns>True if successfully written; otherwise, False</returns>
        Task<bool> Upsert(IEnvironment environment);

        /// <summary>
        /// Gets all the Environments from the database
        /// </summary>
        /// <returns>A collection of Environments in the database</returns>
        Task<IReadOnlyList<IEnvironment>> Get();

        /// <summary>
        /// Gets an Environment by it's Key from the database
        /// </summary>
        /// <param name="key">The key of the environment to retrieve</param>
        /// <returns>The environment with corresponding Key, otherwise null</returns>
        Task<IEnvironment> Get(Guid key);

        /// <summary>
        /// Deletes an environment from the system and database
        /// </summary>
        /// <param name="environment">The environment to delete</param>
        /// <returns>Whether or not the environment was successfully removed from the system and database</returns>
        Task<bool> Delete(Models.IEnvironment environment);
    }
}
