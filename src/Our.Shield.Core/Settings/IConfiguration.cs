namespace Our.Shield.Core.Settings
{
    public interface IConfiguration
    {
        IpAddressValidation IpAddressValidationSettings { get; }

        int PollTimer { get; }
    }

    public interface IIpAddressHeaders
    {
        IIpAddressHeader this[int index] { get; }
    }

    public interface IIpAddressHeader
    {
        string Header { get; }
    }
}
