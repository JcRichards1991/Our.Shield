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

        [HttpPost]
        public bool PostBackendAccess(Models.ViewModel model)
        {
            var db = new Shield.Persistance.UmbracoAccess.ConfigurationContext();

            var persistenceModel = new Shield.Persistance.UmbracoAccess.Configuration
            {
                BackendAccessUrl = model.backendAccessUrl
            };

            return db.Write(persistenceModel);
        }

        [HttpGet]
        public Models.ViewModel GetBackendAccessModel()
        {
            var db = new Shield.Persistance.UmbracoAccess.ConfigurationContext();

            var persistenceModel = db.Read();

            return new Models.ViewModel
            {
                backendAccessUrl = persistenceModel.BackendAccessUrl
            };
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
