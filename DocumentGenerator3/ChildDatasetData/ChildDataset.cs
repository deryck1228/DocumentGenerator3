using Newtonsoft.Json;

namespace DocumentGenerator3.ChildDatasetData
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ChildDataset
    {
        [JsonConverter(typeof(ChildDatasetConverter))]
        /// <summary>
        /// The settings for accessing the child dataset
        /// </summary>
        public IChildSettings settings { get; set; }

    }
}
