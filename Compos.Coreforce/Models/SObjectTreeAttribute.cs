using Newtonsoft.Json;
using System;

namespace Compos.Coreforce.Models
{
    public class SObjectTreeAttribute
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("referenceId")]
        public string ReferenceId { get; set; }

        public SObjectTreeAttribute()
        {
            Type = string.Empty;
            ReferenceId = string.Empty;
        }

        public SObjectTreeAttribute(Type type)
        {
            Type = type.Name;
            ReferenceId = Guid.NewGuid().ToString();
        }

        public SObjectTreeAttribute(Type type, string referenceId)
        {
            Type = type.Name;
            ReferenceId = referenceId;
        }

        public SObjectTreeAttribute(string type, string referenceId)
        {
            Type = type;
            ReferenceId = referenceId;
        }
    }
}
