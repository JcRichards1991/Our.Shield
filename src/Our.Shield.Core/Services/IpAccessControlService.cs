using NetTools;
using Our.Shield.Core.Models;
using Our.Shield.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace Our.Shield.Core.Services
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
                var ipAddressRange = exception.IpAddressType == IpAccessControl.IpAddressType.Single
                    ? exception.FromIpAddress
                    : $"{exception.FromIpAddress}-{exception.ToIpAddress}";


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

        /// <summary>
        /// States whether a specific ip address is valid within the rules of client access control
        /// </summary>
        /// <param name="rule">The Ip Access Control to determine whether to grant access or not</param>
        /// <param name="request">The current HttpContext Request</param>
        /// <returns></returns>
        public bool IsValid(IpAccessControl rule, HttpRequest request)
        {
            var ips = new List<IPAddress>();

            if (ShieldConfiguration.IpAddressValidation.CheckUserHostAddress)
            {
                ips.Add(GetIpAddressRange(request.UserHostAddress).Begin.MapToIPv6());
            }

            foreach (var requestHeader in ShieldConfiguration.IpAddressValidation.RequestHeaders)
            {
                var headerValue = request.Headers[requestHeader];

                if (string.IsNullOrEmpty(headerValue))
                    continue;

                var headerIps = headerValue.Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries);

                foreach (var headerIp in headerIps)
                {
                    var clientRange = GetIpAddressRange(headerIp);

                    if (clientRange != null)
                        ips.Add(clientRange.Begin.MapToIPv6());
                }
            }

            if (rule.Exceptions.Where(x => x.Range != null).Any(exception => exception.Range.Contains(ips)))
            {
                return rule.AccessType != IpAccessControl.AccessTypes.AllowAll;
            }

            return rule.AccessType == IpAccessControl.AccessTypes.AllowAll;
        }

        private IPAddressRange GetIpAddressRange(string ipAddress)
        {
            if (ipAddress.Equals(IPAddress.IPv6Loopback.ToString()))
            {
                return new IPAddressRange(IPAddress.Loopback);
            }

            return IPAddressRange.TryParse(ipAddress, out IPAddressRange range)
                ? range 
                : null;
        }
    }

    public static class IpAddressRangeExtensions
    {
        public static bool Contains(this IPAddressRange addressRange, IEnumerable<IPAddress> ips)
        {
            return ips.Any(addressRange.Contains);
        }
    }
}
