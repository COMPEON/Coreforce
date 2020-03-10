using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Compos.Coreforce.Converter
{
    public class CoreforceNullableIntegerConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return 
                (objectType == typeof(int) ||
                objectType == typeof(decimal) ||
                objectType == typeof(int?) ||
                objectType == typeof(decimal?));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if(reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else if (reader.TokenType == JsonToken.Integer)
            {
                Nullable<int> convertedValue = Convert.ToInt32(reader.Value);
                return convertedValue;
            }
            else if (reader.TokenType == JsonToken.Float)
            {
                Nullable<int> convertedValue = Convert.ToInt32(reader.Value);
                return convertedValue;
            }

            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue((int?)value);
        }
    }
}
