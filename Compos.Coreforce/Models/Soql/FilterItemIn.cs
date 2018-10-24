using Compos.Coreforce.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Compos.Coreforce.Models.Soql
{
    public class FilterItemIn<T> : IFilterItem<T>
    {
        public Expression<Func<T, object>> RelatingObject { get; set; }
        public List<object> FilterCollection { get; set; }
        public FilterConcatination? FilterConcatination { get; set; }

        public FilterItemIn(
            Expression<Func<T, object>> relatingObject,
            List<object> filterCollection
            )
        {
            RelatingObject = relatingObject;
            FilterCollection = filterCollection;
            FilterConcatination = null;
        }

        public FilterItemIn(
            Expression<Func<T, object>> relatingObject,
            List<object> filterCollection,
            FilterConcatination filterConcatination
            )
        {
            RelatingObject = relatingObject;
            FilterCollection = filterCollection;
            FilterConcatination = filterConcatination;
        }

        public string BuildStatement()
        {
            var expression = RelatingObject.GetMemberExpression();

            if (expression == null)
                throw new NullReferenceException($"Could not build WhereItemIn<{typeof(T).Name}>. MemberExpression is null.");

            var whereClause = string.Empty;

            foreach (var filterObject in FilterCollection)
            {
                if (filterObject == null)
                    whereClause += $"+null+,";

                else if (filterObject.GetType() == typeof(DateTime?) || filterObject.GetType() == typeof(DateTime))
                {
                    var dateTime = Convert.ToDateTime(filterObject).ToUniversalTime();
                    whereClause = $"+{dateTime.Year}-{dateTime.Month.ToString("D2")}-{dateTime.Day.ToString("D2")}T{dateTime.Hour.ToString("D2")}:{dateTime.Minute.ToString("D2")}:{dateTime.Second.ToString("D2")}Z+,";
                }

                else if (filterObject.GetType() == typeof(string))
                    whereClause += $"+'{filterObject}'+,";

                else
                    whereClause += $"+{filterObject}+,";
            }

            whereClause = "(" + whereClause.Substring(0, whereClause.Length - 1) + ")";

            if (FilterConcatination.HasValue && FilterConcatination == Soql.FilterConcatination.And)
                return $"+{expression.Member.Name}+IN+{whereClause}+AND";

            if (FilterConcatination.HasValue && FilterConcatination == Soql.FilterConcatination.Or)
                return $"+{expression.Member.Name}+IN+{whereClause}+OR";

            return $"+{expression.Member.Name}+IN+{whereClause}";
        }
    }
}
