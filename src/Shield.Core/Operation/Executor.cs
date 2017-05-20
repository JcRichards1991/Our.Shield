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
            public Models.Interfaces.IOperation Operation;
            public DateTime? LastRan;
            public Task<bool> Task;
            public CancellationTokenSource CancelToken;
        }

        private static readonly Lazy<IDictionary<string, ExecuteStatus>> _register = 
            new Lazy<IDictionary<string, ExecuteStatus>>(() => 
            new Dictionary<string, ExecuteStatus>());

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

                var config = es.Operation.ReadConfiguration();
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

                if (es.Operation.Execute(config))
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

        public bool WriteConfiguration(string id, Models.Configuration config)
        {
            if (!Persistance.Bal.ConfigurationContext.Write(id, config))
            {
                return false;
            }
            
            ranTick = ranRepeat;
            Poll();

            return true;
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

        
        public bool Register(Models.Interfaces.IOperation o)
        {
            if (registerLock.TryEnterWriteLock(taskLockTimeout))
            {
                try
                {
                    if (!_register.Value.ContainsKey(o.Id))
                    {
                        _register.Value.Add(o.Id, new ExecuteStatus
                        {
                            Operation = o
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

        public bool Unregister(string id)
        {
            if (registerLock.TryEnterWriteLock(taskLockTimeout))
            {
                try
                {
                    ExecuteStatus es = null;
                    if (_register.Value.TryGetValue(id, out es))
                    {
                        if (es.Task != null && !es.Task.IsCanceled && !es.Task.IsCompleted && !es.CancelToken.IsCancellationRequested)
                        {
                            es.CancelToken.Cancel();
                        }
                        _register.Value.Remove(id);
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

        public bool Unregister(Models.Interfaces.IOperation o)
        {
            return Unregister(o.Id);
        }

    }
}
