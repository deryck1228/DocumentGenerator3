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
    public class CreateDocumentLinks 
    { 
        public List<KeyValuePair<string, string>> GetDocumentLinksFromQuickbase(AdditionalDocument_Quickbase thisDocLink)
        {

            List<KeyValuePair<string, string>> QuickbaseDownloadLinks = new List<KeyValuePair<string, string>>();

            string authtoken = "QB-USER-TOKEN " + thisDocLink.usertoken;
            string resultsData = "";
            var quickBaseValues = new List<KeyValuePair<string, string>>();
            string Uri = "";
            string json = "";


            if (thisDocLink.qid != "")
            {
                Uri = "https://api.quickbase.com/v1/reports/" + thisDocLink.qid + "/run?tableId=" + thisDocLink.table_dbid;
                json = "";
            }
            else
            {
                Uri = "https://api.quickbase.com/v1/records/query";
                json = "{\"from\":\"" + thisDocLink.table_dbid + "\"," +
                    "\"select\":[3," + thisDocLink.file_attachemnt_fid + "]," +
                    "\"where\":\"" + thisDocLink.query + "\"}";
            }
            WebRequest request = WebRequest.Create(Uri);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("QB-Realm-Hostname", thisDocLink.realm);
            request.Headers.Add("User-Agent", "Azure_Serverless_Functions");
            request.Headers.Add("Authorization", authtoken);

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
                resultsData = responseFromServer;
            }
            response.Close();

            JObject jsonResponse = JObject.Parse(resultsData);
            var items = (JArray)jsonResponse["data"];

            foreach (var item in items)
            {
                var thisRid = item["3"]["value"].ToString();
                var thisDocName = item[thisDocLink.file_attachemnt_fid]["value"]["versions"][0]["fileName"].ToString();
                string thisLink = $"https://{thisDocLink.realm}/up/{thisDocLink.table_dbid}/a/r{thisRid}/e{thisDocLink.file_attachemnt_fid}/v0?usertoken={thisDocLink.usertoken}";
                QuickbaseDownloadLinks.Add(new KeyValuePair<string, string>(thisDocName, thisLink));
            }

            return QuickbaseDownloadLinks;
        }
    }
}
