using System.Configuration;

namespace Our.Shield.Core.Settings
{
    internal class ShieldSection : ConfigurationSection, IConfiguration
    {
        [ConfigurationProperty("pollTimer", DefaultValue = "600", IsRequired = false)]
        [IntegerValidator]
        public int PollTimer { get; set; }

        [ConfigurationProperty("ipAddressValidation", DefaultValue = "600", IsRequired = false)]
        public IpAddressValidation IpAddressValidationSettings { get; set; }
    }
}
