using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.ImageHandling
{
    public class ImageLocation
    {
        [JsonConverter(typeof(ImageLocationConverter))]
        /// <summary>
        /// The settings for accessing the image location
        /// </summary>
        public IImageSettings settings { get; set; }
    }
}
