namespace Shield.Core.Operation
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Umbraco.Core;

    public class Executor
    {
        private const int taskLockTimeout = 1000;       //  in millisecs
#if DEBUG
        private const int poll = 60;                    //  in secs
#else
        private const int poll = 60 * 10;               //in secs
#endif

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

        private static ReaderWriterLockSlim registerLock = new ReaderWriterLockSlim();

        private class ExecuteStatus
        {
            public Persistance.Data.Dto.Environment Environment;
            public IApp App;
            public DateTime? LastRan;
            public Task<bool> Task;
            public CancellationTokenSource CancelToken;
        }

        private static readonly Lazy<IDictionary<string, ExecuteStatus>> _register = 
            new Lazy<IDictionary<string, ExecuteStatus>>(() => 
            new Dictionary<string, ExecuteStatus>());

        private string RegisterKey(Persistance.Data.Dto.Environment ev, IApp app)
        {
            return ev.Id.ToString() + ":" + app.Name;
        }

        public void Init()
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;

            var evs = Persistance.Business.EnvironmentContext.List();
            var apps = Operation.App<Persistance.Serialization.Configuration>.Register;

            foreach (var ev in evs)
            {
                foreach(var app in apps)
                {
                    var a = Operation.App<Persistance.Serialization.Configuration>.Create(app.Key);

                    if(a.Init())
                    {
                        Register(ev, a);
                    }  
                }
            }
            Poll();
            
        }

        private const long ranRepeat = 0L;
        private const long ranNow = 1L;
        private static long ranTick = ranNow;

        private static int runningPoll = 0;

        public void Poll()
        {
            if (DateTime.UtcNow.Ticks < ranTick)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref runningPoll, 0, 1) != 0)
            {
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("START running poll with " + ranTick.ToString());
                ranTick = ranNow;
                if (registerLock.TryEnterUpgradeableReadLock(taskLockTimeout))
                {
                    try
                    {
                        foreach (var reg in _register.Value)
                        {
                            var ct = new CancellationTokenSource();
                            var task = new Task<bool>(() => Execute(reg.Value), ct.Token, TaskCreationOptions.PreferFairness);

                            if (registerLock.TryEnterWriteLock(taskLockTimeout))
                            {
                                try
                                {
                                    reg.Value.CancelToken = ct;
                                    reg.Value.Task = task;
                                    task.Start();
                                }
                                finally
                                {
                                    registerLock.ExitWriteLock();
                                }
                            }
                            else
                            {
                                task.Dispose();
                            }
                        }
                    }
                    finally
                    {
                        registerLock.ExitUpgradeableReadLock();
                    }
                }
                if (Interlocked.CompareExchange(ref ranTick, ranRepeat, ranNow) != ranRepeat)
                {
                    ranTick = DateTime.UtcNow.AddSeconds(poll).Ticks;
                }
                System.Diagnostics.Debug.WriteLine("END running poll with " + ranTick.ToString());
            }
            finally
            {
                runningPoll = 0;
            }
        }

        private bool Execute(ExecuteStatus es)
        {
            try
            {
                es.CancelToken.Token.ThrowIfCancellationRequested();

                var config = es.App.ReadConfiguration();
                if (registerLock.TryEnterReadLock(taskLockTimeout))
                {
                    try
                    {
                        if (es.LastRan != null && config.LastModified < es.LastRan)
                        {
                            return true;
                        }
                    }
                    finally
                    {
                        registerLock.ExitReadLock();
                    }
                }

                if (es.App.Execute(config))
                {
                    if (registerLock.TryEnterWriteLock(taskLockTimeout))
                    {
                        try
                        {
                            es.LastRan = DateTime.UtcNow;
                            return true;
                        }
                        finally
                        {
                            registerLock.ExitWriteLock();
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //  Ignore cancel
            }
            return false;
        }

        public bool WriteConfiguration(Persistance.Data.Dto.Environment environment, IApp app, Persistance.Serialization.Configuration config)
        {
            if (!Persistance.Business.ConfigurationContext.Write((int) environment.Id, app.Id, config))
            {
                return false;
            }
            
            ranTick = ranRepeat;
            Poll();

            return true;
        }

        public bool WriteJournal(Persistance.Data.Dto.Domain domain, IApp app, Persistance.Serialization.Journal journal)
        {
            return Persistance.Business.JournalContext.Write((int) domain.Id, app.Id, journal);
        }

        public Persistance.Serialization.Configuration ReadConfiguration(Persistance.Data.Dto.Environment environment, IApp app, 
            Persistance.Serialization.Configuration defaultConfiguration)
        {
            return Persistance.Business.ConfigurationContext.Read((int) environment.Id, app.Id,
                    Operation.App<Persistance.Serialization.Configuration>.Register[app.Id].BaseType.GenericTypeArguments[0], defaultConfiguration);
        }

        public IEnumerable<Persistance.Serialization.Journal> ReadJournals(Persistance.Data.Dto.Environment environment, IApp app, 
            int page, int itemsPerPage)
        {
            return Persistance.Business.JournalContext.Read((int) environment.Id, app.Id, page, itemsPerPage,
                Operation.App<Persistance.Serialization.Configuration>.Register[app.Id].BaseType.GenericTypeArguments[1]);
        }

        public bool Execute(Persistance.Data.Dto.Environment environment, IApp app, Persistance.Serialization.Configuration config = null)
        {
            var o = Operation.App<Persistance.Serialization.Configuration>.Create(app.Id);

            if (o == null)
            {
                return false;
            }

            if (config == null)
            {
                config = ReadConfiguration(environment, app, o.DefaultConfiguration);

                if (config == null)
                {
                    return false;
                }
            }

            return o.Execute(config);
        }

        
        public bool Register(Persistance.Data.Dto.Environment e, IApp a)
        {
            if (registerLock.TryEnterWriteLock(taskLockTimeout))
            {
                try
                {
                    var key = RegisterKey(e, a);
                    if (!_register.Value.ContainsKey(key))
                    {
                        _register.Value.Add(key, new ExecuteStatus
                        {
                            Environment = e,
                            App = a
                        });
                        return true;
                    }
                }
                finally
                {
                    registerLock.ExitWriteLock();
                }
            }
            return false;
        }

        public bool Unregister(Persistance.Data.Dto.Environment e , IApp a)
        {
            if (registerLock.TryEnterWriteLock(taskLockTimeout))
            {
                try
                {
                    ExecuteStatus es = null;
                    var key = RegisterKey(e, a);
                    if (_register.Value.TryGetValue(key, out es))
                    {
                        if (es.Task != null && !es.Task.IsCanceled && !es.Task.IsCompleted && !es.CancelToken.IsCancellationRequested)
                        {
                            es.CancelToken.Cancel();
                        }
                        _register.Value.Remove(key);
                        return true;
                    }
                }
                finally
                {
                    registerLock.ExitWriteLock();
                }
            }
            return false;
        }

        public bool Unregister(IApp a)
        {
            if (registerLock.TryEnterWriteLock(taskLockTimeout))
            {
                try
                {
                    var removeItems = new List<string>();
                    ExecuteStatus es = null;
                    foreach (var reg in _register.Value)
                    {
                        if (reg.Value.App.Id == a.Id)
                        {
                            if (es.Task != null && !es.Task.IsCanceled && !es.Task.IsCompleted && !es.CancelToken.IsCancellationRequested)
                            {
                                es.CancelToken.Cancel();
                            }
                            removeItems.Add(reg.Key);
                        }
                    }
                    foreach (var item in removeItems)
                    {
                        _register.Value.Remove(item);
                    }
                    return true;
                }
                finally
                {
                    registerLock.ExitWriteLock();
                }
            }
            return false;
        }

    }
}
