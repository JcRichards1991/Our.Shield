using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Our.Shield.Core.Models
{
    [DebuggerDisplay("{value}", Name = nameof(Name))]
    [DebuggerDisplay("{value}", Name = nameof(Enabled))]
    [DebuggerDisplay("{value}", Name = nameof(Key))]
    internal class Environment : IEnvironment
    {
        internal Environment()
        {
        }

        internal Environment(Data.Dtos.IEnvironment data)
        {
            Key = data.Key;
            Name = data.Name;
            Icon = data.Icon;
            //Domains = data.Domains.Select(x => new Domain(x));
            SortOrder = data.SortOrder;
            Enabled = data.Enabled;
            ContinueProcessing = data.ContinueProcessing;
        }

        public Guid Key { get; internal set; }

        public string Name { get; internal set; }

        public string Icon { get; internal set; }

        public IEnumerable<IDomain> Domains { get; internal set; }

        public int SortOrder { get; internal set; }

        public bool Enabled { get; internal set; }

        public bool ContinueProcessing { get; internal set; }

        public override bool Equals(object other)
        {
            switch (other)
            {
                case Environment _:
                    return Key == ((Environment)other).Key;

                case string _:
                    return Key.ToString().Equals((string)other);

                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
