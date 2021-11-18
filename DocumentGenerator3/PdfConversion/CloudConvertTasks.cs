using Newtonsoft.Json;

namespace DocumentGenerator3.PdfConversion
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CloudConvertTasks
    {
        public CloudConvertImport import { get; set; }
        public CloudConvertTask task { get; set; }
        public CloudConvertExport export { get; set; }
    }
}
