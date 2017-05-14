namespace Shield.UmbracoAccess.Models
{
    using Core.Operation;

    public class Operation : Core.Models.Operation<ViewModels.Configuration>
    {
        public override string Id => nameof(UmbracoAccess);


        public override Core.Models.Configuration DefaultConfiguration
        {
            get
            {
                return new ViewModels.Configuration
                {
                    BackendAccessUrl = Core.ApplicationSettings.UmbracoPath,
                    IpAddresses = new IpAddress[0]
                };
            }
        }

        public override bool Execute(Core.Models.Configuration c)
        {
            System.Diagnostics.Debug.WriteLine("START running execute");
            var config = c as ViewModels.Configuration;

            Fortress.UnwatchAll(Id);

            var id = Fortress.Watch(Id, new System.Text.RegularExpressions.Regex("joniff"), 2, (count, app) =>
            {
                System.Diagnostics.Debug.WriteLine("woohoo Joniff");
                return Fortress.Cycle.Continue;

                //if(config == null || !config.Enable)
                //{
                //    return;
                //}

                //if (configLock.TryEnterReadLock(configLockTimeout))
                //{
                //    try
                //    {
                //        if(filePath == UmbracoPath || filePath == config.BackendAccessUrl)
                //        {
                //            //If starts with umbraco path or config backend url
                //            // Let request through

                //            if (!config.IpAddresses.Any(x => x.ipAddress == ""))
                //            {
                //                string url = null;
                //                switch (config.UnauthorisedUrlType)
                //                {
                //                    case Enums.UnautorisedUrlType.Url:
                //                        url = config.UnauthorisedUrl;
                //                        break;

                //                    case Enums.UnautorisedUrlType.XPath:
                //                        var xpathNode = UmbracoContext.Current.ContentCache.GetSingleByXPath(config.UnauthorisedUrlXPath);
                //                        url = xpathNode.Url;
                //                        break;

                //                    case Enums.UnautorisedUrlType.ContentPicker:
                //                        var contentPickerNode = UmbracoContext.Current.ContentCache.GetById(config.UnauthorisedUrlContentPicker);
                //                        url = contentPickerNode.Url;
                //                        break;
                //                }

                //                if (config.RedirectRewrite == Enums.RedirectRewrite.Redirect)
                //                {
                //                    context.Response.Redirect(url);
                //                }
                //                else
                //                {
                //                    context.RewritePath(url);
                //                }
                //            }
                //            return;
                //        }
                //    }
                //    finally
                //    {
                //        configLock.ExitReadLock();
                //    }
                //}

            }, 0, null);

            id = Fortress.Watch(Id, null, 1, (count, app) => {
                return Fortress.Cycle.Continue;

            }, 0, null);
            

            return true;
        }
    }
}
