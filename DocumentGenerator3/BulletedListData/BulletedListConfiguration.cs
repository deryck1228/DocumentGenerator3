using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.BulletedListData
{

    public class BulletedListConfiguration
    {
        public string type { get; set; }
        public List<BulletedListLine> Lines { get; set; } = new List<BulletedListLine>();
    }
}
