namespace Our.Shield.Core.Models
{
    using System;

    /// <summary>
    /// Journal interface
    /// </summary>
    public interface IJournal
    {
        /// <summary>
        /// App Id of the journal
        /// </summary>
        string AppId { get; }

        /// <summary>
        /// Environment Id of the journal
        /// </summary>
        int EnvironmentId { get; }

        /// <summary>
        /// Datestamp of when the journal was created
        /// </summary>
        DateTime Datestamp { get; }
    }
}
