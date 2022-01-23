using NetTools;
using Our.Shield.Core.Enums;
using Our.Shield.Core.Extensions;
using Our.Shield.Core.Models;
using Our.Shield.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Umbraco.Web.Security;

namespace Our.Shield.Core.Services
{
    /// <summary>
    /// Implements <see cref="IIpAccessControlService"/>
    /// </summary>
    public class IpAccessControlService : IIpAccessControlService
    {
        /// <inheritdoc />
        public IEnumerable<string> InitIpAccessControl(IpAccessControl rule)
        {
            foreach (var ipAccessRule in rule.IpAccessRules)
            {
                var ipAddressRange = ipAccessRule.IpAddressType == IpAddressType.Single
                    ? ipAccessRule.FromIpAddress
                    : $"{ipAccessRule.FromIpAddress}-{ipAccessRule.ToIpAddress}";

                if (!IPAddressRange.TryParse(ipAddressRange, out var range))
                {
                    yield return ipAddressRange;
                    continue;
                }

                range.Begin = range.Begin.MapToIPv6();
                range.End = range.End.MapToIPv6();
                ipAccessRule.Range = range;
            }
        }

        /// <inheritdoc />
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
                {
                    continue;
                }

                var headerIps = headerValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var headerIp in headerIps)
                {
                    var clientRange = GetIpAddressRange(headerIp);

                    if (clientRange != null)
                    {
                        ips.Add(clientRange.Begin.MapToIPv6());
                    }
                }
            }

            if (rule.IpAccessRules.Where(x => x.Range != null).Any(exception => exception.Range.Contains(ips)))
            {
                return rule.AccessType != AccessTypes.AllowAll;
            }

            return rule.AccessType == AccessTypes.AllowAll;
        }

        /// <inheritdoc />
        public bool IsRequestAuthenticatedUmbracoUser(HttpApplication httpApp)
        {
            var httpContext = new HttpContextWrapper(httpApp.Context);
            var umbracoAuthTicket = httpContext.GetUmbracoAuthTicket();

            if (umbracoAuthTicket != null)
            {
                httpContext.RenewUmbracoAuthTicket();
                return true;
            }

            return false;
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
}
