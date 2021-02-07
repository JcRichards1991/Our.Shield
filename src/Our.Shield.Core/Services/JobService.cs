using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence;

namespace Our.Shield.Core.Services
{
    internal class JobService : IJobService
    {
        //  Starting id for Jobs
        private const int JobIdStart = 1000;

        private static readonly Locker JobLock = new Locker();

        private static readonly Lazy<IDictionary<Guid, Job>> Jobs =
            new Lazy<IDictionary<Guid, Job>>(() =>
            new Dictionary<Guid, Job>());

        private int _registerCount = JobIdStart;
        
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

        public IJob Job(Guid key)
        {
            return JobLock.Read(() => Jobs.Value[key].DeepCopy());
        }
        
        public void Init(ISqlContext sqlContext)
        {
            throw new NotImplementedException();

            //var evs = DbContext.Instance.Environment.Read();

            //foreach (var ev in evs)
            //{
            //    Register(new Models.Environment(ev), sqlContext);
            //}
        }

        public void Register(IEnvironment environment)
        {
            var appIds = App<IAppConfiguration>.Register;

            foreach (var appId in appIds)
            {
                var app = App<IAppConfiguration>.Create(appId.Key);

                if (app.Init())
                {
                    Register(environment, app);
                }
            }
        }

        public bool Execute(Job job)
        {
            try
            {
                job.CancelToken?.Token.ThrowIfCancellationRequested();

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
        
        public bool WriteConfiguration(IJob job, IAppConfiguration config)
        {
            throw new NotImplementedException();

            //if (!DbContext.Instance.Configuration.Write(job.Environment.Id, job.App.Id, job.Key, config))
            //{
            //    return false;
            //}

            //return true;
        }

        public bool WriteJournal(IJob job, IJournal journal)
        {
            throw new NotImplementedException();
            //return DbContext.Instance.Journal.Write(job.Environment.Id, job.App.Id, journal);
        }
        
        public IAppConfiguration ReadConfiguration(IJob job, IAppConfiguration defaultConfiguration = null)
        {
            throw new NotImplementedException();

            //return DbContext.Instance.Configuration.Read(job.Environment.Id, job.App.Id, ((Job)job).ConfigType,
            //    defaultConfiguration ?? App<IAppConfiguration>.Create(job.App.Id).DefaultConfiguration);
        }

        public IAppConfiguration ReadConfiguration(Guid environmentId, string appId, IAppConfiguration defaultConfiguration = null)
        {
            if (defaultConfiguration == null)
            {
                defaultConfiguration = App<IAppConfiguration>.Create(appId).DefaultConfiguration;
            }

            throw new NotImplementedException();

            //return DbContext.Instance.Configuration.Read(environmentId, appId, defaultConfiguration.GetType(),
            //    defaultConfiguration);
        }
        
        public IEnumerable<T> ListJournals<T>(IJob job, int page, int itemsPerPage, out int totalPages) where T : IJournal =>
            throw new NotImplementedException(); //DbContext.Instance.Journal.Read<T>(job.Environment.Id, job.App.Id, page, itemsPerPage, out totalPages);
        
        public bool Register(IEnvironment environment, IApp app)
        {
            throw new NotImplementedException();

            //Job job = null;

            //if (JobLock.Write(() =>
            //{
            //    job = new Job
            //    {
            //        Id = _registerCount++,
            //        Key = DbContext.Instance.Configuration.ReadUniqueKey(environment.Id, app.Id),
            //        Environment = environment,
            //        App = app,
            //        ConfigType = app.GetType().BaseType?.GenericTypeArguments[0]
            //    };

            //    Jobs.Value.Add(job.Id, job);
            //}))
            //{
            //    if (job != null)
            //        return Execute(job);
            //}

            //return false;
        }

        public bool Unregister(Guid key)
        {
            return JobLock.Write(() =>
            {
                if (!Jobs.Value.TryGetValue(key, out var job))
                {
                    return;
                }

                if (job.Task != null && !job.Task.IsCanceled && !job.Task.IsCompleted && !job.CancelToken.IsCancellationRequested)
                {
                    job.CancelToken.Cancel();
                }

                Jobs.Value.Remove(key);
            });
        }

        public bool Unregister(IJob job) => Unregister(job.Key);

        public bool Unregister(IEnvironment environment)
        {
            return JobLock.Write(() =>
            {
                var keys = new List<Guid>();

                foreach (var job in Jobs.Value)
                {
                    if (job.Value.Environment.Key == environment.Key)
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

        public bool Unregister(string appId)
        {
            return JobLock.Write(() =>
            {
                var removeItems = new List<Guid>();

                foreach (var reg in Jobs.Value)
                {
                    var job = reg.Value;

                    if (job.App.Id != appId)
                    {
                        continue;
                    }

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

        public bool Unregister(IApp app) => Unregister(app.Id);
    }
}
