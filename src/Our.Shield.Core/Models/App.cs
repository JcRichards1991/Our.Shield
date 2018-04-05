using Newtonsoft.Json;
using Our.Shield.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using Umbraco.Core;
using Umbraco.Core.Persistence.Migrations;

namespace Our.Shield.Core.Models
{
    /// <inheritdoc />
    /// <summary>
    /// Definition of an App to plugin to Our.Shield custom umbraco section
    /// </summary>
    /// <typeparam name="TC">The type of configuration for the app</typeparam>
    public abstract class App<TC> : IApp where TC : IAppConfiguration
    {
        /// <inheritdoc />
        [JsonProperty("id")]
        public abstract string Id { get; }

        /// <inheritdoc />
        [JsonProperty("name")]
        public abstract string Name { get; }

        /// <inheritdoc />
        [JsonProperty("description")]
        public abstract string Description { get; }

        /// <inheritdoc />
        [JsonProperty("icon")]
        public abstract string Icon { get; }

        /// <inheritdoc />
        public virtual bool Init() =>
            true;

        /// <summary>
        /// Dictionary of the registered App Id, and it's type
        /// </summary>
        public static IDictionary<string, Type> Register =>
            Operation.Frisk.Register<App<TC>>();

        /// <inheritdoc />
        [JsonIgnore]
        public virtual IAppConfiguration DefaultConfiguration =>
            default(TC);

        /// <summary>
        /// Create a derived App with a particular Id
        /// </summary>
        /// <param name="id">The id of the App to create</param>
        /// <returns></returns>
        public static IApp Create(string id)
        {
            if (Register.TryGetValue(id, out var derivedType))
            {
                return Activator.CreateInstance(derivedType) as IApp;
            }
            return null;
        }

        /// <summary>
        /// Create a derived App from a particular type
        /// </summary>
        /// <param name="type">The type of App to create</param>
        /// <returns></returns>
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

        /// <summary>
        /// Checks whether or not two Apps are the same
        /// </summary>
        /// <param name="other"></param>
        /// <returns>True if equals; Otherwise False</returns>
        public override bool Equals(object other)
        {
            if (!(other is App<IAppConfiguration> otherApp))
            {
                return false;
            }
            return Id == otherApp.Id;
        }

        /// <summary>
        /// Gets the HashCode for the App
        /// </summary>
        /// <returns>The Id's HashCode</returns>
        public override int GetHashCode() =>
            Id.GetHashCode();

        [JsonIgnore]
        private IDictionary<string, IMigration> _migrations;

        /// <inheritdoc />
        [JsonIgnore]
        public IDictionary<string, IMigration> Migrations
        {
            get => _migrations;
            set => _migrations = value;
        }

		public string Localize(string area, string key) => ApplicationContext.Current.Services.TextService.Localize($"{area}/{key}", CultureInfo.CurrentCulture);
    }
}
