using System.Collections.Generic;

namespace Our.Shield.Core.Settings
{
    public interface IShieldSettings
    {
        IIpAddressValidation IpAddressValidation { get; }
    }

    public interface IIpAddressValidation
    {
        bool CheckUserHostAddress { get; }

        IEnumerable<string> RequestHeaders { get; }
    }
}
