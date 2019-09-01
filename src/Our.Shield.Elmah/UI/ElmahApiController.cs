using Our.Shield.Elmah.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Our.Shield.Core.Services;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using ElmahCore = Elmah;

namespace Our.Shield.Elmah.UI
{
    [PluginController(Core.UI.Constants.App.Alias)]
    public class ElmahApiController : UmbracoAuthorizedJsonController
    {
        private readonly ShieldService _shieldService;

        public ElmahApiController()
        {
            _shieldService = new ShieldService();
        }

        [HttpGet]
        public ElmahApiJsonModel GetErrors(Guid appKey, int page, int resultsPerPage)
        {
            var environment = _shieldService.GetEnvironmentByAppKey(appKey);
            var domains = environment.Domains.Select(x => x.Name).ToList();

            var errorLog = ElmahCore
                .ErrorLog
                .GetDefault(HttpContext.Current);

            var errors = new List<ElmahCore.ErrorLogEntry>();
            int totalErrorCount;

            if (!domains.Any())
            {
                totalErrorCount = errorLog.GetErrors(page - 1, resultsPerPage, errors);
            }
            else
            {
                var p = 0;

                totalErrorCount = errorLog.GetErrors(0, resultsPerPage, errors);

                do
                {
                    p++;
                    errorLog.GetErrors(p, resultsPerPage, errors);

                } while (errors.Count < totalErrorCount);

                var filteredErrors = new List<ElmahCore.ErrorLogEntry>();

                foreach (var error in errors)
                {
                    var fullErrorInfo = errorLog.GetError(error.Id);

                    foreach (var domain in domains)
                    {
                        if (domain.Contains(fullErrorInfo.Error.ServerVariables["HTTP_HOST"]))
                        {
                            filteredErrors.Add(fullErrorInfo);
                            break;
                        }
                    }
                }

                totalErrorCount = filteredErrors.Count;

                errors = filteredErrors
                    .Skip((page - 1) * resultsPerPage)
                    .Take(resultsPerPage)
                    .ToList();
            }

            var totalPages = (int)Math.Ceiling((double)totalErrorCount / resultsPerPage);

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

        public ElmahErrorJsonModel GetError(string id)
        {
            var error = ElmahCore.ErrorLog.GetDefault(HttpContext.Current).GetError(id);
            return new ElmahErrorJsonModel
            {
                Id = error.Id,
                Error = error.Error
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
