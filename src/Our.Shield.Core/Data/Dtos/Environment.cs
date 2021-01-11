using NPoco;
using Our.Shield.Shared;
using System.Diagnostics;

namespace Our.Shield.Core.Data.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    [TableName(nameof(Shield) + "Environments")]
    [PrimaryKey(nameof(Key), AutoIncrement = false)]
    [ExplicitColumns]
    [DebuggerDisplay("{value}", Name = nameof(Name))]
    [DebuggerDisplay("{value}", Name = nameof(Enabled))]
    [DebuggerDisplay("{value}", Name = nameof(Key))]
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
        public string Name { get; set; }

        /// <inheritdoc />
        public string Icon { get; set; }

        /// <inheritdoc />
        public int SortOrder { get; set; }

        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public bool ContinueProcessing { get; set; }

        /// <inheritdoc />
        public string Domains { get; set; }
    }
}
