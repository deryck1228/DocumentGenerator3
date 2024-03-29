﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace DocumentGenerator3.ParentDatasetData
{
    public class ParentDatasetConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IParentSettings);
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

            string serviceTypeName = $"DocumentGenerator3.ParentDatasetData.ParentSettings_{jsonObject["service"].ToString()}";
            string objectToInstantiate = $"{serviceTypeName}, DocumentGenerator3";

            var thisObjectType = Type.GetType(objectToInstantiate);
            var parentDataset = Activator.CreateInstance(thisObjectType);

            serializer.Populate(jsonObject.CreateReader(), parentDataset);
            return parentDataset;
        }
    }
}
