using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Umbraco.Core;

namespace Shield.Persistance.Bal
{
    public abstract class Record
    {
        public abstract Guid Id { get; }

        protected virtual T Read<T>() where T : JsonValues, new()
        {
            throw new NotImplementedException();
        }

        protected virtual IEnumerable<T> Read<T>(int page, int itemsPerPage) where T : JsonValues, new()
        {
            throw new NotImplementedException();
        }

        protected virtual bool Write<T>(T values) where T : Bal.JsonValues
        {
            throw new NotImplementedException();
        }

    }
}
