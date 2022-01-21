using AutoMapper;
using LightInject;
using Newtonsoft.Json;
using Our.Shield.Core.Data.Accessors;
using Our.Shield.Core.Factories;
using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using Our.Shield.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Our.Shield.Core.Services
{
    internal class JobService : IJobService
    {
        private static readonly Locker JobLock = new Locker();

        private static readonly Lazy<IDictionary<Guid, Job>> Jobs =
            new Lazy<IDictionary<Guid, Job>>(() =>
            new Dictionary<Guid, Job>());

        private readonly IEnvironmentAccessor _environmentAccessor;
        private readonly IAppAccessor _appAccessor;
        private readonly IAppFactory _appFactory;
        private readonly IMapper _mapper;

        public JobService(
            IEnvironmentAccessor environmentAccessor,
            IAppAccessor appAccessor,
            IAppFactory appFactory,
            [Inject(nameof(Shield))] IMapper mapper)
        {
            GuardClauses.NotNull(environmentAccessor, nameof(environmentAccessor));
            GuardClauses.NotNull(appAccessor, nameof(appAccessor));
            GuardClauses.NotNull(appFactory, nameof(appFactory));
            GuardClauses.NotNull(mapper, nameof(mapper));

            _environmentAccessor = environmentAccessor;
            _appAccessor = appAccessor;
            _appFactory = appFactory;
            _mapper = mapper;
        }

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

                        jobs.Add(kvp.Value.DeepCopy(_appFactory.Create(kvp.Value.App.Id)));
                    }

                    return results;
                });
            }
        }

        public IJob Job(Guid key)
        {
            return JobLock.Read(() =>
            {
                var job = Jobs.Value[key];

                return job.DeepCopy(_appFactory.Create(job.App.Id));
            });
        }

        public async void Init()
        {
            var environments = await _environmentAccessor.Read();

            foreach (var environment in environments)
            {
                await Register(_mapper.Map<Models.Environment>(environment));
            }
        }

        public async Task Register(IEnvironment environment)
        {
            var appIds = _appFactory.GetRegistedAppsIds();

            foreach (var appId in appIds)
            {
                var app = _appFactory.Create(appId);

                if (app.Init())
                {
                    await Register(environment, app);
                }
            }
        }

        public async Task<bool> Execute(Job job)
        {
            try
            {
                job.CancelToken?.Token.ThrowIfCancellationRequested();

                var config = await ReadConfiguration(job, job.App.DefaultConfiguration);

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

        internal async Task<Data.Dtos.App> ReadApp(string appId, Guid environmentKey, IAppConfiguration appConfiguration)
        {
            var app = await _appAccessor.Read(appId, environmentKey);

            if (app != null)
            {
                return app;
            }

            app = new Data.Dtos.App
            {
                AppId = appId,
                EnvironmentKey = environmentKey,
                Configuration = JsonConvert.SerializeObject(appConfiguration ?? _appFactory.Create(appId).DefaultConfiguration)
            };

            app.Key = _appAccessor.Create(app).Result;

            return app;
        }

        public async Task<IAppConfiguration> ReadConfiguration(IJob job, IAppConfiguration defaultConfiguration)
        {
            var app = await ReadApp(job.App.Id, job.Environment.Key, defaultConfiguration);
            if (app == null)
            {
                return defaultConfiguration;
            }

            return JsonConvert.DeserializeObject(app.Configuration, defaultConfiguration.GetType()) as IAppConfiguration;
        }

        public IEnumerable<T> ListJournals<T>(IJob job, int page, int itemsPerPage, out int totalPages) where T : IJournal =>
            throw new NotImplementedException(); //DbContext.Instance.Journal.Read<T>(job.Environment.Id, job.App.Id, page, itemsPerPage, out totalPages);

        public async Task<bool> Register(IEnvironment environment, IApp app)
        {
            Job job = null;
            var dbApp = await ReadApp(app.Id, environment.Key, app.DefaultConfiguration);
            app.Key = dbApp.Key;

            if (JobLock.Write(() =>
            {
                job = new Job
                {
                    Key = dbApp.Key,
                    Environment = environment,
                    App = app,
                    ConfigType = app.GetType().BaseType?.GenericTypeArguments[0]
                };

                Jobs.Value.Add(job.Key, job);
            }))
            {
                if (job != null)
                {
                    return await Execute(job);
                }
            }

            return false;
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
