using DocumentGenerator3.AdditionalDocumentsToBind;
using DocumentGenerator3.BulletedListData;
using DocumentGenerator3.ChildDatasetData;
using DocumentGenerator3.DocumentDelivery;
using DocumentGenerator3.ImageHandling;
using DocumentGenerator3.ParentDatasetData;
using DocumentGenerator3.TemplateData;
using DocumentGenerator3.InteractableObjectsData;
using System.Collections.Generic;
using DocumentGenerator3.LoggingData;
using System;

namespace DocumentGenerator3
{
    public class DocumentGeneratorPayload
    {
        /// <summary>
        /// The unique id of a specific execution of the document generator, defaulting to a GUID if none is provided externally
        /// </summary>
        public string execution_id { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// The name of the finished document
        /// </summary>
        public string document_name { get; set; }
        /// <summary>
        /// The extension of the finished document
        /// </summary>
        public string document_type { get; set; }
        /// <summary>
        /// The timezone in which any date time data is to be displayed for the completed document
        /// </summary>
        public string document_display_timezone { get; set; } = "Eastern Standard Time";
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
        public List<ChildDataset> child_datasets { get; set; } = new List<ChildDataset>();
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
        /// <summary>
        /// A list of objects specific to a particular template that need to be interacted with in some way, e.g. checking a checkbox
        /// </summary>
        public InteractableObjects interactable_objects { get; set; } = new();
        /// <summary>
        /// The method and location for delivering logging data
        /// </summary>
        public LoggingMethod logging_method { get; set; }

    }
}
