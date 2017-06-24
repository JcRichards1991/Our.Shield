namespace Shield.Core.Operation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    public interface IFrisk
    {
        string Id { get; }
    }
    
    public static class Frisk
    {

        private static readonly Type[] interests = new Type[]
        {
            typeof(Operation.Operation<Persistance.Serialization.Configuration>),
            typeof(UI.TreeNode)
        };

        private static void registerAssembly(Assembly currAssembly, ref Dictionary<string, Dictionary<string, Type>> installed)
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

            foreach (Type type in typesInAsm)
            {
                if (type == null || !type.IsClass || type.IsAbstract)
                {
                    continue;
                }

                foreach (var interest in interests)
                {
                    if ((type.BaseType != null && type.BaseType.Name == interest.Name && type.BaseType.Namespace == interest.Namespace) || type.IsSubclassOf(interest))
                    {
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
                            derivedObject = System.Activator.CreateInstance(type) as IFrisk;
                        }

                        if (derivedObject != null)
                        {
                            installed[interest.FullName].Add(derivedObject.Id, derivedObject.GetType());
                        }
                    }
                }

            }
        }

        private static readonly Lazy<Dictionary<string, Dictionary<string, Type>>> register =
            new Lazy<Dictionary<string, Dictionary<string, Type>>>(() =>
            {
                var installed = new Dictionary<string, Dictionary<string, Type>>();
                foreach (var interest in interests)
                {
                    installed.Add(interest.FullName, new Dictionary<string, Type>());
                }
                var filenames = new HashSet<string>();

                //  First read those in current domain
                foreach (Assembly currAssembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (!currAssembly.IsDynamic)
                    {
                        filenames.Add(currAssembly.Location.ToLowerInvariant());
                    }
                    registerAssembly(currAssembly, ref installed);
                }

                //  Now see if any dlls haven't been loaded yet
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

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
                        registerAssembly(currAssembly, ref installed);
                    }
                    catch (BadImageFormatException)
                    {
                        // Not a .net assembly  - ignore
                    }
                }
                return installed;
            });

        public static IDictionary<string, Type> Register<T>()
        {
            Dictionary<string, Type> results;
            if (register.Value.TryGetValue(typeof(T).FullName, out results))
            {
                return results;
            }
            return new Dictionary<string, Type>();  //  Return empty dictionary
        }
    }
}
