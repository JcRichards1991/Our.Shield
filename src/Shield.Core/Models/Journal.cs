using System;

namespace Shield.Core.Models
{
    public abstract class Journal : IJournal
    {
        public string AppId { get; internal set; }

        public int EnvironmentId { get; }

        public DateTime Datestamp { get; }
    }
}
