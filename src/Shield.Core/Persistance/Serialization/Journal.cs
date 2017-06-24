namespace Shield.Core.Persistance.Serialization
{
    using System;

    public class Journal
    {
        /// <summary>
        /// The Date stamp of the journal
        /// </summary>
        public DateTime Datestamp { get; set; }

        /// <summary>
        /// Gets or sets the Message.
        /// </summary>
        public string Message { get; set; }
    }
}
