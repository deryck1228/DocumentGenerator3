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

            if (QBSettings.rid == "")
            {
                string postedDocumentResponse = SendFileAttachmentUsingXmlApi("API_AddRecord"); 
            }
            else
            {
                string postedDocumentResponse = SendFileAttachmentUsingXmlApi("API_EditRecord");
            }

            WebRequest request = WebRequest.Create("https://api.quickbase.com/v1/records");

            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("QB-Realm-Hostname", QBSettings.realm);
            request.Headers.Add("User-Agent", "Azure_Serverless_Functions");
            request.Headers.Add("Authorization", authtoken);

            var records = new List<string>();

            //records.Add("\"" + QBSettings.document_field_id + "\":{\"value\":{\"fileName\":\"" + DocumentData.originalPayload.document_name + DocumentData.originalPayload.document_type + "\",\"data\":\"" + encodedDocument + "\"}}");

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

            //Console.WriteLine(encodedDocument);

            WebResponse response = request.GetResponse();

            using (dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                var resultsData = responseFromServer;
            }

            return DocumentData;
        }

        private string SendFileAttachmentUsingXmlApi(string QBAction)
        {
            var QBSettings = (DeliverySettings_quickbase)DocumentData.originalPayload.delivery_method.settings;
            var encodedDocument = Convert.ToBase64String(DocumentData.fileContents);
            string resultsData = "";

            WebRequest request = WebRequest.Create($"https://{QBSettings.realm}/db/{QBSettings.table_dbid}?usertoken={QBSettings.usertoken}");

            request.Method = "POST";
            request.ContentType = "application/xml";
            request.Headers.Add("QUICKBASE-ACTION", QBAction);

            string ridNode = "";
            if (QBSettings.rid != "")
            {
                ridNode = $"<rid>{QBSettings.rid}</rid>"; 
            }

            string xmlBody = "<qdbapi>" +
                                $"<field fid=\"{QBSettings.document_field_id}\" filename=\"{DocumentData.originalPayload.document_name + DocumentData.originalPayload.document_type}\">{encodedDocument}</field>" +
                                ridNode +
                            "</qdbapi>";

            byte[] byteArray = Encoding.UTF8.GetBytes(xmlBody);
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

           //Console.WriteLine(encodedDocument);

            WebResponse response = request.GetResponse();

            using (dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                resultsData = responseFromServer;
            }

            //TODO: retrieve newly created rid and update DocumentData.originalPayload.delivery_method.settings.rid

            if (QBAction == "API_AddRecord")
            {
                int pFrom = resultsData.IndexOf("<rid>") + "<rid>".Length;
                int pTo = resultsData.LastIndexOf("</rid>");

                string newRid = resultsData.Substring(pFrom, pTo - pFrom); 

                QBSettings.rid = newRid;
            }

            return resultsData;
        }

    }
}
