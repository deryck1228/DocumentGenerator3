using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.ImageHandling
{
    public class ImageSettings_quickbase : IImageSettings
    {
        /// <summary>
        /// The name of the specific service being invoked for this image location
        /// </summary>
        public string service { get; set; }
        /// <summary>
        /// The main DBID of the Quickbase app in which the image location is stored
        /// </summary>
        public string app_dbid { get; set; }
        /// <summary>
        /// The DBID of the Quickbase table in which the image location is stored
        /// </summary>
        public string table_dbid { get; set; }
        /// <summary>
        /// The Quickbase realm in which the image location is stored
        /// </summary>
        public string realm { get; set; }
        /// <summary>
        /// The apptoken used to access data for the image location
        /// </summary>
        public string apptoken { get; set; }
        /// <summary>
        /// The usertoken used to access teh data for the image location
        /// </summary>
        public string usertoken { get; set; }
        /// <summary>
        /// The id value of the record ID# for the image location record in Quickbase
        /// </summary>
        public string rid { get; set; }
        /// <summary>
        /// The field id of the field in which the image is stored in Quickbase
        /// </summary>
        public string file_attachment_field_id { get; set; }
    }
}
