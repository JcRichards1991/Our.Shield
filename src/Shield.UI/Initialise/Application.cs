using System.Collections.Generic;
using System.Linq;
using umbraco.businesslogic;
using umbraco.interfaces;
using Umbraco.Core;
using Umbraco.Core.Sync;

namespace Shield.UI.Initialse
{
    [Application(Constants.App.Alias, Constants.App.Name, Constants.App.Icon, 1000)]
    public class Application : IApplication
    {

    }

    public class Register : Umbraco.Core.ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarting(umbracoApplication, applicationContext);

            ServerRegistrarResolver.Current.SetServerRegistrar(new FrontEndReadOnlyServerRegistrar());
        }
    }

    public class FrontEndReadOnlyServerRegistrar : IServerRegistrar2
    {
        public IEnumerable<IServerAddress> Registrations
        {
            get { return Enumerable.Empty<IServerAddress>(); }
        }
        public ServerRole GetCurrentServerRole()
        {
            return ServerRole.Slave;
        }
        public string GetCurrentServerUmbracoApplicationUrl()
        {
            return "http://shield.local/josh";
        }
    }
}
