using System.Collections.Generic;
using System.Configuration;

namespace Our.Shield.Core.Settings
{
    internal class IpAddressValidationElement : ConfigurationElement
    {
        [ConfigurationProperty("checkUserHostAddress", DefaultValue = "true", IsRequired = false)]
        public bool CheckUserHostAddress
        {
            get => (bool)this["checkUserHostAddress"];
            set => this["checkUserHostAddress"] = value;
        }

        [ConfigurationProperty("requestHeaders", IsRequired = false)]
        [ConfigurationCollection(typeof(RequestHeaderElementCollection))]
        public RequestHeaderElementCollection RequestHeaders
        {
            get
            {
                var o = this["requestHeaders"];
                return o as RequestHeaderElementCollection;
            }
        }

        public IEnumerable<RequestHeaderElement> RequestHeadersCollection
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
}
