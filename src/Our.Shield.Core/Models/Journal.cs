namespace Our.Shield.Core.Models
{
    using System;

    /// <summary>
    /// The Journal class to inherit from
    /// </summary>
    public abstract class Journal : IJournal
    {
        /// <summary>
        /// App Id of the journal
        /// </summary>
        public string AppId { get; internal set; }

        /// <summary>
        /// Environment Id of the journal
        /// </summary>
        public int EnvironmentId { get; }

        /// <summary>
        /// Datestamp of when the journal was created
        /// </summary>
        public DateTime Datestamp { get; }
    }
}
