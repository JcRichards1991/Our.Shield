using System.Collections.Generic;
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

        [ConfigurationProperty("requestHeaders", DefaultValue = default(RequestHeaders), IsRequired = false)]
        [ConfigurationCollection(typeof(RequestHeaders))]
        public RequestHeaders RequestHeaders
        {
            get
            {
                var o = this["requestHeaders"];
                return o as RequestHeaders;
            }
        }

        public IEnumerable<RequestHeader> RequestHeadersCollection
        {
            get
            {
                using (var enumerator = RequestHeaders.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        yield return enumerator.Current;
                    }
                }
            }
        }
    }

    public class RequestHeaders : ConfigurationElementCollection
    {
        public RequestHeader this[int index]
        {
            get => BaseGet(index) as RequestHeader;
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new RequestHeader this[string header]
        {
            get => (RequestHeader)BaseGet(header);
            set
            {
                if (BaseGet(header) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(header)));
                }
                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new RequestHeader();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RequestHeader)element).Header;
        }

        public new IEnumerator<RequestHeader> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }
    }

    public class RequestHeader : ConfigurationElement
    {
        [ConfigurationProperty("header", IsRequired = true, IsKey = true)]
        public string Header => this["header"] as string;
    }
}
