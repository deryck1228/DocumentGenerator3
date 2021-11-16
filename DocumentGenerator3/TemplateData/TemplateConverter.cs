using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var profession = default(ITemplateLocation);

            switch (jsonObject["JobTitle"].ToString())
            {
                case "Software Developer":
                    profession = new Programming();
                    break;
                case "Copywriter":
                    profession = new Writing();
                    break;
            }

            serializer.Populate(jsonObject.CreateReader(), profession);
            return profession;
        }
    }
}
