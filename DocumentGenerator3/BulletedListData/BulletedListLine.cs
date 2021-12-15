using System.Collections.Generic;

namespace DocumentGenerator3.BulletedListData
{
    public class BulletedListLine
    {
        public string symbol { get; set; }
        public string text { get; set; }
        public List<BulletedListLine> Lines { get; set; } = new List<BulletedListLine>();
    }
}
