using Our.Shield.Core.Attributes;
using Our.Shield.Core.Models;
using Our.Shield.Core.Persistance.Business;
using Semver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Persistence.Migrations;

namespace Our.Shield.Core.Operation
{
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

        private const int JobIdStart = 1000;             //  Starting id for Jobs

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
        /// Is this the first time our package has ran
        /// </summary>
        public bool IsFirstExecution = false;

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
        /// <param name="id">The desired Id</param>
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

        private void LoadMigrations(IApp app, ApplicationContext applicationContext)
        {
            app.Migrations = new Dictionary<string, IMigration>();

            foreach (var attribute in app.GetType().GetCustomAttributes<AppMigrationAttribute>(true))
            {
                var migration = Activator.CreateInstance(attribute.Migration, 
                    applicationContext.DatabaseContext.SqlSyntax,
                    applicationContext.ProfilingLogger.Logger) as IMigration;

                var version = migration.GetType().GetCustomAttribute<MigrationAttribute>(true);

                app.Migrations.Add(version.TargetVersion.ToString(), migration);
            }
        }

        private void RunMigrations(IApp app, ApplicationContext applicationContext)
        {
            if (!app.Migrations.Any())
            {
                return;
            }
            
            var currentVersion = new SemVersion(0, 0, 0);
            var productName = nameof(Shield) + app.Id;

            var migrations = ApplicationContext.Current.Services.MigrationEntryService.GetAll(productName);
            var latestMigration = migrations.OrderByDescending(x => x.Version).FirstOrDefault();

            if (latestMigration != null)
            {
                currentVersion = latestMigration.Version;
            }

            var latestVersion = currentVersion;

            foreach (var migration in app.Migrations)
            {
                var version = new SemVersion(new Version(migration.Key));
                if (latestVersion < version)
                {
                    latestVersion = version;
                }
            }

            if (latestVersion == currentVersion)
            {
                return;
            }

            var logger = applicationContext.ProfilingLogger.Logger;

            MigrationRunner migrationsRunner = new MigrationRunner(
                applicationContext.Services.MigrationEntryService, 
                logger, 
                currentVersion, 
                latestVersion, 
                productName, 
                app.Migrations.Values.ToArray());

            try
            {
                migrationsRunner.Execute(ApplicationContext.Current.DatabaseContext.Database);
            }
            catch (Exception ex)
            {
                logger.Error(this.GetType(), $"Error running {productName} migration", ex);
            }
        }

        /// <summary>
        /// The initializations method for the job service
        /// </summary>
        public void Init(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            var evs = DbContext.Instance.Environment.Read();

            foreach (var ev in evs)
            {
                Register(new Models.Environment(ev), applicationContext);
            }
            Poll(true);
        }

        public void Register(IEnvironment environment, ApplicationContext applicationContext)
        {
            var appIds = App<IConfiguration>.Register;
            foreach(var appId in appIds)
            {
                var app = App<IConfiguration>.Create(appId.Key);

                LoadMigrations(app, applicationContext);
                RunMigrations(app, applicationContext);

                if (app.Init())
                {
                    Register(environment, app);
                }
            }
        }

        private const long ranRepeat = 0L;
        private const long ranNow = 1L;
        private long ranTick = ranNow;

        private int runningPoll = 0;

        public void Poll(bool forceUpdate)
        {
            if (forceUpdate)
            {
                ranTick = ranRepeat;
            }
            Poll();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Poll()
        {
            if (DateTime.UtcNow.Ticks < ranTick)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref Instance.runningPoll, 0, 1) != 0)
            {
                return;
            }

            try
            {
                //System.Diagnostics.Debug.WriteLine("START running poll with " + ranTick.ToString());
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
                //System.Diagnostics.Debug.WriteLine("END running poll with " + ranTick.ToString());
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

                var config = ReadConfiguration(job, job.App.DefaultConfiguration);
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

                if (job.App.Execute(job, config))
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
        /// Writes an app configuration to the database
        /// </summary>
        /// <param name="job">the job handling the write</param>
        /// <param name="config">the configuration to write</param>
        /// <returns>True if successfully written; otherwise, False</returns>
        public bool WriteConfiguration(IJob job, IConfiguration config)
        {
            if (!DbContext.Instance.Configuration.Write(job.Environment.Id, job.App.Id, config))
            {
                return false;
            }
            
            Poll(true);

            return true;
        }

        /// <summary>
        /// writes an journal to the database
        /// </summary>
        /// <param name="job">the job handling the write</param>
        /// <param name="journal">the journal to write</param>
        /// <returns>True if successfully written; otherwise, False</returns>
        public bool WriteJournal(IJob job, IJournal journal)
        {
            return DbContext.Instance.Journal.Write(job.Environment.Id, job.App.Id, journal);
        }

        /// <summary>
        /// Reads an app configuration from the database
        /// </summary>
        /// <param name="job">the job handling the read</param>
        /// <param name="defaultConfiguration">the default configuration for the app</param>
        /// <returns>Default configuration if not stored within the database; otherwised the configuration</returns>
        public IConfiguration ReadConfiguration(IJob job, IConfiguration defaultConfiguration = null)
        {
            return DbContext.Instance.Configuration.Read(job.Environment.Id, job.App.Id, ((Job) job).ConfigType, 
                defaultConfiguration ?? App<IConfiguration>.Create(job.App.Id).DefaultConfiguration);
        }

        /// <summary>
        /// Gets a collection of journals from the database
        /// </summary>
        /// <typeparam name="T">The type of journal to read</typeparam>
        /// <param name="job">The job handling the reading</param>
        /// <param name="page">The page of journals to return</param>
        /// <param name="itemsPerPage">The number of journals to return per page</param>
        /// <param name="totalPages">The total number of pages</param>
        /// <returns></returns>
        public IEnumerable<T> ListJournals<T>(IJob job, int page, int itemsPerPage, out int totalPages) where T : IJournal =>
            DbContext.Instance.Journal.Read<T>(job.Environment.Id, job.App.Id, page, itemsPerPage, out totalPages);

        /// <summary>
        /// Adds an environment and an app as a job to the job service
        /// </summary>
        /// <param name="environment">The environment to add</param>
        /// <param name="app">The app to add</param>
        /// <returns>True if successfully added; otherwise, False</returns>
        public bool Register(IEnvironment environment, IApp app)
        {
            if (jobLock.TryEnterWriteLock(taskLockTimeout))
            {
                try
                {
                    var job = new Job
                    {
                        Id = registerCount++,
                        Environment = environment,
                        App = app,
                        ConfigType = app.GetType().BaseType.GenericTypeArguments[0]
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
        /// Removes a Job from the JobService by it's id
        /// </summary>
        /// <param name="id">The Job id</param>
        /// <returns></returns>
        public bool Unregister(int id)
        {
            if (jobLock.TryEnterWriteLock(taskLockTimeout))
            {
                try
                {
                    Job job = null;
                    if (jobs.Value.TryGetValue(id, out job))
                    {
                        if (job.Task != null && !job.Task.IsCanceled && !job.Task.IsCompleted && !job.CancelToken.IsCancellationRequested)
                        {
                            job.CancelToken.Cancel();
                        }
                        jobs.Value.Remove(id);
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
        /// Removes a job from the job service
        /// </summary>
        /// <param name="job">the job to remove</param>
        /// <returns>True if successfully removed; otherwise, False</returns>
        public bool Unregister(IJob job) => Unregister(job.Id);

        /// <summary>
        /// Removes a collection of jobs from the job service
        /// </summary>
        /// <param name="jobsToUnregister">The collection of jobs to remove</param>
        /// <returns>True if successfully removed; otherwise, False</returns>
        public bool Unregister(IEnvironment environment)
        {
            if (jobLock.TryEnterWriteLock(taskLockTimeout))
            {
                try
                {
                    var keys = new List<int>();
                    foreach (var job in jobs.Value)
                    {
                        if (job.Value.Environment.Id == environment.Id)
                        {
                            if (job.Value.Task != null && !job.Value.Task.IsCanceled && !job.Value.Task.IsCompleted && !job.Value.CancelToken.IsCancellationRequested)
                            {
                                job.Value.CancelToken.Cancel();
                            }
                            keys.Add(job.Key);
                        }
                    }
                    foreach (var key in keys)
                    {
                        jobs.Value.Remove(key);
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
        /// Removes all jobs from the job service where the app's id are the same
        /// </summary>
        /// <param name="appId">The id of the app</param>
        /// <returns>True if successfully removed; otherwise, False</returns>
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
                        if (reg.Value.App.Id == appId)
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
        /// Removes all jobs from the job service where the app are the same
        /// </summary>
        /// <param name="app">the app to remove</param>
        /// <returns>True if successfully removed; otherwise, False</returns>
        public bool Unregister(IApp app) => Unregister(app.Id);

    }
}
