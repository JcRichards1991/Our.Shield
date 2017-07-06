namespace Our.Shield.Core.Models
{
    using System;

    public class IJournal
    {
        string AppId { get; }

        int EnvironmentId { get; }

        DateTime Datestamp { get; }
    }
}
