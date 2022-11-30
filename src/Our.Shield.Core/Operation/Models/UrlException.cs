using Our.Shield.Core.Models;
using System;
using System.Text.RegularExpressions;

namespace Our.Shield.Core.Operation.Models
{
    internal class UrlException
    {
        public Guid EnvironmentKey;

        public string AppId;

        public Regex Regex;

        public UmbracoUrl Url;

        public bool CalculatedUrl;

        public string CalculatedUrlWithoutSlash;

        public string CalculatedUrlWithSlash;
    }
}
