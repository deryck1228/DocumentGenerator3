using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.TemplateData
{
    public class GetTemplateDataService_quickbase
    {
        public TemplateSettings_quickbase Settings { get; set; }

        public byte[] GetFileContents()
        {
            var fileName = "https://" + Settings.realm +
                "/up/" + Settings.table_dbid +
                "/a/r" + Settings.key_id +
                "/e" + Settings.document_fid +
                "/v0?usertoken=" + Settings.usertoken;

            WebRequest request = WebRequest.Create(fileName);
            // Get the response.
            WebResponse response = request.GetResponse();

            byte[] fileContents;
            using (WebClient client = new WebClient())
            {
                fileContents = client.DownloadData(fileName);
            }
            Stream stream = new MemoryStream();
            stream.Write(fileContents, 0, (int)fileContents.Length);

            return fileContents;
        }
    }
}
