using Newtonsoft.Json;
using System.Collections.Generic;

namespace DocumentGenerator3.AdditionalDocumentsToBind
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AdditionalDocument_direct_link : IAdditionalDocument
    {
        /// <summary>
        /// The name of the specific service being invoked for this additional document
        /// </summary>
        public string service { get; set; }
        /// <summary>
        /// The url link to this additional document, specify 'self' here to reference the document under construction
        /// </summary>
        public string link { get; set; }
        /// <summary>
        /// The name of the additional document, defaults to 'self.docx' for the document under construction
        /// </summary>
        public string doc_name { get; set; } = "self.docx";

        public List<KeyValuePair<string, string>> GetDocumentLinks()
        {
            List<KeyValuePair<string, string>> thisList = new List<KeyValuePair<string, string>>();
            thisList.Add(new KeyValuePair<string, string>(doc_name, link));
            return thisList;
        }
    }
}
