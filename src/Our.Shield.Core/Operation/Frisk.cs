﻿using Our.Shield.Core.Models;
using Our.Shield.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Our.Shield.Core.Operation
{
    /// <summary>
    /// 
    /// </summary>
    public static class Frisk
    {

        private static readonly Type[] Interests =
        {
            typeof(App<IAppConfiguration>)
        };

        private static void RegisterAssembly(Assembly currAssembly, ref Dictionary<string, Dictionary<string, Type>> installed)
        {
            Type[] typesInAsm;
            try
            {
                typesInAsm = currAssembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                typesInAsm = ex.Types;
            }

            foreach (var type in typesInAsm)
            {
                if (type == null || !type.IsClass || type.IsAbstract)
                {
                    continue;
                }

                foreach (var interest in Interests)
                {
                    if ((type.BaseType == null || type.BaseType.Name != interest.Name ||
                         type.BaseType.Namespace != interest.Namespace) && !type.IsSubclassOf(interest))
                    {
                        continue;
                    }

                    IFrisk derivedObject = null;
                    if (type.ContainsGenericParameters)
                    {
                        //TEST CODE - Not needed currently, keep for now


                        //var cn = type.GetTypeInfo().GenericTypeParameters[0].Namespace + "." + type.GetTypeInfo().GenericTypeParameters[0].Name;
                        //var ci = Activator.CreateInstance(currAssembly.FullName, cn);

                        //var pn = type.GetTypeInfo().Namespace + "." + type.GetTypeInfo().Name;

                        //var t = type.GetGenericTypeDefinition().Name;
                        //var t1 = type.GetTypeInfo().GenericTypeParameters;
                        //Type tt = Type.GetType(pn).MakeGenericType(ci.GetType());

                        //var f = Activator.CreateInstance(tt);
                               

                        //derivedObject = System.Activator.CreateInstance(type.GetGenericTypeDefinition().MakeGenericType(type.GetTypeInfo().GenericTypeParameters)) as IFrisk;
                    }
                    else
                    {
                        var constructors = type.GetConstructors();

                        if (constructors.None())
                        {
                            derivedObject = Activator.CreateInstance(type) as IFrisk;
                        }
                        else
                        {
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

                                derivedObject = constructor.Invoke(objectList.ToArray()) as IFrisk;
                            }
                        }
                    }

                    if (derivedObject != null && !string.IsNullOrEmpty(interest.FullName))
                    {
                        installed[interest.FullName].Add(derivedObject.Id, derivedObject.GetType());
                    }
                }

            }
        }

        private static readonly Lazy<Dictionary<string, Dictionary<string, Type>>> RegisteredInterests = new Lazy<Dictionary<string, Dictionary<string, Type>>>(() =>
        {
            var installed = Interests.ToDictionary(key => key.FullName, value => new Dictionary<string, Type>());
            var filenames = new HashSet<string>();

            //  First read those in current domain
            foreach (var currAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!currAssembly.IsDynamic)
                {
                    filenames.Add(currAssembly.Location.ToLowerInvariant());
                }
                RegisterAssembly(currAssembly, ref installed);
            }

            //  Now see if any dlls haven't been loaded yet
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var di = new DirectoryInfo(path);
            foreach (var file in di.GetFiles("*.dll"))
            {
                if (filenames.Contains(file.FullName.ToLowerInvariant()))
                {
                    continue;
                }

                try
                {
                    var currAssembly = Assembly.ReflectionOnlyLoadFrom(file.FullName);
                    if (!currAssembly.IsDynamic)
                    {
                        filenames.Add(currAssembly.Location.ToLowerInvariant());
                    }
                    RegisterAssembly(currAssembly, ref installed);
                }
                catch (BadImageFormatException)
                {
                    // Not a .net assembly - ignore
                }
            }

            return installed;
        });

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDictionary<string, Type> GetRegistedInterestTypes<T>()
        {
            var typeFullName = typeof(T).FullName;

            return string.IsNullOrEmpty(typeFullName)
                ? new Dictionary<string, Type>()
                : RegisteredInterests.Value.TryGetValue(typeFullName, out var results)
                    ? results
                    : new Dictionary<string, Type>();
        }
    }
}
