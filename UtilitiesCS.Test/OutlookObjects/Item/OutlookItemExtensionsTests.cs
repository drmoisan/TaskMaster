using System;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookExtensions;
using InteropMailItem = Microsoft.Office.Interop.Outlook.MailItem;
using InteropMeetingItem = Microsoft.Office.Interop.Outlook.MeetingItem;
using InteropTaskItem = Microsoft.Office.Interop.Outlook.TaskItem;

namespace UtilitiesCS.Test.OutlookObjects.Item
{
    [TestClass]
    public class OutlookItemExtensionsTests
    {
        [TestMethod]
        public void Try_WhenWrappingOutlookItem_ShouldReturnSafeWrapper()
        {
            var innerItem = new Mock<InteropMailItem>().Object;
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            var result = outlookItem.Try();

            result.Should().BeOfType<OutlookItemTry>();
            result.InnerObject.Should().BeSameAs(innerItem);
        }

        [TestMethod]
        public void Try_WhenWrappingFlaggableInterface_ShouldReturnFlaggableSafeWrapper()
        {
            var flaggable = new Mock<IOutlookItemFlaggable>();
            flaggable.SetupGet(x => x.InnerObject).Returns(new Mock<InteropTaskItem>().Object);
            flaggable.SetupGet(x => x.ItemType).Returns(typeof(InteropTaskItem));
            flaggable.SetupGet(x => x.Args).Returns(Array.Empty<object>());
            flaggable.SetupGet(x => x.Complete).Returns(true);

            var result = flaggable.Object.Try();

            result.Should().BeOfType<OutlookItemFlaggableTry>();
            result.Complete.Should().BeTrue();
        }

        [TestMethod]
        public void TryGet_WhenWrappingOutlookItem_ShouldReturnTryGetWrapper()
        {
            var outlookItem = new UtilitiesCS.OutlookItem(
                new SubjectOnlyItem { Subject = "Quarterly review" }
            );

            var success = outlookItem.TryGet().Subject(out string subject);

            success.Should().BeTrue();
            subject.Should().Be("Quarterly review");
        }

        [TestMethod]
        public void IsValid_WhenItemIsNull_ShouldReturnFalse()
        {
            UtilitiesCS.OutlookItem outlookItem = null;

            var result = outlookItem.IsValid();

            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsValid_WhenInnerObjectIsNull_ShouldReturnFalse()
        {
            var result = new UtilitiesCS.OutlookItem(null).IsValid();

            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsValid_WhenInnerObjectIsSupported_ShouldReturnTrue()
        {
            var outlookItem = new UtilitiesCS.OutlookItem(new Mock<InteropMailItem>().Object);

            var result = outlookItem.IsValid();

            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsValid_WhenInnerObjectIsUnsupported_ShouldReturnFalse()
        {
            var outlookItem = new UtilitiesCS.OutlookItem(new object());

            var result = outlookItem.IsValid();

            result.Should().BeFalse();
        }

        [DataTestMethod]
        [DataRow(typeof(InteropMailItem), OlItemType.olMailItem)]
        [DataRow(typeof(InteropTaskItem), OlItemType.olTaskItem)]
        [DataRow(typeof(InteropMeetingItem), OlItemType.olAppointmentItem)]
        public void GetOlItemType_WhenInnerObjectIsSupported_ShouldReturnMappedValue(
            Type outlookType,
            OlItemType expected
        )
        {
            var innerObject = CreateInteropMock(outlookType);
            var outlookItem = new Mock<IOutlookItem>();
            outlookItem.SetupGet(x => x.InnerObject).Returns(innerObject);

            var result = outlookItem.Object.GetOlItemType();

            result.Should().Be(expected);
        }

        [TestMethod]
        public void GetOlItemType_WhenInnerObjectIsUnsupported_ShouldThrowArgumentException()
        {
            var outlookItem = new Mock<IOutlookItem>();
            outlookItem.SetupGet(x => x.InnerObject).Returns(new object());

            System.Action act = () => outlookItem.Object.GetOlItemType();

            act.Should().Throw<ArgumentException>().WithMessage("Object is not a supported type*");
        }

        private static object CreateInteropMock(Type outlookType)
        {
            if (outlookType == typeof(InteropMailItem))
            {
                return new Mock<InteropMailItem>().Object;
            }

            if (outlookType == typeof(InteropTaskItem))
            {
                return new Mock<InteropTaskItem>().Object;
            }

            if (outlookType == typeof(InteropMeetingItem))
            {
                return new Mock<InteropMeetingItem>().Object;
            }

            throw new ArgumentOutOfRangeException(nameof(outlookType));
        }

        private sealed class SubjectOnlyItem
        {
            public string Subject { get; set; }
        }

        #region Extended Tests — P2-T13

        [TestMethod]
        public void TryGet_ShouldReturnValueWhenCallSucceeds()
        {
            string result = OutlookItemExtensions.TryGet(() => "hello");
            result.Should().Be("hello");
        }

        [TestMethod]
        public void TryGet_ShouldReturnDefault_WhenCallThrowsSystemException()
        {
            string result = OutlookItemExtensions.TryGet<string>(() =>
                throw new InvalidOperationException()
            );
            result.Should().BeNull();
        }

        [TestMethod]
        public void TrySet_ShouldNotThrow_WhenSetterThrowsSystemException()
        {
            System.Action act = () =>
                OutlookItemExtensions.TrySet<string>(
                    _ => throw new InvalidOperationException(),
                    "value"
                );

            act.Should().NotThrow();
        }

        [TestMethod]
        public void TryCall_Action_ShouldNotThrow_WhenActionThrowsSystemException()
        {
            System.Action act = () =>
                OutlookItemExtensions.TryCall(() => throw new InvalidOperationException());

            act.Should().NotThrow();
        }

        [TestMethod]
        public void TryCall_Func_ShouldReturnDefault_WhenFuncThrowsSystemException()
        {
            var result = OutlookItemExtensions.TryCall<string>(() =>
                throw new InvalidOperationException()
            );

            result.Should().BeNull();
        }

        [TestMethod]
        public void TryCall_Func_ShouldReturnValue_WhenFuncSucceeds()
        {
            var result = OutlookItemExtensions.TryCall(() => 42);

            result.Should().Be(42);
        }

        [DataTestMethod]
        [DataRow(typeof(AppointmentItem), OlItemType.olAppointmentItem)]
        [DataRow(typeof(ContactItem), OlItemType.olContactItem)]
        [DataRow(typeof(JournalItem), OlItemType.olJournalItem)]
        [DataRow(typeof(NoteItem), OlItemType.olNoteItem)]
        [DataRow(typeof(PostItem), OlItemType.olPostItem)]
        public void GetOlItemType_WhenInnerObjectIsAdditionalSupportedType_ShouldReturnMappedValue(
            Type outlookType,
            OlItemType expected
        )
        {
            var innerObject = CreateInteropMockExtended(outlookType);
            var outlookItem = new Mock<IOutlookItem>();
            outlookItem.SetupGet(x => x.InnerObject).Returns(innerObject);

            var result = outlookItem.Object.GetOlItemType();

            result.Should().Be(expected);
        }

        [TestMethod]
        public void IsValid_WhenInnerObjectIsAppointment_ShouldReturnTrue()
        {
            var outlookItem = new UtilitiesCS.OutlookItem(new Mock<AppointmentItem>().Object);
            outlookItem.IsValid().Should().BeTrue();
        }

        [TestMethod]
        public void IsValid_WhenInnerObjectIsTask_ShouldReturnTrue()
        {
            var outlookItem = new UtilitiesCS.OutlookItem(new Mock<InteropTaskItem>().Object);
            outlookItem.IsValid().Should().BeTrue();
        }

        [TestMethod]
        public void IsValid_WhenInnerObjectIsContact_ShouldReturnTrue()
        {
            var outlookItem = new UtilitiesCS.OutlookItem(new Mock<ContactItem>().Object);
            outlookItem.IsValid().Should().BeTrue();
        }

        [TestMethod]
        public void IsValid_WhenInnerObjectIsNote_ShouldReturnTrue()
        {
            var outlookItem = new UtilitiesCS.OutlookItem(new Mock<NoteItem>().Object);
            outlookItem.IsValid().Should().BeTrue();
        }

        [TestMethod]
        public void IsValid_WhenInnerObjectIsPost_ShouldReturnTrue()
        {
            var outlookItem = new UtilitiesCS.OutlookItem(new Mock<PostItem>().Object);
            outlookItem.IsValid().Should().BeTrue();
        }

        private static object CreateInteropMockExtended(Type outlookType)
        {
            if (outlookType == typeof(AppointmentItem))
                return new Mock<AppointmentItem>().Object;
            if (outlookType == typeof(ContactItem))
                return new Mock<ContactItem>().Object;
            if (outlookType == typeof(JournalItem))
                return new Mock<JournalItem>().Object;
            if (outlookType == typeof(NoteItem))
                return new Mock<NoteItem>().Object;
            if (outlookType == typeof(PostItem))
                return new Mock<PostItem>().Object;
            throw new ArgumentOutOfRangeException(nameof(outlookType));
        }

        #endregion
    }
}
