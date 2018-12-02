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
        public abstract string Id { get; }
        
        public abstract string Name { get; }
        
        public abstract string Description { get; }
        
        public abstract string Icon { get; }
        
        public virtual bool Init() =>
            true;
        
        public static IDictionary<string, Type> Register =>
            Operation.Frisk.Register<App<TC>>();
        
        [JsonIgnore]
        public virtual IAppConfiguration DefaultConfiguration =>
            default(TC);
        
        public static IApp Create(string id)
        {
            if (!Register.TryGetValue(id, out var derivedType))
                return null;

            return Activator.CreateInstance(derivedType) as IApp;
        }
        
        public static IApp Create(Type type) =>
            Activator.CreateInstance(type) as IApp;
        
        public virtual bool Execute(IJob job, IAppConfiguration config) =>
            true;
        
        public bool WriteConfiguration(IJob job, IAppConfiguration config) =>
            JobService.Instance.WriteConfiguration(job, config);
        
        public bool WriteConfiguration(int jobId, IAppConfiguration config) =>
            WriteConfiguration(JobService.Instance.Job(jobId), config);
        
        public IAppConfiguration ReadConfiguration(IJob job) =>
            JobService.Instance.ReadConfiguration(job, DefaultConfiguration);
        
        public IAppConfiguration ReadConfiguration(int jobId) =>
            ReadConfiguration(JobService.Instance.Job(jobId));
        
        public bool WriteJournal(IJob job, IJournal journal) =>
            JobService.Instance.WriteJournal(job, journal);
        
        public bool WriteJournal(int jobId, IJournal journal) =>
            WriteJournal(JobService.Instance.Job(jobId), journal);
        
        public IEnumerable<T> ListJournals<T>(IJob job, int page, int itemsPerPage, out int totalPages) where T : IJournal =>
            JobService.Instance.ListJournals<T>(job, page, itemsPerPage, out totalPages);
        
        public IEnumerable<T> ListJournals<T>(int jobId, int page, int itemsPerPage, out int totalPages) where T : IJournal =>
            ListJournals<T>(JobService.Instance.Job(jobId), page, itemsPerPage, out totalPages);
        
        public override bool Equals(object other)
        {
            if (!(other is App<IAppConfiguration> otherApp))
            {
                return false;
            }
            return Id == otherApp.Id;
        }
        
        public override int GetHashCode() =>
            Id.GetHashCode();

        [JsonIgnore]
        private IDictionary<string, IMigration> _migrations;
        
        [JsonIgnore]
        public IDictionary<string, IMigration> Migrations
        {
            get => _migrations;
            set => _migrations = value;
        }

		public string Localize(string area, string key) => ApplicationContext.Current.Services.TextService.Localize($"{area}/{key}", CultureInfo.CurrentCulture);
    }
}
