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
#if DEBUG
        private const int PollSecs = 60000;                    //  in secs
#else
        private const int PollSecs = 60 * 10;               //in secs
#endif

        private const int JobIdStart = 1000;             //  Starting id for Jobs

        private static readonly Lazy<JobService> JobServiceInstance = new Lazy<JobService>(() => new JobService());

        private JobService()
        {
        }

        /// <summary>
        /// Accessor for instance
        /// </summary>
        public static JobService Instance =>
            JobServiceInstance.Value;

        private static readonly Locker JobLock = new Locker();

        private static readonly Lazy<IDictionary<int, Job>> Jobs = 
            new Lazy<IDictionary<int, Job>>(() => 
            new Dictionary<int, Job>());

        private int _registerCount = JobIdStart;

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
				return JobLock.Read(() => 
				{
	                var results = new Dictionary<IEnvironment, IList<IJob>>();
                    foreach (var kvp in Jobs.Value)
                    {
                        if (!results.TryGetValue(kvp.Value.Environment, out var jobs))
                        {
                            jobs = new List<IJob>();
                            results.Add(kvp.Value.Environment, jobs);
                        }
                        jobs.Add(kvp.Value.DeepCopy());
                    }
					return results;
                });
            }
        }

        /// <summary>
        /// Gets the job for a given id
        /// </summary>
        /// <param name="id">The desired Id</param>
        /// <returns></returns>
        public IJob Job(int id)
        {
			return JobLock.Read(() => Jobs.Value[id].DeepCopy());
        }                    

        private void LoadMigrations(IApp app, ApplicationContext applicationContext)
        {
            app.Migrations = new Dictionary<string, IMigration>();

            foreach (var attribute in app.GetType().GetCustomAttributes<AppMigrationAttribute>(true))
            {
                if (!(Activator.CreateInstance(attribute.Migration,
                    applicationContext.DatabaseContext.SqlSyntax,
                    applicationContext.ProfilingLogger.Logger) is IMigration migration))
                {
                    continue;
                }

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
            
            var currentVersion = new SemVersion(0);
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
                var semver = new SemVersion(new Version(migration.Key));
                if (latestVersion < semver)
                {
                    latestVersion = semver;
                }
            }

            if (latestVersion == currentVersion)
            {
                return;
            }

            var logger = applicationContext.ProfilingLogger.Logger;

            var migrationsRunner = new MigrationRunner(
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
                logger.Error(GetType(), $"Error running {productName} migration", ex);
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

        public void Register(IEnvironment environment, ApplicationContext applicationContext = null)
        {
            var appIds = App<IConfiguration>.Register;
            foreach(var appId in appIds)
            {
                var app = App<IConfiguration>.Create(appId.Key);

                if(applicationContext != null)
                {
                    LoadMigrations(app, applicationContext);
                    RunMigrations(app, applicationContext);
                }

                if (app.Init())
                {
                    Register(environment, app);
                }
            }
        }

        private const long RanRepeat = 0L;
        private const long RanNow = 1L;
        private long _ranTick = RanNow;

        private int _runningPoll;

        public void Poll(bool forceUpdate)
        {
            if (forceUpdate)
            {
                _ranTick = RanRepeat;
            }
            Poll();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Poll()
        {
            if (DateTime.UtcNow.Ticks < _ranTick)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref Instance._runningPoll, 0, 1) != 0)
            {
                return;
            }

            try
            {
                //System.Diagnostics.Debug.WriteLine("START running poll with " + ranTick.ToString());
                _ranTick = RanNow;
				if (JobLock.Write(() => 
				{
                    foreach (var reg in Jobs.Value)
                    {
                        var ct = new CancellationTokenSource();
                        var task = new Task<bool>(() => Execute(reg.Value), ct.Token, TaskCreationOptions.PreferFairness);

                        reg.Value.CancelToken = ct;
                        reg.Value.Task = task;
                    }
                }))
				{
					if (JobLock.Read(() =>
					{
						foreach (var reg in Jobs.Value)
						{
							reg.Value.Task.Start();
						}
					}))
					{
						if (Interlocked.CompareExchange(ref _ranTick, RanRepeat, RanNow) != RanRepeat)
						{
							_ranTick = DateTime.UtcNow.AddSeconds(PollSecs).Ticks;
						}
					}
				}
                //System.Diagnostics.Debug.WriteLine("END running poll with " + ranTick.ToString());
            }
            finally
            {
                _runningPoll = 0;
            }
        }

        private bool Execute(Job job)
        {
            try
            {
                job.CancelToken.Token.ThrowIfCancellationRequested();

                var config = ReadConfiguration(job, job.App.DefaultConfiguration);
				if (JobLock.Read(() => job.LastRan != null && config.LastModified < job.LastRan))
				{
					return true;
				}

                if (job.App.Execute(job, config))
                {
					JobLock.Write(() =>
					{
                        job.LastRan = DateTime.UtcNow;
                    });
					return true;
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

        public IConfiguration ReadConfiguration(int environmentId, string appId, IConfiguration defaultConfiguration = null)
        {
            if (defaultConfiguration == null)
            {
                defaultConfiguration = App<IConfiguration>.Create(appId).DefaultConfiguration;
            }

            return DbContext.Instance.Configuration.Read(environmentId, appId, defaultConfiguration.GetType(),
                defaultConfiguration);
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
            return JobLock.Write(() =>
			{
                var job = new Job
                {
                    Id = _registerCount++,
                    Environment = environment,
                    App = app,
                    ConfigType = app.GetType().BaseType?.GenericTypeArguments[0]
                };

                Jobs.Value.Add(job.Id, job);
            });
        }

        /// <summary>
        /// Removes a Job from the JobService by it's id
        /// </summary>
        /// <param name="id">The Job id</param>
        /// <returns></returns>
        public bool Unregister(int id)
        {
            return JobLock.Write(() =>
			{
			    if (!Jobs.Value.TryGetValue(id, out var job))
                    return;

			    if (job.Task != null && !job.Task.IsCanceled && !job.Task.IsCompleted && !job.CancelToken.IsCancellationRequested)
			    {
			        job.CancelToken.Cancel();
			    }
			    Jobs.Value.Remove(id);
			});
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
        /// <param name="environment">The environment to unregister</param>
        /// <returns>True if successfully removed; otherwise, False</returns>
        public bool Unregister(IEnvironment environment)
        {
            return JobLock.Write(() => 
			{
                var keys = new List<int>();
                foreach (var job in Jobs.Value)
                {
                    if (job.Value.Environment.Id == environment.Id)
                    {
                        if (job.Value.Task != null && !job.Value.Task.IsCanceled && !job.Value.Task.IsCompleted && 
							!job.Value.CancelToken.IsCancellationRequested)
                        {
                            job.Value.CancelToken.Cancel();
                        }
                        keys.Add(job.Key);
                    }
                }
                foreach (var key in keys)
                {
                    Jobs.Value.Remove(key);
                }
            });
        }

        /// <summary>
        /// Removes all jobs from the job service where the app's id are the same
        /// </summary>
        /// <param name="appId">The id of the app</param>
        /// <returns>True if successfully removed; otherwise, False</returns>
        public bool Unregister(string appId)
        {
            return JobLock.Write(() => 
            {
                var removeItems = new List<int>();

                foreach (var reg in Jobs.Value)
                {
                    var job = reg.Value;

                    if (job.App.Id != appId)
                        continue;

                    if (job.Task != null && !job.Task.IsCanceled && !job.Task.IsCompleted && !job.CancelToken.IsCancellationRequested)
                    {
                        job.CancelToken.Cancel();
                    }
                    removeItems.Add(reg.Key);
                }

                foreach (var item in removeItems)
                {
                    Jobs.Value.Remove(item);
                }
            });
        }

        /// <summary>
        /// Removes all jobs from the job service where the app are the same
        /// </summary>
        /// <param name="app">the app to remove</param>
        /// <returns>True if successfully removed; otherwise, False</returns>
        public bool Unregister(IApp app) => Unregister(app.Id);
    }
}
