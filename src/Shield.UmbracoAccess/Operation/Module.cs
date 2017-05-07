using System.Web;
using System;
using System.Threading;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Shield.UmbracoAccess.Operation.Module), nameof(Shield.UmbracoAccess.Operation.Module.Register))]
namespace Shield.UmbracoAccess.Operation
{
    public class Module : IHttpModule
    {
        private const int configLockTimeout = 1000;

        private static ReaderWriterLockSlim configLock = new ReaderWriterLockSlim();

        private static Configuration config = null;

        public static bool Config(Configuration value)
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

            if (configLock.TryEnterReadLock(configLockTimeout))
            {
                try
                {
                    if(filePath == UmbracoPath || filePath == config.BackendAccessUrl)
                    {
                        //If starts with umbraco path or config backend url
                        // Let request through

                        context.RewritePath("");
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
