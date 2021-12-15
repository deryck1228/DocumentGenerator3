using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.BulletedListData
{
    public class BulletedListLocation
    {
        [JsonConverter(typeof(BulletedListSettingsConverter))]
        /// <summary>
        /// The settings for accessing the parent dataset
        /// </summary>
        public IBulletedListSettings settings { get; set; }
    }
}
