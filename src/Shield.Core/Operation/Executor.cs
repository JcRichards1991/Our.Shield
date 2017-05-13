namespace Shield.Core.Operation
{
    using System;
    using System.Collections.Generic;
    using Umbraco.Core;

    public class Executor
    {
        private static readonly Lazy<Executor> _instance = new Lazy<Executor>(() => new Executor());

        private Executor()
        {
        }

        // accessor for instance
        public static Executor Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        public void Init()
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;
            var ops = Models.Operation<Models.Configuration>.Register;

            foreach(var op in ops)
            {
                var o = Models.Operation<Models.Configuration>.Create(op.Key);

                if(o.Init())
                {
                    Register(o);
                    var config = ReadConfiguration(o.Id, o.DefaultConfiguration);

                    if (config != null && config.Enable)
                    {
                        o.Execute(config);
                    }
                }  
            }
        }

        public bool WriteConfiguration(string id, Models.Configuration config)
        {
            return Persistance.Bal.ConfigurationContext.Write(id, config);
        }

        public bool WriteJournal(string id, Models.Journal journal)
        {
            return Persistance.Bal.JournalContext.Write(id, journal);
        }

        public Models.Configuration ReadConfiguration(string id, Models.Configuration defaultConfiguration)
        {
            return Persistance.Bal.ConfigurationContext.Read(id,
                    Models.Operation<Models.Configuration>.Register[id].BaseType.GenericTypeArguments[0], defaultConfiguration);
        }

        public IEnumerable<Models.Journal> ReadJournals(string id, int page, int itemsPerPage)
        {
            return Persistance.Bal.JournalContext.Read(id, page, itemsPerPage,
                Models.Operation<Models.Configuration>.Register[id].BaseType.GenericTypeArguments[1]);
        }

        public bool Execute(string id, Models.Configuration config = null)
        {
            var o = Models.Operation<Models.Configuration>.Create(id);

            if (o == null)
            {
                return false;
            }

            if (config == null)
            {
                config = ReadConfiguration(id, o.DefaultConfiguration);

                if (config == null)
                {
                    return false;
                }
            }

            return o.Execute(config);
        }

        
        private static readonly Lazy<IDictionary<string, Models.Interfaces.IOperation>> _register = 
            new Lazy<IDictionary<string, Models.Interfaces.IOperation>>(() => new Dictionary<string, Models.Interfaces.IOperation>());

        public bool Register(Models.Interfaces.IOperation o)
        {
            lock (_register)
            {
                if (!_register.Value.ContainsKey(o.Id))
                {
                    _register.Value.Add(o.Id, o);
                    return true;
                }
            }
            return false;
        }

        public bool Unregister(string id)
        {
            lock (_register)
            {
                if (_register.Value.ContainsKey(id))
                {
                    _register.Value.Remove(id);
                    return true;
                }
            }
            return false;
        }

        public bool Unregister(Models.Interfaces.IOperation o)
        {
            lock (_register)
            {
                if (_register.Value.ContainsKey(o.Id))
                {
                    _register.Value.Remove(o.Id);
                    return true;
                }
            }
            return false;
        }

    }
}
