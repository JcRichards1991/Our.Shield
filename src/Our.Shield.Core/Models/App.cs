using Newtonsoft.Json;
using Our.Shield.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using Umbraco.Core.Migrations;
using Umbraco.Core.Services;

namespace Our.Shield.Core.Models
{
    /// <inheritdoc />
    /// <typeparam name="TC">The type of configuration for the app</typeparam>
    public abstract class App<TC> : IApp where TC : IAppConfiguration
    {
        /// <inheritdoc />
        protected readonly ILocalizedTextService LocalizedTextService;

        protected App(ILocalizedTextService localizedTextService)
        {
            LocalizedTextService = localizedTextService;
        }

        /// <inheritdoc />
        public abstract string Id { get; }

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract string Description { get; }

        /// <inheritdoc />
        public abstract string Icon { get; }

        /// <inheritdoc />
        public virtual bool Init() =>
            true;

        /// <inheritdoc />
        public static IDictionary<string, Type> Register =>
            Operation.Frisk.Register<App<TC>>();

        /// <inheritdoc />
        [JsonIgnore]
        public virtual IAppConfiguration DefaultConfiguration =>
            default(TC);

        /// <inheritdoc />
        public static IApp Create(string id)
        {
            if (!Register.TryGetValue(id, out var derivedType))
                return null;

            return Activator.CreateInstance(derivedType) as IApp;
        }

        /// <inheritdoc />
        public static IApp Create(Type type) =>
            Activator.CreateInstance(type) as IApp;

        /// <inheritdoc />
        public virtual bool Execute(IJob job, IAppConfiguration config) =>
            true;

        /// <inheritdoc />
        public bool WriteConfiguration(IJob job, IAppConfiguration config) =>
            JobService.Instance.WriteConfiguration(job, config);

        /// <inheritdoc />
        public bool WriteConfiguration(int jobId, IAppConfiguration config) =>
            WriteConfiguration(JobService.Instance.Job(jobId), config);

        /// <inheritdoc />
        public IAppConfiguration ReadConfiguration(IJob job) =>
            JobService.Instance.ReadConfiguration(job, DefaultConfiguration);

        /// <inheritdoc />
        public IAppConfiguration ReadConfiguration(int jobId) =>
            ReadConfiguration(JobService.Instance.Job(jobId));

        /// <inheritdoc />
        public bool WriteJournal(IJob job, IJournal journal) =>
            JobService.Instance.WriteJournal(job, journal);

        /// <inheritdoc />
        public bool WriteJournal(int jobId, IJournal journal) =>
            WriteJournal(JobService.Instance.Job(jobId), journal);

        /// <inheritdoc />
        public IEnumerable<T> ListJournals<T>(IJob job, int page, int itemsPerPage, out int totalPages) where T : IJournal =>
            JobService.Instance.ListJournals<T>(job, page, itemsPerPage, out totalPages);

        /// <inheritdoc />
        public IEnumerable<T> ListJournals<T>(int jobId, int page, int itemsPerPage, out int totalPages) where T : IJournal =>
            ListJournals<T>(JobService.Instance.Job(jobId), page, itemsPerPage, out totalPages);

        /// <inheritdoc />
        public override bool Equals(object other)
        {
            if (!(other is App<IAppConfiguration> otherApp))
            {
                return false;
            }
            return Id == otherApp.Id;
        }

        /// <inheritdoc />
        public override int GetHashCode() =>
            Id.GetHashCode();

        /// <inheritdoc />
        [JsonIgnore]
        private IDictionary<string, IMigration> _migrations;

        /// <inheritdoc />
        [JsonIgnore]
        public IDictionary<string, IMigration> Migrations
        {
            get => _migrations;
            set => _migrations = value;
        }

        /// <inheritdoc />
        public string Localize(string area, string key) => LocalizedTextService.Localize($"{area}/{key}", CultureInfo.CurrentCulture);
    }
}
