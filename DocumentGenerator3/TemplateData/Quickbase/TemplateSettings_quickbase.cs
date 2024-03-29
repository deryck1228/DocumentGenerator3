﻿using Newtonsoft.Json;

namespace DocumentGenerator3.TemplateData
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TemplateSettings_quickbase : ITemplateLocation
    {
        /// <summary>
        /// The name of the specific service being invoked for this template
        /// </summary>
        public string service { get; set; }
        /// <summary>
        /// The main DBID of the Quickbase app in which the template is stored
        /// </summary>
        public string app_dbid { get; set; }
        /// <summary>
        /// The DBID of the Quickbase table in which the template is stored
        /// </summary>
        public string table_dbid { get; set; }
        /// <summary>
        /// The Quickbase realm in which the template is stored
        /// </summary>
        public string realm { get; set; }
        /// <summary>
        /// The apptoken used to access data for the template
        /// </summary>
        public string apptoken { get; set; }
        /// <summary>
        /// The usertoken used to access the data for the template
        /// </summary>
        public string usertoken { get; set; }
        /// <summary>
        /// The id value of the key field (usually record ID#) for the record in Quickbase
        /// </summary>
        public string key_id { get; set; }
        /// <summary>
        /// The id of the field in which the template document is stored
        /// </summary>
        public string document_fid { get; set; }
        /// <summary>
        /// The document type of the template document
        /// </summary>
        public string template_type { get; set; } = "word";
    }
}
