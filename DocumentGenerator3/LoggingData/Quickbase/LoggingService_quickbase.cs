using DocumentGenerator3.DocumentDelivery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.LoggingData.Quickbase
{
    public class LoggingService_quickbase
    {
        public DocumentData DocumentData { get; set; }
        public DocumentData SendToQuickbase()
        {
            var QBSettings = (LoggingSettings_quickbase)DocumentData.originalPayload.logging_method.settings;
            string authtoken = "QB-USER-TOKEN " + QBSettings.usertoken;

            WebRequest request = WebRequest.Create("https://api.quickbase.com/v1/records");

            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("QB-Realm-Hostname", QBSettings.realm);
            request.Headers.Add("User-Agent", "Azure_Serverless_Functions");
            request.Headers.Add("Authorization", authtoken);

            var records = new List<string>();

            if (QBSettings.document_field_data != "")
            {
                var mapping = QBSettings.document_field_data.Split("|");
                foreach (var map in mapping)
                {
                    var kvp = map.Split(":");
                    records.Add("\"" + kvp[0].ToString() + "\":{\"value\":\"" + kvp[1].ToString() + "\"}");
                }
            }

            if (QBSettings.rid != "")
            {
                records.Add("\"3\":{\"value\":\"" + QBSettings.rid + "\"}");
            }
            if (QBSettings.execution_id_field_id != "")
            {
                records.Add("\"" + QBSettings.execution_id_field_id + "\":{\"value\":\"" + DocumentData.DocGenLog.ExecutionId + "\"}");
            }
            if (QBSettings.message_field_id != "")
            {
                records.Add("\"" + QBSettings.message_field_id + "\":{\"value\":\"" + CleanMessageString(DocumentData.DocGenLog.Message) + "\"}");
            }
            if (QBSettings.inner_message_field_id != "")
            {
                records.Add("\"" +QBSettings.inner_message_field_id + "\":{\"value\":\"" + CleanMessageString(DocumentData.DocGenLog.InnerMessage) + "\"}");
            }

            var allRecords = String.Join(",", records);

            string json = "{\"to\":\"" + QBSettings.table_dbid +
                "\",\"data\":[{" + allRecords + "}]}";


            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();

            using (dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                var resultsData = responseFromServer;
            }

            return DocumentData;
        }

        private string CleanMessageString(string text)
        {
            return text
                .Replace("\r\n", " ") // Replace new lines (Windows) with space
                .Replace("\n", " ") // Replace new lines (Unix/Mac) with space
                .Replace("\"", "'") // Replace double quotes with single quotes
                .Replace("\\", "/"); // Optional: replace backslashes with forward slashes if needed
        }

    }
}
