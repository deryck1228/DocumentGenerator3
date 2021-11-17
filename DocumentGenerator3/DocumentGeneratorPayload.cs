using DocumentGenerator3.ChildDatasetData;
using DocumentGenerator3.ParentDatasetData;
using DocumentGenerator3.TemplateData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3
{
    public class DocumentGeneratorPayload
    {
        /// <summary>
        /// The name of the finished document
        /// </summary>
        public string document_name { get; set; }
        /// <summary>
        /// The extension of the finished document
        /// </summary>
        public string document_type { get; set; }
        /// <summary>
        /// The location where the template document is stored
        /// </summary>
        public TemplateLocation template_location { get; set; }
        /// <summary>
        /// The location and connection information for the parent dataset
        /// </summary>
        public ParentDataset parent_dataset { get; set; }
        /// <summary>
        /// A list of locations and connection informatin for any child datasets
        /// </summary>
        public List<ChildDataset> child_datasets { get; set; }
        /// <summary>
        /// The method and location for delivering the completed document
        /// </summary>
        public DeliveryMethod delivery_method { get; set; }
        /// <summary>
        /// A list of additional documents to be bound to the completed document as a pdf binder
        /// </summary>
        public List<AdditionalDocument> additional_documents { get; set; } = new List<AdditionalDocument>();
    }


    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AdditionalFiles
    {
        public string service { get; set; }
        public AdditionalFiles_Quickbase quickbase_settings { get; set; }

    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AdditionalFiles_Quickbase
    {
        public string app_dbid { get; set; }

        public string table_dbid { get; set; }

        public string realm { get; set; }

        public string apptoken { get; set; }

        public string usertoken { get; set; }

        public string query { get; set; }

        public string attachment_fid { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class DeliveryMethod
    {


        public string service { get; set; }
        public DeliverySettings_Email email_settings { get; set; }
        public DeliverSettings_Quickbase quickbase_settings { get; set; }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (service.ToLower() == "quickbase" && quickbase_settings is null)
            {
                throw new InvalidOperationException("quickbase_settings must be supplied if service is set to 'quickbase'");
            }
            if (service.ToLower() == "email" && email_settings is null)
            {
                throw new InvalidOperationException("email_settings must be supplied if service is set to 'email'");
            }
        }

    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class DeliverySettings_Email
    {

        public string from_name { get; set; }


        public string to_name { get; set; }


        public string to_email { get; set; }


        public string subject_line { get; set; }


        public string body_text { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class DeliverSettings_Quickbase
    {

        public string app_dbid { get; set; }


        public string table_dbid { get; set; }


        public string realm { get; set; }


        public string apptoken { get; set; }


        public string usertoken { get; set; }


        public string rid { get; set; }


        public string document_field_id { get; set; }

        public string document_field_data { get; set; } = "";
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AdditionalDocument
    {
        public string service { get; set; }
        public AdditionalDocument_DirectLink direct_link { get; set; }
        public AdditionalDocument_Quickbase quickbase_link { get; set; }

    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AdditionalDocument_DirectLink
    {
        public string link { get; set; }
        public string doc_name { get; set; } = "self.docx";
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AdditionalDocument_Quickbase
    {
        public string app_dbid { get; set; }

        public string table_dbid { get; set; }

        public string realm { get; set; }

        public string apptoken { get; set; }

        public string usertoken { get; set; }

        public string query { get; set; }

        public string qid { get; set; } = "";
        public string file_attachemnt_fid { get; set; }
    }
}
