using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.ChildDatasetData
{
    public class GetChildDatasetDataService_quickbase
    {
        public QBTableMetadata Metadata { get; set; }

        public async Task<QBTableMetadata> GetChildDataAsync()
        {
            string authtoken = "QB-USER-TOKEN " + Metadata.childDataset.usertoken;
            string resultsData = "";
            var quickBaseValues = new List<KeyValuePair<string, string>>();
            string Uri = "";
            string json = "";

            string sortClause = "";
            string groupByClause = "";

            if (Metadata.childDataset.sortOrder != "")
            {
                sortClause = $"\"sortBy\":[{Metadata.childDataset.sortOrder}],";
                sortClause = sortClause.Replace("'", "\"");
            }

            if (Metadata.childDataset.groupBy != "")
            {
                groupByClause = $"\"groupBy\":[{Metadata.childDataset.groupBy}],";
                groupByClause = groupByClause.Replace("'", "\"");
                GetGroupByData();
            }

            if (Metadata.childDataset.query == "")
            {
                Uri = "https://api.quickbase.com/v1/reports/" + Metadata.childDataset.id + "/run?tableId=" + Metadata.childDataset.table_dbid;
                Uri = $"https://api.quickbase.com/v1/reports/{Metadata.childDataset.id}/run?tableId={Metadata.childDataset.table_dbid}";
                json = "";
            }
            else
            {
                Uri = "https://api.quickbase.com/v1/records/query";
                json = "{\"from\":\"" + Metadata.childDataset.table_dbid + "\"," +
                    "\"select\":[" + Metadata.childDataset.field_order + "]," +
                    "\"where\":\"" + Metadata.childDataset.query + "\"," +
                    sortClause +
                    groupByClause +
                    "\"options\":{\"skip\":" + Metadata.skip + "}}";
            }


            var maxRetryAttempts = 5;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("QB-Realm-Hostname", Metadata.childDataset.realm);
            client.DefaultRequestHeaders.Add("User-Agent", "Azure_Serverless_Functions");
            client.DefaultRequestHeaders.Add("Authorization", authtoken);

            try
            {
                await RetryHelper.RetryOnExceptionAsync<HttpRequestException>
                    (maxRetryAttempts, async () =>
                    {
                        var data = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await client.PostAsync(Uri, data);
                        resultsData = await response.Content.ReadAsStringAsync();
                        response.EnsureSuccessStatusCode();
                    });
            }
            catch (Exception ex)
            {
                //All retries here are failed.
                Console.WriteLine("Exception: " + ex.Message);
            }

            JObject jsonResponse = JObject.Parse(resultsData);
            var items = jsonResponse.GetValue("data");

            //List<JToken> filteredItemList = new();
            JArray filteredItemJArray = new();

            if (Metadata.childDataset.summary_report_filter_fid != "")
            {
                foreach (var item in items)
                {
                    if (Metadata.childDataset.id == "25")
                    {
                        Console.WriteLine("");
                    }

                    if (item[Metadata.childDataset.summary_report_filter_fid]["value"].ToString() == Metadata.childDataset.summary_report_filter_value)
                    {
                        filteredItemJArray.Add(item);
                    }

                }
            }
            var filteredItems = jsonResponse.GetValue("data").Where(i => i[Metadata.childDataset.summary_report_filter_fid]["value"].ToString() == Metadata.childDataset.summary_report_filter_value);
            var x = (JArray)jsonResponse["data"];
            string csv = "";

            var columnHeaders = Metadata.childDataset.column_headers.Split(',').ToList();
            Metadata.countOfColumns = columnHeaders.Count;
            var fieldOrder = Metadata.childDataset.field_order.Split(',').Select(Int32.Parse).ToList();

            //if (columnHeaders.Count != 1 && columnHeaders[0] != "")
            //{
            if (Metadata.childDataset.summary_report_filter_fid == "")
            {
                csv = ConvertToCsvString(items, columnHeaders, fieldOrder); 
            }
            else
            {
                csv = ConvertToCsvString(filteredItemJArray, columnHeaders, fieldOrder);
            }
            //}

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

        public QBTableMetadata GetChildData()
        {
            string authtoken = "QB-USER-TOKEN " + Metadata.childDataset.usertoken;
            string resultsData = "";
            var quickBaseValues = new List<KeyValuePair<string, string>>();
            string Uri = "";
            string json = "";

            string sortClause = "";
            string groupByClause = "";

            if (Metadata.childDataset.sortOrder != "")
            {
                sortClause = $"\"sortBy\":[{Metadata.childDataset.sortOrder}],";
                sortClause = sortClause.Replace("'", "\"");
            }

            if(Metadata.childDataset.groupBy != "")
            {
                groupByClause = $"\"groupBy\":[{Metadata.childDataset.groupBy}],";
                groupByClause = groupByClause.Replace("'", "\"");
                GetGroupByData();
            }

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
                    sortClause +
                    groupByClause + 
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
            var filteredItems = jsonResponse.GetValue("data").Where(i => i[Metadata.childDataset.summary_report_filter_fid]["value"].ToString() == Metadata.childDataset.summary_report_filter_value);
            var x = (JArray)jsonResponse["data"];
            string csv = "";

            if (Metadata.childDataset.summary_report_filter_fid != "")
            {
                

                foreach (var item in items)
                {

                }
            }

            var columnHeaders = Metadata.childDataset.column_headers.Split(',').ToList();
            Metadata.countOfColumns = columnHeaders.Count;
            var fieldOrder = Metadata.childDataset.field_order.Split(',').Select(Int32.Parse).ToList();

            if (columnHeaders.Count != 1 && columnHeaders[0] != "")
            {
                csv = ConvertToCsvString(items, columnHeaders, fieldOrder); 
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

        private void GetGroupByData()
        {
            string authtoken = "QB-USER-TOKEN " + Metadata.childDataset.usertoken;
            string resultsData = "";
            var quickBaseValues = new List<KeyValuePair<string, string>>();
            string Uri = "";
            string json = "";

            string sortClause = "";
            string groupByClause = "";

            if (Metadata.childDataset.sortOrder != "")
            {
                sortClause = $"\"sortBy\":[{Metadata.childDataset.sortOrder}],";
                sortClause = sortClause.Replace("'", "\"");
            }

            if (Metadata.childDataset.groupBy != "")
            {
                groupByClause = $"\"groupBy\":[{Metadata.childDataset.groupBy}],";
                groupByClause = groupByClause.Replace("'", "\"");
            }

            var groupBy = JObject.Parse(Metadata.childDataset.groupBy);
            string groupByFid = groupBy["fieldId"].ToString();

            if (Metadata.childDataset.query == "")
            {
                Uri = "https://api.quickbase.com/v1/reports/" + Metadata.childDataset.id + "/run?tableId=" + Metadata.childDataset.table_dbid;
                json = "";
            }
            else
            {
                Uri = "https://api.quickbase.com/v1/records/query";
                json = "{\"from\":\"" + Metadata.childDataset.table_dbid + "\"," +
                    "\"select\":[" + groupByFid  + "]," +
                    "\"where\":\"" + Metadata.childDataset.query + "\"," +
                    sortClause +
                    groupByClause +
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

            foreach(JObject item in items)
            {
                string groupByValue = item[groupByFid]["value"].ToString();
                Metadata.thisGroupByList.Add(groupByValue);
            }
        }

        private string ConvertToCsvString(JToken items, List<string> columnHeaders, List<int> fieldOrder)
        {
            string csv;
            if (items != null) //&& items.Count() > 0
            {
                List<string> csvRows = new List<string>();
                var timesThrough = 1;
                if (Metadata.skip != 0 || columnHeaders[0] == "")
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
                        columnValue = columnValue.Replace(",", "*%*"); //Replace commas with bizarre characters, for later handling when adding data to table
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
                    if (timesThrough == 1 && columnHeaders[0] != "")
                    {
                        string columnName = columnHeaders[iterator];
                        eachHeader.Add(columnName);
                        iterator++;
                    }
                }
                if (timesThrough == 1 && columnHeaders[0] != "")
                {
                    string Headers = String.Join(",", eachHeader.ToArray());

                    csvRows.Add(Headers);
                }

                timesThrough++;
                csv = string.Join("\n", csvRows.ToArray());
            }

            return csv;
        }

        private string ConvertToCsvString(JArray items, List<string> columnHeaders, List<int> fieldOrder)
        {
            string csv;
            if (items != null) //&& items.Count() > 0
            {
                List<string> csvRows = new List<string>();
                var timesThrough = 1;
                if (Metadata.skip != 0 || columnHeaders[0] == "")
                {
                    timesThrough++;
                }

                //string itemsStr = items.ToString();
                //var jsonToken = JArray.Parse(itemsStr);
                int iterator = 0;

                foreach (var itemsToken in items)
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
                        columnValue = columnValue.Replace(",", "*%*"); //Replace commas with bizarre characters, for later handling when adding data to table
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
                    if (timesThrough == 1 && columnHeaders[0] != "")
                    {
                        string columnName = columnHeaders[iterator];
                        eachHeader.Add(columnName);
                        iterator++;
                    }
                }
                if (timesThrough == 1 && columnHeaders[0] != "")
                {
                    string Headers = String.Join(",", eachHeader.ToArray());

                    csvRows.Add(Headers);
                }

                timesThrough++;
                csv = string.Join("\n", csvRows.ToArray());
            }

            return csv;
        }
    }
}
