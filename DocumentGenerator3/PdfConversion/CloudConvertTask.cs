using Newtonsoft.Json;
using System.Collections.Generic;

namespace DocumentGenerator3.PdfConversion
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CloudConvertTask
    {
        public string operation { get; set; }
        public List<string> input { get; set; }
        public string output_format { get; set; }
        public string filename { get; set; }

    }
}
