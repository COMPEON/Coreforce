using CoreforceFilter = Compos.Coreforce.Filter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Compos.Coreforce.UnitTests.Filter
{
    [TestClass]
    public class FilterTests
    {
        [TestMethod]
        public void Filter_ObjectBased_StringComparison_GetValidSoql()
        {
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter<Account>(x => x.Id, "=", "0001D0001");

            Assert.AreEqual("+Id+=+'0001D0001'", filter.Get());
        }

        [TestMethod]
        public void Filter_ObjectBased_StringComparisonAndOrLink_GetValidSoql()
        {
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter<Account>(x => x.Id, "=", "0001D0001", CoreforceFilter.FilterConcatination.Or);

            Assert.AreEqual("+Id+=+'0001D0001'+OR", filter.Get());
        }

        [TestMethod]
        public void Filter_ObjectBased_StringComparisonAndAndLink_GetValidSoql()
        {
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter<Account>(x => x.Id, "=", "0001D0001", CoreforceFilter.FilterConcatination.And);

            Assert.AreEqual("+Id+=+'0001D0001'+AND", filter.Get());
        }

        [TestMethod]
        public void Filter_Dynamic_StringComparison_GetValidSoql()
        {
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter("Id", "=", "0001D0001");

            Assert.AreEqual("+Id+=+'0001D0001'", filter.Get());
        }

        [TestMethod]
        public void Filter_Dynamic_StringComparisonAndOrLink_GetValidSoql()
        {
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter("Id", "=", "0001D0001", CoreforceFilter.FilterConcatination.Or);

            Assert.AreEqual("+Id+=+'0001D0001'+OR", filter.Get());
        }

        [TestMethod]
        public void Filter_Dynamic_StringComparisonAndAndLink_GetValidSoql()
        {
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter("Id", "=", "0001D0001", CoreforceFilter.FilterConcatination.And);

            Assert.AreEqual("+Id+=+'0001D0001'+AND", filter.Get());
        }

        [TestMethod]
        public void Filter_ObjectBased_IntegerComparison_GetValidSoql()
        {
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter<Account>(x => x.NumberOfEmployees, ">", 100);

            Assert.AreEqual("+NumberOfEmployees+>+100", filter.Get());
        }

        [TestMethod]
        public void Filter_ObjectBased_IntegerComparisonAndOrLink_GetValidSoql()
        {
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter<Account>(x => x.NumberOfEmployees, ">", 100, CoreforceFilter.FilterConcatination.Or);

            Assert.AreEqual("+NumberOfEmployees+>+100+OR", filter.Get());
        }

        [TestMethod]
        public void Filter_ObjectBased_IntegerComparisonAndAndLink_GetValidSoql()
        {
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter<Account>(x => x.NumberOfEmployees, ">", 100, CoreforceFilter.FilterConcatination.And);

            Assert.AreEqual("+NumberOfEmployees+>+100+AND", filter.Get());
        }

        [TestMethod]
        public void Filter_Dynamic_IntegerComparison_GetValidSoql()
        {
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter("NumberOfEmployees", ">", 100);

            Assert.AreEqual("+NumberOfEmployees+>+100", filter.Get());
        }

        [TestMethod]
        public void Filter_Dynamic_IntegerComparisonAndOrLink_GetValidSoql()
        {
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter("NumberOfEmployees", ">", 100, CoreforceFilter.FilterConcatination.Or);

            Assert.AreEqual("+NumberOfEmployees+>+100+OR", filter.Get());
        }

        [TestMethod]
        public void Filter_Dynamic_IntegerComparisonAndAndLink_GetValidSoql()
        {
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter("NumberOfEmployees", ">", 100, CoreforceFilter.FilterConcatination.And);

            Assert.AreEqual("+NumberOfEmployees+>+100+AND", filter.Get());
        }

        [TestMethod]
        public void Filter_ObjectBased_DecimalComparison_GetValidSoql()
        {
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter<Account>(x => x.Profit, ">", 100.23);

            Assert.AreEqual("+Profit+>+100.23", filter.Get());
        }

        [TestMethod]
        public void Filter_ObjectBased_DecimalComparisonAndOrLink_GetValidSoql()
        {
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter<Account>(x => x.Profit, ">", 100.23, CoreforceFilter.FilterConcatination.Or);

            Assert.AreEqual("+Profit+>+100.23+OR", filter.Get());
        }

        [TestMethod]
        public void Filter_ObjectBased_DecimalComparisonAndAndLink_GetValidSoql()
        {
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter<Account>(x => x.Profit, ">", 100.23, CoreforceFilter.FilterConcatination.And);

            Assert.AreEqual("+Profit+>+100.23+AND", filter.Get());
        }

        [TestMethod]
        public void Filter_Dynamic_DecimalComparison_GetValidSoql()
        {
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter("Profit", ">", 100.23);

            Assert.AreEqual("+Profit+>+100.23", filter.Get());
        }

        [TestMethod]
        public void Filter_Dynamic_DecimalComparisonAndOrLink_GetValidSoql()
        {
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter("Profit", ">", 100.23, CoreforceFilter.FilterConcatination.Or);

            Assert.AreEqual("+Profit+>+100.23+OR", filter.Get());
        }

        [TestMethod]
        public void Filter_Dynamic_DecimalComparisonAndAndLink_GetValidSoql()
        {
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter("Profit", ">", 100.23, CoreforceFilter.FilterConcatination.And);

            Assert.AreEqual("+Profit+>+100.23+AND", filter.Get());
        }

        [TestMethod]
        public void Filter_ObjectBased_DateTimeComparison_GetValidSoql()
        {
            DateTime dateTime = new DateTime(2016, 1, 1, 18, 0, 2, DateTimeKind.Utc);
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter<Account>(x => x.CreatedDate, ">", dateTime);

            Assert.AreEqual("+CreatedDate+>+2016-01-01T18:00:02Z", filter.Get());
        }

        [TestMethod]
        public void Filter_ObjectBased_DateTimeComparisonAndOrLink_GetValidSoql()
        {
            DateTime dateTime = new DateTime(2016, 1, 1, 18, 0, 2, DateTimeKind.Utc);
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter<Account>(x => x.CreatedDate, ">", dateTime, CoreforceFilter.FilterConcatination.Or);

            Assert.AreEqual("+CreatedDate+>+2016-01-01T18:00:02Z+OR", filter.Get());
        }

        [TestMethod]
        public void Filter_ObjectBased_DateTimeComparisonAndAndLink_GetValidSoql()
        {
            DateTime dateTime = new DateTime(2016, 1, 1, 18, 0, 2, DateTimeKind.Utc);
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter<Account>(x => x.CreatedDate, ">", dateTime, CoreforceFilter.FilterConcatination.And);

            Assert.AreEqual("+CreatedDate+>+2016-01-01T18:00:02Z+AND", filter.Get());
        }

        [TestMethod]
        public void Filter_Dynamic_DateTimeComparison_GetValidSoql()
        {
            DateTime dateTime = new DateTime(2016, 1, 1, 18, 0, 2, DateTimeKind.Utc);
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter("CreatedDate", ">", dateTime);

            Assert.AreEqual("+CreatedDate+>+2016-01-01T18:00:02Z", filter.Get());
        }

        [TestMethod]
        public void Filter_Dynamic_DateTimeComparisonAndOrLink_GetValidSoql()
        {
            DateTime dateTime = new DateTime(2016, 1, 1, 18, 0, 2, DateTimeKind.Utc);
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter("CreatedDate", ">", dateTime, CoreforceFilter.FilterConcatination.Or);

            Assert.AreEqual("+CreatedDate+>+2016-01-01T18:00:02Z+OR", filter.Get());
        }

        [TestMethod]
        public void Filter_Dynamic_DateTimeComparisonAndAndLink_GetValidSoql()
        {
            DateTime dateTime = new DateTime(2016, 1, 1, 18, 0, 2, DateTimeKind.Utc);
            CoreforceFilter.IFilter filter = new CoreforceFilter.Filter("CreatedDate", ">", dateTime, CoreforceFilter.FilterConcatination.And);

            Assert.AreEqual("+CreatedDate+>+2016-01-01T18:00:02Z+AND", filter.Get());
        }

        [TestMethod]
        public void FilterCollection_Dynamic_DateTimeComparisonAndAndLink_GetValidSoql()
        {
            DateTime dateTime = new DateTime(2016, 1, 1, 18, 0, 2, DateTimeKind.Utc);
            CoreforceFilter.IFilterCollection filterCollection = new CoreforceFilter.FilterCollection(new CoreforceFilter.Filter("CreatedDate", ">", dateTime, CoreforceFilter.FilterConcatination.And));

            Assert.AreEqual("+(+CreatedDate+>+2016-01-01T18:00:02Z+AND+)", filterCollection.Get());
        }
    }

    public class Account
    {
        public string Id { get; set; }
        public int NumberOfEmployees { get; set; }
        public decimal Profit { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
