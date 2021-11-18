using Newtonsoft.Json;

namespace DocumentGenerator3.PdfConversion
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CloudConvertImport
    {
        public string operation { get; set; }
        public string file { get; set; }
        public string filename { get; set; }

    }
}
