using Our.Shield.Core.Models;
using Our.Shield.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Our.Shield.Core.Factories
{
    /// <summary>
    /// Implements <see cref="IAppFactory"/>
    /// </summary>
    public class AppFactory : IAppFactory
    {
        private static Lazy<IDictionary<string, Type>> RegisteredApps = new Lazy<IDictionary<string, Type>>(() =>
        {
            return Operation.Frisk.GetRegistedInterestTypes<App<IAppConfiguration>>();
        });

        /// <inheritdoc />
        public IEnumerable<string> GetRegistedAppsIds() => RegisteredApps.Value.Keys;

        /// <inheritdoc />
        public IApp Create(string appId)
         {
            if (!RegisteredApps.Value.TryGetValue(appId, out var derivedType))
            {
                return null;
            }

            var constructors = derivedType.GetTypeInfo().DeclaredConstructors;

            if (constructors.HasValues())
            {
                //  Use the first constructor we find
                foreach (var constructor in constructors)
                {
                    var objectList = new List<object>();
                    var constructorParameters = constructor.GetParameters();

                    if (constructorParameters.HasValues())
                    {
                        foreach (var constructorParameter in constructorParameters)
                        {
                            objectList.Add(Umbraco.Core.Composing.Current.Factory.GetInstance(constructorParameter.ParameterType));
                        }
                    }

                    return constructor.Invoke(objectList.ToArray()) as IApp;
                }
            }

            return Activator.CreateInstance(derivedType) as IApp;
        }
    }
}
