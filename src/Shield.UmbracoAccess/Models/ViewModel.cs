using System.Collections.Generic;

namespace Shield.UmbracoAccess.Models
{
    /// <summary>
    /// Umbraco Access View Model for the edit view.
    /// </summary>
    public class ViewModel
    {
        /// <summary>
        /// Gets or sets the Backend Access URL.
        /// </summary>
        public string backendAccessUrl { get; set; }

        /// <summary>
        /// Gets or sets whether or not to redirect or rewrite.
        /// </summary>
        public Enums.RedirectRewrite RedirectRewrite { get; set; }

        /// <summary>
        /// Gets or sets the Unauthorised URL Type.
        /// </summary>
        public Enums.UnautorisedUrlType unauthorisedUrlType { get; set; }

        /// <summary>
        /// Gets or sets the Unauthorised URL.
        /// </summary>
        public string unauthorisedUrl { get; set; }

        /// <summary>
        /// Gets or sets the Unauthorised URL by XPath.
        /// </summary>
        public string unauthorisedUrlXPath { get; set; }

        /// <summary>
        /// Gets or Sets the Unauthorised URL by Content Picker.
        /// </summary>
        public string unauthorisedUrlContentPicker { get; set; }

        /// <summary>
        /// Gets or sets the Ip Addresses.
        /// </summary>
        public PropertyEditors.IpAddress.Models.IpAddress[] ipAddresses { get; set; }
    }
}
