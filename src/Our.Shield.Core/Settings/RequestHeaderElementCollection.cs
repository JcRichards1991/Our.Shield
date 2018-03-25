using System.Collections.Generic;
using System.Configuration;

namespace Our.Shield.Core.Settings
{
    internal class RequestHeaderElementCollection : ConfigurationElementCollection
    {
        public RequestHeaderElement this[int index]
        {
            get => BaseGet(index) as RequestHeaderElement;
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new RequestHeaderElement this[string header]
        {
            get => (RequestHeaderElement)BaseGet(header);
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
            return new RequestHeaderElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RequestHeaderElement)element).Header;
        }

        public new IEnumerator<RequestHeaderElement> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }
    }
}
