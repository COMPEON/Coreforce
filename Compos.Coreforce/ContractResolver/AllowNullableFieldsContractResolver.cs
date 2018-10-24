using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Compos.Coreforce.Extensions;
using System.Linq;

namespace Compos.Coreforce.ContractResolver
{
    public class AllowNullableFieldsContractResolver<T> : DefaultContractResolver
    {
        private readonly Expression<Func<T, object>>[] _fieldsToUpdate;

        public AllowNullableFieldsContractResolver(params Expression<Func<T, object>>[] fieldsToUpdate)
        {
            _fieldsToUpdate = fieldsToUpdate;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            List<MemberExpression> memberExpressions = new List<MemberExpression>();
            IList<JsonProperty> retval = base.CreateProperties(type, memberSerialization);

            if (_fieldsToUpdate != null && _fieldsToUpdate.Length > 0)
            {
                foreach (var expression in _fieldsToUpdate)
                {
                    memberExpressions.Add(expression.GetMemberExpression());
                }
            }

            foreach (JsonProperty property in retval)
            {
                if (memberExpressions.Any(x => x?.Member?.Name == property.PropertyName) || memberExpressions.Count == 0)
                {
                    property.NullValueHandling = NullValueHandling.Include;
                }
                else
                {
                    property.NullValueHandling = NullValueHandling.Ignore;
                }
            }

            return retval;
        }
    }
}
