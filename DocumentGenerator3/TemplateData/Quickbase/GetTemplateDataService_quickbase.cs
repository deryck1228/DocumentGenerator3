using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.TemplateData
{
    public class GetTemplateDataService_quickbase
    {
        public TemplateSettings_quickbase Settings { get; set; }

        public byte[] GetFileContents()
        {
            byte[] fileContents;

            var fileName = "https://" + Settings.realm +
                "/up/" + Settings.table_dbid +
                "/a/r" + Settings.key_id +
                "/e" + Settings.document_fid +
                "/v0?usertoken=" + Settings.usertoken;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Send a GET request to the specified URL
                    HttpResponseMessage response = client.GetAsync(fileName).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response as a byte array
                        fileContents = response.Content.ReadAsByteArrayAsync().Result;

                        // Write the content to a MemoryStream
                        using (Stream stream = new MemoryStream())
                        {
                            stream.Write(fileContents, 0, fileContents.Length);
                            // If you need to use the stream afterwards, make sure to reset its position
                            stream.Position = 0;

                            // Your further processing with the stream

                            return fileContents;
                        }
                    }

                    Console.WriteLine($"Failed to download the file. Status code: {response.StatusCode}");
                    throw new HttpRequestException($"Failed to download the file. Status code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
