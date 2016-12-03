using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Core.Sync;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Shield.UI.UmbracoAccess.Controllers
{
    [PluginController(Constants.App.Name)]
    public class UmbracoAccessApiController : UmbracoAuthorizedJsonController
    {
        [HttpPost]
        public void Post()
        {
            var t = ServerRegistrarResolver.Current;

            throw new NotImplementedException();
        }
        
        [HttpGet]
        public IEnumerable<object> Get()
        {
            return Enumerable.Empty<object>();
        }

        
        [HttpGet]
        public void Delete (string ipAddress)
        {
        }

        public void GetLog()
        {
        }
    }
}
