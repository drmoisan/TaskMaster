using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookExtensions;

namespace UtilitiesCS.Test.OutlookObjects.Item
{
    [TestClass]
    public class OutlookItemFlaggableTryTests
    {
        [TestMethod]
        public void Complete_WhenUnderlyingGetterSucceeds_ShouldReturnValue()
        {
            var item = CreateBaseFlaggable();
            item.SetupGet(x => x.Complete).Returns(true);

            var result = new OutlookItemFlaggableTry(item.Object).Complete;

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Complete_WhenUnderlyingGetterThrowsSystemException_ShouldReturnFalse()
        {
            var item = CreateBaseFlaggable();
            item.SetupGet(x => x.Complete).Throws(new InvalidOperationException("boom"));

            var result = new OutlookItemFlaggableTry(item.Object).Complete;

            result.Should().BeFalse();
        }

        [TestMethod]
        public void CompleteSetter_WhenUnderlyingSetterSucceeds_ShouldForwardValue()
        {
            var item = CreateBaseFlaggable();
            bool assigned = false;
            item.SetupSet(x => x.Complete = It.IsAny<bool>())
                .Callback<bool>(value => assigned = value);
            var wrapper = new OutlookItemFlaggableTry(item.Object);

            wrapper.Complete = true;

            assigned.Should().BeTrue();
        }

        [TestMethod]
        public void DueDate_WhenUnderlyingGetterSucceeds_ShouldReturnValue()
        {
            var expected = new DateTime(2026, 4, 20);
            var item = CreateBaseFlaggable();
            item.SetupGet(x => x.DueDate).Returns(expected);

            var result = new OutlookItemFlaggableTry(item.Object).DueDate;

            result.Should().Be(expected);
        }

        [TestMethod]
        public void DueDateSetter_WhenUnderlyingSetterThrowsSystemException_ShouldNotThrow()
        {
            var item = CreateBaseFlaggable();
            item.SetupSet(x => x.DueDate = It.IsAny<DateTime>())
                .Throws(new InvalidOperationException("boom"));
            var wrapper = new OutlookItemFlaggableTry(item.Object);

            System.Action act = () => wrapper.DueDate = new DateTime(2026, 5, 1);

            act.Should().NotThrow();
        }

        [TestMethod]
        public void FlagAsTaskSetter_WhenUnderlyingSetterSucceeds_ShouldForwardValue()
        {
            var item = CreateBaseFlaggable();
            bool assigned = false;
            item.SetupSet(x => x.FlagAsTask = It.IsAny<bool>())
                .Callback<bool>(value => assigned = value);
            var wrapper = new OutlookItemFlaggableTry(item.Object);

            wrapper.FlagAsTask = true;

            assigned.Should().BeTrue();
        }

        [TestMethod]
        public void FlagAsTask_WhenUnderlyingGetterSucceeds_ShouldReturnValue()
        {
            var item = CreateBaseFlaggable();
            item.SetupGet(x => x.FlagAsTask).Returns(true);

            var result = new OutlookItemFlaggableTry(item.Object).FlagAsTask;

            result.Should().BeTrue();
        }

        [TestMethod]
        public void TaskSubject_WhenUnderlyingGetterThrowsSystemException_ShouldReturnNull()
        {
            var item = CreateBaseFlaggable();
            item.SetupGet(x => x.TaskSubject).Throws(new InvalidOperationException("boom"));

            var result = new OutlookItemFlaggableTry(item.Object).TaskSubject;

            result.Should().BeNull();
        }

        [TestMethod]
        public void TaskSubjectSetter_WhenUnderlyingSetterSucceeds_ShouldForwardValue()
        {
            var item = CreateBaseFlaggable();
            string assigned = null;
            item.SetupSet(x => x.TaskSubject = It.IsAny<string>())
                .Callback<string>(value => assigned = value);
            var wrapper = new OutlookItemFlaggableTry(item.Object);

            wrapper.TaskSubject = "Follow up";

            assigned.Should().Be("Follow up");
        }

        [TestMethod]
        public void TotalWork_WhenUnderlyingGetterSucceeds_ShouldReturnValue()
        {
            var item = CreateBaseFlaggable();
            item.SetupGet(x => x.TotalWork).Returns(90);

            var result = new OutlookItemFlaggableTry(item.Object).TotalWork;

            result.Should().Be(90);
        }

        [TestMethod]
        public void GetUdfString_WhenUnderlyingCallThrowsSystemException_ShouldReturnNull()
        {
            var item = CreateBaseFlaggable();
            var userProperties = new Mock<Microsoft.Office.Interop.Outlook.UserProperties>();
            userProperties
                .Setup(x => x.Find("Priority", true))
                .Throws(new InvalidOperationException("boom"));
            item.SetupGet(x => x.UserProperties).Returns(userProperties.Object);

            var result = new OutlookItemFlaggableTry(item.Object).GetUdfString("Priority");

            result.Should().BeNull();
        }

        [TestMethod]
        public void TaskStartDate_WhenUnderlyingGetterSucceeds_ShouldReturnValue()
        {
            var expected = new DateTime(2026, 4, 1);
            var item = CreateBaseFlaggable();
            item.SetupGet(x => x.TaskStartDate).Returns(expected);

            var result = new OutlookItemFlaggableTry(item.Object).TaskStartDate;

            result.Should().Be(expected);
        }

        [TestMethod]
        public void TaskStartDateSetter_WhenUnderlyingSetterThrows_ShouldNotThrow()
        {
            var item = CreateBaseFlaggable();
            item.SetupSet(x => x.TaskStartDate = It.IsAny<DateTime>())
                .Throws(new InvalidOperationException("boom"));
            var wrapper = new OutlookItemFlaggableTry(item.Object);

            System.Action act = () => wrapper.TaskStartDate = new DateTime(2026, 5, 1);

            act.Should().NotThrow();
        }

        [TestMethod]
        public void TotalWorkSetter_WhenUnderlyingSetterThrows_ShouldNotThrow()
        {
            var item = CreateBaseFlaggable();
            item.SetupSet(x => x.TotalWork = It.IsAny<int>())
                .Throws(new InvalidOperationException("boom"));
            var wrapper = new OutlookItemFlaggableTry(item.Object);

            System.Action act = () => wrapper.TotalWork = 42;

            act.Should().NotThrow();
        }

        [TestMethod]
        public void IOutlookItem_TaskStartDate_WhenGetterSucceeds_ShouldReturnValue()
        {
            var expected = new DateTime(2026, 6, 15);
            var item = CreateBaseFlaggable();
            item.SetupGet(x => x.TaskStartDate).Returns(expected);

            IOutlookItem wrapper = new OutlookItemFlaggableTry(item.Object);

            wrapper.TaskStartDate.Should().Be(expected);
        }

        private static Mock<IOutlookItemFlaggable> CreateBaseFlaggable()
        {
            var item = new Mock<IOutlookItemFlaggable>();
            item.SetupGet(x => x.InnerObject).Returns(new object());
            item.SetupGet(x => x.ItemType).Returns(typeof(object));
            item.SetupGet(x => x.Args).Returns(Array.Empty<object>());
            return item;
        }
    }
}
