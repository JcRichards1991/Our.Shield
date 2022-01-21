using System.Collections.Generic;

namespace Our.Shield.Core.Models.Responses
{
    /// <summary>
    /// 
    /// </summary>
    public class GetEnvironmentAppsResponse : BaseResponse
    {
        /// <summary>
        /// The Apps for the requested Environment
        /// </summary>
        public IReadOnlyList<IApp> Apps { get; set; }
    }
}
