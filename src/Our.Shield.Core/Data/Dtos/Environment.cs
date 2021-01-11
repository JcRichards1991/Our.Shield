using NPoco;
using System.Diagnostics;

namespace Our.Shield.Core.Data.Dtos
{
    [TableName(nameof(Shield) + "Environments")]
    [PrimaryKey(nameof(Key), AutoIncrement = false)]
    [ExplicitColumns]
    [DebuggerDisplay("{value}", Name = nameof(Name))]
    [DebuggerDisplay("{value}", Name = nameof(Enabled))]
    [DebuggerDisplay("{value}", Name = nameof(Key))]
    internal class Environment : Dto, IEnvironment
    {
        internal Environment()
        {
        }

        internal Environment(Models.IEnvironment env)
        {
            Key = env.Key;
            Name = env.Name;
            Icon = env.Icon;
            SortOrder = env.SortOrder;
            Enabled = env.Enabled;
            ContinueProcessing = env.ContinueProcessing;
            //Domains = env.Domains;
        }

        public string Name { get; set; }

        public string Icon { get; set; }

        public int SortOrder { get; set; }

        public bool Enabled { get; set; }

        public bool ContinueProcessing { get; set; }

        public string Domains { get; set; }
    }
}
