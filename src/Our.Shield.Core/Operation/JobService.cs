namespace Our.Shield.Core.Operation
{
    using Models;
    using Persistance.Business;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    internal class JobService
    {
        private const int taskLockTimeout = 1000;       //  in millisecs

#if DEBUG
        private const int poll = 60;                    //  in secs
#else
        private const int poll = 60 * 10;               //in secs
#endif

        public const int JobIdStart = 1000;             //  Starting id for Jobs

        private static readonly Lazy<JobService> _instance = new Lazy<JobService>(() => new JobService());

        private JobService()
        {
        }

        /// <summary>
        /// Accessor for instance
        /// </summary>
        public static JobService Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private static ReaderWriterLockSlim jobLock = new ReaderWriterLockSlim();

        private static readonly Lazy<IDictionary<int, Job>> jobs = 
            new Lazy<IDictionary<int, Job>>(() => 
            new Dictionary<int, Job>());

        private int registerCount = JobIdStart;

        /// <summary>
        /// Get a list of environments that have been installed
        /// </summary>
        public IDictionary<IEnvironment, IList<IJob>> Environments
        {
            get
            {
                if (jobLock.TryEnterReadLock(taskLockTimeout))
                {
                    try
                    {
                        var results = new Dictionary<IEnvironment, IList<IJob>>();
                        foreach (var kvp in jobs.Value)
                        {
                            IList<IJob> jobs = null;
                            if (!results.TryGetValue(kvp.Value.Environment, out jobs))
                            {
                                jobs = new List<IJob>();
                                results.Add(kvp.Value.Environment, jobs);
                            }
                            jobs.Add(kvp.Value.DeepCopy());
                        }

                        return results;
                    }
                    finally
                    {
                        jobLock.ExitReadLock();
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the job for a given id
        /// </summary>
        /// <param name="id">
        /// The desired Id
        /// </param>
        /// <returns></returns>
        public IJob Job(int id)
        {
            if (jobLock.TryEnterReadLock(taskLockTimeout))
            {
                try
                {
                    return jobs.Value[id].DeepCopy();
                }
                finally
                {
                    jobLock.ExitReadLock();
                }
            }
            return null;
        }                    

        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            var evs = DbContext.Instance.Environment.List();
            var appIds = App<IConfiguration>.Register;

            foreach (var ev in evs)
            {
                var enviroment = new Models.Environment(ev);
                foreach(var appId in appIds)
                {
                    var app = App<IConfiguration>.Create(appId.Key);

                    if(app.Init())
                    {
                        Register(enviroment, app);
                    }
                }
            }
            Poll();
        }

        private const long ranRepeat = 0L;
        private const long ranNow = 1L;
        private static long ranTick = ranNow;

        private static int runningPoll = 0;

        /// <summary>
        /// 
        /// </summary>
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
                if (jobLock.TryEnterUpgradeableReadLock(taskLockTimeout))
                {
                    try
                    {
                        foreach (var reg in jobs.Value)
                        {
                            var ct = new CancellationTokenSource();
                            var task = new Task<bool>(() => Execute(reg.Value), ct.Token, TaskCreationOptions.PreferFairness);

                            if (jobLock.TryEnterWriteLock(taskLockTimeout))
                            {
                                try
                                {
                                    reg.Value.CancelToken = ct;
                                    reg.Value.Task = task;
                                    task.Start();
                                }
                                finally
                                {
                                    jobLock.ExitWriteLock();
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
                        jobLock.ExitUpgradeableReadLock();
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

        private bool Execute(Job job)
        {
            try
            {
                job.CancelToken.Token.ThrowIfCancellationRequested();

                var app = App<IConfiguration>.Create(job.AppId);
                if (app == null)
                {
                    return false;
                }

                var config = ReadConfiguration(job, app.DefaultConfiguration);
                if (jobLock.TryEnterReadLock(taskLockTimeout))
                {
                    try
                    {
                        if (job.LastRan != null && config.LastModified < job.LastRan)
                        {
                            return true;
                        }
                    }
                    finally
                    {
                        jobLock.ExitReadLock();
                    }
                }

                if (app.Execute(job, config))
                {
                    if (jobLock.TryEnterWriteLock(taskLockTimeout))
                    {
                        try
                        {
                            job.LastRan = DateTime.UtcNow;
                            return true;
                        }
                        finally
                        {
                            jobLock.ExitWriteLock();
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool WriteConfiguration(IJob job, IConfiguration config)
        {
            if (!DbContext.Instance.Configuration.Write(job.Environment.Id, job.AppId, config))
            {
                return false;
            }
            
            ranTick = ranRepeat;
            Poll();

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="journal"></param>
        /// <returns></returns>
        public bool WriteJournal(IJob job, IJournal journal)
        {
            return DbContext.Instance.Journal.Write(job.Environment.Id, job.AppId, journal);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="defaultConfiguration"></param>
        /// <returns></returns>
        public IConfiguration ReadConfiguration(IJob job, IConfiguration defaultConfiguration = null)
        {
            return DbContext.Instance.Configuration.Read(job.Environment.Id, job.AppId, ((Job) job).ConfigType, 
                defaultConfiguration ?? App<IConfiguration>.Create(job.AppId).DefaultConfiguration);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="job"></param>
        /// <param name="page"></param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        public IEnumerable<T> ListJournals<T>(IJob job, int page, int itemsPerPage) where T : IJournal
        {
            return DbContext.Instance.Journal.List<T>(job.Environment.Id, job.AppId, page, itemsPerPage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool Execute(IJob job, IConfiguration config = null)
        {
            var app = App<IConfiguration>.Create(job.AppId);

            if (app == null)
            {
                return false;
            }

            if (config == null)
            {
                config = ReadConfiguration(job, app.DefaultConfiguration);

                if (config == null)
                {
                    return false;
                }
            }

            return app.Execute(job, config);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public bool Register(IEnvironment e, string appId)
        {
            if (jobLock.TryEnterWriteLock(taskLockTimeout))
            {
                try
                {
                    var appType = App<IConfiguration>.Register[appId];
                    var job = new Job
                    {
                        Id = registerCount++,
                        Environment = e,
                        AppId = appId,
                        AppType = appType,
                        ConfigType = appType.BaseType.GenericTypeArguments[0]
                    };

                    jobs.Value.Add(job.Id, job);
                    return true;
                }
                finally
                {
                    jobLock.ExitWriteLock();
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="app"></param>
        /// <returns></returns>
        public bool Register(IEnvironment e, IApp app) => Register(e, app.Id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Unregister(int key)
        {
            if (jobLock.TryEnterWriteLock(taskLockTimeout))
            {
                try
                {
                    Job job = null;
                    if (jobs.Value.TryGetValue(key, out job))
                    {
                        if (job.Task != null && !job.Task.IsCanceled && !job.Task.IsCompleted && !job.CancelToken.IsCancellationRequested)
                        {
                            job.CancelToken.Cancel();
                        }
                        jobs.Value.Remove(key);
                        return true;
                    }
                }
                finally
                {
                    jobLock.ExitWriteLock();
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public bool Unregister(IJob job) => Unregister(job.Id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public bool Unregister(string appId)
        {
            if (jobLock.TryEnterWriteLock(taskLockTimeout))
            {
                try
                {
                    var removeItems = new List<int>();
                    Job job = null;
                    foreach (var reg in jobs.Value)
                    {
                        if (reg.Value.AppId == appId)
                        {
                            if (job.Task != null && !job.Task.IsCanceled && !job.Task.IsCompleted && !job.CancelToken.IsCancellationRequested)
                            {
                                job.CancelToken.Cancel();
                            }
                            removeItems.Add(reg.Key);
                        }
                    }
                    foreach (var item in removeItems)
                    {
                        jobs.Value.Remove(item);
                    }
                    return true;
                }
                finally
                {
                    jobLock.ExitWriteLock();
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public bool Unregister(IApp app) => Unregister(app.Id);

    }
}
