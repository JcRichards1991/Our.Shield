using System;

namespace Our.Shield.Core.Data.Dtos
{
    /// <summary>
    /// Base DTO
    /// </summary>
    public abstract class Dto : IDto
    {
        /// <inheritdoc />
        public Guid Key { get; set; }

        /// <inheritdoc />
        public DateTime LastModifiedDateUtc { get; set; }
    }
}
