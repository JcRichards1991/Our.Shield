namespace Our.Shield.Core.Persistance.Business
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Umbraco.Core;
    using Umbraco.Core.Persistence;

    /// <summary>
    /// 
    /// </summary>
    public class DbContext
    {
        private static readonly Lazy<DbContext> instance = new Lazy<DbContext>(() => new DbContext());

        /// <summary>
        /// 
        /// </summary>
        public static DbContext Instance
        {
            get
            {
                return instance.Value;
            }
        }

        private static readonly Lazy<ConfigurationContext> configurationContext = new Lazy<ConfigurationContext>(() => new ConfigurationContext());
        private static readonly Lazy<DomainContext> domainContext = new Lazy<DomainContext>(() => new DomainContext());
        private static readonly Lazy<EnvironmentContext> environmentContext = new Lazy<EnvironmentContext>(() => new EnvironmentContext());
        private static readonly Lazy<JournalContext> journalContext = new Lazy<JournalContext>(() => new JournalContext());

        /// <summary>
        /// 
        /// </summary>
        public ConfigurationContext Configuration
        {
            get
            {
                return configurationContext.Value;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public DomainContext Domain
        {
            get
            {
                return domainContext.Value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public EnvironmentContext Environment
        {
            get
            {
                return environmentContext.Value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public JournalContext Journal
        {
            get
            {
                return journalContext.Value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal Umbraco.Core.Persistence.UmbracoDatabase Database
        {
            get
            {
                return ApplicationContext.Current.DatabaseContext.Database;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected Umbraco.Core.Persistence.SqlSyntax.ISqlSyntaxProvider Syntax
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
            if (domains == null || !domains.Any())
            {
                return Enumerable.Empty<Data.Dto.Domain>();
            }
            var umbracoDomains = UmbracoDomains();
            foreach (var domain in domains)
            {
                if (domain.UmbracoDomainId != null)
                {
                    string match = null;
                    if (umbracoDomains.TryGetValue((int) domain.UmbracoDomainId, out match))
                    {
                        domain.Name = match;
                    }
                }
            }
            return domains.Where(x => !string.IsNullOrWhiteSpace(x.Name));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        protected Data.Dto.Domain MapUmbracoDomain(Data.Dto.Domain domain)
        {
            var umbracoDomains = UmbracoDomains();
            if (domain.UmbracoDomainId != null)
            {
                string match = null;
                if (umbracoDomains.TryGetValue((int) domain.UmbracoDomainId, out match))
                {
                    domain.Name = match;
                }
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
        internal static IEnumerable<T> FetchAll<T>(this Umbraco.Core.Persistence.UmbracoDatabase database)
        {
            return database.Fetch<T>(new Sql().Select("*").From<T>(ApplicationContext.Current.DatabaseContext.SqlSyntax));
        }
    }

}
