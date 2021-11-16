using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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

            // Durable Activity Function.

            return outputs;
        }

        [FunctionName("CreateDocument_GetTemplate_quickbase")]
        public static string GetTemplateFromQuickbase([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Fetching template document from Quickbase");
            return $"Hello {name}!";
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