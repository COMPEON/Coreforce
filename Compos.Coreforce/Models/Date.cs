using Compos.Coreforce.Converter;
using Newtonsoft.Json;
using System;

namespace Compos.Coreforce
{
    [JsonConverter(typeof(DateConverter))]
    public struct Date : IComparable
    {
        private readonly DateTime? value;

        public Date(DateTime? date)
        {
            value = date;
        }

        public Date(string date)
        {
            DateTime dateTime;

            if (DateTime.TryParse(date, out dateTime))
                value = dateTime;

            else
                value = null;
        }

        public string GetDate()
        {
            if (value.HasValue)
                return value.Value.ToString("yyyy-MM-dd");

            return null;
        }

        public DateTime? GetDateAsDatetime()
        {
            return value;
        }

        public static implicit operator Date(DateTime date)
        {
            return new Date(date);
        }

        public static implicit operator Date(string date)
        {
            return new Date(date);
        }

        public override string ToString()
        {
            if (value.HasValue)
                return value.Value.ToString("yyyy-MM-dd");

            return null;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            Date? otherDate = obj as Date?;
            if (otherDate.HasValue)
                if (value.HasValue)
                    return value.Value.CompareTo(otherDate.Value.GetDateAsDatetime());
                else
                    return -1;
            else
                throw new ArgumentException("Object is not a Date");
        }

        public static bool operator ==(Date leftDate, Date rightDate)
        {
            if (!leftDate.value.HasValue && !rightDate.value.HasValue ||
                (leftDate.value.HasValue && rightDate.value.HasValue &&
                leftDate.value.Value.Year == rightDate.value.Value.Year &&
                leftDate.value.Value.Month == rightDate.value.Value.Month &&
                leftDate.value.Value.Day == rightDate.value.Value.Day))
                return true;

            return false;
        }

        public static bool operator !=(Date leftDate, Date rightDate)
        {
            if (!leftDate.value.HasValue && !rightDate.value.HasValue ||
                (leftDate.value.HasValue && rightDate.value.HasValue &&
                leftDate.value.Value.Year == rightDate.value.Value.Year &&
                leftDate.value.Value.Month == rightDate.value.Value.Month &&
                leftDate.value.Value.Day == rightDate.value.Value.Day))
                return false;

            return true;
        }

        public static bool operator >(Date leftDate, Date rightDate)
        {
            if (leftDate.value.HasValue && rightDate.value.HasValue &&
                ((leftDate.value.Value.Year == rightDate.value.Value.Year && leftDate.value.Value.Month == rightDate.value.Value.Month && leftDate.value.Value.Day > rightDate.value.Value.Day) ||
                (leftDate.value.Value.Year == rightDate.value.Value.Year && leftDate.value.Value.Month > rightDate.value.Value.Month) ||
                (leftDate.value.Value.Year > rightDate.value.Value.Year)))
                return true;

            return false;
        }

        public static bool operator <(Date leftDate, Date rightDate)
        {
            if (leftDate.value.HasValue && rightDate.value.HasValue &&
                ((leftDate.value.Value.Year == rightDate.value.Value.Year && leftDate.value.Value.Month == rightDate.value.Value.Month && leftDate.value.Value.Day < rightDate.value.Value.Day) ||                
                (leftDate.value.Value.Year == rightDate.value.Value.Year && leftDate.value.Value.Month < rightDate.value.Value.Month) ||
                (leftDate.value.Value.Year < rightDate.value.Value.Year)))
                return true;

            return false;
        }

        public static bool operator >=(Date leftDate, Date rightDate)
        {
            if (leftDate.value.HasValue && rightDate.value.HasValue &&
                (leftDate == rightDate || leftDate > rightDate))
                return true;

            return false;
        }

        public static bool operator <=(Date leftDate, Date rightDate)
        {
            if (leftDate.value.HasValue && rightDate.value.HasValue &&
                (leftDate == rightDate || leftDate < rightDate))
                return true;

            return false;
        }
    }
}
