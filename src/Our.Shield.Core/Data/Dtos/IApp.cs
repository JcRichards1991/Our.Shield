using System;

namespace Our.Shield.Core.Data.Dtos
{
    /// <summary>
    /// The DTO representation of an App
    /// </summary>
    public interface IApp : IDto
    {
        /// <summary>
        /// The App Id of the <see cref="IApp"/>
        /// </summary>
        string AppId { get; set; }

        /// <summary>
        /// The <see cref="IEnvironment"/> Key that the <see cref="IApp"/> belongs too
        /// </summary>
        Guid EnvironmentKey { get; set; }

        /// <summary>
        /// Whether or not the <see cref="IApp"/> is Enabled
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// The Confirguration for the <see cref="IApp"/>
        /// </summary>
        string Configuration { get; set; }
    }
}
