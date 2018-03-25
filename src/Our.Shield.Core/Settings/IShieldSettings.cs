using System.Collections.Generic;

namespace Our.Shield.Core.Settings
{
    public interface IShieldSettings
    {
        int PollTimer { get; }

        IIpAddressValidation IpAddressValidation { get; }
    }

    public interface IIpAddressValidation
    {
        bool CheckUserHostAddress { get; }

        IEnumerable<string> RequestHeaders { get; }
    }
}
