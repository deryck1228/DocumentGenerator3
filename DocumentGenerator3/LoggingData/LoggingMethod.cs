using DocumentGenerator3.DocumentDelivery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.LoggingData
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class LoggingMethod
    {
        [JsonConverter(typeof(LoggingConverter))]
        /// <summary>
        /// The settings for delivering the completed document
        /// </summary>
        public ILoggingSettings settings { get; set; }
    }
}
