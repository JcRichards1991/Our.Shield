using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;

namespace Shield.Core.Persistance.Business
{
    internal class DbContext
    {
        private static readonly Lazy<DbContext> instance = new Lazy<DbContext>(() => new DbContext());

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

        public ConfigurationContext Configuration
        {
            get
            {
                return configurationContext.Value;
            }
        }
        
        public DomainContext Domain
        {
            get
            {
                return domainContext.Value;
            }
        }

        public EnvironmentContext Environment
        {
            get
            {
                return environmentContext.Value;
            }
        }
        public JournalContext Journal
        {
            get
            {
                return journalContext.Value;
            }
        }

        internal Umbraco.Core.Persistence.UmbracoDatabase Database
        {
            get
            {
                return ApplicationContext.Current.DatabaseContext.Database;
            }
        }

        protected Umbraco.Core.Persistence.SqlSyntax.ISqlSyntaxProvider Syntax
        {
            get
            {
                return ApplicationContext.Current.DatabaseContext.SqlSyntax;
            }
        }

        protected IDictionary<int, string> UmbracoDomains() => Database.Fetch<Data.Dto.UmbracoDomainDto>("SELECT *").ToDictionary(x => x.Id, y => y.DomainName);

        protected IEnumerable<Data.Dto.Domain> MapUmbracoDomains(IEnumerable<Data.Dto.Domain> domains)
        {
            var umbracoDomains = UmbracoDomains();
            foreach ( var domain in domains)
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
}
