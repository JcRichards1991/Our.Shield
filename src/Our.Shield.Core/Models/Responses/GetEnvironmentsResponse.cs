using Our.Shield.Core.Controllers.Api;
using System.Collections.Generic;

namespace Our.Shield.Core.Models.Responses
{
    /// <summary>
    /// API response model for <see cref="ShieldApiController.GetEnvironments"/>
    /// </summary>
    public class GetEnvironmentsResponse : BaseResponse
    {
        /// <summary>
        /// The Environments in the system
        /// </summary>
        public IReadOnlyList<IEnvironment> Environments { get; set; }
    }
}
