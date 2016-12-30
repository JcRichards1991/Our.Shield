using Newtonsoft.Json;
using System;
using Umbraco.Core;

namespace Shield.Persistance.Bal
{
    public abstract class Record
    {
        public abstract Guid Id { get; }
    }
}
