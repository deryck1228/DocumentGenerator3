using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.DocumentDelivery.Email
{
    public class MimeHandler
    {
        private readonly List<KeyValuePair<string,string>> MimeValues = new List<KeyValuePair<string, string>>() 
        {
            new KeyValuePair<string, string>(".pdf","application/pdf"),
            new KeyValuePair<string, string>(".docx","application/vnd.openxmlformats-officedocument.wordprocessingml.document")
        };

        public string GetMimeValues(string documentType)
        {
            try
            {
                return MimeValues.FirstOrDefault(m => m.Key == documentType).Value;
            }
            catch
            {
                throw new Exception($"Mime type is not supported for '{documentType}'");
            }

        }

    }
}
