using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Helpers
{
    public class IPAddressAndDomainRestrictionsHelper
    {
        public void AddIpAddress(Models.IP ip)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                Configuration config = serverManager.GetApplicationHostConfiguration();
                ConfigurationSection ipSecuritySection = config.GetSection("system.webServer/security/ipSecurity", "Default Web Site");
                ConfigurationElementCollection ipSecurityCollection = ipSecuritySection.GetCollection();

                ConfigurationElement addElement = ipSecurityCollection.CreateElement("add");
                addElement["allowed"] = false;

                if (ip.IPAddress.Contains("/"))
                {
                    addElement["ipAddress"] = ip.IPAddress.Split('/')[0];
                    addElement["subnetMask"] = $"255.255.255.{ip.IPAddress.Split('/')[1]}";
                }
                else
                {
                    addElement["ipAddress"] = ip.IPAddress;
                }

                ipSecurityCollection.Add(addElement);
                serverManager.CommitChanges();
            }
        }
    }
}
