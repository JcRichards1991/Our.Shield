using System.Configuration;

namespace Our.Shield.Core.Settings
{
    internal class ShieldSection : ConfigurationSection
    {
        [ConfigurationProperty("pollTimer", DefaultValue = "600", IsRequired = false)]
        [IntegerValidator]
        public int PollTimer
        {
            get => (int) this["pollTimer"];
            set => this["pollTimer"] = value;
        }

        [ConfigurationProperty("ipAddressValidation", IsRequired = false)]
        public IpAddressValidationElement IpAddressValidation
        {
            get => base["ipAddressValidation"] as IpAddressValidationElement;
            set => base["ipAddressValidation"] = value;
        }
    }
}
