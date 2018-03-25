using System.Configuration;

namespace Our.Shield.Core.Settings
{
    public class ShieldSettings : IShieldSettings
    {
        internal ShieldSettings()
        {
            var shieldSection = (ShieldSection) ConfigurationManager.GetSection("shieldConfiguration");

            PollTimer = shieldSection.PollTimer;
            IpAddressValidation = new IpAddressValidation(shieldSection);
        }

        public int PollTimer { get; }

        public IIpAddressValidation IpAddressValidation { get; }
    }
}
