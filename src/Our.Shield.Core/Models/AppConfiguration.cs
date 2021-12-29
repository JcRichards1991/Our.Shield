using System;
using System.Diagnostics;

namespace Our.Shield.Core.Models
{
    [DebuggerDisplay("Enable: {Enable}")]
    public abstract class AppConfiguration : IAppConfiguration
    {
        public bool Enable { get; set; }
        
        public DateTime? LastModified { get; internal set; }
    }
}
