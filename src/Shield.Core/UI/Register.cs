namespace Shield.Core.UI
{
    using Umbraco.Core;

    /// <summary>
    /// Initialization class.
    /// </summary>
    public class Register : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarted(umbracoApplication, applicationContext);
            applicationContext.Services.SectionService.MakeNew(Shield.Core.Constants.App.Name, Shield.Core.Constants.App.Alias, Shield.Core.Constants.App.Icon);
        }
    }
}
