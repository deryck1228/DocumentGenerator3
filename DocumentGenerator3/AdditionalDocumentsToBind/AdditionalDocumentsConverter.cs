using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.AdditionalDocumentsToBind
{
    public class AdditionalDocumentsConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IAdditionalDocument);
        }

        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            //var template = default(ITemplateLocation);

            string serviceTypeName = $"DocumentGenerator3.AdditionalDocumentsToBind.AdditionalDocument_{jsonObject["service"].ToString()}";
            string objectToInstantiate = $"{serviceTypeName}, DocumentGenerator3";

            var thisObjectType = Type.GetType(objectToInstantiate);
            var additionalDocument = Activator.CreateInstance(thisObjectType);

            serializer.Populate(jsonObject.CreateReader(), additionalDocument);
            return additionalDocument;
        }
    }
}
