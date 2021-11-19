using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace DocumentGenerator3.AdditionalDocumentsToBind
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AdditionalDocument_quickbase : IAdditionalDocument
    {
        public string service { get; set; }
        public string app_dbid { get; set; }

        public string table_dbid { get; set; }

        public string realm { get; set; }

        public string apptoken { get; set; }

        public string usertoken { get; set; }

        public string query { get; set; }

        public string qid { get; set; } = "";
        public string file_attachemnt_fid { get; set; }

        public List<KeyValuePair<string, string>> GetDocumentLinks()
        {

            List<KeyValuePair<string, string>> QuickbaseDownloadLinks = new List<KeyValuePair<string, string>>();

            string authtoken = "QB-USER-TOKEN " + usertoken;
            string resultsData = "";
            var quickBaseValues = new List<KeyValuePair<string, string>>();
            string Uri = "";
            string json = "";


            if (qid != "")
            {
                Uri = "https://api.quickbase.com/v1/reports/" + qid + "/run?tableId=" + table_dbid;
                json = "";
            }
            else
            {
                Uri = "https://api.quickbase.com/v1/records/query";
                json = "{\"from\":\"" + table_dbid + "\"," +
                    "\"select\":[3," + file_attachemnt_fid + "]," +
                    "\"where\":\"" + query + "\"}";
            }
            WebRequest request = WebRequest.Create(Uri);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("QB-Realm-Hostname", realm);
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
                var thisDocName = item[file_attachemnt_fid]["value"]["versions"][0]["fileName"].ToString();
                string thisLink = $"https://{realm}/up/{table_dbid}/a/r{thisRid}/e{file_attachemnt_fid}/v0?usertoken={usertoken}";
                QuickbaseDownloadLinks.Add(new KeyValuePair<string, string>(thisDocName, thisLink));
            }

            return QuickbaseDownloadLinks;
        }
    }
}
