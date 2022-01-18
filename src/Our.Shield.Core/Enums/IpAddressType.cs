using System.ComponentModel;

namespace Our.Shield.Core.Enums
{
    public enum IpAddressType
    {
        [Description("The Value is an IP Address")]
        Single = 0,

        [Description("The Value is an IP Address Range")]
        Range = 1
    }
}
