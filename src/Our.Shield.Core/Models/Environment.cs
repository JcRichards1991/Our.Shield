namespace Our.Shield.Core.Models
{
    using Newtonsoft.Json;
    using Operation;
    using Persistance.Business;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Environment Class
    /// </summary>
    public class Environment : IEnvironment
    {
        /// <summary>
        /// The Id of the Environment
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// The Name of the Environment
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The Icon of the Environment
        /// </summary>
        [JsonProperty("icon")]
        public string Icon { get; set; }

        /// <summary>
        /// The Domains for the Environment
        /// </summary>
        [JsonProperty("domains")]
        public IEnumerable<IDomain> Domains { get; set; }

        /// <summary>
        /// Checks whether or not two Environments are the same
        /// </summary>
        /// <param name="other">The object to test against</param>
        /// <returns>True if equals; Otherwise, False</returns>
        public override bool Equals(object other)
        {
            if (other is Environment)
            {
                return Id == ((Environment) other).Id;
            }
            if (other is int)
            {
                return Id == ((int) other);
            }
            if (other is string)
            {
                return Id.ToString().Equals(((string) other));
            }
            return false;
        }

        /// <summary>
        /// Gets the Hascode for the Environment
        /// </summary>
        /// <returns>The Environment Id</returns>
        public override int GetHashCode()
        {
            return Id;
        }

        /// <summary>
        /// Default constructure
        /// </summary>
        internal Environment()
        {
        }

        /// <summary>
        /// Creates an Environment object
        /// </summary>
        /// <param name="data">The DTO object to create the Environment</param>
        internal Environment(Persistance.Data.Dto.Environment data)
        {
            Id = (int) data.Id;
            Name = data.Name;
            Icon = data.Icon;
            Domains = data.Domains.Select(x => new Domain(x));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="type"></param>
        /// <param name="totalPages"></param>
        /// <returns></returns>
        public IEnumerable<IJournal> JournalListing(int page, int itemsPerPage, Type type, out int totalPages) =>
            DbContext.Instance.Journal.Read(Id, page, itemsPerPage, type, out totalPages);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="page"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="totalPages"></param>
        /// <returns></returns>
        public IEnumerable<T> JournalListing<T>(int page, int itemsPerPage, out int totalPages) where T : IJournal =>
            DbContext.Instance.Journal.Read(Id, page, itemsPerPage, typeof(T), out totalPages).Select(x => (T)x);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool WriteEnvironment() =>
            JobService.Instance.WriteEnvironment(this);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool DeleteEnvironment() =>
            JobService.Instance.DeleteEnvironment(this);
    }
}
