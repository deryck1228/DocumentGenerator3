using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.ChildDatasetData
{
    public class QBTableMetadata
    {
        /// <summary>
        /// The original child dataset for this table
        /// </summary>
        public ChildSettings_quickbase childDataset { get; set; }
        /// <summary>
        /// The number of records to be used for chunking incoming data
        /// </summary>
        public int chunkSize { get; set; } = 0;
        /// <summary>
        /// The skip value of the current iteration of fetching data from Quickbase
        /// </summary>
        public int skip { get; set; } = 0;
        /// <summary>
        /// The total record count of data to be returned from Quickbase
        /// </summary>
        public int recordCount { get; set; } = 0;
        /// <summary>
        /// The table data as a csv string
        /// </summary>
        public string thisCSV { get; set; }
        /// <summary>
        /// The list of the records group by field
        /// </summary>
        public List<string> thisGroupByList { get; set; } = new();
        /// <summary>
        /// The number of columns in the csv string
        /// </summary>
        public int countOfColumns { get;set; } = 0;
    }
}
