using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Our.Shield.Core.Models;
using System;
using System.Reflection;

namespace Our.Shield.Core.ContractResolvers
{
    internal class AppConfigurationShouldSerializeContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyName.Equals(nameof(IAppConfiguration.Enabled), StringComparison.InvariantCultureIgnoreCase)
                || property.PropertyName.Equals(nameof(IAppConfiguration.LastModifiedDateUtc), StringComparison.InvariantCultureIgnoreCase))
            {
                property.ShouldSerialize = instance => false;
            }

            return property;
        }
    }
}
