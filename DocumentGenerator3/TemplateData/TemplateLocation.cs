using Newtonsoft.Json;

namespace DocumentGenerator3.TemplateData
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TemplateLocation
    {
        [JsonConverter(typeof(TemplateConverter))]
        /// <summary>
        /// The settings for accessing the template
        /// </summary>
        public ITemplateLocation settings { get; set; }
    }
}
