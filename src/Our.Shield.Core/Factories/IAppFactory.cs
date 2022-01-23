using Our.Shield.Core.Models;
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
    }
}
