using DocumentGenerator3.AdditionalDocumentsToBind;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.PdfConversion
{
    public class ConvertToPdfService
    {
        public DocumentData DocumentData { get; set; }

        public DocumentData SendJobToCloudConvert()
        {
            var config = new ConfigurationBuilder()
                 .AddEnvironmentVariables()
                 .Build();
            var apiKey = config["CloudConvertAPIKey"];
            string resultsData = "";

            string postData = "";

            string encodedDocument = Convert.ToBase64String(DocumentData.fileContents);

            CloudConvertPayload payload = CreateCloudConvertPayload(encodedDocument);

            foreach (var doc in DocumentData.originalPayload.additional_documents)
            {
                string serviceTypeName = $"DocumentGenerator3.AdditionalDocumentsToBind.AdditionalDocument_{doc.settings.service}";
                string objectToInstantiate = $"{serviceTypeName}, DocumentGenerator3";

                var thisObjectType = Type.GetType(objectToInstantiate);
                var additionalDocument = Activator.CreateInstance(thisObjectType) as IAdditionalDocument;

                additionalDocument = doc.settings;

                DocumentData.CloudConvertDocumentLinks.AddRange(additionalDocument.GetDocumentLinks());
            }

            if (DocumentData.originalPayload.additional_documents.Count > 0)
            {
                postData = CreateCloudConvertPayloadWithAttachments(encodedDocument);
            }

            JObject jsonResponse;
            postData = SendToCloudConvert(apiKey, out resultsData, postData, payload, out jsonResponse);
            DocumentData.CloudConvertJobURL = jsonResponse["data"]["links"]["self"].ToString();

            return DocumentData;
        }

        public async Task<DocumentData> GetCompletedJob()
        {
            var config = new ConfigurationBuilder()
                  .AddEnvironmentVariables()
                  .Build();
            var apiKey = config["CloudConvertAPIKey"];
            string resultsData = "";

            var request = (HttpWebRequest)WebRequest.Create(DocumentData.CloudConvertJobURL);

            request.Method = "GET";
            request.ContentType = "application/json";
            request.Headers.Add("User-Agent", "Azure_Serverless_Functions");
            request.Headers.Add("Authorization", "Bearer " + apiKey);

            WebResponse response = (HttpWebResponse)request.GetResponse();

            using (Stream dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                resultsData = responseFromServer;
            }
            Console.WriteLine(resultsData);

            JObject jsonResponse = JObject.Parse(resultsData);
            DocumentData.CloudConvertStatus = jsonResponse["data"]["status"].ToString();
            if (DocumentData.CloudConvertStatus == "finished")
            {
                DocumentData.CloudConvertFileDownloadURL = jsonResponse["data"]["tasks"][0]["result"]["files"][0]["url"].ToString();
            }

            return DocumentData;
        }

        public DocumentData DownloadPdf()
        {
            using (WebClient client = new WebClient())
            {
                DocumentData.fileContents = client.DownloadData(DocumentData.CloudConvertFileDownloadURL);
            }

            return DocumentData;
        }

        private string CreateCloudConvertPayloadWithAttachments(string encodedDocument)
        {
            string postData;
            CloudConvertJob Job = new CloudConvertJob();

            int docIterator = 0;

            foreach (var doc in DocumentData.CloudConvertDocumentLinks)
            {
                if (doc.Value == "self")
                {
                    CloudConvertImportBase64 importDoc = new CloudConvertImportBase64()
                    {
                        File = encodedDocument,
                        Filename = doc.Key,
                        Name = "importDoc"
                    };

                    Job.Tasks.Add(importDoc);
                }
                else
                {
                    CloudConvertImportUrl thisAdditionalDoc = new CloudConvertImportUrl()
                    {
                        Name = "importAdditionalDoc" + docIterator.ToString(),
                        Filename = doc.Key,
                        Url = doc.Value
                    };

                    Job.Tasks.Add(thisAdditionalDoc);
                }

                docIterator++;
            }

            CloudConvertMerge cloudConvertMerge = new CloudConvertMerge()
            {
                Name = "mergeTask",
                Input = new List<string>()
            };

            CloudConvertExportUrl exportUrl = new CloudConvertExportUrl()
            {
                Name = "export",
                Input = new List<string>() { "\"" + cloudConvertMerge.Name + "\"" }
            };

            foreach (var doc in Job.Tasks)
            {
                cloudConvertMerge.Input.Add("\"" + doc.Name + "\"");
            }

            Job.Tasks.Add(cloudConvertMerge);
            Job.Tasks.Add(exportUrl);
            postData = Job.Serialize();

            Console.WriteLine(postData);
            return postData;
        }

        private CloudConvertPayload CreateCloudConvertPayload(string encodedDocument)
        {
            CloudConvertPayload payload = new CloudConvertPayload { tasks = new CloudConvertTasks() };
            payload.tasks.import = new CloudConvertImport
            {
                file = encodedDocument,
                filename = DocumentData.originalPayload.document_name + ".docx",
                operation = "import/base64"
            };
            payload.tasks.task = new CloudConvertTask
            {
                operation = "convert",
                input = new List<string>() { "import" },
                filename = DocumentData.originalPayload.document_name + ".pdf",
                output_format = "pdf"
            };
            payload.tasks.export = new CloudConvertExport
            {
                operation = "export/url",
                input = new List<string>() { "task" },
                inline = false,
                archive_multiple_files = false
            };
            return payload;
        }

        private string SendToCloudConvert(string apiKey, out string resultsData, string postData, CloudConvertPayload payload, out JObject jsonResponse)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://api.cloudconvert.com/v2/jobs");

            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("User-Agent", "Azure_Serverless_Functions");
            request.Headers.Add("Authorization", "Bearer " + apiKey);


            if (DocumentData.originalPayload.additional_documents.Count == 0 || DocumentData.CloudConvertDocumentLinks.Count <= 1)
            {
                postData = JsonConvert.SerializeObject(payload);
                Console.WriteLine(postData);
            }

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse response = (HttpWebResponse)request.GetResponse();

            using (dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                resultsData = responseFromServer;
            }
            Console.WriteLine(resultsData);

            jsonResponse = JObject.Parse(resultsData);

            return postData;
        }
    }
}
