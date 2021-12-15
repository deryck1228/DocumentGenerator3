using DocumentGenerator3.BulletedListData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3
{
    public class DocumentData
    {
        public string fileName { get; set; }
        public byte[] fileContents { get; set; }
        public string fileExtension { get; set; }
        public byte[] fileContentsWithData { get; set; }
        public DocumentGeneratorPayload originalPayload { get; set; }
        public List<KeyValuePair<string, string>> parentData { get; set; }
        public List<KeyValuePair<string, string>> listOfTableCSVs { get; set; } = new();
        public List<KeyValuePair<string, BulletedListConfiguration>> bulletedListCollection { get; set; } = new();
        public Exception errorMessage { get; set; }
        public string CloudConvertJobURL { get; set; }
        public string CloudConvertStatus { get; set; } = "";
        public string CloudConvertFileDownloadURL { get; set; }
        public List<KeyValuePair<string, string>> CloudConvertDocumentLinks { get; set; } = new List<KeyValuePair<string, string>>();
    }
}
