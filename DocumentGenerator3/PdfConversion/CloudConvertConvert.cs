using System;
using System.Collections.Generic;

namespace DocumentGenerator3.PdfConversion
{
    public class CloudConvertConvert : ICloudConvertTask
    {
        public string Operation { get; set; } = "convert";
        public string Name { get; set; }
        public List<string> Input { get; set; }
        public string Output_format { get; set; }
        public string Filename { get; set; }
        public string Serialize()
        {
            return "\"" + Name + "\":{\"operation\":\"" + Operation + "\",\"output_format\":\"" + Output_format + "\",\"filename\":\"" + Filename + "\",\"input\":[" + String.Join(",", Input) + "]}";
        }

    }
}
