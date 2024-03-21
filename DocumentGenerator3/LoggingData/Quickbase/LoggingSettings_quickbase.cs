using DocumentGenerator3.DocumentDelivery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.LoggingData.Quickbase
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class LoggingSettings_quickbase : ILoggingSettings
    {
        public string service { get; set; }
        public string app_dbid { get; set; }
        public string table_dbid { get; set; }
        public string realm { get; set; }
        public string apptoken { get; set; }
        public string usertoken { get; set; }
        public string rid { get; set; }
        public string document_field_data { get; set; } = "";
        public string execution_id_field_id { get; set; }
        public string message_field_id { get; set; }
        public string inner_message_field_id { get; set;}
        public string logging_level { get; set; }
    }
}

