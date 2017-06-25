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
    public static class JournalContext
    {
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
        public static IEnumerable<Serialization.Journal> Read(int environmentId, string appId, int page, int itemsPerPage, Type type)
        {
            var syntax = ApplicationContext.Current.DatabaseContext.SqlSyntax;

            var sql = new Sql();
            sql.Select("j.*")
                .From<Data.Dto.Journal>(syntax)
                .InnerJoin<Data.Dto.Domain>(syntax).On<Data.Dto.Journal,Data.Dto.Domain>(syntax, j => j.DomainId, d => d.Id)
                .Where<Data.Dto.Domain>(d => d.EnvironmentId == environmentId, syntax);
            
            if (!string.IsNullOrWhiteSpace(appId))
            {
                sql.Where<Data.Dto.Journal>(j => j.appId == appId, syntax);
            }

            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;

                var records = db.Page<Data.Dto.Journal>(page, itemsPerPage, sql);

                if (records?.Items?.Count == 0)
                {
                    return Enumerable.Empty<Serialization.Journal>();
                }

                return records.Items.Select(x =>
                {
                    try
                    {
                        return JsonConvert.DeserializeObject(x.Value, type) as Serialization.Journal;
                    }
                    catch (JsonSerializationException jEx)
                    {
                        LogHelper.Error(typeof(ConfigurationContext), $"Error Deserialising journal for environment with Id: {environmentId}; Record Id: {x.Id}; Type:{type}", jEx);
                        return null;
                    }
                    
                }).Where(x => x != null);
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(JournalContext), $"Error getting journals for environment with Id: {environmentId}", ex);
            }
            return Enumerable.Empty<Serialization.Journal>();
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
        public static bool Write(int domainId, string appId, Serialization.Journal journal)
        {
            journal.Datestamp = DateTime.UtcNow;

            var record = new Data.Dto.Journal()
            {
                DomainId = domainId,
                appId = appId,
                Value = JsonConvert.SerializeObject(journal)
            };

            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                db.Insert(record);
                return true;
            }
            catch(Exception ex)
            {
                LogHelper.Error(typeof(JournalContext), $"Error writing Journal for app name: {appId}; with message: {journal.Message}", ex);
            }
            return false;
        }
    }
}
