using Our.Shield.Shared.Enums;
using Our.Shield.Shared.Extensions;
using System.Collections.Generic;

namespace Our.Shield.Core.Models.Responses
{
    /// <summary>
    /// Base Abstract class for Responses
    /// </summary>
    public class BaseResponse
    {
        /// <summary>
        /// The Error Code if an error occurred
        /// </summary>
        public ErrorCode ErrorCode { get; set; }

        /// <summary>
        /// Any Warnings if any occurred
        /// </summary>
        public IList<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Checks whether an error has occurred
        /// </summary>
        /// <returns>True if <see cref="ErrorCode"/> does not equal <see cref="ErrorCode.None"/>, otherwise False</returns>
        public bool HasError() => ErrorCode != ErrorCode.None;

        /// <summary>
        /// Checks if there are any warnings
        /// </summary>
        /// <returns>True if there are <see cref="Warnings"/>, otherwise, False</returns>
        public bool HasWarnings() => Warnings.None();
    }
}
