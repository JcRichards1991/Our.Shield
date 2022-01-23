using Our.Shield.Core.Enums;
using System;
using System.Threading.Tasks;

namespace Our.Shield.Core.Services
{
    /// <summary>
    /// Service for handling Journals
    /// </summary>
    public interface IJournalService
    {
        /// <summary>
        /// Writes a Journal message to the system for an environment action
        /// </summary>
        /// <param name="environmentName">The name of the environment</param>
        /// <param name="environmentKey">The key of the environment</param>
        /// <param name="action">The type of action the user has performed</param>
        /// <returns></returns>
        Task WriteEnvironmentJournal(
            string environmentName,
            Guid environmentKey,
            JournalEnvironmentAction action);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localizedTextKey"></param>
        /// <param name="appKey"></param>
        /// <param name="environmentKey"></param>
        /// <param name="localizedTextTokens"></param>
        /// <returns></returns>
        Task WriteAppJournal(
            Guid appKey,
            Guid environmentKey,
            string localizedTextKey,
            params string[] localizedTextTokens);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="appKey"></param>
        /// <param name="environmentKey"></param>
        /// <returns></returns>
        Task WriteAppUpdateJournal(string appId, Guid appKey, Guid environmentKey);
    }
}
