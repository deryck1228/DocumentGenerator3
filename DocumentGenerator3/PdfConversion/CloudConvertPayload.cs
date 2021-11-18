using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.PdfConversion
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CloudConvertPayload
    {
        public CloudConvertTasks tasks { get; set; }
    }
}
