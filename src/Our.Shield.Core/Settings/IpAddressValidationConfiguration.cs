using System.Configuration;

namespace Our.Shield.Core.Settings
{
    public class IpAddressValidation : ConfigurationElement
    {
        [ConfigurationProperty("checkUserHostAddress", DefaultValue = "true", IsRequired = false)]
        public bool CheckUserHostAddress
        {
            get => (bool)this["checkUserHostAddress"];
            set => this["checkUserHostAddress"] = value;
        }

        [ConfigurationProperty("ipAddressHeaders", IsRequired = false)]
        [ConfigurationCollection(typeof(IpAddressHeaders), AddItemName = "ipAddressHeader")]
        public IIpAddressHeaders IpAddressHeaders
        {
            get
            {
                var o = this["ipAddressHeaders"];
                return o as IIpAddressHeaders;
            }
        }
    }

    public class IpAddressHeaders : ConfigurationElementCollection, IIpAddressHeaders
    {
        public IIpAddressHeader this[int index]
        {
            get => BaseGet(index) as IpAddressHeader;
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value as IpAddressHeader);
            }
        }

        public new IIpAddressHeader this[string header]
        {
            get => (IpAddressHeader)BaseGet(header);
            set
            {
                if (BaseGet(header) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(header)));
                }
                BaseAdd(value as IpAddressHeader);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new IpAddressHeader();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((IIpAddressHeader)element).Header;
        }
    }

    public class IpAddressHeader : ConfigurationElement, IIpAddressHeader
    {
        [ConfigurationProperty("header", IsRequired = true)]
        public string Header => this["header"] as string;
    }
}
