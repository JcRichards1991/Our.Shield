using System;

namespace Our.Shield.Core.Models
{
    public abstract class AppConfiguration : IAppConfiguration
    {
        public bool Enable { get; set; }
        
        public DateTime? LastModified { get; internal set; }
    }
}
