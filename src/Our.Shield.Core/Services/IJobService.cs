using Our.Shield.Core.Models;
using System;
using System.Collections.Generic;

namespace Our.Shield.Core.Services
{
    public interface IJobService
    {
        IDictionary<IEnvironment, IList<IJob>> Environments { get; }

        IJob Job(Guid key);

        void Init();

        void Register(IEnvironment environment);

        bool Execute(Job job);

        bool WriteConfiguration(
            IJob job,
            IAppConfiguration config);

        bool WriteJournal(
            IJob job,
            IJournal journal);

        IAppConfiguration ReadConfiguration(
            IJob job,
            IAppConfiguration defaultConfiguration = null);

        IAppConfiguration ReadConfiguration(
            string appId,
            Guid environmentKey,
            IAppConfiguration defaultConfiguration = null);

        IEnumerable<T> ListJournals<T>(
            IJob job,
            int page,
            int itemsPerPage,
            out int totalPages)
            where T : IJournal;

        bool Register(IEnvironment environment, IApp app);

        bool Unregister(Guid id);

        bool Unregister(IJob job);

        bool Unregister(IEnvironment environment);

        bool Unregister(string appId);

        bool Unregister(IApp app);
    }
}
