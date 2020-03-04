using Our.Shield.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Environment = Our.Shield.Core.Persistence.Data.Dto.Environment;

namespace Our.Shield.Core.Persistence.Business
{
    /// <inheritdoc />
    /// <summary>
    /// The Environment Context for handling CRUD operations to and from the database
    /// </summary>
    public class EnvironmentContext : DbContext
    {
        /// <summary>
        /// Reads a collection of environments from the database
        /// </summary>
        /// <returns>A collection of environments</returns>
        public IEnumerable<Environment> Read()
        {
            try
            {
                var environments = Database.FetchAll<Environment>().ToArray();
                var domains = MapUmbracoDomains(Database.FetchAll<Data.Dto.Domain>()).ToArray();

                foreach (var environment in environments)
                {
                    environment.Domains = domains.Where(x => x.EnvironmentId == environment.Id);
                }

                return environments;
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(EnvironmentContext), "Error listing environments", ex);
                return Enumerable.Empty<Environment>();
            }
        }

        /// <summary>
        /// Reads a single environment from the database by it's id
        /// </summary>
        /// <param name="id">the id of the environment to read</param>
        /// <returns>null if doesn't exist; otherwise the environment</returns>
        public Environment Read(int id)
        {
            try
            {
                var environment = Database.SingleOrDefault<Environment>(id);
                if (environment != null)
                {
                    environment.Domains = MapUmbracoDomains(Database.FetchAll<Data.Dto.Domain>())
                        .Where(x => x.EnvironmentId == environment.Id);
                }
                return environment;
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(EnvironmentContext), $"Error reading environment with id: {id}", ex);
                return null;
            }
        }

        /// <summary>
        /// Reads a single environment from the database by it's id
        /// </summary>
        /// <param name="key">The GUID key of the environment to read</param>
        /// <returns>null if doesn't exist; otherwise the environment</returns>
        public Environment Read(Guid key)
        {
            try
            {
                var sql = new Sql();
                sql.Select("*")
                    .From<Environment>(Syntax)
                    .Where<Environment>(x => x.Key == key);

                var environment = Database.FirstOrDefault<Environment>(sql);

                if (environment != null)
                {
                    environment.Domains = MapUmbracoDomains(Database.FetchAll<Data.Dto.Domain>())
                        .Where(x => x.EnvironmentId == environment.Id);
                }
                return environment;
            }
            catch (Exception ex)
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
            var dto = new Environment
            {
                Id = environment.Id,
                Key = environment.Key == Guid.Empty ? Guid.NewGuid() : environment.Key,
                Name = environment.Name,
                Icon = environment.Icon,
                Enable = environment.Enable,
                ContinueProcessing = environment.ContinueProcessing,
                SortOrder = environment.SortOrder,
                ColorIndicator = "#df7f48"
            };

            try
            {
                if (environment.Id != 0 && Database.Exists<Environment>(environment.Id))
                {
                    Database.Update(dto);
                }
                else
                {
                    ((Models.Environment)environment).Id = (int)((decimal)Database.Insert(dto));
                    ((Models.Environment)environment).Key = dto.Key;
                }

                foreach (var domain in environment.Domains)
                {
                    ((Domain)domain).EnvironmentId = environment.Id;
                    Instance.Domain.Write(domain);
                }

                Instance.Domain.Delete(environment.Id, environment.Domains.Select(x => x.Id));

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(EnvironmentContext), $"Error writing environment with id: {environment.Id}", ex);
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
                if (id != 0 && Database.Exists<Environment>(id))
                {
                    if (!Instance.Journal.Delete(id))
                        return false;
                    if (!Instance.Configuration.Delete(id))
                        return false;
                    if (!Instance.Domain.Delete(id))
                        return false;

                    Database.Delete<Environment>(id);

                    //reset sortOrder for environments
                    var records = Database.FetchAll<Environment>().OrderBy(x => x.SortOrder).ToArray();

                    for (var i = 0; i < records.Length - 1; i++)
                    {
                        records[i].SortOrder = i;
                        Database.Update(records[i]);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(DomainContext), $"Error deleting Environment with id: {id}", ex);
            }
            return false;
        }
    }
}
