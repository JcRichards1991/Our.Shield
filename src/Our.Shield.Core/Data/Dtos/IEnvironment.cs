using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Our.Shield.Core.Data.Dtos
{
    /// <summary>
    /// The DTO representation of an Environment
    /// </summary>
    public interface IEnvironment : IDto
    {
        /// <summary>
        /// Name of the <see cref="IEnvironment"/>
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Icon of the <see cref="IEnvironment"/>
        /// </summary>
        string Icon { get; set; }

        /// <summary>
        /// Sort Order of the <see cref="IEnvironment"/>
        /// </summary>
        int SortOrder { get; set; }

        /// <summary>
        /// Whether or not this <see cref="IEnvironment"/> is Enabled
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Whether or not this <see cref="IEnvironment"/> is set up to Continue Processing to allow the next Environment to handle the request
        /// </summary>
        bool ContinueProcessing { get; set; }

        /// <summary>
        /// Domains this <see cref="IEnvironment"/> handles requests for
        /// </summary>
        string Domains { get; set; }
    }
}
