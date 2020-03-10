using Newtonsoft.Json;
using System;

namespace Compos.Coreforce.Converter
{
    public class DateConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (serializer.Deserialize(reader) == null)
                return null;

            return new Date(serializer.Deserialize(reader).ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                serializer.Serialize(writer, null);

            else
            {
                var date = (Date)value;
                serializer.Serialize(writer, date.GetDate());
            }
        }
    }
}
