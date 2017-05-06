using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;

namespace Shield.Core.Operation
{
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
            var ops = Operation<Configuration>.Register;

            foreach(var op in ops)
            {
                var o = Operation<Configuration>.Create(op.Key);

                if(o.Init())
                {
                    Register(o);
                    var record = db.SingleOrDefault<Persistance.Dal.Configuration>((object) o.Id);

                    if (record != null && record.Enable)
                    {
                        var sc = (Configuration) JsonConvert.DeserializeObject(record.Value, op.Value.GenericTypeArguments[0].GetType());
                        o.Execute(sc);
                    }
                }  
            }
        }

        public bool Save(string id, bool enable, Configuration config)
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;
            var record = new Persistance.Dal.Configuration()
            {
                Id = id,
                LastModified = DateTime.UtcNow,
                Enable = enable,
                Value = JsonConvert.SerializeObject(config)
            };

            if (db.Exists<Persistance.Dal.Configuration>(id))
            {
                db.Update(record);
            }
            else
            {
                db.Insert(record);
            }
            return true;
        }

        public bool Execute(string id, Configuration config = null)
        {
            var o = Operation<Configuration>.Create(id);

            if (o == null)
            {
                return false;
            }

            if (config == null)
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                var record = db.SingleOrDefault<Persistance.Dal.Configuration>((object) id);
                if (record == null)
                {
                    return false;
                }
                config = (Configuration) JsonConvert.DeserializeObject(record.Value, 
                    Operation<Configuration>.Register[id].GenericTypeArguments[0].GetType());
                if (config == null)
                {
                    return false;
                }
            }

            return o.Execute(config);
        }

        private static readonly Lazy<IDictionary<string, IOperation>> _register = 
            new Lazy<IDictionary<string, IOperation>>(() => new Dictionary<string, IOperation>());

        public bool Register(IOperation o)
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

        public bool Unregister(IOperation o)
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
