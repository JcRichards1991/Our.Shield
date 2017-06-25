namespace Shield.Core.Persistance.Business
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Umbraco.Core;
    using Umbraco.Core.Logging;
    using Umbraco.Core.Persistence;

    /// <summary>
    /// The Environment Context.
    /// </summary>
    public static class EnvironmentContext
    {
        public static IEnumerable<Data.Dto.Environment> List()
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                var syntax = ApplicationContext.Current.DatabaseContext.SqlSyntax;
                var environments = db.Fetch<Data.Dto.Environment>("select *");
                var environmentsMap = environments.ToDictionary(x => x.Id, x => new List<Data.Dto.Domain>());
                var sql = new Sql()
                    .Select("d.*")
                    .From<Data.Dto.Domain>(syntax)
                    .Append("as d")
                    .Where("d." + nameof(Data.Dto.Domain.UmbracoDomainId) + " IS NULL OR " +
                        "(SELECT count(ud.id) FROM umbracoDomains as ud WHERE ud.id = d.UmbracoDomainId) > 0");
                var domains = db.Fetch<Data.Dto.Domain>(sql);

                foreach (var domain in domains)
                {
                    environmentsMap[domain.EnvironmentId].Add(domain);
                }

                foreach (var environment in environments)
                {
                    environment.Domains = environmentsMap[environment.Id];
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
        /// Reads a environment from the database.
        /// </summary>
        /// <param name="id">
        /// The id of the configuration.
        /// </param>
        /// <param name="type">
        /// The type of configuration to return;
        /// </param>
        /// <returns>
        /// The Configuration as the desired type.
        /// </returns>
        public static Data.Dto.Environment Read(int id)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                var environment = db.SingleOrDefault<Data.Dto.Environment>((object)id);
                if (environment != null)
                {
                    environment.Domains = db.Fetch<Data.Dto.Domain>("WHERE " + nameof(Data.Dto.Domain.EnvironmentId) + " = @0", environment.Id);
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
        /// Writes a Configuration to the database.
        /// </summary>
        /// <param name="id">
        /// The id of Configuration to write.
        /// </param>
        /// <param name="config">
        /// The configuration to write to the database
        /// </param>
        /// <returns>
        /// If successfull, returns true, otherwise false.
        /// </returns>
        public static bool Write(Data.Dto.Environment environment)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                if (environment.Id != null && db.Exists<Data.Dto.Environment>(environment.Id))
                {
                    db.Update(environment);
                }
                else
                {
                    environment.Id = db.Insert(environment) as int?;
                }

                return true;
            }
            catch(Exception ex)
            {
                LogHelper.Error(typeof(EnvironmentContext), $"Error writing environment with id: {environment.Id}", ex);
            }
            return false;
        }
    }
}
