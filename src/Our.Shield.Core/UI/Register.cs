using Umbraco.Core;

namespace Our.Shield.Core.UI
{
    /// <inheritdoc />
    /// <summary>
    /// Initialization class
    /// </summary>
    public class Register : ApplicationEventHandler
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarted(umbracoApplication, applicationContext);
            applicationContext.Services.SectionService.MakeNew(Constants.App.Name, Constants.App.Alias, Constants.App.Icon);
        }
    }
}
