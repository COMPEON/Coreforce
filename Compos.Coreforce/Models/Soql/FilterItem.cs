using Compos.Coreforce.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Compos.Coreforce.Models.Soql
{
    public class FilterItem<T> : IFilterItem<T>
    {
        public Expression<Func<T, object>> RelatingObject { get; set; }
        public object CompareObject { get; set; }
        public FilterOperator FilterOperator { get; set; }
        public FilterConcatination? FilterConcatination { get; set; }

        public FilterItem(
            Expression<Func<T, object>> relatingObject,
            object compareObject,
            FilterOperator filterOperator,
            FilterConcatination filterConcatination
            )
        {
            RelatingObject = relatingObject;
            CompareObject = compareObject;
            FilterOperator = filterOperator;
            FilterConcatination = filterConcatination;
        }

        public FilterItem(
            Expression<Func<T, object>> relatingObject,
            object compareObject,
            FilterOperator filterOperator
            )
        {
            RelatingObject = relatingObject;
            CompareObject = compareObject;
            FilterOperator = filterOperator;
            FilterConcatination = null;
        }

        public string BuildStatement()
        {
            var expression = RelatingObject.GetMemberExpression();

            if (expression == null)
                throw new NullReferenceException($"Could not build WhereItem<{typeof(T).Name}>. MemberExpression is null.");

            string compareString = string.Empty;

            if (CompareObject == null || CompareObject.ToString().ToLower().Contains("null"))
                compareString = "null";

            else if (expression.Type == typeof(string))
                compareString = $"'{CompareObject}'";

            else if (expression.Type == typeof(DateTime?) || expression.Type == typeof(DateTime))
            {
                var dateTime = Convert.ToDateTime(CompareObject).ToUniversalTime();
                compareString = $"{dateTime.Year}-{dateTime.Month.ToString("D2")}-{dateTime.Day.ToString("D2")}T{dateTime.Hour.ToString("D2")}:{dateTime.Minute.ToString("D2")}:{dateTime.Second.ToString("D2")}Z";
            }

            else
                compareString = $"{CompareObject.ToString().ToLower()}";

            if (FilterConcatination.HasValue && FilterConcatination == Soql.FilterConcatination.And)
                return $"+{expression.Member.Name}+{GetFilterOperatorAsString()}+{compareString}+AND";
            if (FilterConcatination.HasValue && FilterConcatination == Soql.FilterConcatination.Or)
                return $"+{expression.Member.Name}+{GetFilterOperatorAsString()}+{compareString}+OR";

            return $"+{expression.Member.Name}+{GetFilterOperatorAsString()}+{compareString}";
        }

        public string GetFilterOperatorAsString()
        {
            var filterOperatorAsString = string.Empty;

            switch (FilterOperator)
            {
                case FilterOperator.Equals:
                    return "=";
                case FilterOperator.GreaterThan:
                    return ">";
                case FilterOperator.GreaterThanOrEquals:
                    return ">=";
                case FilterOperator.LessThan:
                    return "<";
                case FilterOperator.LessThanOrEquals:
                    return "<=";
                case FilterOperator.NotEquals:
                    return "!=";
            }

            throw new NotImplementedException("FilterOperator not defined.");
        }
    }
}
