using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.DocumentDelivery
{
    public class DeliverDocumentService_quickbase
    {
        public DocumentData DocumentData { get; set; }
        public DocumentData SendToQuickbase()
        {
            var QBSettings = (DeliverySettings_quickbase)DocumentData.originalPayload.delivery_method.settings;
            string authtoken = "QB-USER-TOKEN " + QBSettings.usertoken;

            var encodedDocument = Convert.ToBase64String(DocumentData.fileContents);

            WebRequest request = WebRequest.Create("https://api.quickbase.com/v1/records");

            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("QB-Realm-Hostname", QBSettings.realm);
            request.Headers.Add("User-Agent", "Azure_Serverless_Functions");
            request.Headers.Add("Authorization", authtoken);

            var records = new List<string>();

            records.Add("\"" + QBSettings.document_field_id + "\":{\"value\":{\"fileName\":\"" + DocumentData.originalPayload.document_name + DocumentData.originalPayload.document_type + "\",\"data\":\"" + encodedDocument + "\"}}");

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
    }
}
