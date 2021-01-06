using Our.Shield.Core.Persistence.Data.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Our.Shield.Core.Persistence.Business
{
    public class DbContext
    {
        private static readonly Lazy<DbContext> DatabaseInstance = new Lazy<DbContext>(() => new DbContext());
        
        public static DbContext Instance => DatabaseInstance.Value;

        private static readonly Lazy<ConfigurationContext> ConfigurationContext = new Lazy<ConfigurationContext>(() => new ConfigurationContext());
        private static readonly Lazy<DomainContext> DomainContext = new Lazy<DomainContext>(() => new DomainContext());
        private static readonly Lazy<EnvironmentContext> EnvironmentContext = new Lazy<EnvironmentContext>(() => new EnvironmentContext());
        private static readonly Lazy<JournalContext> JournalContext = new Lazy<JournalContext>(() => new JournalContext());
        
        public ConfigurationContext Configuration => ConfigurationContext.Value;

        public DomainContext Domain => DomainContext.Value;

        public EnvironmentContext Environment => EnvironmentContext.Value;

        public JournalContext Journal => JournalContext.Value;

        protected UmbracoDatabase Database => ApplicationContext.Current.DatabaseContext.Database;

        internal ISqlSyntaxProvider Syntax => ApplicationContext.Current.DatabaseContext.SqlSyntax;

        private IDictionary<int, string> UmbracoDomains() => Database.FetchAll<UmbracoDomainDto>().ToDictionary(x => x.Id, y => y.DomainName);
        
        protected IEnumerable<Domain> MapUmbracoDomains(IEnumerable<Domain> domains)
        {
            var domainArray = domains.ToArray();

            if (!domainArray.Any())
            {
                return Enumerable.Empty<Domain>();
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
        
        protected Domain MapUmbracoDomain(Domain domain)
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
    
    internal static class DbContextExtension
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
