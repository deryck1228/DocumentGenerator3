using Newtonsoft.Json;
using System.Collections.Generic;

namespace DocumentGenerator3.AdditionalDocumentsToBind
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AdditionalDocument_direct_link : IAdditionalDocument
    {
        public string service { get; set; }
        public string link { get; set; }
        public string doc_name { get; set; } = "self.docx";

        public List<KeyValuePair<string, string>> GetDocumentLinks()
        {
            List<KeyValuePair<string, string>> thisList = new List<KeyValuePair<string, string>>();
            thisList.Add(new KeyValuePair<string, string>(doc_name, link));
            return thisList;
        }
    }
}
