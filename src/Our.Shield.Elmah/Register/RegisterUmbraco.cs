using System.Web.Routing;
using Umbraco.Core;

namespace Our.Shield.Elmah.Register
{
	class RegisterUmbraco : ApplicationEventHandler
	{
		/// <inheritdoc />
		/// <summary>
		/// </summary>
		/// <param name="umbracoApplication"></param>
		/// <param name="applicationContext"></param>
		protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
		{
			base.ApplicationStarted(umbracoApplication, applicationContext);
			RouteTable.Routes.Ignore("elmah.axd/{*pathInfo}");
		}
	}
}
