using Our.Shield.Core.Services;
using System;

namespace Our.Shield.Core.Models.Responses
{
    /// <summary>
    /// The Response Model for the <see cref="IEnvironmentService"/>.<see cref="IEnvironmentService.Upsert"/>
    /// </summary>
    public class UpsertEnvironmentResponse : BaseResponse
    {
        /// <summary>
        /// The Key of the environment
        /// </summary>
        public Guid Key { get;set; }
    }
}
