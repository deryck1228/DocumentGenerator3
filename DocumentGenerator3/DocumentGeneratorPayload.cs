using DocumentGenerator3.AdditionalDocumentsToBind;
using DocumentGenerator3.BulletedListData;
using DocumentGenerator3.ChildDatasetData;
using DocumentGenerator3.DocumentDelivery;
using DocumentGenerator3.ImageHandling;
using DocumentGenerator3.ParentDatasetData;
using DocumentGenerator3.TemplateData;
using System.Collections.Generic;
using System.Linq;
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
        /// <summary>
        /// A list of image locations from which images can be downloaded and inserted into the document
        /// </summary>
        public List<ImageLocation> image_locations { get; set; } = new List<ImageLocation>();
        /// <summary>
        /// A list of bulleted list locations from which bulleted lists can be constructed and inserted into the document
        /// </summary>
        public List<BulletedListLocation> bulleted_lists { get; set; } = new();
    }
}
