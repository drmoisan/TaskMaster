using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UtilitiesCS.Test.OutlookObjects.Item
{
    [Obsolete]
    [TestClass]
    public class OlItemSummaryTests
    {
        [TestMethod]
        public void ExtractSummary_ShouldReturnExpectedValues_ForAppointmentItem()
        {
            // Arrange
            var item = new Mock<AppointmentItem>();
            item.SetupGet(x => x.Subject).Returns("Project sync");
            item.SetupGet(x => x.Start).Returns(new DateTime(2025, 12, 25, 12, 5, 3));
            item.SetupGet(x => x.Parent).Returns(CreateFolder(@"\\Inbox\Calendar"));

            // Act
            Dictionary<OlItemSummary.Details, string> summary = OlItemSummary.ExtractSummary(item.Object);

            // Assert
            summary.Should().BeEquivalentTo(new Dictionary<OlItemSummary.Details, string>
            {
                { OlItemSummary.Details.Type, typeof(AppointmentItem).ToString() },
                { OlItemSummary.Details.Subject, "Project sync" },
                { OlItemSummary.Details.Date, "12-25-2025 12:05 PM" },
                { OlItemSummary.Details.Folderpath, @"\\Inbox\Calendar" },
            });
        }

        [TestMethod]
        public void ExtractSummary_ShouldPreserveNullSubject_ForAppointmentItem()
        {
            // Arrange
            var item = new Mock<AppointmentItem>();
            item.SetupGet(x => x.Subject).Returns((string)null);
            item.SetupGet(x => x.Start).Returns(new DateTime(2025, 12, 25, 12, 5, 3));
            item.SetupGet(x => x.Parent).Returns(CreateFolder(@"\\Inbox\Calendar"));

            // Act
            Dictionary<OlItemSummary.Details, string> summary = OlItemSummary.ExtractSummary(item.Object);

            // Assert
            summary[OlItemSummary.Details.Subject].Should().BeNull();
            summary[OlItemSummary.Details.Folderpath].Should().Be(@"\\Inbox\Calendar");
        }

        [TestMethod]
        public void ExtractSummary_ShouldReturnExpectedValues_ForReadableMailItem()
        {
            // Arrange
            var item = new Mock<MailItem>();
            item.SetupGet(x => x.MessageClass).Returns("IPM.Note");
            item.SetupGet(x => x.Subject).Returns("Weekly report");
            item.SetupGet(x => x.SentOn).Returns(new DateTime(2025, 12, 25, 12, 5, 3));
            item.SetupGet(x => x.Parent).Returns(CreateFolder(@"\\Inbox\Reports"));

            // Act
            Dictionary<OlItemSummary.Details, string> summary = OlItemSummary.ExtractSummary(item.Object);

            // Assert
            summary.Should().BeEquivalentTo(new Dictionary<OlItemSummary.Details, string>
            {
                { OlItemSummary.Details.Type, typeof(MailItem).ToString() },
                { OlItemSummary.Details.Subject, "Weekly report" },
                { OlItemSummary.Details.Date, "12-25-2025 12:05 PM" },
                { OlItemSummary.Details.Folderpath, @"\\Inbox\Reports" },
            });
        }

        [TestMethod]
        public void ExtractSummary_ShouldReturnFallbackValues_ForUnreadableMailItem()
        {
            // Arrange
            var item = new Mock<MailItem>();
            item.SetupGet(x => x.MessageClass).Returns("IPM.Note.Secure");

            // Act
            Dictionary<OlItemSummary.Details, string> summary = OlItemSummary.ExtractSummary(item.Object);

            // Assert
            summary.Should().BeEquivalentTo(new Dictionary<OlItemSummary.Details, string>
            {
                { OlItemSummary.Details.Type, typeof(MailItem).ToString() },
                { OlItemSummary.Details.Subject, "IPM.Note.Secure" },
            });
        }

        [TestMethod]
        public void Extract_ShouldReturnFilteredSummary_WhenFlagsSelectSubset()
        {
            // Arrange
            var item = new Mock<MailItem>();
            item.SetupGet(x => x.MessageClass).Returns("IPM.Note");
            item.SetupGet(x => x.Subject).Returns("Weekly report");
            item.SetupGet(x => x.SentOn).Returns(new DateTime(2025, 12, 25, 12, 5, 3));
            item.SetupGet(x => x.Parent).Returns(CreateFolder(@"\\Inbox\Reports"));
            OlItemSummary.Details flags = OlItemSummary.Details.Subject | OlItemSummary.Details.Date;

            // Act
            string summary = OlItemSummary.Extract(item.Object, flags);

            // Assert
            summary.Should().Be("Subject: Weekly report, Date: 12-25-2025 12:05 PM");
        }

        [TestMethod]
        public void Extract_ShouldReturnRuntimeType_WhenItemTypeIsUnsupported()
        {
            // Arrange
            var item = new Mock<TaskItem>();
            item.SetupGet(x => x.Subject).Returns("Do the thing");

            // Act
            string summary = OlItemSummary.Extract(item.Object, OlItemSummary.Details.All);

            // Assert
            summary.Should().StartWith("Details.Type: Castle.Proxies.TaskItemProxy");
        }

        [TestMethod]
        public void ExtractSummary_ObjectOverload_ShouldThrowArgumentException_ForUnsupportedType()
        {
            // Arrange
            var item = new object();

            // Act
            System.Action action = () => OlItemSummary.ExtractSummary(item);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("System.Object is an unsupported type");
        }

        [TestMethod]
        public void ToString_ShouldIncludeSelectedFlags_AndRenderNullValuesAsEmptyText()
        {
            // Arrange
            var summary = new Dictionary<OlItemSummary.Details, string>
            {
                { OlItemSummary.Details.Type, typeof(MailItem).ToString() },
                { OlItemSummary.Details.Subject, null },
                { OlItemSummary.Details.Date, "12-25-2025 12:05 PM" },
            };

            // Act
            string result = summary.ToString(OlItemSummary.Details.Subject | OlItemSummary.Details.Date);

            // Assert
            result.Should().Be("Subject: , Date: 12-25-2025 12:05 PM");
        }

        private static MAPIFolder CreateFolder(string folderPath)
        {
            var folder = new Mock<MAPIFolder>();
            folder.SetupGet(x => x.FolderPath).Returns(folderPath);
            return folder.Object;
        }
    }
}