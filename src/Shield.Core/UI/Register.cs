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
            applicationContext.Services.SectionService.MakeNew(Constants.App.Name, Constants.App.Alias, Constants.App.Icon);
        }
    }
}
