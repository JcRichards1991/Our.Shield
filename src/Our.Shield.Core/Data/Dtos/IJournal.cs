using System;

namespace Our.Shield.Core.Data.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    public interface IJournal : IDto
    {
        /// <summary>
        /// The <see cref="IEnvironment"/> Key for the <see cref="IJournal"/>
        /// </summary>
        Guid EnvironmentKey { get; set; }

        /// <summary>
        /// The <see cref="IApp"/> Key for the <see cref="IJournal"/>
        /// </summary>
        Guid AppKey { get; set; }

        /// <summary>
        /// The Message logged for the <see cref="IJournal"/>
        /// </summary>
        string MessageJson { get; set; }
    }
}
