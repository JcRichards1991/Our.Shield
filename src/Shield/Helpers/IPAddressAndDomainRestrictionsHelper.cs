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
            ConfigurationSection ipSecuritySection = config.GetSection("system.webServer/security/ipSecurity", "vainradical.local/umbraco"); //TODO: dynamically set the website
            return ipSecuritySection.GetCollection();
        }

        /// <summary>
        /// Adds an Ip address to the IIS module
        /// </summary>
        /// <param name="ip">The IP Model to add</param>
        public void AddIpAddress(Models.Ip ip)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                var collection = GetCollection(serverManager);

                if (!IsIpDefine(ip.IpAddress, collection))
                {
                    ConfigurationElement addElement = collection.CreateElement("add");
                    addElement["allowed"] = ip.Allow;

                    if (ip.IpAddress.Contains("/"))
                    {
                        addElement["ipAddress"] = ip.IpAddress.Split('/')[0];
                        addElement["subnetMask"] = $"255.255.255.{ip.IpAddress.Split('/')[1]}";
                    }
                    else
                    {
                        addElement["ipAddress"] = ip.IpAddress;
                    }

                    collection.Add(addElement);
                    serverManager.CommitChanges();
                }
            }
        }

        private bool IsIpDefine(string ipAddress, ConfigurationElementCollection collection)
        {
            var item = collection.FirstOrDefault(x => ((string)x.GetAttributeValue("ipAddress")).Equals(ipAddress));

            if(item == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets all ups defined within the IIS module
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Models.Ip> GetIps()
        {
            ConfigurationElementCollection ipCollection = null;

            using (ServerManager serverManager = new ServerManager())
            {
                ipCollection = GetCollection(serverManager);
            }

            var retList = new List<Models.Ip>();

            foreach (var item in ipCollection)
            {
                retList.Add(new Models.Ip
                {
                    IpAddress = (string)item.GetAttributeValue("ipAddress"),
                    Allow = (bool)item.GetAttributeValue("allowed")
                });
            }
            return retList;
        }

        /// <summary>
        /// Removes an IP Address from the IIS module
        /// </summary>
        /// <param name="ipAddress">The IP address to remove</param>
        public void RemoveIpAddress(string ipAddress)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                var collection = GetCollection(serverManager);

                if(IsIpDefine(ipAddress, collection))
                {
                    var elementToRemove = collection.FirstOrDefault(x => ((string)x.GetAttributeValue("ipAddress")).Equals(ipAddress));

                    collection.Remove(elementToRemove);
                    serverManager.CommitChanges();
                }
            }
        }
    }
}
