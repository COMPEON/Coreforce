using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compos.Coreforce.UnitTests
{
    [TestClass]
    public class DateTests
    {
        [TestMethod]
        public void OperatorTests()
        {
            var date = new Date("2019-11-28");

            // Year
            Assert.IsFalse(date == new Date(new DateTime(2020, 11, 28)));
            Assert.IsTrue(date != new Date(new DateTime(2020, 11, 28)));
            Assert.IsFalse(date > new Date(new DateTime(2020, 11, 28)));
            Assert.IsTrue(date < new Date(new DateTime(2020, 11, 28)));
            Assert.IsFalse(date >= new Date(new DateTime(2020, 11, 28)));
            Assert.IsTrue(date <= new Date(new DateTime(2020, 11, 28)));

            Assert.IsTrue(date == new Date(new DateTime(2019, 11, 28)));
            Assert.IsFalse(date != new Date(new DateTime(2019, 11, 28)));
            Assert.IsFalse(date > new Date(new DateTime(2019, 11, 28)));
            Assert.IsFalse(date < new Date(new DateTime(2019, 11, 28)));
            Assert.IsTrue(date >= new Date(new DateTime(2019, 11, 28)));
            Assert.IsTrue(date <= new Date(new DateTime(2019, 11, 28)));

            Assert.IsFalse(date == new Date(new DateTime(2018, 11, 28)));
            Assert.IsTrue(date != new Date(new DateTime(2018, 11, 28)));
            Assert.IsTrue(date > new Date(new DateTime(2018, 11, 28)));
            Assert.IsFalse(date < new Date(new DateTime(2018, 11, 28)));
            Assert.IsTrue(date >= new Date(new DateTime(2018, 11, 28)));
            Assert.IsFalse(date <= new Date(new DateTime(2018, 11, 28)));

            // Month
            Assert.IsFalse(date == new Date(new DateTime(2019, 12, 28)));
            Assert.IsTrue(date != new Date(new DateTime(2019, 12, 28)));
            Assert.IsFalse(date > new Date(new DateTime(2019, 12, 28)));
            Assert.IsTrue(date < new Date(new DateTime(2019, 12, 28)));
            Assert.IsFalse(date >= new Date(new DateTime(2019, 12, 28)));
            Assert.IsTrue(date <= new Date(new DateTime(2019, 12, 28)));

            Assert.IsTrue(date == new Date(new DateTime(2019, 11, 28)));
            Assert.IsFalse(date != new Date(new DateTime(2019, 11, 28)));
            Assert.IsFalse(date > new Date(new DateTime(2019, 11, 28)));
            Assert.IsFalse(date < new Date(new DateTime(2019, 11, 28)));
            Assert.IsTrue(date >= new Date(new DateTime(2019, 11, 28)));
            Assert.IsTrue(date <= new Date(new DateTime(2019, 11, 28)));

            Assert.IsFalse(date == new Date(new DateTime(2019, 10, 28)));
            Assert.IsTrue(date != new Date(new DateTime(2019, 10, 28)));
            Assert.IsTrue(date > new Date(new DateTime(2019, 10, 28)));
            Assert.IsFalse(date < new Date(new DateTime(2019, 10, 28)));
            Assert.IsTrue(date >= new Date(new DateTime(2019, 10, 28)));
            Assert.IsFalse(date <= new Date(new DateTime(2019, 10, 28)));

            // Day
            Assert.IsFalse(date == new Date(new DateTime(2019, 11, 29)));
            Assert.IsTrue(date != new Date(new DateTime(2019, 11, 29)));
            Assert.IsFalse(date > new Date(new DateTime(2019, 11, 29)));
            Assert.IsTrue(date < new Date(new DateTime(2019, 11, 29)));
            Assert.IsFalse(date >= new Date(new DateTime(2019, 11, 29)));
            Assert.IsTrue(date <= new Date(new DateTime(2019, 11, 29)));

            Assert.IsTrue(date == new Date(new DateTime(2019, 11, 28)));
            Assert.IsFalse(date != new Date(new DateTime(2019, 11, 28)));
            Assert.IsFalse(date > new Date(new DateTime(2019, 11, 28)));
            Assert.IsFalse(date < new Date(new DateTime(2019, 11, 28)));
            Assert.IsTrue(date >= new Date(new DateTime(2019, 11, 28)));
            Assert.IsTrue(date <= new Date(new DateTime(2019, 11, 28)));

            Assert.IsFalse(date == new Date(new DateTime(2019, 11, 27)));
            Assert.IsTrue(date != new Date(new DateTime(2019, 11, 27)));
            Assert.IsTrue(date > new Date(new DateTime(2019, 11, 27)));
            Assert.IsFalse(date < new Date(new DateTime(2019, 11, 27)));
            Assert.IsTrue(date >= new Date(new DateTime(2019, 11, 27)));
            Assert.IsFalse(date <= new Date(new DateTime(2019, 11, 27)));

            // Mixed
            Assert.IsTrue(date > new Date(new DateTime(1999, 3, 4)));
            Assert.IsTrue(date < new Date(new DateTime(2300, 7, 13)));
            Assert.IsFalse(date >= new Date(new DateTime(2019, 11, 30)));
        }

        [TestMethod]
        public void TestCompareTo()
        {
            List<Date> dates = new List<Date>()
            {
                new Date("2018-11-05"),
                new Date("2018-01-25"),
                new Date("2015-12-15"),
                new Date("2018-05-24"),
                new Date("2013-04-29"),
                new Date("2018-05-12"),
                new Date("2019-08-19")
            };

            var cmpDates1 = dates.OrderByDescending(x => x).ToList();
            var cmpDates2 = dates.OrderBy(x => x).ToList();

            Assert.AreEqual("2019-08-19", cmpDates1[0].GetDate());
            Assert.AreEqual("2018-11-05", cmpDates1[1].GetDate());
            Assert.AreEqual("2018-05-24", cmpDates1[2].GetDate());
            Assert.AreEqual("2018-05-12", cmpDates1[3].GetDate());
            Assert.AreEqual("2018-01-25", cmpDates1[4].GetDate());
            Assert.AreEqual("2015-12-15", cmpDates1[5].GetDate());
            Assert.AreEqual("2013-04-29", cmpDates1[6].GetDate());

            Assert.AreEqual("2013-04-29", cmpDates2[0].GetDate());
            Assert.AreEqual("2015-12-15", cmpDates2[1].GetDate());
            Assert.AreEqual("2018-01-25", cmpDates2[2].GetDate());
            Assert.AreEqual("2018-05-12", cmpDates2[3].GetDate());
            Assert.AreEqual("2018-05-24", cmpDates2[4].GetDate());
            Assert.AreEqual("2018-11-05", cmpDates2[5].GetDate());
            Assert.AreEqual("2019-08-19", cmpDates2[6].GetDate());
        }
    }
}
