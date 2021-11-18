using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.DocumentDelivery
{
    public class DeliveryConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IDeliverySettings);
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

            string serviceTypeName = $"DocumentGenerator3.DocumentDelivery.DeliverySettings_{jsonObject["service"].ToString()}";
            string objectToInstantiate = $"{serviceTypeName}, DocumentGenerator3";

            var thisObjectType = Type.GetType(objectToInstantiate);
            var deliverySettings = Activator.CreateInstance(thisObjectType);

            serializer.Populate(jsonObject.CreateReader(), deliverySettings);
            return deliverySettings;
        }
    }
}