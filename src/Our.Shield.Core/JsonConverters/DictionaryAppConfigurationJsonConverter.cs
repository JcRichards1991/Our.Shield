using Newtonsoft.Json;
using Our.Shield.Core.Models;
using Our.Shield.Shared.Extensions;
using System;
using System.Collections.Generic;

namespace Our.Shield.Core.JsonConverters
{
    /// <summary>
    /// Handles converting <see cref="IDictionary{IApp, IAppConfiguration}"/> to a json array of objects
    /// </summary>
    public class DictionaryAppConfigurationJsonConverter : JsonConverter<IDictionary<IApp, IAppConfiguration>>
    {
        /// <inheritdoc />
        public override IDictionary<IApp, IAppConfiguration> ReadJson(
            JsonReader reader,
            Type objectType,
            IDictionary<IApp, IAppConfiguration> existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void WriteJson(
            JsonWriter writer,
            IDictionary<IApp, IAppConfiguration> value,
            JsonSerializer serializer)
        {
            writer.WriteStartArray();

            if (value.HasValues())
            {
                foreach (var kvp in value)
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName("app");
                    writer.WriteRawValue(JsonConvert.SerializeObject(kvp.Key));

                    writer.WritePropertyName("configuration");
                    writer.WriteRawValue(JsonConvert.SerializeObject(kvp.Value));

                    writer.WriteEndObject();
                }
            }

            writer.WriteEndArray();
        }
    }
}
