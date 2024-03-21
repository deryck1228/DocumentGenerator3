using System.Collections.Generic;

namespace DocumentGenerator3
{
    public class CsvWithMetadata
    {
        public string Csv { get; set; }
        public List<string> GroupByData { get; set; }
        public int CountOfColumns { get; set; }
    }
}