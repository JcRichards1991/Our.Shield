using Our.Shield.Core.Controllers.Api;

namespace Our.Shield.Core.Models.Responses
{
    /// <summary>
    /// API response model for <see cref="ShieldApiController.GetEnvironment"/>
    /// </summary>
    public class GetEnvironmentResponse : BaseResponse
    {
        /// <summary>
        /// The Environment requested
        /// </summary>
        public IEnvironment Environment { get; set; }
    }
}
