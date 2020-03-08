using Our.Shield.Core.Models;
using Our.Shield.Core.Models.CacheRefresherJson;
using Our.Shield.Core.Persistence.Business;
using Our.Shield.Core.Services;
using System;
using Umbraco.Core.Cache;

namespace Our.Shield.Core.CacheRefreshers
{
    public class ConfigurationCacheRefresher : JsonCacheRefresherBase<ConfigurationCacheRefresher>
    {
        public override Guid UniqueIdentifier => new Guid(UI.Constants.DistributedCache.ConfigurationCacheRefresherId);

        public override string Name => "Shield Configuration Cache Refresher";

        protected override ConfigurationCacheRefresher Instance => this;

        public override void Refresh(string json)
        {
            var cacheInstruction = Newtonsoft.Json.JsonConvert.DeserializeObject<ConfigurationCacheRefresherJsonModel>(json);

            var job = JobService.Instance.Job(cacheInstruction.Key);

            if (job == null)
            {
                //  Invalid id
                return;
            }

            switch (cacheInstruction.CacheRefreshType)
            {
                case Enums.CacheRefreshType.Write:
                    JobService.Instance.Execute((Job)job);
                    break;

                default:
                    return;
            }

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
