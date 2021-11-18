using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.PdfConversion
{
    public class CloudConvertJob
    {
        public List<ICloudConvertTask> Tasks { get; set; } = new List<ICloudConvertTask>();

        public string Serialize()
        {
            List<string> ListOfSerializedTasks = new List<string>();
            foreach (var task in Tasks)
            {
                ListOfSerializedTasks.Add(task.Serialize());
            }
            return "{\"tasks\":{" + String.Join(",", ListOfSerializedTasks) + "}}";
        }
    }
}
