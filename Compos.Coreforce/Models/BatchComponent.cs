using Compos.Coreforce.Attributes;
using Compos.Coreforce.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Compos.Coreforce.Models.Configuration;

namespace Compos.Coreforce.Models
{
    public class BatchComponent<T>
    {
        [JsonProperty("method")]
        public string Method { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("richInput")]
        public Dictionary<string, object> RichInput { get; set; }
        [JsonIgnore]
        public BatchMethod BatchMethod { get; set; }

        public bool ShouldSerializerichInput()
        {
            if (BatchMethod == BatchMethod.PATCH)
                return true;

            return false;
        }

        public BatchComponent(T obj, BatchMethod batchMethod, Expression<Func<T, object>>[] fieldsToUpdate)
        {
            RichInput = new Dictionary<string, object>();
            Method = batchMethod.ToString();

            var id = typeof(T).GetProperties().SingleOrDefault(x => x.Name.ToLower().Equals("id"));

            if (id is null)
                throw new NullReferenceException("Could not find an id property.");

            Url = $"{CoreforceConfiguration.ApiVersion}/sobjects/{typeof(T).Name}/{id.GetValue(obj)}";
            List<string> fieldsToUpdateFieldNames = new List<string>();

            foreach (var field in fieldsToUpdate)
            {
                var memberExpression = field.GetMemberExpression();

                if (memberExpression?.Member?.Name != "")
                    fieldsToUpdateFieldNames.Add(memberExpression?.Member?.Name);
            }

            if (batchMethod == BatchMethod.PATCH)
            {
                foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties()
                    .Where(x =>
                        !x.CustomAttributes.Any(y =>
                            y.AttributeType == typeof(Readonly))).ToList())
                {
                    var value = propertyInfo.GetValue(obj);

                    if ((value == null || (propertyInfo.GetType().Name == typeof(string).Name && string.IsNullOrEmpty((string)value))) ||
                        (fieldsToUpdate != null && !fieldsToUpdateFieldNames.Contains(propertyInfo.Name) && fieldsToUpdate.Length > 0))
                        continue;

                    else
                        RichInput.Add(propertyInfo.Name, ChangeType(value, propertyInfo.PropertyType));
                }
            }
        }

        private object ChangeType(object value, Type conversionType)
        {
            var type = conversionType;

            if (
                type.IsGenericType && 
                type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))
                )
            {
                if (value == null)
                    return null;

                type = Nullable.GetUnderlyingType(type);
            }

            return Convert.ChangeType(value, type);
        }
    }
}
