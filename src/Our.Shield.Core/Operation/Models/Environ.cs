using Our.Shield.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Our.Shield.Core.Operation.Models
{
    internal class Environ
    {
        public Environ(IEnvironment environment, int pipelineStagesLength)
        {
            Key = environment.Key;
            Name = string.Copy(environment.Name);
            SortOrder = environment.SortOrder;
            ContinueProcessing = environment.ContinueProcessing;
            Domains = GetDomains(environment.Domains);
            WatchLocks = new Locker[pipelineStagesLength];
            Watchers = new List<Watcher>[pipelineStagesLength];

            for (var index = 0; index != pipelineStagesLength; index++)
            {
                WatchLocks[index] = new Locker();
                Watchers[index] = new List<Watcher>();
            }
        }

        public readonly Guid Key;

        public readonly string Name;

        public readonly int SortOrder;

        public readonly bool ContinueProcessing;

        public readonly List<string> Domains;

        public readonly Locker[] WatchLocks;

        public readonly List<Watcher>[] Watchers;

        private static List<string> GetDomains(IEnumerable<IDomain> domains)
        {
            var domainsArrary = domains.ToArray();
            if (!domainsArrary.Any())
            {
                return null;
            }

            var results = new List<string>();

            foreach (var domain in domainsArrary)
            {
                UriBuilder urlwithPort, urlWithoutPort;
                try
                {
                    urlwithPort = urlWithoutPort = new UriBuilder(domain.FullyQualifiedUrl);

                }
                catch (Exception ex)
                {
                    Serilog.Log.Error(ex, "{DomainName} is not a valid domain", domain.FullyQualifiedUrl);
                    continue;
                }

                if (urlwithPort.Scheme == null)
                {
                    urlwithPort.Scheme = Uri.UriSchemeHttp;
                    UriBuilder urlHttpsWithoutPort;
                    var urlHttpsWithPort = urlHttpsWithoutPort = new UriBuilder(domain.FullyQualifiedUrl)
                    {
                        Scheme = Uri.UriSchemeHttps
                    };

                    results.Add(urlHttpsWithoutPort.ToString().Replace($":{urlHttpsWithPort.Port}", string.Empty));
                    results.Add(urlHttpsWithPort.ToString());
                }

                results.Add(urlWithoutPort.ToString().Replace($":{urlwithPort.Port}", string.Empty));
                results.Add(urlwithPort.ToString());
            }

            return results;
        }
    }
}
