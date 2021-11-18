using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.ChildDatasetData
{
    public class GetChildDatasetDataService_quickbase
    {
        public QBTableMetadata Metadata { get; set; }
        public QBTableMetadata GetChildData()
        {
            string authtoken = "QB-USER-TOKEN " + Metadata.childDataset.usertoken;
            string resultsData = "";
            var quickBaseValues = new List<KeyValuePair<string, string>>();
            string Uri = "";
            string json = "";


            if (Metadata.childDataset.query == "")
            {
                Uri = "https://api.quickbase.com/v1/reports/" + Metadata.childDataset.id + "/run?tableId=" + Metadata.childDataset.table_dbid;
                json = "";
            }
            else
            {
                Uri = "https://api.quickbase.com/v1/records/query";
                json = "{\"from\":\"" + Metadata.childDataset.table_dbid + "\"," +
                    "\"select\":[" + Metadata.childDataset.field_order + "]," +
                    "\"where\":\"" + Metadata.childDataset.query + "\"," +
                    "\"options\":{\"skip\":" + Metadata.skip + "}}";
            }
            WebRequest request = WebRequest.Create(Uri);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("QB-Realm-Hostname", Metadata.childDataset.realm);
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
            var items = jsonResponse.GetValue("data");
            var x = (JArray)jsonResponse["data"];
            string csv = "";

            var columnHeaders = Metadata.childDataset.column_headers.Split(',').ToList();
            var fieldOrder = Metadata.childDataset.field_order.Split(',').Select(Int32.Parse).ToList();

            if (items != null && items.Count() > 0)
            {
                List<string> csvRows = new List<string>();
                var timesThrough = 1;
                if (Metadata.skip != 0)
                {
                    timesThrough++;
                }
                string itemsStr = items.ToString();
                var jsonToken = JArray.Parse(itemsStr);
                int iterator = 0;

                foreach (var itemsToken in jsonToken)
                {
                    List<string> eachHeader = new List<string>();
                    List<string> eachColumn = new List<string>();

                    foreach (var i in fieldOrder)
                    {
                        var specificItem = itemsToken[i.ToString()];

                        if (timesThrough == 1)
                        {
                            string columnName = columnHeaders[iterator];
                            eachHeader.Add(columnName);
                            iterator++;

                        }
                        string columnValue = itemsToken[i.ToString()]["value"].ToString();
                        columnValue = columnValue.Replace(",", " ");
                        columnValue = columnValue.Replace("\n", " ").Replace("\r", " "); //replace newline character in value with space to avoid being parsed out during csv conversion
                        eachColumn.Add(columnValue);

                    }
                    if (timesThrough == 1)
                    {
                        string Headers = String.Join(",", eachHeader.ToArray());

                        csvRows.Add(Headers);
                    }

                    timesThrough++;
                    string eachRow = String.Join(",", eachColumn.ToArray());
                    csvRows.Add(eachRow);
                }
                csv = string.Join("\n", csvRows.ToArray());
            }
            else
            {
                List<string> csvRows = new List<string>();
                var timesThrough = 1;
                string itemsStr = items.ToString();
                var jsonToken = JArray.Parse(itemsStr);
                int iterator = 0;

                List<string> eachHeader = new List<string>();
                foreach (var i in fieldOrder)
                {
                    if (timesThrough == 1)
                    {
                        string columnName = columnHeaders[iterator];
                        eachHeader.Add(columnName);
                        iterator++;
                    }
                }
                if (timesThrough == 1)
                {
                    string Headers = String.Join(",", eachHeader.ToArray());

                    csvRows.Add(Headers);
                }

                timesThrough++;
                csv = string.Join("\n", csvRows.ToArray());
            }

            var metadata = jsonResponse.GetValue("metadata");
            Metadata.recordCount = Convert.ToInt32(metadata["totalRecords"].ToString());
            Metadata.skip = Metadata.skip + Metadata.chunkSize;
            if (Metadata.chunkSize == 0)
            {
                Metadata.chunkSize = Convert.ToInt32(metadata["numRecords"].ToString());
            }

            Metadata.thisCSV = csv;

            return Metadata;
        }
    }
}
