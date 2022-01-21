using Our.Shield.Core.Models;
using System;
using System.Collections.Generic;

namespace Our.Shield.Core.Factories
{
    /// <summary>
    /// Factory for creating <see cref="App{TC}"/>
    /// </summary>
    public interface IAppFactory
    {
        /// <summary>
        /// Gets the registered App Ids installed in the system
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetRegistedAppsIds();

        /// <summary>
        /// Creates an <see cref="App{TC}"/>
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        IApp Create(string appId);

        /// <summary>
        /// Creates an <see cref="App{TC}"/>
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        IApp Create(string appId, Guid key);
    }
}
