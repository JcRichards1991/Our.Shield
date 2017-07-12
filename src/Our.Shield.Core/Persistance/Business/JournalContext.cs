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
        private IEnumerable<IJournal> GetListingResults(int environmentId, int page, int itemsPerPage, Type type, params string[] appIds)
        {
            var sql = new Sql();
            sql.Select("*")
                .From<Data.Dto.Journal>(Syntax)
                .Where<Data.Dto.Journal>(j => j.EnvironmentId == environmentId, Syntax);

            
            if (appIds != null && appIds.Any(x => !string.IsNullOrEmpty(x)))
            {
                var ids = appIds.Where(x => x != null);
                sql.Where<Data.Dto.Journal>(j => ids.Contains(j.AppId), Syntax);
            }

            sql.OrderByDescending<Data.Dto.Journal>(j => j.Datestamp, Syntax);

            try
            {
                var results = Database.Page<Data.Dto.Journal>(page, itemsPerPage, sql);

                var typedRecords = results.Items
                    .Select(x =>
                        {
                            try
                            {
                                var journal = JsonConvert.DeserializeObject(x.Value, type) as Journal;
                                journal.AppId = x.AppId;
                                journal.Datestamp = x.Datestamp;
                                journal.EnvironmentId = x.EnvironmentId;

                                return journal;
                            }
                            catch (JsonSerializationException jEx)
                            {
                                LogHelper.Error(typeof(JournalContext), $"Error Deserialising journal for environment with Id: {environmentId}; Record Id: {x.Id}; Type:{type}", jEx);
                                return null;
                            }
                            catch (Exception selectEx)
                            {
                                LogHelper.Error(typeof(JournalContext), $"An error occured getting journals for environment with Id: {environmentId}; Record Id: {x.Id};", selectEx);
                                return null;
                            }

                        })
                    .Where(x => x != null); ;

                return typedRecords;
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(JournalContext), $"Error getting journals for environment with Id: {environmentId}; App Ids: {string.Join(", ", appIds)}", ex);
            }
            return Enumerable.Empty<IJournal>();
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
        public IEnumerable<IJournal> List(int environmentId, string appId, int page, int itemsPerPage, Type type)
        {
            return GetListingResults(environmentId, page, itemsPerPage, type, appId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environmentId"></param>
        /// <param name="page"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<IJournal> List(int environmentId, int page, int itemsPerPage, Type type) => List(environmentId, null, page, itemsPerPage, type);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="environmentId"></param>
        /// <param name="appId"></param>
        /// <param name="page"></param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        public IEnumerable<T> List<T>(int environmentId, string appId, int page, int itemsPerPage) where T : IJournal
        {
            return List(environmentId, appId, page, itemsPerPage, typeof(T)).Select(x => (T) x);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environmentId"></param>
        /// <param name="appIds"></param>
        /// <param name="page"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<IJournal> ListMultiple(int environmentId, IEnumerable<string> appIds, int page, int itemsPerPage, Type type)
        {
            return GetListingResults(environmentId, page, itemsPerPage, type, appIds.ToArray());
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
