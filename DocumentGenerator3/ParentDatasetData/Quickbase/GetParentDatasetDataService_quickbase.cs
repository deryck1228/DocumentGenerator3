using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.ParentDatasetData
{
    public class GetParentDatasetDataService_quickbase
    {
        public ParentSettings_quickbase Settings { get; set; }

        public List<KeyValuePair<string, string>> GetParentData()
        {
            string authtoken = "QB-USER-TOKEN " + Settings.usertoken;
            string resultsData = "";
            var quickBaseValues = new List<KeyValuePair<string, string>>();

            WebRequest request = WebRequest.Create("https://api.quickbase.com/v1/records/query");

            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("QB-Realm-Hostname", Settings.realm);
            request.Headers.Add("User-Agent", "Azure_Serverless_Functions");
            request.Headers.Add("Authorization", authtoken);

            string json = "{\"from\":\"" + Settings.table_dbid +
                "\",\"select\":[\"a\"]," +
                "\"where\":\"{" + Settings.merge_field_id + ".EX.'" + Settings.rid + "'}\"}";


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
            resultsData = resultsData.Replace(@"\r\n", "*|*"); //replace newline character in value with unusual character sequence to avoid being parsed out during csv conversion
            JObject jsonResponse = JObject.Parse(resultsData);
            var items = jsonResponse.GetValue("data");
            if (items != null)
            {
                foreach (var l in items.Children<JObject>())
                {
                    foreach (var f in l.Properties())
                    {
                        quickBaseValues.Add(new KeyValuePair<string, string>(f.Name.ToString(), f.Value["value"].ToString()));
                        //Find 
                    }
                }
            }

            return quickBaseValues;
        }

        private void TransformStringsToBulletedLists(string jsonToTransform)
        {

        }
    }
}
