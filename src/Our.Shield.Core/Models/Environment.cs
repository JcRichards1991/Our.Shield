using System;
using System.Collections.Generic;
using System.Linq;
using Our.Shield.Core.Persistence.Data.Migrations.Dto.Environment;

namespace Our.Shield.Core.Models
{
    public class Environment : IEnvironment
    {
        public int Id { get; internal set; }
        
        public string Name { get; internal set; }

        public Guid Key { get; internal set; }
        
        public string Icon { get; internal set; }
        
        public IEnumerable<IDomain> Domains { get; internal set; }
        
        public int SortOrder { get; internal set; }
        
        public bool Enable { get; internal set; }
        
        public bool ContinueProcessing { get; internal set; }
        
        public string ColorIndicator { get; internal set; }

        public override bool Equals(object other)
        {
            switch (other)
            {
                case Environment _:
                    return Id == ((Environment) other).Id;
                case int _:
                    return Id == (int) other;
                case string _:
                    return Id.ToString().Equals((string) other);
            }
            return false;
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Id;
        }

        internal Environment()
        {
        }
        
        internal Environment(Environment107 data)
        {
            Id = data.Id;
            Key = data.Key;
            Name = data.Name;
            Icon = data.Icon;
            Domains = data.Domains.Select(x => new Domain(x));
            SortOrder = data.SortOrder;
            Enable = data.Enable;
            ContinueProcessing = data.ContinueProcessing;
            ColorIndicator = data.ColorIndicator;
        }
    }
}
