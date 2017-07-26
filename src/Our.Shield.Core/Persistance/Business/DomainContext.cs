namespace Our.Shield.Core.Persistance.Business
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using Umbraco.Core.Logging;

    /// <summary>
    /// The Journal context
    /// </summary>
    public class DomainContext : DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="enviromentId"></param>
        /// <returns></returns>
        public IEnumerable<Data.Dto.Domain> List(int? enviromentId = null)
        {
            try
            {
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
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Data.Dto.Domain Read(int id)
        {
            try
            {
                return MapUmbracoDomain(Database.SingleOrDefault<Data.Dto.Domain>((object)id));
            }
            catch(Exception ex)
            {
                LogHelper.Error(typeof(DomainContext), $"Error reading domain with id: {id}", ex);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
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
                    domain.Id = (int)((decimal)Database.Insert(domain));
                }

                return true;
            }
            catch(Exception ex)
            {
                LogHelper.Error(typeof(DomainContext), $"Error writing domain with id: {domain.Id}", ex);
            }
            return false;
        }

        public bool Write(IEnumerable<Data.Dto.Domain> domains)
        {
            try
            {
                foreach (var domain in domains)
                {
                    Write(domain);
                }

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(DomainContext), $"Error writing domains", ex);
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
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
                LogHelper.Error(typeof(DomainContext), $"Error deleting domain with id: {domain.Id}", ex);
            }
            return false;
        }
    }
}
