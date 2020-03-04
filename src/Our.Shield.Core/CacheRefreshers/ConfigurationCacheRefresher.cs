using Our.Shield.Core.Models.CacheRefresherJson;
using Our.Shield.Core.UI;
using System;
using Umbraco.Core.Cache;

namespace Our.Shield.Core.CacheRefreshers
{
    public class ConfigurationCacheRefresher : JsonCacheRefresherBase<ConfigurationCacheRefresher>
    {
        public override Guid UniqueIdentifier => new Guid(Constants.DistributedCache.ConfigurationCacheRefresherId);

        public override string Name => "Shield Configuration Cache Refresher";

        protected override ConfigurationCacheRefresher Instance => this;

        public override void Refresh(string json)
        {
            var cacheInstruction = Newtonsoft.Json.JsonConvert.DeserializeObject<ConfigurationCacheRefresherJsonModel>(json);

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
