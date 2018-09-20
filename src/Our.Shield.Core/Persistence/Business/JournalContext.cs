using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Our.Shield.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Our.Shield.Core.Persistence.Business
{
    public class JournalContext : DbContext
    {
        private class ShouldSerializeContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                if (property.PropertyName.Equals(nameof(IJournal.EnvironmentId), StringComparison.InvariantCultureIgnoreCase) ||
                    property.PropertyName.Equals(nameof(IJournal.AppId), StringComparison.InvariantCultureIgnoreCase) ||
                    property.PropertyName.Equals(nameof(IJournal.Datestamp), StringComparison.InvariantCultureIgnoreCase))
                {
                    property.ShouldSerialize = instance => false;
                }
                return property;
            }
        }

        private IEnumerable<IJournal> GetListingResults(int? environmentId, string appId, int page, int itemsPerPage, Type type, out int totalPages)
        {
            totalPages = 0;

            var sql = new Sql();
            sql.Select("*").From<Data.Dto.Journal>(Syntax);

            if (environmentId.HasValue)
            { 
                // ReSharper disable once ImplicitlyCapturedClosure
                sql.Where<Data.Dto.Journal>(j => j.EnvironmentId == environmentId);
            }
            
            if (appId != null)
            {
                sql.Where<Data.Dto.Journal>(j => j.AppId == appId);
            }

            sql.OrderByDescending<Data.Dto.Journal>(j => j.Datestamp, Syntax);

            try
            {
                var results = Database.Page<Data.Dto.Journal>(page, itemsPerPage, sql);
                totalPages = (int) results.TotalPages;

                var typedRecords = results.Items
                    .Select(x =>
                        {
                            try
                            {
                                if (!(JsonConvert.DeserializeObject(x.Value, type) is Journal journal))
                                    return null;

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
                    .Where(x => x != null);

                return typedRecords;
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(JournalContext), $"Error getting journals for environment with Id: {environmentId}; App Id: {appId}", ex);
            }
            
            return Enumerable.Empty<IJournal>();
        }

        public IEnumerable<IJournal> Read(int environmentId, string appId, int page, int itemsPerPage, Type type, out int totalPages) => 
            GetListingResults(environmentId, appId, page, itemsPerPage, type, out totalPages);

        public IEnumerable<IJournal> Read(int environmentId, int page, int itemsPerPage, Type type, out int totalPages) => 
            GetListingResults(environmentId, null, page, itemsPerPage, type, out totalPages);

        public IEnumerable<T> Read<T>(int environmentId, string appId, int page, int itemsPerPage, out int totalPages) where T : IJournal => 
            GetListingResults(environmentId, appId, page, itemsPerPage, typeof(T), out totalPages).Select(x => (T)x);

        public IEnumerable<IJournal> Read(int page, int itemsPerPage, Type type, out int totalPages) => 
            GetListingResults(null, null, page, itemsPerPage, type, out totalPages);

        public IEnumerable<T> Read<T>(int page, int itemsPerPage, out int totalPages) where T : IJournal =>
            GetListingResults(null, null, page, itemsPerPage, typeof(T), out totalPages).Select(x => (T)x);
        
        public bool Write(int environmentId, string appId, IJournal journal)
        {
            try
            {
                Database.Insert(new Data.Dto.Journal()
                {
                    EnvironmentId = environmentId,
                    AppId = appId,
                    Datestamp = DateTime.UtcNow,
                    Value = JsonConvert.SerializeObject(journal, Formatting.None,
                        new JsonSerializerSettings { ContractResolver = new ShouldSerializeContractResolver() })
                });
                return true;
            }
            catch(Exception ex)
            {
                LogHelper.Error(typeof(JournalContext), $"Error writing Journal for environment Id: {environmentId} app Id: {appId}", ex);
            }
            return false;
        }
        
        public bool Delete(int environmentId)
        {
            try
            {
                var sql = new Sql();

                sql.Where(nameof(Data.Dto.Journal.EnvironmentId) + " = @0", environmentId);
                Database.Delete<Data.Dto.Journal>(sql);

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(JournalContext), $"Error deleting journals for environment with Id: {environmentId}", ex);
            }
            return false;
        }
    }
}
