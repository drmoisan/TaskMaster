using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses;
using System;
using System.Collections.Generic;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class DictionaryExtensions_Test
    {
        [TestMethod]
        public void ContentEquals_Test_Identical()
        {
            Dictionary<string, string> target = new()
            {
                {"Type", typeof(AppointmentItem).ToString() },
                {"Subject", "TestSubjectString" },
                {"Date", "2025-12-25 12:05 PM" }
            };
            Dictionary<string, string> test = new()
            {
                {"Type", typeof(AppointmentItem).ToString() },
                {"Subject", "TestSubjectString" },
                {"Date", "2025-12-25 12:05 PM" }
            };
            bool result = test.ContentEquals(target);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ContentEquals_Test_DifferentOrder()
        {
            Dictionary<string, string> target = new()
            {
                {"Type", typeof(AppointmentItem).ToString() },
                {"Subject", "TestSubjectString" },
                {"Date", "2025-12-25 12:05 PM" }
            };
            Dictionary<string, string> test = new()
            {
                {"Subject", "TestSubjectString" },
                {"Type", typeof(AppointmentItem).ToString() },
                {"Date", "2025-12-25 12:05 PM" }
            };
            bool result = test.ContentEquals(target);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ContentEquals_Test_MissingElements()
        {
            Dictionary<string, string> target = new()
            {
                {"Type", typeof(AppointmentItem).ToString() },
                {"Subject", "TestSubjectString" },
                {"Date", "2025-12-25 12:05 PM" }
            };
            Dictionary<string, string> test = new()
            {
                {"Subject", "TestSubjectString" },
                {"Date", "2025-12-25 12:05 PM" }
            };
            bool result = test.ContentEquals(target);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ContentEquals_Test_DifferentValue()
        {
            Dictionary<string, string> target = new()
            {
                {"Type", typeof(AppointmentItem).ToString() },
                {"Subject", "TestSubjectString" },
                {"Date", "2025-12-25 12:05 PM" }
            };
            Dictionary<string, string> test = new()
            {
                {"Type", typeof(AppointmentItem).ToString() },
                {"Subject", "TestSubjectString2" },
                {"Date", "2025-12-25 12:05 PM" }
            };
            bool result = test.ContentEquals(target);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ContentEquals_Test_DifferentKey()
        {
            Dictionary<string, string> target = new()
            {
                {"Type", typeof(AppointmentItem).ToString() },
                {"Subject", "TestSubjectString" },
                {"Date", "2025-12-25 12:05 PM" }
            };
            Dictionary<string, string> test = new()
            {
                {"Type", typeof(AppointmentItem).ToString() },
                {"Subject2", "TestSubjectString" },
                {"Date", "2025-12-25 12:05 PM" }
            };
            bool result = test.ContentEquals(target);
            Assert.IsFalse(result);
        }
    }
}
