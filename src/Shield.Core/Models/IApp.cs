namespace Shield.Core.Models
{
    using System.Collections.Generic;

    public interface IApp : IFrisk
    {
        /// <summary>
        /// Name of the plugin
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of the plugin
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Css class of icon
        /// </summary>
        string Icon { get; }

        bool Init();

        bool Execute(IJob job, IConfiguration config);

        bool WriteConfiguration(IJob job, IConfiguration config);

        bool WriteConfiguration(int jobId, IConfiguration config);

        bool WriteJournal(IJob job, IJournal journal);

        bool WriteJournal(int jobId, IJournal journal);

        IConfiguration ReadConfiguration(IJob job);

        IConfiguration ReadConfiguration(int jobId);

        IEnumerable<T> ListJournals<T>(IJob job, int page, int itemsPerPage) where T : IJournal;

        IEnumerable<T> ListJournals<T>(int jobId, int page, int itemsPerPage) where T : IJournal;

        IConfiguration DefaultConfiguration { get; }
    }
}
