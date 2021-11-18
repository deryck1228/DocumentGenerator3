using System;
using System.Collections.Generic;

namespace DocumentGenerator3.PdfConversion
{
    public class CloudConvertExportUrl : ICloudConvertTask
    {
        public string Operation { get; set; } = "export/url";
        public string Name { get; set; }
        public List<string> Input { get; set; }
        public string Inline { get; set; } = "false";
        public string Archive_multiple_files { get; set; } = "false";
        public string Serialize()
        {
            return "\"" + Name + "\":{\"operation\":\"" + Operation + "\",\"inline\":" + Inline + ",\"archive_multiple_files\":" + Archive_multiple_files + ",\"input\":[" + String.Join(",", Input) + "]}";
        }
    }
}
