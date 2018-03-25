using System.Configuration;

namespace Our.Shield.Core.Settings
{
    internal class RequestHeaderElement : ConfigurationElement
    {
        [ConfigurationProperty("header", IsRequired = true, IsKey = true)]
        public string Header => this["header"] as string;
    }
}
