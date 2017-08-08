using System;
using System.Collections.Generic;
using System.Linq;
using Our.Shield.Core.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Our.Shield.Core.Persistance.Business
{
    /// <summary>
    /// The Environment Context for handling CRUD operations to and from the database
    /// </summary>
    public class EnvironmentContext : DbContext
    {
        /// <summary>
        /// Reads a collection of environments from the database
        /// </summary>
        /// <returns>A collection of environments</returns>
        public IEnumerable<Data.Dto.Environment> Read()
        {
            try
            {
                var environments = Database.FetchAll<Data.Dto.Environment>();
                var domains = MapUmbracoDomains(Database.FetchAll<Data.Dto.Domain>());

                foreach (var environment in environments)
                {
                    environment.Domains = domains.Where(x => x.EnvironmentId == environment.Id);
                }

                return environments;
            }
            catch(Exception ex)
            {
                LogHelper.Error(typeof(EnvironmentContext), $"Error listing environments", ex);
                return Enumerable.Empty<Data.Dto.Environment>();
            }
        }

        /// <summary>
        /// Reads a single environment from the database by it's id
        /// </summary>
        /// <param name="id">the id of the environment to read</param>
        /// <returns>null if doesn't exist; otherwise the environment</returns>
        public Data.Dto.Environment Read(int id)
        {
            try
            {
                var environment = Database.SingleOrDefault<Data.Dto.Environment>((object)id);
                if (environment != null)
                {
                    environment.Domains = MapUmbracoDomains(Database.FetchAll<Data.Dto.Domain>())
                        .Where(x => x.EnvironmentId == environment.Id);
                }
                return environment;
            }
            catch(Exception ex)
            {
                LogHelper.Error(typeof(EnvironmentContext), $"Error reading environment with id: {id}", ex);
                return null;
            }
        }

        /// <summary>
        /// Writes a new environment to the database; Or updates a pre-existing environment
        /// </summary>
        /// <param name="environment">the environment to write</param>
        /// <returns>True if successfully written; otherwise, False</returns>
        public bool Write(IEnvironment environment)
        {
            var dto = new Data.Dto.Environment
            {
                Name = environment.Name,
                Icon = environment.Icon,
                Id = environment.Id,
                Domains = environment.Domains.Select(x => new Persistance.Data.Dto.Domain
                {
                    Id = x.Id,
                    Name = x.Name,
                    UmbracoDomainId = x.UmbracoDomainId
                }),
                SortOrder = environment.SortOrder,
                Enable = environment.Enable,
                ContinueProcessing = environment.ContinueProcessing
            };
            
            try
            {
                if (environment.Id != 0 && Database.Exists<Data.Dto.Environment>(environment.Id))
                {
                    Database.Update(environment);
                }
                else
                {
                    ((Models.Environment)environment).Id = (int)((decimal)Database.Insert(dto));
                }

                foreach (var item in environment.Domains)
                {
                    var domain = new Data.Dto.Domain
                    {
                        Id = item.Id,
                        EnvironmentId = environment.Id,
                        Name = item.Name,
                        UmbracoDomainId = item.UmbracoDomainId
                    };

                    Instance.Domain.Write(domain);
                }

                return true;
            }
            catch(Exception ex)
            {
                LogHelper.Error(typeof(EnvironmentContext), $"Error writing environment with id: {environment.Id}", ex);
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environments"></param>
        /// <returns></returns>
        public bool SortEnvironments(IEnumerable<IEnvironment> environments)
        {
            try
            {
                foreach (var environment in environments)
                {
                    var dto = new Data.Dto.Environment
                    {
                        Id = environment.Id,
                        Name = environment.Name,
                        Icon = environment.Icon,
                        Enable = environment.Enable,
                        ContinueProcessing = environment.ContinueProcessing,
                        SortOrder = environment.SortOrder
                    };

                    Database.Update(dto);
                }
                return true;
            }
            catch(Exception ex)
            {
                LogHelper.Error(typeof(EnvironmentContext), $"Error sorting environments", ex);
            }

            return false;
        }

        /// <summary>
        /// Removes an environment from the database
        /// </summary>
        /// <param name="id">the id of the environment to remove</param>
        /// <returns>True if successfully removed; otherwise, False</returns>
        public bool Delete(int id)
        {
            try
            {
                if (id != 0 && Database.Exists<Data.Dto.Environment>(id))
                {
                    Database.Delete<Data.Dto.Environment>(id);
                    return true;
                }
            }
            catch(Exception ex)
            {
                LogHelper.Error(typeof(DomainContext), $"Error deleting Environment with id: {id}", ex);
            }
            return false;
        }
    }
}
