using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.ImageHandling
{
    public class ImageSettings_direct_link : IImageSettings
    {
        public string service { get; set; }
        public string image_id { get; set; }
        public string image_name {get; set; }
        public string image_extension { get; set; }
        public string image_link {get; set; }
        public byte[] image_bytes { get; set; }
        /// <summary>
        /// The width of the image in pixels
        /// </summary>
        public int? image_width { get; set; }
        /// <summary>
        /// The height of the image in pixels
        /// </summary>
        public int? image_height { get; set; }

        public void DownloadImage()
        {
            throw new NotImplementedException();
        }
    }
}
