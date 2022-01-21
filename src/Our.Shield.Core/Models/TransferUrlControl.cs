﻿using Newtonsoft.Json;
using Our.Shield.Core.Enums;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class TransferUrlControl
    {
        /// <summary>
        /// What action do we take for this URL
        /// </summary>
        [JsonProperty("transferType")]
        public TransferType TransferType { get; set; }

        /// <summary>
        /// The value of the URL
        /// </summary>
        [JsonProperty("url")]
        public UmbracoUrl Url { get; set; }
    }
}
