using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using System;
using System.Collections.Generic;
using UtilitiesCS.OutlookExtensions;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class OutlookItemSummary_Test
    {
        [TestMethod]
        public void ExtractSummary_Test_AppointmentItem()
        {
            Mock<AppointmentItem> item = new Mock<AppointmentItem>();
            item.SetupGet(x => x.Subject).Returns("TestSubjectString");
            item.SetupGet(x => x.Start).Returns(new DateTime(2025, 12, 25, 12, 5, 3));
            Dictionary<OlItemSummary.Details, string> target = new()
            {
                {OlItemSummary.Details.Type, typeof(AppointmentItem).ToString() },
                {OlItemSummary.Details.Subject, "TestSubjectString" },
                {OlItemSummary.Details.Date, "12-25-2025 12:05 PM" }
            };
            Dictionary<OlItemSummary.Details, string> test = OlItemSummary.ExtractSummary(item.Object);
            Assert.IsTrue(test.ContentEquals(target));
        }

        [TestMethod]
        public void ExtractSummary_Test_MeetingItem()
        {
            Mock<MeetingItem> item = new Mock<MeetingItem>();
            item.SetupGet(x => x.Subject).Returns("TestSubjectString");
            item.SetupGet(x => x.SentOn).Returns(new DateTime(2025, 12, 25, 12, 5, 3));
            Dictionary<OlItemSummary.Details, string> target = new()
            {
                {OlItemSummary.Details.Type, typeof(MeetingItem).ToString() },
                {OlItemSummary.Details.Subject, "TestSubjectString" },
                {OlItemSummary.Details.Date, "12-25-2025 12:05 PM" }
            };
            Dictionary<OlItemSummary.Details, string> test = OlItemSummary.ExtractSummary(item.Object);
            Assert.IsTrue(test.ContentEquals(target));
        }

        [TestMethod]
        public void ExtractSummary_Test_TaskRequestItem()
        {
            Mock<TaskRequestItem> item = new Mock<TaskRequestItem>();
            item.SetupGet(x => x.Subject).Returns("TestSubjectString");
            item.SetupGet(x => x.CreationTime).Returns(new DateTime(2025, 12, 25, 12, 5, 3));
            Dictionary<OlItemSummary.Details, string> target = new()
            {
                {OlItemSummary.Details.Type, typeof(TaskRequestItem).ToString() },
                {OlItemSummary.Details.Subject, "TestSubjectString" },
                {OlItemSummary.Details.Date, "12-25-2025 12:05 PM" }
            };
            Dictionary<OlItemSummary.Details, string> test = OlItemSummary.ExtractSummary(item.Object);
            Assert.IsTrue(test.ContentEquals(target));
        }

        [TestMethod]
        public void ExtractSummary_Test_TaskRequestUpdateItem()
        {
            Mock<TaskRequestUpdateItem> item = new Mock<TaskRequestUpdateItem>();
            item.SetupGet(x => x.Subject).Returns("TestSubjectString");
            item.SetupGet(x => x.LastModificationTime).Returns(new DateTime(2025, 12, 25, 12, 5, 3));
            Dictionary<OlItemSummary.Details, string> target = new()
            {
                {OlItemSummary.Details.Type, typeof(TaskRequestUpdateItem).ToString() },
                {OlItemSummary.Details.Subject, "TestSubjectString" },
                {OlItemSummary.Details.Date, "12-25-2025 12:05 PM" }
            };
            Dictionary<OlItemSummary.Details, string> test = OlItemSummary.ExtractSummary(item.Object);
            Assert.IsTrue(test.ContentEquals(target));
        }

        [TestMethod]
        public void ExtractSummary_Test_EmailReadable()
        {
            Mock<MailItem> item = new Mock<MailItem>();
            item.SetupGet(x => x.MessageClass).Returns("Dummy Readable Message Class");
            item.SetupGet(x => x.Subject).Returns("TestSubjectString");
            item.SetupGet(x => x.SentOn).Returns(new DateTime(2025, 12, 25, 12, 5, 3));
            Dictionary<OlItemSummary.Details, string> target = new()
            {
                {OlItemSummary.Details.Type, typeof(MailItem).ToString() },
                {OlItemSummary.Details.Subject, "TestSubjectString" },
                {OlItemSummary.Details.Date, "12-25-2025 12:05 PM" }
            };
            Dictionary<OlItemSummary.Details, string> test = OlItemSummary.ExtractSummary(item.Object);
            Assert.IsTrue(test.ContentEquals(target));
        }

        [TestMethod]
        public void ExtractSummary_Test_EmailUnReadable()
        {
            //"IPM.Note.SMIME" | item.MessageClass == "IPM.Note.Secure" | item.MessageClass == "IPM.Note.Secure.Sign" | item.MessageClass == "IPM.Outlook.Recall";
            Mock<MailItem> item = new Mock<MailItem>();
            item.SetupGet(x => x.MessageClass).Returns("IPM.Note.Secure");
            Dictionary<OlItemSummary.Details, string> target = new()
            {
                {OlItemSummary.Details.Type, typeof(MailItem).ToString() },
                {OlItemSummary.Details.Subject, "IPM.Note.Secure" }
            };
            Dictionary<OlItemSummary.Details, string> test = OlItemSummary.ExtractSummary(item.Object);
            Assert.IsTrue(test.ContentEquals(target));
        }

        [TestMethod]
        public void Extract_Test_OtherObject()
        {
            Mock<TaskItem> item = new Mock<TaskItem>();
            item.SetupGet(x => x.Subject).Returns("TestSubjectString");
            item.SetupGet(x => x.CreationTime).Returns(new DateTime(2025, 12, 25, 12, 5, 3));
            OlItemSummary.Details options = GenericBitwiseStatic<OlItemSummary.Details>.Or(
                new List<OlItemSummary.Details> 
                { 
                    OlItemSummary.Details.Type, 
                    OlItemSummary.Details.Subject, 
                    OlItemSummary.Details.Date 
                });
            string test = OlItemSummary.Extract(item.Object, options);
            string target = "Details.Type: Castle.Proxies.TaskItemProxy";
            Assert.AreEqual(target, test);
        }

        [TestMethod]
        public void ToString_TestAll()
        {
            Dictionary<OlItemSummary.Details, string> testDict = new()
            {
                {OlItemSummary.Details.Type, typeof(MailItem).ToString() },
                {OlItemSummary.Details.Subject, "TestSubjectString" },
                {OlItemSummary.Details.Date, "12-25-2025 12:05 PM" }
            };
            string target = "Type: Microsoft.Office.Interop.Outlook.MailItem, Subject: TestSubjectString, Date: 12-25-2025 12:05 PM";
            string test = testDict.ToString(OlItemSummary.Details.All);
            Assert.AreEqual(target, test);
        }

        [TestMethod]
        public void ToString_TestSubset()
        {
            Dictionary<OlItemSummary.Details, string> testDict = new()
            {
                {OlItemSummary.Details.Type, typeof(MailItem).ToString() },
                {OlItemSummary.Details.Subject, "TestSubjectString" },
                {OlItemSummary.Details.Date, "12-25-2025 12:05 PM" }
            };
            OlItemSummary.Details options = GenericBitwiseStatic<OlItemSummary.Details>.Or(
                new List<OlItemSummary.Details>
                {
                    OlItemSummary.Details.Subject,
                    OlItemSummary.Details.Date
                });
            string target = "Subject: TestSubjectString, Date: 12-25-2025 12:05 PM";
            string test = testDict.ToString(options);
            Assert.AreEqual(target, test);
        }

        [TestMethod]
        public void Extract_Test_EmailReadable_All()
        {
            Mock<MailItem> item = new Mock<MailItem>();
            item.SetupGet(x => x.MessageClass).Returns("Dummy Readable Message Class");
            item.SetupGet(x => x.Subject).Returns("TestSubjectString");
            item.SetupGet(x => x.SentOn).Returns(new DateTime(2025, 12, 25, 12, 5, 3));
            string target = "Type: Microsoft.Office.Interop.Outlook.MailItem, Subject: TestSubjectString, Date: 12-25-2025 12:05 PM";
            string test = OlItemSummary.Extract(item.Object, OlItemSummary.Details.All);
            Assert.AreEqual(target, test);
        }

        [TestMethod]
        public void Extract_Test_EmailReadable_Subset()
        {
            Mock<MailItem> item = new Mock<MailItem>();
            item.SetupGet(x => x.MessageClass).Returns("Dummy Readable Message Class");
            item.SetupGet(x => x.Subject).Returns("TestSubjectString");
            item.SetupGet(x => x.SentOn).Returns(new DateTime(2025, 12, 25, 12, 5, 3));
            string target = "Subject: TestSubjectString, Date: 12-25-2025 12:05 PM";
            OlItemSummary.Details options = GenericBitwiseStatic<OlItemSummary.Details>.Or(
                new List<OlItemSummary.Details>
                {
                    OlItemSummary.Details.Subject,
                    OlItemSummary.Details.Date
                });
            string test = OlItemSummary.Extract(item.Object, options);
            Assert.AreEqual(target, test);
        }

        [TestMethod]
        public void TestOutlookItemTryGet()
        {
            string random = "Trying";
            var item = new OutlookItem(random as object);
            var result = item.Try().Attachments;
        }
    }
}
