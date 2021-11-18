using Newtonsoft.Json;

namespace DocumentGenerator3.DocumentDelivery
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class DeliverySettings_quickbase : IDeliverySettings
    {
        public string service { get; set; }
        public string app_dbid { get; set; }


        public string table_dbid { get; set; }


        public string realm { get; set; }


        public string apptoken { get; set; }


        public string usertoken { get; set; }


        public string rid { get; set; }


        public string document_field_id { get; set; }

        public string document_field_data { get; set; } = "";
    }
}
