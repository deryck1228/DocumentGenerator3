using System;
using System.Collections.Generic;

namespace DocumentGenerator3.PdfConversion
{
    public class CloudConvertMerge : ICloudConvertTask
    {
        public string Operation { get; set; } = "merge";
        public string Name { get; set; }
        public string Output_format { get; set; } = "pdf";
        public string Engine { get; set; } = "poppler";
        public List<string> Input { get; set; }
        public string Serialize()
        {
            return "\"" + Name + "\":{\"operation\":\"" + Operation + "\",\"output_format\":\"" + Output_format + "\",\"engine\":\"" + Engine + "\",\"input\":[" + String.Join(",", Input) + "]}";
        }
    }
}
