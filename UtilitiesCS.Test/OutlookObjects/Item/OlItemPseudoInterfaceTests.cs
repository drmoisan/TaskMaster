using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using InteropMailItem = Microsoft.Office.Interop.Outlook.MailItem;

namespace UtilitiesCS.Test.OutlookObjects.Item
{
    [TestClass]
    public class OlItemPseudoInterfaceTests
    {
        [TestMethod]
        public void SetCategories_WhenItemIsSupported_ShouldAssignCategoriesAndSave()
        {
            var mailItem = new Mock<InteropMailItem>();
            mailItem.SetupProperty(x => x.Categories);

            mailItem.Object.SetCategories("Project X");

            mailItem.Object.Categories.Should().Be("Project X");
            mailItem.Verify(x => x.Save(), Times.Once);
        }

        [TestMethod]
        public void GetCategories_WhenItemIsSupported_ShouldReturnCategories()
        {
            var taskItem = new Mock<TaskItem>();
            taskItem.SetupGet(x => x.Categories).Returns("Alpha; Beta");

            var result = taskItem.Object.GetCategories();

            result.Should().Be("Alpha; Beta");
        }

        [TestMethod]
        public void SetCategories_WhenItemIsUnsupported_ShouldThrowArgumentException()
        {
            var unsupported = new object();

            System.Action act = () => unsupported.SetCategories("Ignored");

            act.Should().Throw<ArgumentException>().WithMessage("Unsupported type*");
        }

        [TestMethod]
        public void NoConflicts_WhenConflictCountIsZero_ShouldReturnTrue()
        {
            var conflicts = new Mock<Conflicts>();
            conflicts.SetupGet(x => x.Count).Returns(0);
            var mailItem = new Mock<InteropMailItem>();
            mailItem.SetupGet(x => x.Conflicts).Returns(conflicts.Object);

            var result = mailItem.Object.NoConflicts();

            result.Should().BeTrue();
        }

        [TestMethod]
        public void NoConflicts_WhenConflictLookupThrows_ShouldReturnFalse()
        {
            var mailItem = new Mock<InteropMailItem>();
            mailItem.SetupGet(x => x.Conflicts).Throws(new InvalidOperationException("boom"));

            var result = mailItem.Object.NoConflicts();

            result.Should().BeFalse();
        }

        [TestMethod]
        public void OnlyMailItems_ShouldReturnOnlyMailItemsFromSelection()
        {
            var mail1 = new Mock<InteropMailItem>().Object;
            var mail2 = new Mock<InteropMailItem>().Object;
            var task = new Mock<TaskItem>().Object;
            var selection = CreateSelection(mail1, task, mail2);
            var method = typeof(UtilitiesCS.OlItemPseudoInterface).GetMethod(
                nameof(UtilitiesCS.OlItemPseudoInterface.OnlyMailItems),
                BindingFlags.Public | BindingFlags.Static
            );

            var result = (
                (System.Collections.IEnumerable)
                    method!.Invoke(null, new object[] { selection.Object })
            )
                .Cast<object>()
                .ToArray();

            result.Should().Equal(mail1, mail2);
        }

        [TestMethod]
        public void OnlySupportedObjects_ShouldReturnSupportedOutlookObjectsOnly()
        {
            var mail = new Mock<InteropMailItem>().Object;
            var meeting = new Mock<MeetingItem>().Object;
            var appointment = new Mock<AppointmentItem>().Object;
            var task = new Mock<TaskItem>().Object;
            var unsupported = new object();
            var selection = CreateSelection(mail, unsupported, meeting, appointment, task);

            var result = selection.Object.OnlySupportedObjects();

            result.Should().Equal(mail, meeting, appointment, task);
        }

        private static Mock<Selection> CreateSelection(params object[] items)
        {
            var selection = new Mock<Selection>();
            selection
                .Setup(x => x.GetEnumerator())
                .Returns(((IEnumerable<object>)items).GetEnumerator());
            return selection;
        }

        #region Extended Tests — P2-T13

        [TestMethod]
        public void SetCategories_WhenItemIsTaskItem_ShouldAssignCategoriesAndSave()
        {
            var taskItem = new Mock<TaskItem>();
            taskItem.SetupProperty(x => x.Categories);

            taskItem.Object.SetCategories("Task Category");

            taskItem.Object.Categories.Should().Be("Task Category");
            taskItem.Verify(x => x.Save(), Times.Once);
        }

        [TestMethod]
        public void SetCategories_WhenItemIsAppointmentItem_ShouldAssignCategoriesAndSave()
        {
            var apptItem = new Mock<AppointmentItem>();
            apptItem.SetupProperty(x => x.Categories);

            apptItem.Object.SetCategories("Appt Category");

            apptItem.Object.Categories.Should().Be("Appt Category");
            apptItem.Verify(x => x.Save(), Times.Once);
        }

        [TestMethod]
        public void SetCategories_WhenItemIsMeetingItem_ShouldAssignCategoriesAndSave()
        {
            var meetingItem = new Mock<MeetingItem>();
            meetingItem.SetupProperty(x => x.Categories);

            meetingItem.Object.SetCategories("Meeting Category");

            meetingItem.Object.Categories.Should().Be("Meeting Category");
            meetingItem.Verify(x => x.Save(), Times.Once);
        }

        [TestMethod]
        public void GetCategories_WhenItemIsMailItem_ShouldReturnCategories()
        {
            var mailItem = new Mock<InteropMailItem>();
            mailItem.SetupGet(x => x.Categories).Returns("Mail Cat");

            var result = mailItem.Object.GetCategories();

            result.Should().Be("Mail Cat");
        }

        [TestMethod]
        public void GetCategories_WhenItemIsAppointmentItem_ShouldReturnCategories()
        {
            var apptItem = new Mock<AppointmentItem>();
            apptItem.SetupGet(x => x.Categories).Returns("Appt Cat");

            var result = apptItem.Object.GetCategories();

            result.Should().Be("Appt Cat");
        }

        [TestMethod]
        public void GetCategories_WhenItemIsMeetingItem_ShouldReturnCategories()
        {
            var meetingItem = new Mock<MeetingItem>();
            meetingItem.SetupGet(x => x.Categories).Returns("Meeting Cat");

            var result = meetingItem.Object.GetCategories();

            result.Should().Be("Meeting Cat");
        }

        [TestMethod]
        public void GetCategories_WhenItemIsUnsupported_ShouldThrowArgumentException()
        {
            var unsupported = new object();

            System.Action act = () => unsupported.GetCategories();

            act.Should().Throw<ArgumentException>().WithMessage("Unsupported type*");
        }

        [TestMethod]
        public void NoConflicts_WhenItemIsTaskItem_ShouldReturnTrue()
        {
            var conflicts = new Mock<Conflicts>();
            conflicts.SetupGet(x => x.Count).Returns(0);
            var taskItem = new Mock<TaskItem>();
            taskItem.SetupGet(x => x.Conflicts).Returns(conflicts.Object);

            var result = taskItem.Object.NoConflicts();

            result.Should().BeTrue();
        }

        [TestMethod]
        public void NoConflicts_WhenItemIsMeetingItem_ShouldReturnTrue()
        {
            var conflicts = new Mock<Conflicts>();
            conflicts.SetupGet(x => x.Count).Returns(0);
            var meetingItem = new Mock<MeetingItem>();
            meetingItem.SetupGet(x => x.Conflicts).Returns(conflicts.Object);

            var result = meetingItem.Object.NoConflicts();

            result.Should().BeTrue();
        }

        [TestMethod]
        public void NoConflicts_WhenItemIsAppointmentItem_ShouldReturnTrue()
        {
            var conflicts = new Mock<Conflicts>();
            conflicts.SetupGet(x => x.Count).Returns(0);
            var apptItem = new Mock<AppointmentItem>();
            apptItem.SetupGet(x => x.Conflicts).Returns(conflicts.Object);

            var result = apptItem.Object.NoConflicts();

            result.Should().BeTrue();
        }

        [TestMethod]
        public void NoConflicts_WhenItemIsUnsupported_ShouldReturnFalse()
        {
            var unsupported = new object();

            var result = unsupported.NoConflicts();

            // The unsupported type throws ArgumentException which is caught and returns false
            result.Should().BeFalse();
        }

        #endregion
    }
}
