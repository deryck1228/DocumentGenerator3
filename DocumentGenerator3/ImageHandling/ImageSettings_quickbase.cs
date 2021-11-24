using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.ImageHandling
{
    public class ImageSettings_quickbase : IImageSettings
    {
        private string _image_link;

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
        /// The usertoken used to access the data for the image location
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
        /// <summary>
        /// The unique ID of the image as shown the image slug of the template document
        /// </summary>
        public string image_id { get; set; }
        /// <summary>
        /// The name of the image file
        /// </summary>
        public string image_name { get; set; }
        /// <summary>
        /// The file extension of the image file
        /// </summary>
        public string image_extension { get; set; }
        /// <summary>
        /// The retrieved image as bytes
        /// </summary>
        public byte[] image_bytes { get; set; }
        /// <summary>
        /// The download link to the image file
        /// </summary>
        public string image_link { get => _image_link; set => _image_link = $"https://{realm}/up/{table_dbid}/a/r{rid}/e{file_attachment_field_id}/v0?usertoken={usertoken}"; }
        /// <summary>
        /// The width of the image in pixels
        /// </summary>
        public int image_width { get; set; }
        /// <summary>
        /// The height of the image in pixels
        /// </summary>
        public int image_height { get; set; }

        public void DownloadImage()
        {
            using (WebClient client = new WebClient())
            {
                image_bytes = client.DownloadData(image_link);

            }
        }
    }
}
