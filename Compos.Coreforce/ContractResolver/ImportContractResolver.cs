using Compos.Coreforce.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Compos.Coreforce.ContractResolver
{
    internal class ImportContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> retval = base.CreateProperties(type, memberSerialization);

            foreach (var property in retval)
            {
                property.NullValueHandling = NullValueHandling.Ignore;
            }

            retval = retval.Where(p => (p.AttributeProvider.GetAttributes(typeof(Readonly), true).Count == 0 &&
                                       !p.Ignored) || p.PropertyName == "attributes").ToList();

            return retval;
        }
    }
}
