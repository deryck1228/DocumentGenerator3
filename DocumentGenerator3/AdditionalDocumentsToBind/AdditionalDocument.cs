using Newtonsoft.Json;

namespace DocumentGenerator3.AdditionalDocumentsToBind
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AdditionalDocument
    {
        [JsonConverter(typeof(AdditionalDocumentsConverter))]
        /// <summary>
        /// The settings for accessing the additional documents to be bound
        /// </summary>
        public IAdditionalDocument settings { get; set; }

    }
}
