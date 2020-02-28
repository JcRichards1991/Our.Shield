using Our.Shield.Swagger.Extensions;
using System.Linq;
using System.Web.Http;
using Umbraco.Core;

namespace Our.Shield.Site.App_Start
{
    public class Startup : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //GlobalConfiguration.Configuration.SetSwaggerDocsConfig(c =>
            //{
            //    c.SingleApiVersion("v1", "Website");
            //    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            //});
        }
    }
}