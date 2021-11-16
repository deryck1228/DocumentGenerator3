using DocumentGenerator3.ParentDatasetData;
using Newtonsoft.Json;

namespace DocumentGenerator3.ParentDatasetData
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ParentDataset
    {
        [JsonConverter(typeof(ParentDatasetConverter))]
        /// <summary>
        /// The settings for accessing the parent dataset
        /// </summary>
        public IParentSettings settings { get; set; }

    }
}
