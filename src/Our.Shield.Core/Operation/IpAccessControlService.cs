using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NetTools;
using Our.Shield.Core.Models;
using Umbraco.Core.Logging;

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
                IPAddressRange range;
                if (!IPAddressRange.TryParse(exception.Value, out range))
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
        /// <param name="clientIp"></param>
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

            foreach (var exception in rule.Exceptions.Where(x => x.Range != null))
            {
                if (exception.Range.Contains(ip6))
                {
                    return rule.AccessType == IpAccessControl.AccessTypes.AllowAll ? false : true;
                }
            }
            return rule.AccessType == IpAccessControl.AccessTypes.AllowAll ? true : false;
        }
    }
}
