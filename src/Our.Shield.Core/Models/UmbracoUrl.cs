using Newtonsoft.Json;

namespace Our.Shield.Core.Models
{
    public enum UmbracoUrlTypes
    {
        Url,
        XPath,
        ContentPicker
    }

    public class UmbracoUrl
    {
        /// <summary>
        /// The type of Url
        /// </summary>
        [JsonProperty("type")]
        public UmbracoUrlTypes Type { get; set; }

        /// <summary>
        /// The value of the Url
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }

		public override bool Equals(object obj)
		{
			var other = (UmbracoUrl) obj;
			return (Type == other.Type && Value.Equals(other.Value)) ? true : false;
		}
	}
}
