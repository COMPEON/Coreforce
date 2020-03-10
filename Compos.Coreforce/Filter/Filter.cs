using Compos.Coreforce.Attributes;
using Compos.Coreforce.Extensions;
using System;
using System.Globalization;
using System.Linq.Expressions;

namespace Compos.Coreforce.Filter
{
    public class Filter<T> : IFilter
    {
        private readonly Expression<Func<T, object>> field;
        private readonly object value;
        private readonly string filterOperator;
        private readonly FilterConcatination? concatination;

        public Expression<Func<T, object>> Field
        {
            get => field;
        }

        public object Value
        {
            get => value;
        }

        public string FilterOperator
        {
            get => filterOperator;
        }

        public FilterConcatination? Concatination
        {
            get => concatination;
        }

        public Filter(
            Expression<Func<T, object>> field, 
            string filterOperator, 
            object value
            )
        {
            this.field = field;
            this.filterOperator = filterOperator;
            this.value = value;
            this.concatination = null;
        }

        public Filter(
            Expression<Func<T, object>> field, 
            string filterOperator, 
            object value, 
            FilterConcatination concatination
            )
        {
            this.field = field;
            this.filterOperator = filterOperator;
            this.value = value;
            this.concatination = concatination;
        }

        public string Get()
        {
            var expression = field.GetMemberExpression();

            if (expression == null)
                throw new CoreforceException(CoreforceError.ProcessingError);

            string compareString = string.Empty;

            if (value == null || value.ToString().ToLower().Contains("null"))
                compareString = "null";

            else if (expression.Type == typeof(string))
                compareString = $"'{value}'";

            else if (expression.Type == typeof(Date?) || expression.Type == typeof(Date))
            {
                var date = new Date(value.ToString());
                var dateAsString = date.GetDate();

                compareString = $"{dateAsString}";
            }

            else if (expression.Type == typeof(DateTime?) || expression.Type == typeof(DateTime))
            {
                var dateTime = Convert.ToDateTime(value).ToUniversalTime();
                compareString = $"{dateTime.Year}-{dateTime.Month.ToString("D2")}-{dateTime.Day.ToString("D2")}T{dateTime.Hour.ToString("D2")}:{dateTime.Minute.ToString("D2")}:{dateTime.Second.ToString("D2")}Z";
            }

            else if (value.GetType() == typeof(bool?) || value.GetType() == typeof(bool))
                compareString = $"{value}";

            else
                compareString = $"{(value as IFormattable).ToString(null, CultureInfo.InvariantCulture).ToLower()}";

            if (concatination.HasValue && concatination == FilterConcatination.And)
                return $"+{expression.Member.Name}+{filterOperator}+{compareString}+AND";
            if (concatination.HasValue && concatination == FilterConcatination.Or)
                return $"+{expression.Member.Name}+{filterOperator}+{compareString}+OR";

            return $"+{expression.Member.Name}+{filterOperator}+{compareString}";
        }
    }

    public class Filter : IFilter
    {
        private readonly string field;
        private readonly object value;
        private readonly string filterOperator;
        private readonly FilterConcatination? concatination;

        public string Field
        {
            get => field;
        }

        public object Value
        {
            get => value;
        }

        public string FilterOperator
        {
            get => filterOperator;
        }

        public FilterConcatination? Concatination
        {
            get => concatination;
        }

        public Filter(
            string field,
            string filterOperator,
            object value
            )
        {
            this.field = field;
            this.filterOperator = filterOperator;
            this.value = value;
            this.concatination = null;
        }

        public Filter(
            string field,
            string filterOperator,
            object value,
            FilterConcatination concatination
            )
        {
            this.field = field;
            this.filterOperator = filterOperator;
            this.value = value;
            this.concatination = concatination;
        }

        public string Get()
        {
            string compareString = string.Empty;

            if (value == null || value.ToString().ToLower().Contains("null"))
                compareString = "null";

            else if (value.GetType() == typeof(string))
                compareString = $"'{value}'";

            else if(value.GetType() == typeof(Date?) || value.GetType() == typeof(Date))
            {
                var date = new Date(value.ToString());
                var dateAsString = date.GetDate();

                compareString = $"{dateAsString}";
            }

            else if (value.GetType() == typeof(DateTime?) || value.GetType() == typeof(DateTime))
            {
                var dateTime = Convert.ToDateTime(value).ToUniversalTime();
                compareString = $"{dateTime.Year}-{dateTime.Month.ToString("D2")}-{dateTime.Day.ToString("D2")}T{dateTime.Hour.ToString("D2")}:{dateTime.Minute.ToString("D2")}:{dateTime.Second.ToString("D2")}Z";
            }

            else if(value.GetType() == typeof(bool?) || value.GetType() == typeof(bool))
                compareString = $"{value}";

            else
                compareString = $"{(value as IFormattable).ToString(null, CultureInfo.InvariantCulture).ToLower()}";

            if (concatination.HasValue && concatination == FilterConcatination.And)
                return $"+{field}+{filterOperator}+{compareString}+AND";
            if (concatination.HasValue && concatination == FilterConcatination.Or)
                return $"+{field}+{filterOperator}+{compareString}+OR";

            return $"+{field}+{filterOperator}+{compareString}";
        }
    }
}
