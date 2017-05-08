[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Shield.UmbracoAccess.Operation.Module), nameof(Shield.UmbracoAccess.Operation.Module.Register))]
namespace Shield.UmbracoAccess.Operation
{
    using System.Web;
    using System;
    using System.Linq;
    using System.Threading;
    using Umbraco.Web;

    public class Module : IHttpModule
    {
        private const int configLockTimeout = 1000;

        private static ReaderWriterLockSlim configLock = new ReaderWriterLockSlim();

        private static ViewModels.Configuration config = null;

        public static bool Config(ViewModels.Configuration value)
        {
            if (configLock.TryEnterWriteLock(configLockTimeout))
            {
                try
                {
                    config = value;
                    return true;
                }
                finally
                {
                    configLock.ExitWriteLock();
                }
            }
            return false;
        }

        private string umbracoPath = null;

        private string UmbracoPath
        {
            get
            {
                if (umbracoPath == null)
                {
                    umbracoPath = ApplicationSettings.GetUmbracoPath();
                }

                return umbracoPath;
            }
        }

        public static void Register()
        {
            // Register our module
            Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(typeof(Module));
        }

        public void Init(HttpApplication application)
        {
            application.BeginRequest += (new EventHandler(this.Application_BeginRequest));

            //application.EndRequest += (new EventHandler(this.Application_EndRequest));
        }

        private void Application_BeginRequest(Object source, EventArgs e)
        {
            // Create HttpApplication and HttpContext objects to access
            // request and response properties.
            HttpApplication application = (HttpApplication)source;
            HttpContext context = application.Context;
            string filePath = context.Request.FilePath;

            if(config == null || !config.Enable)
            {
                return;
            }

            if (configLock.TryEnterReadLock(configLockTimeout))
            {
                try
                {
                    if(filePath == UmbracoPath || filePath == config.BackendAccessUrl)
                    {
                        //If starts with umbraco path or config backend url
                        // Let request through

                        if (!config.IpAddresses.Any(x => x.ipAddress == ""))
                        {
                            string url = null;
                            switch (config.UnauthorisedUrlType)
                            {
                                case Enums.UnautorisedUrlType.Url:
                                    url = config.UnauthorisedUrl;
                                    break;

                                case Enums.UnautorisedUrlType.XPath:
                                    var xpathNode = UmbracoContext.Current.ContentCache.GetSingleByXPath(config.UnauthorisedUrlXPath);
                                    url = xpathNode.Url;
                                    break;

                                case Enums.UnautorisedUrlType.ContentPicker:
                                    var contentPickerNode = UmbracoContext.Current.ContentCache.GetById(config.UnauthorisedUrlContentPicker);
                                    url = contentPickerNode.Url;
                                    break;
                            }

                            if (config.RedirectRewrite == Enums.RedirectRewrite.Redirect)
                            {
                                context.Response.Redirect(url);
                            }
                            else
                            {
                                context.RewritePath(url);
                            }
                        }
                        return;
                    }
                }
                finally
                {
                    configLock.ExitReadLock();
                }
            }
        }

        //private void Application_EndRequest(Object source, EventArgs e) { }

        public void Dispose() { }
    }
}
