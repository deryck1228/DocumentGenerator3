using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.BulletedListData
{
    public class BulletedListSettings_quickbase : IBulletedListSettings
    {
        /// <summary>
        /// The name of the specific service being invoked for this bulleted list configuration
        /// </summary>
        public string service { get; set; }
        /// <summary>
        /// The main DBID of the Quickbase app in which the bulleted list configuration is stored
        /// </summary>
        public string app_dbid { get; set; }
        /// <summary>
        /// The DBID of the Quickbase table in which the bulleted list configuration is stored
        /// </summary>
        public string table_dbid { get; set; }
        /// <summary>
        /// The Quickbase realm in which the bulleted list configuration is stored
        /// </summary>
        public string realm { get; set; }
        /// <summary>
        /// The apptoken used to access data for the bulleted list configuration
        /// </summary>
        public string apptoken { get; set; }
        /// <summary>
        /// The usertoken used to access the data for the bulleted list configuration
        /// </summary>
        public string usertoken { get; set; }
        /// <summary>
        /// The id value of the record ID# for the bulleted list configuration record in Quickbase
        /// </summary>
        public string rid { get; set; }
        /// <summary>
        /// The field id of the field in Quickbase in which the bulleted list configuration resides
        /// </summary>
        public string fid { get; set; }


        public KeyValuePair<string, BulletedListConfiguration> GetBulletedListConfiguration()
        {
            
            string authtoken = "QB-USER-TOKEN " + usertoken;
            string resultsData = "";

            string Uri = "https://api.quickbase.com/v1/records/query";
            string json = "{\"from\":\"" + table_dbid + "\"," +
                "\"select\":[3," + fid + "]," +
                "\"where\":\"{'3'.EX." + rid + "}\"}";

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

            foreach(var item in items)
            {

                var thisKey = "{{" + fid + "}}";
                var thisJsonStr = item[fid]["value"].ToString();
                BulletedListConfiguration deserializedConfig = JsonConvert.DeserializeObject<BulletedListConfiguration>(thisJsonStr);

                KeyValuePair<string, BulletedListConfiguration> configuration = new(thisKey, deserializedConfig);
                return configuration;

            }

            throw new Exception($"Quickbase did not contain a bulleted list configuration in field {fid} for Record ID {rid}");
        }
    }
}
