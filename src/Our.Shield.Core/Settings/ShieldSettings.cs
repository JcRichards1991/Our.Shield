using System.Configuration;

namespace Our.Shield.Core.Settings
{
    public class ShieldSettings : IShieldSettings
    {
        internal ShieldSettings()
        {
            var shieldSection = (ShieldSection) ConfigurationManager.GetSection("shieldConfiguration");

            if (shieldSection != null)
            {
                PollTimer = shieldSection.PollTimer;
                IpAddressValidation = new IpAddressValidation(shieldSection);
            }
            else
            {
                PollTimer = 600;
                IpAddressValidation = new IpAddressValidation();
            }
        }

        public int PollTimer { get; }

        public IIpAddressValidation IpAddressValidation { get; }
    }
}
