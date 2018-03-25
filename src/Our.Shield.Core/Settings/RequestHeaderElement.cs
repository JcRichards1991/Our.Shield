using System.Configuration;

namespace Our.Shield.Core.Settings
{
    internal class RequestHeaderElement : ConfigurationElement
    {
        [ConfigurationProperty("header", IsRequired = true, IsKey = true)]
        public string Header
        {
            get => this["header"] as string;
            set => this["header"] = value;
        }
    }
}
