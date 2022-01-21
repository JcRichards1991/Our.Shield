using Our.Shield.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Our.Shield.Core.Services
{
    public interface IJobService
    {
        IDictionary<IEnvironment, IList<IJob>> Environments { get; }

        IJob Job(Guid key);

        void Init();

        Task Register(IEnvironment environment);

        Task<bool> Execute(Job job);

        bool WriteConfiguration(
            IJob job,
            IAppConfiguration config);

        bool WriteJournal(
            IJob job,
            IJournal journal);

        Task<IAppConfiguration> ReadConfiguration(
            IJob job,
            IAppConfiguration defaultConfiguration);

        IEnumerable<T> ListJournals<T>(
            IJob job,
            int page,
            int itemsPerPage,
            out int totalPages)
            where T : IJournal;

        Task<bool> Register(IEnvironment environment, IApp app);

        bool Unregister(Guid id);

        bool Unregister(IJob job);

        bool Unregister(IEnvironment environment);

        bool Unregister(string appId);

        bool Unregister(IApp app);
    }
}
