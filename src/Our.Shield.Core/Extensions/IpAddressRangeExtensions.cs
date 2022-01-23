using NetTools;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Our.Shield.Core.Extensions
{
    public static class IpAddressRangeExtensions
    {
        public static bool Contains(this IPAddressRange addressRange, IEnumerable<IPAddress> ips)
        {
            return ips.Any(addressRange.Contains);
        }
    }
}
