using Newtonsoft.Json;
using System;

namespace Shield.Persistance.UmbracoAccess
{
    public class ConfigurationContext : Bal.ConfigurationContext
    {
        internal static readonly Guid id = new Guid("fa3bfb2e-b98d-40c3-ab2c-fa33b27f89ba");

        public override Guid Id { get { return id; } }

        public Configuration Read()
        {
            return Read<Configuration>();
        }

        public bool Write(Configuration model)
        {
            return Write<Configuration>(model);
        }

    }
}