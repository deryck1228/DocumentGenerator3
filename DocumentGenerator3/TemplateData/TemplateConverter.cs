using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace DocumentGenerator3.TemplateData
{
    public class TemplateConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ITemplateLocation);
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

            string serviceTypeName = $"DocumentGenerator3.TemplateData.TemplateSettings_{jsonObject["service"].ToString()}";
            string objectToInstantiate = $"{serviceTypeName}, DocumentGenerator3";

            var thisObjectType = Type.GetType(objectToInstantiate);
            var template= Activator.CreateInstance(thisObjectType);

            serializer.Populate(jsonObject.CreateReader(), template);
            return template;
        }
    }
}
