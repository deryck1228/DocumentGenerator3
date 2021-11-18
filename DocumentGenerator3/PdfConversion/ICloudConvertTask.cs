using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.PdfConversion
{
    public interface ICloudConvertTask
    {
        string Name { get; set; }
        string Operation { get; set; }
        string Serialize();
    }
}
