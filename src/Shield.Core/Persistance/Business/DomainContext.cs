namespace Shield.Core.Persistance.Business
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Umbraco.Core;
    using Umbraco.Core.Logging;
    using Umbraco.Core.Persistence;

    /// <summary>
    /// The Journal context
    /// </summary>
    public static class DomainContext
    {
        public static IEnumerable<Data.Dto.Domain> List(int? enviromentId = null)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                if (enviromentId == null)
                {
                    return db.Fetch<Data.Dto.Domain>("select *");
                }
                return db.Fetch<Data.Dto.Domain>("where environmentId = @0", enviromentId);
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
        public static Data.Dto.Domain Read(int id)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                return db.SingleOrDefault<Data.Dto.Domain>((object)id);
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
        public static bool Write(Data.Dto.Domain domain)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                if (domain.Id != null && db.Exists<Data.Dto.Domain>(domain.Id))
                {
                    db.Update(domain);
                }
                else
                {
                    domain.Id = db.Insert(domain) as int?;
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
        public static bool Delete(Data.Dto.Domain domain)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                if (domain.Id != null && db.Exists<Data.Dto.Domain>(domain.Id))
                {
                    db.Update(domain);
                }
                else
                {
                    domain.Id = db.Insert(domain) as int?;
                }

                return true;
            }
            catch(Exception ex)
            {
                LogHelper.Error(typeof(DomainContext), $"Error writing environment with id: {domain.Id}", ex);
            }
            return false;
        }

    }
}
