using System.Collections.Generic;
using System.Configuration;

namespace Our.Shield.Core.Settings
{
    public class IpAddressValidation : IIpAddressValidation
    {
        internal IpAddressValidation(ShieldSection shieldSection)
        {
            CheckUserHostAddress = shieldSection.IpAddressValidation.CheckUserHostAddress;

            var requestHeaders = new List<string>();
            using (var enumerator = shieldSection.IpAddressValidation.RequestHeadersCollection.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current != null)
                        requestHeaders.Add(enumerator.Current.Header);
                }
            }
            RequestHeaders = requestHeaders;
        }

        public bool CheckUserHostAddress { get; }

        public IEnumerable<string> RequestHeaders { get; }
    }
}
