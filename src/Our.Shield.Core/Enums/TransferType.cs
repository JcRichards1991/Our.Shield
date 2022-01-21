using System.ComponentModel;

namespace Our.Shield.Core.Enums
{
    /// <summary>
    /// Transfer types
    /// </summary>
    public enum TransferType
    {
        /// <summary>
        /// 
        /// </summary>
        [Description("Stops all further communication with this session. The client will be left hanging, they will conclude your server is unreachable/invalid/unresponsive")]
        PlayDead = -1,

        /// <summary>
        /// Redirect the request
        /// </summary>
        [Description("Informs the client of a new URL to use. Their address bar will be updated to reflect this new URL")]
        Redirect = 0,

        /// <summary>
        /// Rewrite the request
        /// </summary>
        [Description("Internally restarts the process with a new URL, but their Address bar will not reflect this change")]
        Rewrite = 1,

        /// <summary>
        /// Transfer the request
        /// </summary>
        [Description("Used to rewrite to an action type file. i.e. user control (.aspx, .ascx, etc.)")]
        TransferRequest = 2,

        /// <summary>
        /// 
        /// </summary>
        [Description("Used to rewrite to a physical file. i.e. CSS, JS")]
        TransmitFile = 3
    }
}
