using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.ImageHandling
{
    public interface IImageSettings_word : IImageSettings
    {
        int image_width { get; set; }
        int image_height { get; set; }
    }
}
