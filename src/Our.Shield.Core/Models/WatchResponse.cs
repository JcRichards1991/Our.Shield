namespace Our.Shield.Core.Models
{
	public class WatchResponse
	{
		/// <summary>
		/// The type of result to return for a given Web Request Watcher
		/// </summary>
		public enum Cycles
		{
			/// <summary>
			/// End the current request (i.e. Request has been redirected)
			/// </summary>
			Stop,
			/// <summary>
			/// Continue with the current request
			/// </summary>
			Continue,
			/// <summary>
			/// Restart the current request (i.e. Request has been rewrite-ed to another location)
			/// </summary>
			Restart,
			/// <summary>
			/// An error has occurred in the Web Request Watcher
			/// </summary>
			Error
		}

		public Cycles Cycle;
		public TransferUrl Transfer;
		public bool IsExceptionUrl;

		public WatchResponse(Cycles cycle)
		{
			Cycle = cycle;
		}

		public WatchResponse(TransferUrl transfer, bool isExceptionUrl = false)
		{
			Transfer = transfer;
			IsExceptionUrl = isExceptionUrl;
		}
	}
}
