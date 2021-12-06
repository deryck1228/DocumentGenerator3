using Newtonsoft.Json;
using System.Collections.Generic;

namespace DocumentGenerator3.ParentDatasetData
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ParentSettings_quickbase : IParentSettings
    {
        /// <summary>
        /// The name of the specific service being invoked for this parent dataset
        /// </summary>
        public string service { get; set; }
        /// <summary>
        /// The main DBID of the Quickbase app in which the parent dataset is stored
        /// </summary>
        public string app_dbid { get; set; }
        /// <summary>
        /// The DBID of the Quickbase table in which the parent dataset is stored
        /// </summary>
        public string table_dbid { get; set; }
        /// <summary>
        /// The Quickbase realm in which the parent dataset is stored
        /// </summary>
        public string realm { get; set; }
        /// <summary>
        /// The apptoken used to access data for the parent dataset
        /// </summary>
        public string apptoken { get; set; }
        /// <summary>
        /// The usertoken used to access teh data for the parent dataset
        /// </summary>
        public string usertoken { get; set; }
        /// <summary>
        /// The id value of the record ID# for the parent dataset record in Quickbase
        /// </summary>
        public string rid { get; set; }
        /// <summary>
        /// The merge field id used to determine equality when post to or getting from the parent dataset in Quickbase
        /// </summary>
        public string merge_field_id { get; set; }
        public List<BulletedListContainer> fids_containing_lists { get; set; }
    }
}
