using Our.Shield.Elmah.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using ElmahCore = Elmah;

namespace Our.Shield.Elmah.UI
{
    [PluginController(Core.UI.Constants.App.Alias)]
    public class ElmahApiController : UmbracoAuthorizedJsonController
    {
        [HttpGet]
        public ElmahApiJsonModel GetErrors(int page, int resultsPerPage)
        {
            var errors = new List<ElmahCore.ErrorLogEntry>();
            var totalErrorCount = ElmahCore.ErrorLog.GetDefault(HttpContext.Current).GetErrors(page - 1, resultsPerPage, errors);
            var totalPages = (int) Math.Ceiling((double) totalErrorCount / resultsPerPage);
        
            return new ElmahApiJsonModel
            {
                TotalPages = totalPages,
                Errors = errors.Select(x => new ElmahErrorJsonModel
                {
                    Id = x.Id,
                    Error = x.Error
                })
            };
        }

        [HttpPost]
        public void GenerateTestException()
        {
            var exception = new Exception("This is a test exception and can be safely ignored");
            ElmahCore.ErrorSignal.FromCurrentContext().Raise(exception);
        }
    }
}
