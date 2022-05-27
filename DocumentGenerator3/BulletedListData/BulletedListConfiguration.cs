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
        /// <summary>
        /// The font to be applied to the bulleted list
        /// </summary>
        public string font_family { get; set; } = "Times New Roman";
        /// <summary>
        /// The font size to be applied to the bulleted list
        /// </summary>
        public string font_size { get; set; } = "10";
        public List<BulletedListLine> Lines { get; set; } = new List<BulletedListLine>();
    }
}
