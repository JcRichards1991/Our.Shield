using Our.Shield.Core.Models;
using Our.Shield.Core.Models.Requests;
using Our.Shield.Core.Models.Responses;
using System;
using System.Threading.Tasks;

namespace Our.Shield.Core.Services
{
    /// <summary>
    /// Service for handling <see cref="IApp"/>
    /// </summary>
    public interface IAppService
    {
        /// <summary>
        /// Gets an app by it's key with it's configuration
        /// </summary>
        /// <param name="key">The key of the app to return</param>
        /// <returns></returns>
        Task<GetAppResponse> GetApp(Guid key);

        /// <summary>
        /// Gets the apps for an environment with their configuration
        /// </summary>
        /// <param name="environmentKey">The environment key for the apps to return for</param>
        /// <returns></returns>
        Task<GetAppsResponse> GetApps(Guid environmentKey);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<UpdateAppConfigurationResponse> UpdateAppConfiguration(UpdateAppConfigurationRequest request);
    }
}
