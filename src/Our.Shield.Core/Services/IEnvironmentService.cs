using Our.Shield.Core.Models.Requests;
using Our.Shield.Core.Models.Responses;
using System;
using System.Threading.Tasks;

namespace Our.Shield.Core.Services
{
    /// <summary>
    /// The Environment Service interface
    /// </summary>
    public interface IEnvironmentService
    {
        /// <summary>
        /// Updates or inserts the environment into the system and updates the database
        /// </summary>
        /// <param name="request"><see cref="UpsertEnvironmentRequest"/></param>
        /// <returns>True if successfully written; otherwise, False</returns>
        Task<UpsertEnvironmentResponse> Upsert(UpsertEnvironmentRequest request);

        /// <summary>
        /// Gets all the Environments from the database
        /// </summary>
        /// <returns>A collection of Environments in the database</returns>
        Task<GetEnvironmentsResponse> Get();

        /// <summary>
        /// Gets an Environment by it's Key from the database
        /// </summary>
        /// <param name="key">The key of the environment to retrieve</param>
        /// <returns>The environment with corresponding Key, otherwise null</returns>
        Task<GetEnvironmentResponse> Get(Guid key);

        /// <summary>
        /// Deletes an environment from the system and database
        /// </summary>
        /// <param name="Key">The key of the environment to delete</param>
        /// <returns></returns>
        Task<DeleteEnvironmentResponse> Delete(Guid Key);

        /// <summary>
        /// Sorts the environments in the given order
        /// </summary>
        /// <param name="request"><see cref="UpdateEnvironmentsSortOrderRequest"/></param>
        /// <returns></returns>
        Task<BaseResponse> SortEnvironments(UpdateEnvironmentsSortOrderRequest request);
    }
}
