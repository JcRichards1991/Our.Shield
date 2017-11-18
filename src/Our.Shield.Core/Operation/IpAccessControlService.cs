using NetTools;
using Our.Shield.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Our.Shield.Core.Operation
{
    public class IpAccessControlService
    {
        /// <summary>
        /// Return a list of ranges that contain invalid ranges
        /// </summary>
        /// <returns>List of errored ranges</returns>
        public IEnumerable<string> InitIpAccessControl(IpAccessControl rule)
        {
            var errors = new List<string>();
            foreach (var exception in rule.Exceptions)
            {
                if (!IPAddressRange.TryParse(exception.Value, out var range))
                {
                    errors.Add(exception.Value);
                }
                range.Begin = range.Begin.MapToIPv6();
                range.End = range.End.MapToIPv6();
                exception.Range = range;
            }
            return errors;
        }

        /// <summary>
        /// States whether a specific ip address is valid within the rules of client access control
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public bool IsValid(IpAccessControl rule, string ipAddress)
        {
            IPAddressRange clientRange;
            if (ipAddress.Equals(IPAddress.IPv6Loopback.ToString()))
            {
                clientRange = new IPAddressRange(IPAddress.Loopback);
            }
            else if (!IPAddressRange.TryParse(ipAddress, out clientRange))
            { 
                return false;
            }

            var ip6 = clientRange.Begin.MapToIPv6();

            if (rule.Exceptions.Where(x => x.Range != null).Any(exception => exception.Range.Contains(ip6)))
            {
                return rule.AccessType != IpAccessControl.AccessTypes.AllowAll;
            }
            return rule.AccessType == IpAccessControl.AccessTypes.AllowAll;
        }
    }
}
