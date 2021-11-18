using Newtonsoft.Json;

namespace DocumentGenerator3.DocumentDelivery
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class DeliverySettings_email : IDeliverySettings
    {
        public string service { get; set; }
        public string from_name { get; set; }


        public string to_name { get; set; }


        public string to_email { get; set; }


        public string subject_line { get; set; }


        public string body_text { get; set; }
    }
}
