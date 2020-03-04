using Our.Shield.Core.Models.CacheRefresherJson;
using Our.Shield.Core.Persistence.Business;
using Our.Shield.Core.Services;
using Our.Shield.Core.UI;
using System;
using System.Linq;
using Umbraco.Core.Cache;

namespace Our.Shield.Core.CacheRefreshers
{
    public class EnvironmentCacheRefresher : JsonCacheRefresherBase<EnvironmentCacheRefresher>
    {
        public override Guid UniqueIdentifier => new Guid(Constants.DistributedCache.EnvironmentCacheRefresherId);

        public override string Name => "Shield Environment Cache Refresher";

        protected override EnvironmentCacheRefresher Instance => this;

        public override void Refresh(string json)
        {
            var cacheInstruction = Newtonsoft.Json.JsonConvert.DeserializeObject<EnvironmentCacheRefresherJsonModel>(json);
            var environments = JobService.Instance.Environments.Keys;
            var environment = environments.FirstOrDefault(x => x.Key == cacheInstruction.Key);

            switch (cacheInstruction.CacheRefreshType)
            {
                case Enums.CacheRefreshType.Write:
                    var dbEnv = new Models.Environment(DbContext.Instance.Environment.Read(cacheInstruction.Key));

                    if (environment == null)
                    {
                        JobService.Instance.Register(dbEnv);
                    }
                    else
                    {
                        JobService.Instance.Unregister(environment);
                        JobService.Instance.Register(dbEnv);
                    }

                    break;

                case Enums.CacheRefreshType.Remove:
                    if (environment == null)
                    {
                        return;
                    }
                    else
                    {
                        JobService.Instance.Unregister(environment);
                        break;
                    }

                case Enums.CacheRefreshType.ReOrder:
                    var dbEnvs = DbContext.Instance.Environment.Read().Select(x => new Models.Environment(x));

                    foreach (var env in dbEnvs)
                    {
                        if (!environments.Any(x => x.Id.Equals(env.Id) && !x.SortOrder.Equals(env.SortOrder)))
                            continue;

                        JobService.Instance.Unregister(env);
                        JobService.Instance.Register(env);
                    }

                    break;
            }

            JobService.Instance.Poll(true);

            base.Refresh(json);
        }

        public override void RefreshAll()
        {
            throw new NotImplementedException();
        }

        public override void Refresh(int id)
        {
            throw new NotImplementedException();
        }

        public override void Refresh(Guid id)
        {
            throw new NotImplementedException();
        }

        public override void Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}
