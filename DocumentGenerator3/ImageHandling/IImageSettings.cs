using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.ImageHandling
{
    public interface IImageSettings
    {
        string service { get; set; }
        string image_id { get; set; }
        string image_name { get; set; }
        string image_extension { get; set; }
        string image_link { set; get; }
        byte[] image_bytes { get; set; }
        int? image_width { get; set; }
        int? image_height { get; set; }
        void DownloadImage();
    }
}
