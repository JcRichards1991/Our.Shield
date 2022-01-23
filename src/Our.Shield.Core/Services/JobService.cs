using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using System;
using System.Collections.Generic;

namespace Our.Shield.Core.Services
{
    internal class JobService : IJobService
    {
        private static readonly Locker JobLock = new Locker();

        private static readonly Lazy<IDictionary<Guid, Job>> Jobs =
            new Lazy<IDictionary<Guid, Job>>(() =>
            new Dictionary<Guid, Job>());

        public bool ExecuteApp(Guid key, IAppConfiguration configuration)
        {
            var job = GetJob(key);

            return Execute(job, configuration);
        }

        public bool Register(IEnvironment environment, IApp app, IAppConfiguration configuration)
        {
            Job job = null;

            if (JobLock.Write(() =>
            {
                job = new Job
                {
                    Key = app.Key,
                    Environment = environment,
                    App = app,
                    ConfigType = app.GetType().BaseType?.GenericTypeArguments[0]
                };

                Jobs.Value.Add(job.Key, job);
            }))
            {
                if (job != null)
                {
                    return Execute(job, configuration);
                }
            }

            return false;
        }

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

                WebRequestHandler.Unwatch(environment.Key);
            });
        }

        private bool Execute(Job job, IAppConfiguration configuration)
        {
            try
            {
                job.CancelToken?.Token.ThrowIfCancellationRequested();

                if (JobLock.Read(() => job.LastRan != null && configuration.LastModifiedDateUtc < job.LastRan))
                {
                    return true;
                }

                job.UnwatchWebRequests();
                job.UnexceptionWebRequest();
                job.UnignoreWebRequest();

                if (job.App.Execute(job, configuration))
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

        private Job GetJob(Guid key)
        {
            return JobLock.Read(() =>
            {
                var job = Jobs.Value[key];

                return job.DeepCopy(job.App);
            });
        }
    }
}
