using Newtonsoft.Json;
using System;

namespace Shield.Persistance.UmbracoAccess
{
    public class Record : Bal.Record
    {
        private static readonly Guid id = new Guid("fa3bfb2e-b98d-40c3-ab2c-fa33b27f89ba");

        public override Guid Id { get { return id; } }




    }
}
