namespace Our.Shield.Core.Persistance.Business
{
    using Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Umbraco.Core.Logging;
    using Umbraco.Core.Persistence;

    /// <summary>
    /// The Journal context
    /// </summary>
    internal class JournalContext : DbContext
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
        public IEnumerable<IJournal> List(int environmentId, string appId, int page, int itemsPerPage, Type type)
        {
            var sql = new Sql();
            sql.Select()
                .From<Data.Dto.Journal>(Syntax)
                .Where<Data.Dto.Domain>(d => d.EnvironmentId == environmentId, Syntax);
            
            if (!string.IsNullOrWhiteSpace(appId))
            {
                sql.Where<Data.Dto.Journal>(j => j.AppId == appId, Syntax);
            }

            try
            {
                var records = Database.Page<Data.Dto.Journal>(page, itemsPerPage, sql);

                if (records?.Items?.Count == 0)
                {
                    return Enumerable.Empty<IJournal>();
                }

                return records.Items.Select(x =>
                {
                    try
                    {
                        return JsonConvert.DeserializeObject(x.Value, type) as IJournal;
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
            return Enumerable.Empty<IJournal>();
        }

        public IEnumerable<IJournal> List(int environmentId, int page, int itemsPerPage, Type type) => List(environmentId, null, page, itemsPerPage, type);

        public IEnumerable<T> List<T>(int environmentId, string appId, int page, int itemsPerPage) where T : IJournal
        {
            return List(environmentId, appId, page, itemsPerPage, typeof(T)) as IEnumerable<T>;
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
        public bool Write(int environmentId, string appId, IJournal journal)
        {
            try
            {
                Database.Insert(new Data.Dto.Journal()
                {
                    EnvironmentId = environmentId,
                    AppId = appId,
                    Datestamp = DateTime.UtcNow,
                    Value = JsonConvert.SerializeObject(journal)
                });
                return true;
            }
            catch(Exception ex)
            {
                LogHelper.Error(typeof(JournalContext), $"Error writing Journal for environment Id: {environmentId} app Id: {appId}", ex);
            }
            return false;
        }
    }
}
