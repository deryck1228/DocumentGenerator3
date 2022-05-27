using Newtonsoft.Json;

namespace DocumentGenerator3.ChildDatasetData
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ChildSettings_quickbase : IChildSettings
    {
        /// <summary>
        /// The name of the specific service being invoked for this child dataset
        /// </summary>
        public string service { get; set; }
        /// <summary>
        /// The main DBID of the Quickbase app in which the child dataset is stored
        /// </summary>
        public string app_dbid { get; set; }
        /// <summary>
        /// The DBID of the Quickbase table in which the child dataset is stored
        /// </summary>
        public string table_dbid { get; set; }
        /// <summary>
        /// The Quickbase realm in which the child dataset is stored
        /// </summary>
        public string realm { get; set; }
        /// <summary>
        /// The apptoken used to access data for the child dataset
        /// </summary>
        public string apptoken { get; set; }
        /// <summary>
        /// The usertoken used to access teh data for the child dataset
        /// </summary>
        public string usertoken { get; set; }
        /// <summary>
        /// The unique id assigned to this child for purposes of determining final locaion of the data in the completed document
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// The Quickbase query string used to select the child data
        /// </summary>
        public string query { get; set; }
        /// <summary>
        /// The Quickbase sort order for a returned query, ex {"fieldId":6,"order":"ASC"}
        /// </summary>
        public string sortOrder { get; set; } = "";
        /// <summary>
        /// The Quickbase groupby clause for grouping a returned query, ex {"fieldId": 47,"grouping": "equal-values"}
        /// </summary>
        public string groupBy { get; set; } = "";
        /// <summary>
        /// The comma-delimited ordered set of field id's for this child dataset
        /// </summary>
        public string field_order { get; set; }
        /// <summary>
        /// The headers for the child dataset, ordered in the same sequence sa the field_order attribute
        /// </summary>
        public string column_headers { get; set; }
        public string table_font_family { get; set; } = "Times New Roman";
        public string table_font_size { get; set; } = "18";
        public string table_header_font_family { get; set; } = "";
        public string table_header_font_size { get; set; } = "";
    }
}
