using Our.Shield.Core.Models.CacheRefresherJson;
using Our.Shield.Core.UI;
using System;
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

            switch (cacheInstruction.CacheRefreshType)
            {
                case Enums.CacheRefreshType.Write:
                    break;

                case Enums.CacheRefreshType.Update:
                    break;

                case Enums.CacheRefreshType.Remove:
                    break;

                case Enums.CacheRefreshType.ReOrder:
                    break;
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
