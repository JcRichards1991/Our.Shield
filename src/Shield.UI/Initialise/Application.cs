using System.Collections.Generic;
using System.Linq;
using umbraco.businesslogic;
using umbraco.interfaces;
using Umbraco.Core;
using Umbraco.Core.Sync;

namespace Shield.UI.Initialse
{
    /// <summary>
    /// Shield custom section
    /// </summary>
    [Application(Constants.App.Alias, Constants.App.Name, Constants.App.Icon, 1000)]
    public class Application : IApplication
    {

    }

    /// <summary>
    /// Initialization class.
    /// </summary>
    public class Register : ApplicationEventHandler
    {
        /// <summary>
        /// Overrides the ApplicationEventHandler ApplicationStarting method.
        /// </summary>
        /// <param name="umbracoApplication">
        /// The Umbraco Application.
        /// </param>
        /// <param name="applicationContext">
        /// The Application Context.
        /// </param>
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarting(umbracoApplication, applicationContext);

            //ServerRegistrarResolver.Current.SetServerRegistrar(new FrontEndReadOnlyServerRegistrar());
        }
    }




    // TEST CODE. 
    // Keep for now so don't need to find it again in future.

    //public class FrontEndReadOnlyServerRegistrar : IServerRegistrar2
    //{
    //    public IEnumerable<IServerAddress> Registrations
    //    {
    //        get { return Enumerable.Empty<IServerAddress>(); }
    //    }
        
    //    public ServerRole GetCurrentServerRole()
    //    {
    //        return ServerRole.Slave;
    //    }
        
    //    public string GetCurrentServerUmbracoApplicationUrl()
    //    {
    //        return "http://shield.local/josh";
    //    }
    //}
}
