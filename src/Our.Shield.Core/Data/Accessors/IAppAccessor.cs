using Our.Shield.Core.Data.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Our.Shield.Core.Data.Accessors
{
    public interface IAppAccessor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        Task<Guid> Create(App app);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<App>> Read();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<App> Read(Guid key);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="environmentKey"></param>
        /// <returns></returns>
        Task<App> Read(string appName, Guid environmentKey);

        /// <summary>
        /// Gets all the apps associated to an environment
        /// </summary>
        /// <param name="environmentKey">The key of the environment</param>
        /// <returns>List of apps associated to the environment</returns>
        Task<IReadOnlyList<App>> ReadByEnvironment(Guid environmentKey);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="App"></param>
        /// <returns></returns>
        Task<bool> Update(App App);
    }
}
