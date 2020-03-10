using Compos.Coreforce.Attributes;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System;

namespace Compos.Coreforce.ContractResolver
{
    internal class CoreforceContractResolver : DefaultContractResolver
    {
        private readonly List<string> _fields = new List<string>();

        public CoreforceContractResolver(List<string> fields)
        {
            foreach (var field in fields)
                _fields.Add(field.ToLower());
        }

        public CoreforceContractResolver()
        { }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> retval = base.CreateProperties(type, memberSerialization);

            foreach (var property in retval)
            {
                if(_fields.Contains(property.PropertyName.ToLower()))
                    property.NullValueHandling = NullValueHandling.Include;

                else
                    property.NullValueHandling = NullValueHandling.Ignore;
            }

            retval = retval.Where(p => (p.AttributeProvider.GetAttributes(typeof(Readonly), true).Count == 0 &&
                                       !p.Ignored)).ToList();

            return retval;
        }
    }
}
