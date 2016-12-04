using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Shield.Persistance.Models
{
    [TableName("Shield.Configuration")]
    [PrimaryKey("Name", autoIncrement = false)]
    [ExplicitColumns]
    public abstract class Configuration : Interfaces.IConfiguration
    {
        [Column("name")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public virtual string Type { get; set; }

        [Column("datestamp")]
        public virtual DateTime Datestamp { get; set; }
        
        private string settings;

        [Column("settings")]
        public string Settings
        {
            get
            {
                return settings;
            }

            set
            {
                IsDirty = true;
                settings = value;
            }
        }

        [Ignore]
        public virtual bool IsDirty { get; set; }

        public virtual bool Save()
        {
            throw new NotImplementedException();
        }

        public virtual Interfaces.IConfiguration Read()
        {
            throw new NotImplementedException();
        }
    }
}
