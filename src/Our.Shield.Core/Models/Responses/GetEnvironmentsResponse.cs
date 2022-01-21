using Our.Shield.Core.Services;
using System.Collections.Generic;

namespace Our.Shield.Core.Models.Responses
{
    /// <summary>
    /// API response model for <see cref="IEnvironmentService.Get()"/> &amp; <see cref="IEnvironmentService.Get(System.Guid)"/>
    /// &amp; <see cref="IEnvironmentService.GetAsync()"/> &amp; <see cref="IEnvironmentService.GetAsync(System.Guid)"/>
    /// </summary>
    public class GetEnvironmentsResponse : BaseResponse
    {
        /// <summary>
        /// The Environments in the system
        /// </summary>
        public IEnumerable<IEnvironment> Environments { get; set; }
    }
}
