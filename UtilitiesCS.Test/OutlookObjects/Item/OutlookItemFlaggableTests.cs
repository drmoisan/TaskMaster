using System;
using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookExtensions;
using InteropMailItem = Microsoft.Office.Interop.Outlook.MailItem;

namespace UtilitiesCS.Test.OutlookObjects.Item
{
    [TestClass]
    public class OutlookItemFlaggableTests
    {
        private const string TotalWorkSchema =
            "http://schemas.microsoft.com/mapi/id/{00062003-0000-0000-C000-000000000046}/81110003";

        [TestMethod]
        public void Constructor_WithIOutlookItem_ShouldWrapExistingMetadata()
        {
            var innerItem = new Mock<InteropMailItem>().Object;
            var outlookItem = new Mock<IOutlookItem>();
            outlookItem.SetupGet(x => x.InnerObject).Returns(innerItem);
            outlookItem.SetupGet(x => x.Args).Returns(Array.Empty<object>());

            var result = new OutlookItemFlaggable(outlookItem.Object);

            result.InnerObject.Should().BeSameAs(innerItem);
            result.ItemType.Should().Be(innerItem.GetType());
        }

        [TestMethod]
        public void Complete_WhenWrappingTaskItem_ShouldReadCompleteProperty()
        {
            var taskItem = new Mock<TaskItem>();
            taskItem.SetupGet(x => x.Complete).Returns(true);

            var result = new OutlookItemFlaggable(taskItem.Object).Complete;

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Complete_WhenWrappingMailItem_ShouldReadFlagStatus()
        {
            var mailItem = new Mock<InteropMailItem>();
            mailItem.SetupGet(x => x.FlagStatus).Returns(OlFlagStatus.olFlagComplete);

            var result = new OutlookItemFlaggable(mailItem.Object).Complete;

            result.Should().BeTrue();
        }

        [TestMethod]
        public void CompleteSetter_WhenReflectionSetterCannotBeBound_ShouldThrowMissingMethodException()
        {
            var mailItem = new ReflectionFriendlyMailItem { FlagStatus = OlFlagStatus.olNoFlag };
            var wrapper = CreateWrapper(mailItem, OlItemType.olMailItem);

            System.Action act = () => wrapper.Complete = true;

            act.Should().Throw<MissingMethodException>();
            mailItem.FlagStatus.Should().Be(OlFlagStatus.olNoFlag);
        }

        [TestMethod]
        public void DueDate_WhenWrappingTaskItem_ShouldReadDueDate()
        {
            var expected = new DateTime(2026, 4, 2, 9, 15, 0);
            var taskItem = new Mock<TaskItem>();
            taskItem.SetupGet(x => x.DueDate).Returns(expected);

            var result = new OutlookItemFlaggable(taskItem.Object).DueDate;

            result.Should().Be(expected);
        }

        [TestMethod]
        public void CompleteSetter_WhenWrappingTaskLikeItem_ShouldThrowMissingMethodException()
        {
            var taskItem = new ReflectionFriendlyTaskItem { Complete = false };
            var wrapper = CreateWrapper(taskItem, OlItemType.olTaskItem);

            System.Action act = () => wrapper.Complete = true;

            act.Should().Throw<MissingMethodException>();
            taskItem.Complete.Should().BeFalse();
        }

        [TestMethod]
        public void DueDateSetter_WhenWrappingTaskLikeItem_ShouldThrowMissingMethodException()
        {
            var original = new DateTime(2026, 4, 2, 9, 15, 0);
            var taskItem = new ReflectionFriendlyTaskItem { DueDate = original };
            var wrapper = CreateWrapper(taskItem, OlItemType.olTaskItem);
            var expected = new DateTime(2026, 4, 4, 10, 0, 0);

            System.Action act = () => wrapper.DueDate = expected;

            act.Should().Throw<MissingMethodException>();
            taskItem.DueDate.Should().Be(original);
        }

        [TestMethod]
        public void DueDate_WhenMailPropertiesAreMissing_ShouldReturnDefaultDate()
        {
            var wrapper = CreateWrapper(new ReflectionFriendlyMailItem(), OlItemType.olMailItem);

            var result = wrapper.DueDate;

            result.Should().Be(default(DateTime));
        }

        [TestMethod]
        public void Complete_WhenWrappedMailLikeObjectHasNoFlagStatus_ShouldThrowArgumentException()
        {
            var wrapper = CreateWrapper(new object(), OlItemType.olMailItem);

            System.Action act = () => _ = wrapper.Complete;

            act.Should().Throw<ArgumentException>().WithMessage("*Complete*");
        }

        [TestMethod]
        public void DueDate_WhenWrappedTaskLikeObjectHasNoDueDate_ShouldThrowArgumentException()
        {
            var wrapper = CreateWrapper(new object(), OlItemType.olTaskItem);

            System.Action act = () => _ = wrapper.DueDate;

            act.Should().Throw<ArgumentException>().WithMessage("*DueDate*");
        }

        [TestMethod]
        public void FlagAsTask_WhenWrappingTaskItem_ShouldAlwaysBeTrue()
        {
            var taskItem = new Mock<TaskItem>();

            var result = new OutlookItemFlaggable(taskItem.Object).FlagAsTask;

            result.Should().BeTrue();
        }

        [TestMethod]
        public void FlagAsTaskSetter_WhenEnableCallCannotBeBound_ShouldLeaveStateUnchanged()
        {
            var mailItem = new ReflectionFriendlyMailItem { FlagStatus = OlFlagStatus.olNoFlag };
            var wrapper = CreateWrapper(mailItem, OlItemType.olMailItem);

            wrapper.FlagAsTask = true;

            mailItem.MarkAsTaskCalls.Should().Be(0);
            mailItem.FlagStatus.Should().Be(OlFlagStatus.olNoFlag);
        }

        [TestMethod]
        public void FlagAsTaskSetter_WhenDisableCallCannotBeBound_ShouldThrowArgumentOutOfRangeException()
        {
            var mailItem = new ReflectionFriendlyMailItem
            {
                FlagStatus = OlFlagStatus.olFlagMarked,
            };
            var wrapper = CreateWrapper(mailItem, OlItemType.olMailItem);

            System.Action act = () => wrapper.FlagAsTask = false;

            act.Should().Throw<ArgumentOutOfRangeException>();
            mailItem.ClearTaskFlagCalls.Should().Be(0);
            mailItem.FlagStatus.Should().Be(OlFlagStatus.olFlagMarked);
        }

        [TestMethod]
        public void TaskSubject_WhenWrappingTaskItem_ShouldProxySubject()
        {
            var taskItem = new Mock<TaskItem>();
            taskItem.SetupProperty(x => x.Subject, "Follow up");
            var wrapper = new OutlookItemFlaggable(taskItem.Object);

            wrapper.TaskSubject.Should().Be("Follow up");
            wrapper.TaskSubject = "Updated follow up";

            taskItem.Object.Subject.Should().Be("Updated follow up");
        }

        [TestMethod]
        public void TotalWork_WhenWrappingMailItem_ShouldReadFromPropertyAccessor()
        {
            var accessor = new Mock<PropertyAccessor>();
            accessor.Setup(x => x.GetProperty(TotalWorkSchema)).Returns(42);
            var mailItem = new Mock<InteropMailItem>();
            mailItem.SetupGet(x => x.PropertyAccessor).Returns(accessor.Object);

            var result = new OutlookItemFlaggable(mailItem.Object).TotalWork;

            result.Should().Be(42);
        }

        [TestMethod]
        public void TotalWorkSetter_WhenTrySetFails_ShouldRetryWithDirectSetProperty()
        {
            var accessor = new Mock<PropertyAccessor>();
            accessor
                .SetupSequence(x => x.SetProperty(TotalWorkSchema, 90))
                .Throws(new InvalidOperationException("Transient failure"))
                .Pass();
            var mailItem = new Mock<InteropMailItem>();
            mailItem.SetupGet(x => x.PropertyAccessor).Returns(accessor.Object);
            mailItem.SetupGet(x => x.FlagStatus).Returns(OlFlagStatus.olNoFlag);
            var wrapper = new OutlookItemFlaggable(mailItem.Object);

            wrapper.TotalWork = 90;

            accessor.Verify(x => x.SetProperty(TotalWorkSchema, 90), Times.Exactly(2));
        }

        [TestMethod]
        public void TaskStartDate_WhenWrappingTaskItem_ShouldReadStartDate()
        {
            var expected = new DateTime(2026, 4, 1, 8, 0, 0);
            var taskItem = new Mock<TaskItem>();
            taskItem.SetupGet(x => x.StartDate).Returns(expected);

            var result = new OutlookItemFlaggable(taskItem.Object).TaskStartDate;

            result.Should().Be(expected);
        }

        [TestMethod]
        public void FlagAsTask_WhenMailFlagStatusIsUnavailable_ShouldReturnFalse()
        {
            var wrapper = CreateWrapper(new ReflectionFriendlyMailItem(), OlItemType.olMailItem);

            var result = wrapper.FlagAsTask;

            result.Should().BeFalse();
        }

        [TestMethod]
        public void FlagAsTask_WhenWrappedMailLikeObjectHasNoFlagStatus_ShouldThrowArgumentException()
        {
            var wrapper = CreateWrapper(new object(), OlItemType.olMailItem);

            System.Action act = () => _ = wrapper.FlagAsTask;

            act.Should().Throw<ArgumentException>().WithMessage("*FlagAsTask*");
        }

        [TestMethod]
        public void TaskStartDateSetter_WhenWrappingMailLikeItem_ShouldThrowMissingMethodException()
        {
            var original = new DateTime(2026, 4, 1, 8, 0, 0);
            var mailItem = new ReflectionFriendlyMailItem
            {
                TaskStartDate = original,
                CreationTime = new DateTime(2026, 3, 31, 16, 0, 0),
            };
            var wrapper = CreateWrapper(mailItem, OlItemType.olMailItem);
            var expected = new DateTime(2026, 4, 5, 12, 30, 0);

            System.Action act = () => wrapper.TaskStartDate = expected;

            act.Should().Throw<MissingMethodException>();
            mailItem.TaskStartDate.Should().Be(original);
        }

        [TestMethod]
        public void TaskStartDate_WhenWrappedTaskLikeObjectHasNoDates_ShouldThrowArgumentException()
        {
            var wrapper = CreateWrapper(new object(), OlItemType.olTaskItem);

            System.Action act = () => _ = wrapper.TaskStartDate;

            act.Should().Throw<ArgumentException>().WithMessage("*TaskStartDate*");
        }

        [TestMethod]
        public void TaskSubjectSetter_WhenWrappingMailLikeItem_ShouldThrowMissingMethodException()
        {
            var mailItem = new ReflectionFriendlyMailItem
            {
                TaskSubject = "Original subject",
                Subject = "Original subject",
            };
            var wrapper = CreateWrapper(mailItem, OlItemType.olMailItem);

            System.Action act = () => wrapper.TaskSubject = "Updated subject";

            act.Should().Throw<MissingMethodException>();
            mailItem.TaskSubject.Should().Be("Original subject");
        }

        [TestMethod]
        public void TaskSubject_WhenWrappedMailLikeObjectHasNoSubject_ShouldThrowArgumentException()
        {
            var wrapper = CreateWrapper(new object(), OlItemType.olMailItem);

            System.Action act = () => _ = wrapper.TaskSubject;

            act.Should().Throw<ArgumentException>().WithMessage("*TaskSubject*");
        }

        [TestMethod]
        public void TotalWork_WhenWrappingTaskLikeItem_ShouldIgnoreSetterFailure()
        {
            var taskItem = new ReflectionFriendlyTaskItem { TotalWork = 30 };
            var wrapper = CreateWrapper(taskItem, OlItemType.olTaskItem);

            wrapper.TotalWork.Should().Be(30);

            wrapper.TotalWork = 45;

            taskItem.TotalWork.Should().Be(30);
        }

        private static OutlookItemFlaggable CreateWrapper(object innerItem, OlItemType itemType)
        {
#pragma warning disable SYSLIB0050
            var wrapper = (OutlookItemFlaggable)
                FormatterServices.GetUninitializedObject(typeof(OutlookItemFlaggable));
#pragma warning restore SYSLIB0050

            SetField(typeof(UtilitiesCS.OutlookItem), wrapper, "_item", innerItem);
            SetField(typeof(UtilitiesCS.OutlookItem), wrapper, "_type", innerItem.GetType());
            SetField(typeof(UtilitiesCS.OutlookItem), wrapper, "_args", Array.Empty<object>());
            SetField(typeof(OutlookItemFlaggable), wrapper, "_olType", itemType);
            return wrapper;
        }

        private static void SetField(
            Type declaringType,
            object instance,
            string fieldName,
            object value
        )
        {
            var field =
                declaringType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new MissingFieldException(declaringType.FullName, fieldName);
            field.SetValue(instance, value);
        }

        public sealed class ReflectionFriendlyMailItem
        {
            public OlFlagStatus FlagStatus { get; set; }

            public DateTime DueDate { get; set; }

            public DateTime TaskDueDate { get; set; }

            public DateTime TaskStartDate { get; set; }

            public DateTime CreationTime { get; set; }

            public string TaskSubject { get; set; }

            public string Subject { get; set; }

            public int MarkAsTaskCalls { get; private set; }

            public int ClearTaskFlagCalls { get; private set; }

            public object ClearTaskFlag()
            {
                ClearTaskFlagCalls++;
                FlagStatus = OlFlagStatus.olNoFlag;
                return new object();
            }

            public void MarkAsTask(OlMarkInterval interval)
            {
                MarkAsTaskCalls++;
                FlagStatus = OlFlagStatus.olFlagMarked;
            }
        }

        public sealed class ReflectionFriendlyTaskItem
        {
            public bool Complete { get; set; }

            public DateTime DueDate { get; set; }

            public int TotalWork { get; set; }
        }
    }
}
