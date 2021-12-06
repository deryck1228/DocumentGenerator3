using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3
{

    public class BulletedList
    {
        public string type { get; set; }
        public List<Line> Lines { get; set; } = new List<Line>();
    }

    public class Line
    {
        public string symbol { get; set; }
        public string text { get; set; }
        public List<Line> Lines { get; set; } = new List<Line>();
    }

    public class BulletedListContainer
    {
        public string ListId { get; set; }
        public BulletedList BulletedList { get; set; } = new BulletedList();
    }
}
