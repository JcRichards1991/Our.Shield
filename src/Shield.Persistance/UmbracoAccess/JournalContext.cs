using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Shield.Persistance.UmbracoAccess
{
    public class JournalAccess : Bal.JournalContext
    {
        public override Guid Id { get { return ConfigurationContext.id; } }

        public IEnumerable<Journal> Read(int page, int itemsPerPage)
        {
            return Read<Journal>(page, itemsPerPage);
        }

        public bool Write(Journal model)
        {
            return Write<Journal>(model);
        }
    }
}
