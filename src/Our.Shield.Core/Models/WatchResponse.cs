using Our.Shield.Core.Enums;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class WatchResponse
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cycle"></param>
        public WatchResponse(Cycle cycle)
        {
            Cycle = cycle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transfer"></param>
        /// <param name="isExceptionUrl"></param>
        public WatchResponse(TransferUrlControl transfer, bool isExceptionUrl = false)
        {
            Transfer = transfer;
            IsExceptionUrl = isExceptionUrl;
        }

        /// <summary>
        /// 
        /// </summary>
        public Cycle Cycle;

        /// <summary>
        /// 
        /// </summary>
        public TransferUrlControl Transfer;

        /// <summary>
        /// 
        /// </summary>
        public bool IsExceptionUrl;
    }
}
