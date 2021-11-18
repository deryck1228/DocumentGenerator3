using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace DocumentGenerator3.DocumentDelivery
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class DeliveryMethod
    {
        [JsonConverter(typeof(DeliveryConverter))]
        /// <summary>
        /// The settings for delivering the completed document
        /// </summary>
        public IDeliverySettings settings { get;set;}
    }
}
