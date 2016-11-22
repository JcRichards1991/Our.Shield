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
        private ConfigurationElementCollection GetCollection(ServerManager serverManager)
        {
            Configuration config = serverManager.GetApplicationHostConfiguration();

            // replace Default Web Site with 'Site + "/umbraco"'
            //for now, going to test against vainradical.local umbraco folder.
            //but at some point, will update to use some configuration file to populate with correct website

            ConfigurationSection ipSecuritySection = config.GetSection("system.webServer/security/ipSecurity", "vainradical.local/umbraco");
            return ipSecuritySection.GetCollection();
        }

        public void AddIpAddress(Models.IP ip)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                ConfigurationElementCollection ipSecurityCollection = GetCollection(serverManager);

                ConfigurationElement addElement = ipSecurityCollection.CreateElement("add");
                addElement["allowed"] = ip.AllowDeny == Enums.Command.Allow;

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

        //public void DeleteIpAddress(string name)
        //{
        //    using (ServerManager serverManager = new ServerManager())
        //    {
        //        ConfigurationElementCollection ipSecurityCollection = GetCollection(serverManager);

        //        ConfigurationElement deleteElement = ipSecurityCollection.
        //    }
        //}
    }
}
