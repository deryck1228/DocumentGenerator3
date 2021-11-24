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
        /// <summary>
        /// The name of the specific service being invoked for this additional document
        /// </summary>
        public string service { get; set; }
        /// <summary>
        /// The main DBID of the Quickbase app in which the additional document is stored
        /// </summary>
        public string app_dbid { get; set; }
        /// <summary>
        /// The DBID of the Quickbase table in which the additional document is stored
        /// </summary>
        public string table_dbid { get; set; }
        /// <summary>
        /// The Quickbase realm in which the additional document is stored
        /// </summary>
        public string realm { get; set; }
        /// <summary>
        /// The apptoken used to access data for the additional document
        /// </summary>
        public string apptoken { get; set; }
        /// <summary>
        /// The usertoken used to access teh data for the additional document
        /// </summary>
        public string usertoken { get; set; }
        /// <summary>
        /// The Quickbase query string used to select the additional document
        /// </summary>
        public string query { get; set; }
        /// <summary>
        /// The field id of the field in Quickbase in which the additional document resides
        /// </summary>
        public string file_attachemnt_fid { get; set; }

        public List<KeyValuePair<string, string>> GetDocumentLinks()
        {

            List<KeyValuePair<string, string>> QuickbaseDownloadLinks = new List<KeyValuePair<string, string>>();

            string authtoken = "QB-USER-TOKEN " + usertoken;
            string resultsData = "";
            var quickBaseValues = new List<KeyValuePair<string, string>>();

            string Uri = "https://api.quickbase.com/v1/records/query";
            string json = "{\"from\":\"" + table_dbid + "\"," +
                "\"select\":[3," + file_attachemnt_fid + "]," +
                "\"where\":\"" + query + "\"}";

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
