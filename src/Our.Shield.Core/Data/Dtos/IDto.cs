using System;

namespace Our.Shield.Core.Data.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDto
    {
        /// <summary>
        /// Unique Identifier of the <see cref="IDto"/>
        /// </summary>
        Guid Key { get; }

        /// <summary>
        /// Last Modified Date of the <see cref="IDto"/> 
        /// </summary>
        DateTime LastModifiedDateUtc { get; }
    }
}
