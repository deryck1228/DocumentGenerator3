using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DocumentGenerator3.ChildDatasetData;
using DocumentGenerator3.DocumentAssembly;
using DocumentGenerator3.ParentDatasetData;
using DocumentGenerator3.TemplateData;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DocumentGenerator3
{
    public static class CreateDocument
    {
        [FunctionName("CreateDocument")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            var payload = context.GetInput<DocumentGeneratorPayload>();

            DocumentData documentData = new DocumentData() { originalPayload = payload};

            var parallelActivities = new List<Task>();
            var listOfChildTasks = new List<Task>();

            Task<byte[]> templateTask = context.CallActivityAsync<byte[]>($"CreateDocument_GetTemplate_{payload.template_location.settings.service}", payload);
            parallelActivities.Add(templateTask);

            Task<List<KeyValuePair<string, string>>> parentDataTask = context.CallActivityAsync<List<KeyValuePair<string, string>>>($"CreateDocument_GetParentData_{payload.parent_dataset.settings.service}", payload);
            parallelActivities.Add(parentDataTask);

            foreach (var table in payload.child_datasets)
            {
                Task<List<KeyValuePair<string, string>>> childTask = context.CallActivityAsync<List<KeyValuePair<string, string>>>($"CreateDocument_GetChildData_{table.settings.service}", table);
                parallelActivities.Add(childTask);
                listOfChildTasks.Add(childTask);
            }

            await Task.WhenAll(parallelActivities);

            documentData.fileContents = templateTask.Result;
            documentData.parentData = parentDataTask.Result;
            foreach (Task<List<KeyValuePair<string, string>>> childTask in listOfChildTasks)
            {
                documentData.listOfTableCSVs.AddRange(childTask.Result); 
            }

            //TODO Add in document assembly
            documentData.fileContents = await context.CallActivityAsync<byte[]>("CreateDocument_AddDataToTemplate_word", documentData);

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
        public static List<KeyValuePair<string, string>> GetChildDataFromQuickbase([ActivityTrigger] ChildDataset childDataset, ILogger log)
        {
            log.LogInformation($"Fetching child data from Quickbase");

            QBTableMetadata tableMetadata = new QBTableMetadata() {  childDataset = childDataset.settings as ChildSettings_quickbase};

            var service = new GetChildDatasetDataService_quickbase() { Metadata = tableMetadata };

            string csv = "";
            List<KeyValuePair<string, string>> quickbaseListOfTableData = new List<KeyValuePair<string, string>>();

            do
            {
                var childData = service.GetChildData();
                csv = csv + childData.thisCSV + "\n";
            } while ((tableMetadata.skip + tableMetadata.chunkSize) < tableMetadata.recordCount);

            string tableTitle = "[[" + tableMetadata.childDataset.table_dbid + "." + tableMetadata.childDataset.id + "]]";
            quickbaseListOfTableData.Add(new KeyValuePair<string, string>(tableTitle, csv));

            return quickbaseListOfTableData;
        }

        [FunctionName("CreateDocument_AddDataToTemplate_word")]
        public static byte[] AddDataToWordTemplate([ActivityTrigger] DocumentData documentData, ILogger log)
        {
            log.LogInformation("Assembling data into word template document");

            var service = new AssembleDataInTemplate_word() { fileData = documentData };

            byte[] completedDoc = service.AssembleData();

            return completedDoc;
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
}