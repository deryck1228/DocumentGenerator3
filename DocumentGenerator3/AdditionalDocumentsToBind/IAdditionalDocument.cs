using System.Collections.Generic;

namespace DocumentGenerator3.AdditionalDocumentsToBind
{
    public interface IAdditionalDocument
    {
        string service { get; set; }
        List<KeyValuePair<string, string>> GetDocumentLinks();
    }
}