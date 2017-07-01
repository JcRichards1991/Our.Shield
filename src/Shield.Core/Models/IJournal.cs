using System;

namespace Shield.Core.Models
{
    public class IJournal
    {
        string AppId { get; }

        int EnvironmentId { get; }

        DateTime Datestamp { get; }
    }
}
