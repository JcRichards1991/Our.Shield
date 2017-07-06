namespace Our.Shield.Core.Persistance.Business
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Umbraco.Core.Logging;

    /// <summary>
    /// The Journal context
    /// </summary>
    internal class DomainContext : DbContext
    {
        public IEnumerable<Data.Dto.Domain> List(int? enviromentId = null)
        {
            try
            {
                var umbracoDomains = UmbracoDomains();

                IEnumerable<Data.Dto.Domain> domains = null;
                if (enviromentId == null)
                {
                    domains = Database.FetchAll<Data.Dto.Domain>();
                }
                return MapUmbracoDomains(Database.Fetch<Data.Dto.Domain>("WHERE environmentId = @0", enviromentId));
            }
            catch(Exception ex)
            {
                LogHelper.Error(typeof(DomainContext), $"Error listing domains", ex);
                return Enumerable.Empty<Data.Dto.Domain>();
            }
        }

        /// <summary>
        /// Reads a Journal from the database.
        /// </summary>
        /// <typeparam name="T">
        /// The type of Journal to read.
        /// </typeparam>
        /// <param name="page">
        /// The page of results to return
        /// </param>
        /// <param name="itemsPerPage">
        /// The number of items per page
        /// </param>
        /// <returns>
        /// The Journal as the desired type.
        /// </returns>
        public Data.Dto.Domain Read(int id)
        {
            try
            {
                var umbracoDomains = UmbracoDomains();
                return MapUmbracoDomain(Database.SingleOrDefault<Data.Dto.Domain>((object)id));
            }
            catch(Exception ex)
            {
                LogHelper.Error(typeof(DomainContext), $"Error reading domain with id: {id}", ex);
                return null;
            }
        }

        /// <summary>
        /// Writes a Journal to the database.
        /// </summary>
        /// <typeparam name="T">
        /// The type of Journal to write.
        /// </typeparam>
        /// <param name="values">
        /// Journal object to write.
        /// </param>
        /// <returns>
        /// If successful, returns true; otherwise false.
        /// </returns>
        public bool Write(Data.Dto.Domain domain)
        {
            try
            {
                if (domain.Id != 0 && Database.Exists<Data.Dto.Domain>(domain.Id))
                {
                    Database.Update(domain);
                }
                else
                {
                    domain.Id = (int) Database.Insert(domain);
                }

                return true;
            }
            catch(Exception ex)
            {
                LogHelper.Error(typeof(DomainContext), $"Error writing environment with id: {domain.Id}", ex);
            }
            return false;
        }

        /// <summary>
        /// Delete a domain to the database.
        /// </summary>
        /// <typeparam name="T">
        /// The type of Journal to write.
        /// </typeparam>
        /// <param name="values">
        /// Journal object to write.
        /// </param>
        /// <returns>
        /// If successful, returns true; otherwise false.
        /// </returns>
        public bool Delete(Data.Dto.Domain domain)
        {
            try
            {
                if (domain.Id != 0 && Database.Exists<Data.Dto.Domain>(domain.Id))
                {
                    Database.Delete<Data.Dto.Domain>(domain.Id);
                    return true;
                }
            }
            catch(Exception ex)
            {
                LogHelper.Error(typeof(DomainContext), $"Error writing environment with id: {domain.Id}", ex);
            }
            return false;
        }

    }
}
