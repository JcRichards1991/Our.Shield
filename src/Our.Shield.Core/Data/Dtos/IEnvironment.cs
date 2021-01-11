using NPoco;
using System;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Our.Shield.Core.Data.Dtos
{
    /// <summary>
    /// The DTO representation of an Environment
    /// </summary>
    public interface IEnvironment
    {
        /// <summary>
        /// Unique Identifier of the <see cref="IEnvironment"/>
        /// </summary>
        [Column(nameof(Key))]
        [PrimaryKeyColumn(AutoIncrement = false)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        Guid Key { get; set; }

        /// <summary>
        /// Last Modified date of the <see cref="IEnvironment"/>
        /// </summary>
        [Column(nameof(LastModifiedDateUtc))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        DateTime LastModifiedDateUtc { get; set; }

        /// <summary>
        /// Name of the <see cref="IEnvironment"/>
        /// </summary>
        [Column(nameof(Name))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        string Name { get; set; }

        /// <summary>
        /// Icon of the <see cref="IEnvironment"/>
        /// </summary>
        [Column(nameof(Icon))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        string Icon { get; set; }

        /// <summary>
        /// Sort Order of the <see cref="IEnvironment"/>
        /// </summary>
        [Column(nameof(SortOrder))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        int SortOrder { get; set; }

        /// <summary>
        /// Whether or not this <see cref="IEnvironment"/> is Enabled
        /// </summary>
        [Column(nameof(Enabled))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        bool Enabled { get; set; }

        /// <summary>
        /// Whether or not this <see cref="IEnvironment"/> is set up to Continue Processing to allow the next Environment to handle the request
        /// </summary>
        [Column(nameof(ContinueProcessing))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        bool ContinueProcessing { get; set; }

        /// <summary>
        /// Domains this <see cref="IEnvironment"/> handles requests for
        /// </summary>
        [Column(nameof(Domains))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        string Domains { get; set; }
    }
}
