using Our.Shield.Core.Controllers.Api;
using System;
using System.Collections.Generic;

namespace Our.Shield.Core.Models.Responses
{
    /// <summary>
    /// API response model for <see cref="ShieldApiController.GetEnvironment(Guid)"/>
    /// </summary>
    public class GetEnvironmentsResponse : BaseResponse
    {
        /// <summary>
        /// The Environments in the system
        /// </summary>
        public IEnumerable<IEnvironment> Environments { get; set; }
    }
}
