namespace Shield.MediaProtection.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Umbraco.Web.Editors;
    using Umbraco.Web.Mvc;

    /// <summary>
    /// Api Controller for the Media Protection area of the custom section
    /// </summary>
    [PluginController(Core.Constants.App.Name)]
    class MediaProtectionApiController : UmbracoAuthorizedJsonController
    {
    }
}
