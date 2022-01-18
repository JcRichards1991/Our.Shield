using System.ComponentModel;

namespace Our.Shield.Core.Enums
{
    public enum AccessTypes
    {
        [Description("Allow all client access except for specific ip addresses")]
        AllowAll = 0,

        [Description("Allow no client access except for these specific ip addresses")]
        AllowNone = 1
    }
}
