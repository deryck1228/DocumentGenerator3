﻿using System;
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
        public List<KeyValuePair<string, string>> listOfTableCSVs { get; set; }
        public Exception errorMessage { get; set; }
        //public Response emailResponse { get; set; }
        public string CloudConvertJobURL { get; set; }
        public string CloudConvertStatus { get; set; } = "";
        public string CloudConvertFileDownloadURL { get; set; }
        public List<KeyValuePair<string, string>> CloudConvertDocumentLinks { get; set; } = new List<KeyValuePair<string, string>>();
    }
}