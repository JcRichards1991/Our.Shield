using NPoco;
using Our.Shield.Shared;
using System.Diagnostics;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Our.Shield.Core.Data.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    [TableName(nameof(Shield) + "Environments")]
    [PrimaryKey(nameof(Key), AutoIncrement = false)]
    [ExplicitColumns]
    [DebuggerDisplay("{Name}", Name = nameof(Name))]
    [DebuggerDisplay("{Enabled}", Name = nameof(Enabled))]
    [DebuggerDisplay("{Key}", Name = nameof(Key))]
    public class Environment : Dto, IEnvironment
    {
        /// <summary>
        /// Initalises a new instance of <see cref="Environment"/> class
        /// </summary>
        public Environment()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Environment"/> class from an <see cref="Models.IEnvironment"/>
        /// </summary>
        /// <param name="env">The <see cref="Models.IEnvironment"/> to initlaize with</param>
        public Environment(Models.IEnvironment env)
        {
            GuardClauses.NotNull(env, nameof(env));

            Key = env.Key;
            Name = env.Name;
            Icon = env.Icon;
            SortOrder = env.SortOrder;
            Enabled = env.Enabled;
            ContinueProcessing = env.ContinueProcessing;
            //Domains = env.Domains;
        }

        /// <inheritdoc />
        [Column(nameof(Name))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Name { get; set; }

        /// <inheritdoc />
        [Column(nameof(Icon))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Icon { get; set; }

        /// <inheritdoc />
        [Column(nameof(SortOrder))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int SortOrder { get; set; }

        /// <inheritdoc />
        [Column(nameof(Enabled))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public bool Enabled { get; set; }

        /// <inheritdoc />
        [Column(nameof(ContinueProcessing))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public bool ContinueProcessing { get; set; }

        /// <inheritdoc />
        [Column(nameof(Domains))]
        public string Domains { get; set; }
    }
}
