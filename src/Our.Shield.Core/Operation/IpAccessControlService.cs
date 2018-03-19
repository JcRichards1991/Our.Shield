using NetTools;
using Our.Shield.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

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
                var ipAddressRange = exception.IPAddressType == IpAccessControl.IPAddressType.Single
                    ? exception.FromIPAddress
                    : $"{exception.FromIPAddress}-{exception.ToIpAddress}";


                if (!IPAddressRange.TryParse(ipAddressRange, out var range))
                {
                    errors.Add(ipAddressRange);
                }
                range.Begin = range.Begin.MapToIPv6();
                range.End = range.End.MapToIPv6();
                exception.Range = range;
            }
            return errors;
        }

        private const string CloudFlareIpAddressHeader = "CF-Connecting-IP";
        private const string ForwardedForHeader = "X-Forwarded-For";

        /// <summary>
        /// States whether a specific ip address is valid within the rules of client access control
        /// </summary>
        /// <param name="rule">The Ip Access Control to determine whether to grant access or not</param>
        /// <param name="request">The current HttpContext Request</param>
        /// <returns></returns>
        public bool IsValid(IpAccessControl rule, HttpRequest request)
        {
            var ips = new List<string>
            {
                request.UserHostAddress
            };

            if (!string.IsNullOrWhiteSpace(request.Headers[CloudFlareIpAddressHeader]))
            {
                ips.Add(request.Headers[CloudFlareIpAddressHeader]);
            }

            if (!string.IsNullOrWhiteSpace(request.Headers[ForwardedForHeader]))
            {
                ips.Add(request.Headers[ForwardedForHeader]);
            }

            IPAddressRange clientRange;
            if (request.UserHostAddress.Equals(IPAddress.IPv6Loopback.ToString()))
            {
                clientRange = new IPAddressRange(IPAddress.Loopback);
            }
            else if (!IPAddressRange.TryParse(request.UserHostAddress, out clientRange))
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
