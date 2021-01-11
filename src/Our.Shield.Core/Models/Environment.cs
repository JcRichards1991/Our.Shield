using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// 
    /// </summary>
    [DebuggerDisplay("{value}", Name = nameof(Name))]
    [DebuggerDisplay("{value}", Name = nameof(Enabled))]
    [DebuggerDisplay("{value}", Name = nameof(Key))]
    public class Environment : IEnvironment
    {
        /// <summary>
        /// Initalizes a new instance of <see cref="Environment"/> class
        /// </summary>
        public Environment()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Environment"/> class from an <see cref="Data.Dtos.IEnvironment"/>
        /// </summary>
        /// <param name="data">The <see cref="Data.Dtos.IEnvironment"/> to initialize from</param>
        public Environment(Data.Dtos.IEnvironment data)
        {
            Key = data.Key;
            Name = data.Name;
            Icon = data.Icon;
            //Domains = data.Domains.Select(x => new Domain(x));
            SortOrder = data.SortOrder;
            Enabled = data.Enabled;
            ContinueProcessing = data.ContinueProcessing;
        }

        /// <inheritdoc />
        public Guid Key { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public string Icon { get; set; }

        /// <inheritdoc />
        public IEnumerable<IDomain> Domains { get; set; }

        /// <inheritdoc />
        public int SortOrder { get; set; }

        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public bool ContinueProcessing { get; set; }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
