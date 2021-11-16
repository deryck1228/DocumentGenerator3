using Newtonsoft.Json;

namespace DocumentGenerator3.TemplateData
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TemplateLocation
    {
        /// <summary>
        /// The name of the service to handle the template's location
        /// </summary>
        public string service { get; set; }
        [JsonConverter(typeof(TemplateConverter))]
        /// <summary>
        /// The settings for accessing the template
        /// </summary>
        public ITemplateLocation settings { get; set; }
    }
}
