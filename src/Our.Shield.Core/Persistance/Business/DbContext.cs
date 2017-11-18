using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Our.Shield.Core.Persistance.Business
{
    /// <summary>
    /// 
    /// </summary>
    public class DbContext
    {
        private static readonly Lazy<DbContext> DatabaseInstance = new Lazy<DbContext>(() => new DbContext());

        /// <summary>
        /// 
        /// </summary>
        public static DbContext Instance => DatabaseInstance.Value;

        private static readonly Lazy<ConfigurationContext> ConfigurationContext = new Lazy<ConfigurationContext>(() => new ConfigurationContext());
        private static readonly Lazy<DomainContext> DomainContext = new Lazy<DomainContext>(() => new DomainContext());
        private static readonly Lazy<EnvironmentContext> EnvironmentContext = new Lazy<EnvironmentContext>(() => new EnvironmentContext());
        private static readonly Lazy<JournalContext> JournalContext = new Lazy<JournalContext>(() => new JournalContext());

        /// <summary>
        /// 
        /// </summary>
        public ConfigurationContext Configuration
        {
            get
            {
                return ConfigurationContext.Value;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public DomainContext Domain
        {
            get
            {
                return DomainContext.Value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public EnvironmentContext Environment
        {
            get
            {
                return EnvironmentContext.Value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public JournalContext Journal
        {
            get
            {
                return JournalContext.Value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal UmbracoDatabase Database
        {
            get
            {
                return ApplicationContext.Current.DatabaseContext.Database;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected ISqlSyntaxProvider Syntax
        {
            get
            {
                return ApplicationContext.Current.DatabaseContext.SqlSyntax;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected IDictionary<int, string> UmbracoDomains() => Database.FetchAll<Data.Dto.UmbracoDomainDto>().ToDictionary(x => x.Id, y => y.DomainName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="domains"></param>
        /// <returns></returns>
        protected IEnumerable<Data.Dto.Domain> MapUmbracoDomains(IEnumerable<Data.Dto.Domain> domains)
        {
            var domainArray = domains.ToArray();

            if (!domainArray.Any())
            {
                return Enumerable.Empty<Data.Dto.Domain>();
            }
            var umbracoDomains = UmbracoDomains();
            foreach (var domain in domainArray)
            {
                if (domain.UmbracoDomainId == null)
                    continue;

                if (umbracoDomains.TryGetValue((int) domain.UmbracoDomainId, out var match))
                {
                    domain.Name = match;
                }
            }
            return domainArray.Where(x => !string.IsNullOrWhiteSpace(x.Name));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        protected Data.Dto.Domain MapUmbracoDomain(Data.Dto.Domain domain)
        {
            var umbracoDomains = UmbracoDomains();
            if (domain.UmbracoDomainId == null)
                return string.IsNullOrWhiteSpace(domain.Name) ? null : domain;

            if (umbracoDomains.TryGetValue((int) domain.UmbracoDomainId, out var match))
            {
                domain.Name = match;
            }
            return string.IsNullOrWhiteSpace(domain.Name) ? null : domain;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class DbContextExtention
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <returns></returns>
        internal static IEnumerable<T> FetchAll<T>(this UmbracoDatabase database)
        {
            return database.Fetch<T>(new Sql().Select("*").From<T>(ApplicationContext.Current.DatabaseContext.SqlSyntax));
        }
    }

}
