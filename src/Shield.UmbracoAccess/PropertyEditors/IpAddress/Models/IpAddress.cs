namespace Shield.UmbracoAccess.PropertyEditors.IpAddress.Models
{
    /// <summary>
    /// IP Address Model
    /// </summary>
    public class IpAddress
    {
        /// <summary>
        /// Gets or set the IP Address
        /// </summary>
        public string ipAddress { get; set; }

        /// <summary>
        /// Gets or sets a description for this IP Address 
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Default constructor for IP Address
        /// </summary>
        /// <param name="ip">the ip address</param>
        public IpAddress(string ip)
        {
            System.Net.IPAddress tempIp;
            if(!System.Net.IPAddress.TryParse(ip, out tempIp))
            {
                throw new System.Exception();
            }

            ipAddress = tempIp.ToString();
        }

        public IpAddress() { }
    }
}
