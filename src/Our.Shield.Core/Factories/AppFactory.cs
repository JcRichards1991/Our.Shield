using Our.Shield.Core.Models;
using System;
using System.Collections.Generic;

namespace Our.Shield.Core.Factories
{
    /// <summary>
    /// Implements <see cref="IAppFactory"/>
    /// </summary>
    public class AppFactory : IAppFactory
    {
        /// <summary>
        /// 
        /// </summary>
        public static Lazy<IDictionary<string, Type>> RegisteredApps = new Lazy<IDictionary<string, Type>>(() =>
        {
            return Operation.Frisk.GetRegistedInterestTypes<App<IAppConfiguration>>();
        });

        /// <inheritdoc />
        public IEnumerable<string> GetRegistedAppsIds() => RegisteredApps.Value.Keys;

        /// <inheritdoc />
        public IApp Create(string appId)
        {
            var types = RegisteredApps;

            if (!types.Value.TryGetValue(appId, out var derivedType))
            {
                return null;
            }

            return Activator.CreateInstance(derivedType) as IApp;
        }

        /// <inheritdoc />
        public IApp Create(string appId, Guid appKey)
        {
            var app = Create(appId);

            if (app == null)
            {
                return null;
            }

            app.Key = appKey;

            return app;
        }
    }
}
