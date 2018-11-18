using Compos.Coreforce.Extensions;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;

namespace Compos.Coreforce.Filter
{
    public class FilterIn<T> : IFilter
    {
        private readonly Expression<Func<T, object>> field;
        private readonly IList<object> values;
        private readonly FilterConcatination? concatination;

        public FilterIn(Expression<Func<T, object>> field, List<object> values)
        {
            this.field = field;
            this.values = values;
            this.concatination = null;
        }

        public FilterIn(Expression<Func<T, object>> field, List<object> values, FilterConcatination concatination)
        {
            this.field = field;
            this.values = values;
            this.concatination = concatination;
        }

        public string Get()
        {
            var expression = field.GetMemberExpression();

            if (expression == null)
                throw new CoreforceException(CoreforceError.ProcessingError);

            var whereClause = string.Empty;

            foreach (var value in values)
            {
                if (value == null)
                    whereClause += $"+null+,";

                else if (value.GetType() == typeof(DateTime?) || value.GetType() == typeof(DateTime))
                {
                    var dateTime = Convert.ToDateTime(value).ToUniversalTime();
                    whereClause = $"+{dateTime.Year}-{dateTime.Month.ToString("D2")}-{dateTime.Day.ToString("D2")}T{dateTime.Hour.ToString("D2")}:{dateTime.Minute.ToString("D2")}:{dateTime.Second.ToString("D2")}Z+,";
                }

                else if (value.GetType() == typeof(string))
                    whereClause += $"+'{value}'+,";

                else
                    whereClause += $"+{value}+,";
            }

            whereClause = "(" + whereClause.Substring(0, whereClause.Length - 1) + ")";

            if (concatination.HasValue && concatination == FilterConcatination.And)
                return $"+{expression.Member.Name}+IN+{whereClause}+AND";

            if (concatination.HasValue && concatination == FilterConcatination.Or)
                return $"+{expression.Member.Name}+IN+{whereClause}+OR";

            return $"+{expression.Member.Name}+IN+{whereClause}";
        }
    }

    public class FilterIn : IFilter
    {
        private readonly string field;
        private readonly IList<object> values;
        private readonly FilterConcatination? concatination;

        public FilterIn(string field, List<object> values)
        {
            this.field = field;
            this.values = values;
            this.concatination = null;
        }

        public FilterIn(string field, List<object> values, FilterConcatination concatination)
        {
            this.field = field;
            this.values = values;
            this.concatination = concatination;
        }

        public string Get()
        {
            var whereClause = string.Empty;

            foreach (var value in values)
            {
                if (value == null)
                    whereClause += $"+null+,";

                else if (value.GetType() == typeof(DateTime?) || value.GetType() == typeof(DateTime))
                {
                    var dateTime = Convert.ToDateTime(value).ToUniversalTime();
                    whereClause = $"+{dateTime.Year}-{dateTime.Month.ToString("D2")}-{dateTime.Day.ToString("D2")}T{dateTime.Hour.ToString("D2")}:{dateTime.Minute.ToString("D2")}:{dateTime.Second.ToString("D2")}Z+,";
                }

                else if (value.GetType() == typeof(string))
                    whereClause += $"+'{value}'+,";

                else
                    whereClause += $"+{value}+,";
            }

            whereClause = "(" + whereClause.Substring(0, whereClause.Length - 1) + ")";

            if (concatination.HasValue && concatination == FilterConcatination.And)
                return $"+{field}+IN+{whereClause}+AND";

            if (concatination.HasValue && concatination == FilterConcatination.Or)
                return $"+{field}+IN+{whereClause}+OR";

            return $"+{field}+IN+{whereClause}";
        }
    }
}
