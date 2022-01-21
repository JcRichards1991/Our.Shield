using System.Configuration;

namespace Our.Shield.Core.Settings
{
    internal class ShieldSettings : IShieldSettings
    {
        internal ShieldSettings()
        {
            var shieldSection = (ShieldSection) ConfigurationManager.GetSection("shieldConfiguration");

            if (shieldSection != null)
            {
                IpAddressValidation = new IpAddressValidation(shieldSection);
            }
            else
            {
                IpAddressValidation = new IpAddressValidation();
            }
        }

        public IIpAddressValidation IpAddressValidation { get; }
    }
}
