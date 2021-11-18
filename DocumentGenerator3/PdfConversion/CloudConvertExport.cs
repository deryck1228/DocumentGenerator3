using Newtonsoft.Json;
using System.Collections.Generic;

namespace DocumentGenerator3.PdfConversion
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CloudConvertExport
    {
        public string operation { get; set; }
        public List<string> input { get; set; }
        public bool inline { get; set; }
        public bool archive_multiple_files { get; set; }
    }
}
