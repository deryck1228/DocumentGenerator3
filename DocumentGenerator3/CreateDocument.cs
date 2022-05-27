using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DocumentGenerator3.BulletedListData;
using DocumentGenerator3.ChildDatasetData;
using DocumentGenerator3.DocumentAssembly;
using DocumentGenerator3.DocumentDelivery;
using DocumentGenerator3.ImageHandling;
using DocumentGenerator3.ParentDatasetData;
using DocumentGenerator3.PdfConversion;
using DocumentGenerator3.TemplateData;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DocumentGenerator3
{
    public static class CreateDocument
    {
        [FunctionName("CreateDocument")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var config = new ConfigurationBuilder()
                  .AddEnvironmentVariables()
                  .Build();
            var pdfServiceName = config["PdfServiceName"];

            var outputs = new List<string>();

            var payload = context.GetInput<DocumentGeneratorPayload>();

            DocumentData documentData = new DocumentData() { originalPayload = payload};

            var parallelActivities = new List<Task>();
            var listOfChildTasks = new List<Task>();
            var listOfImageDownloads = new List<Task>();
            var listOfBulletedListConfigurations = new List<Task>();

            Task<byte[]> templateTask = context.CallActivityAsync<byte[]>($"CreateDocument_GetTemplate_{payload.template_location.settings.service}", payload);
            parallelActivities.Add(templateTask);

            Task<List<KeyValuePair<string, string>>> parentDataTask = context.CallActivityAsync<List<KeyValuePair<string, string>>>($"CreateDocument_GetParentData_{payload.parent_dataset.settings.service}", payload);
            parallelActivities.Add(parentDataTask);

            foreach (var table in payload.child_datasets)
            {
                Task<List<KeyValuePair<string, CsvWithMetadata>>> childTask = context.CallActivityAsync<List<KeyValuePair<string, CsvWithMetadata>>>($"CreateDocument_GetChildData_{table.settings.service}", table);
                parallelActivities.Add(childTask);
                listOfChildTasks.Add(childTask);
            }

            foreach( ImageLocation imageLink in payload.image_locations)
            {
                Task<ImageLocation> imageTask = context.CallActivityAsync<ImageLocation>($"CreateDocument_GetImage_{imageLink.settings.service}", imageLink);
                parallelActivities.Add(imageTask);
                listOfImageDownloads.Add(imageTask);
            }

            foreach( var listLocation in payload.bulleted_lists)
            {
                Task<KeyValuePair<string, BulletedListConfiguration>> bulletedListTask = context.CallActivityAsync<KeyValuePair<string, BulletedListConfiguration>>($"CreateDocument_GetBulletedListData_{listLocation.settings.service}", listLocation);
                parallelActivities.Add(bulletedListTask);
                listOfBulletedListConfigurations.Add(bulletedListTask);
            }

            await Task.WhenAll(parallelActivities);

            documentData.fileContents = templateTask.Result;
            documentData.parentData = parentDataTask.Result;

            foreach (Task<List<KeyValuePair<string, CsvWithMetadata>>> childTask in listOfChildTasks)
            {
                documentData.listOfTableCSVs.AddRange(childTask.Result); 
            }

            foreach (Task<ImageLocation> task in listOfImageDownloads)
            {
                documentData.originalPayload.image_locations.Add(task.Result);
            }

            foreach(Task<KeyValuePair<string,BulletedListConfiguration>> bulletedListTask in listOfBulletedListConfigurations)
            {
                documentData.bulletedListCollection.Add(bulletedListTask.Result);
            }

            documentData.fileContents = await context.CallActivityAsync<byte[]>($"CreateDocument_AddDataToTemplate_{payload.template_location.settings.template_type}", documentData);

            if(payload.document_type == ".pdf")
            {
                documentData = await context.CallActivityAsync<DocumentData>($"CreateDocument_ConvertToPdf_{pdfServiceName}", documentData);
            }

            documentData = await context.CallActivityAsync<DocumentData>($"CreateDocument_DeliverDocument_{payload.delivery_method.settings.service}", documentData);

            return outputs;
        }

        [FunctionName("CreateDocument_GetTemplate_quickbase")]
        public static byte[] GetTemplateFromQuickbase([ActivityTrigger] DocumentGeneratorPayload originalPayload, ILogger log)
        {
            log.LogInformation($"Fetching template document from Quickbase");

            var service = new GetTemplateDataService_quickbase() { Settings = (TemplateSettings_quickbase)originalPayload.template_location.settings };

            var fileData = service.GetFileContents();

            return fileData;
        }

        [FunctionName("CreateDocument_GetParentData_quickbase")]
        public static List<KeyValuePair<string, string>> GetParentDataFromQuickbase([ActivityTrigger] DocumentGeneratorPayload originalPayload, ILogger log)
        {
            log.LogInformation($"Fetching parent data from Quickbase");

            var service = new GetParentDatasetDataService_quickbase() { Settings = (ParentSettings_quickbase)originalPayload.parent_dataset.settings};

            var parentData = service.GetParentData();

            return parentData;
        }

        [FunctionName("CreateDocument_GetChildData_quickbase")]
        public async static Task<List<KeyValuePair<string, CsvWithMetadata>>> GetChildDataFromQuickbase([ActivityTrigger] ChildDataset childDataset, ILogger log)
        {
            log.LogInformation($"Fetching child data from Quickbase");

            QBTableMetadata tableMetadata = new QBTableMetadata() {  childDataset = childDataset.settings as ChildSettings_quickbase};

            var service = new GetChildDatasetDataService_quickbase() { Metadata = tableMetadata };

            string csv = "";
            int countOfColumns = 0;
            List<KeyValuePair<string, CsvWithMetadata>> quickbaseListOfTableData = new List<KeyValuePair<string, CsvWithMetadata>>();

            List<string> thisGroupByList = new();

            do
            {
                var childData = await service.GetChildDataAsync();
                countOfColumns = childData.countOfColumns;
                csv = csv + childData.thisCSV + "\n";
                thisGroupByList.AddRange(childData.thisGroupByList);
            } while ((tableMetadata.skip + tableMetadata.chunkSize) < tableMetadata.recordCount);

            string tableTitle = "[[" + tableMetadata.childDataset.table_dbid + "." + tableMetadata.childDataset.id + "]]";

            CsvWithMetadata csvWithMetadata = new CsvWithMetadata()
            {
                Csv = csv,
                GroupByData = thisGroupByList,
                CountOfColumns = countOfColumns
            };

            quickbaseListOfTableData.Add(new KeyValuePair<string, CsvWithMetadata>(tableTitle, csvWithMetadata));

            return quickbaseListOfTableData;
        }

        [FunctionName("CreateDocument_GetBulletedListData_quickbase")]
        public static KeyValuePair<string, BulletedListConfiguration> GetBulletedLIstDataFromQuickbase([ActivityTrigger] BulletedListLocation bulletedListLocation, ILogger log)
        {
            log.LogInformation("Fetching bulleted list data from Quickbase");

            var configuration = bulletedListLocation.settings.GetBulletedListConfiguration();

            return configuration;
        }

        [FunctionName("CreateDocument_AddDataToTemplate_word")]
        public static byte[] AddDataToWordTemplate([ActivityTrigger] DocumentData documentData, ILogger log)
        {
            log.LogInformation("Assembling data into word template document");

            var service = new AssembleDataInTemplate_word() { fileData = documentData };

            byte[] completedDoc = service.AssembleData();

            return completedDoc;
        }

        [FunctionName("CreateDocument_AddDataToTemplate_dc")]
        public static byte[] AddDataToDocCoreTemplate([ActivityTrigger] DocumentData documentData, ILogger log)
        {
            log.LogInformation("Assembling data into word template document");

            var service = new AssembleDataInTemplate_dc() { fileData = documentData };

            byte[] completedDoc = service.AssembleData();

            return completedDoc;
        }

        [FunctionName("CreateDocument_DeliverDocument_quickbase")]
        public static DocumentData SendCompleteDocumentToQuickbase([ActivityTrigger] DocumentData documentData, ILogger log)
        {
            log.LogInformation("Sending completed document to Quickbase");

            var service = new DeliverDocumentService_quickbase() { DocumentData = documentData};

            DocumentData completedDocument = service.SendToQuickbase();

            return completedDocument;
        }

        [FunctionName("CreateDocument_ConvertToPdf_cloud_convert")]
        public async static Task<DocumentData> ConvertDocumentToPdf_cloud_convert([ActivityTrigger] DocumentData documentData, ILogger log)
        {
            log.LogInformation("Converting document to pdf via Cloud Convert API");

            var service = new ConvertToPdfService_cloud_convert() { DocumentData = documentData };

            documentData = service.SendJob();

            do
            {
                documentData = await service.GetCompletedJob();
            }while (documentData.CloudConvertStatus != "finished" && documentData.CloudConvertStatus != "error");

            if(documentData.CloudConvertStatus == "error")
            {
                throw new Exception("Conversion to pdf failed");
            }

            documentData = service.DownloadPdf();

            return documentData;
        }

        [FunctionName("CreateDocument_GetImage_quickbase")]
        public static ImageLocation GetImageBytesFromQuickbase([ActivityTrigger] ImageLocation imageLocation, ILogger log)
        {
            log.LogInformation("Downloading image from Quickbase");

            imageLocation.settings.DownloadImage();

            return imageLocation;
        }

        [FunctionName("CreateDocument_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;

            DocumentGeneratorPayload data = new DocumentGeneratorPayload();

            try
            {
                data = await req.Content.ReadAsAsync<DocumentGeneratorPayload>();
            }
            catch (Exception e)
            {
                HttpResponseMessage errorMessage = new HttpResponseMessage();
                errorMessage.StatusCode = HttpStatusCode.BadRequest;
                errorMessage.ReasonPhrase = e.Message;
                errorMessage.Content = new StringContent(e.InnerException.ToString(), Encoding.Unicode);
                return errorMessage;
            }
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("CreateDocument", data);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }

    public class CsvWithMetadata
    {
        public string Csv { get; set; }
        public List<string> GroupByData { get; set; }
        public int CountOfColumns { get; set; }
    }
}