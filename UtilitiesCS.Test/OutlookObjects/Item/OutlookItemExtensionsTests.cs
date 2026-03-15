using System;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookExtensions;

namespace UtilitiesCS.Test.OutlookObjects.Item
{
    [TestClass]
    public class OutlookItemExtensionsTests
    {
        [TestMethod]
        public void Try_WhenWrappingOutlookItem_ShouldReturnSafeWrapper()
        {
            var innerItem = new Mock<MailItem>().Object;
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            var result = outlookItem.Try();

            result.Should().BeOfType<OutlookItemTry>();
            result.InnerObject.Should().BeSameAs(innerItem);
        }

        [TestMethod]
        public void Try_WhenWrappingFlaggableInterface_ShouldReturnFlaggableSafeWrapper()
        {
            var flaggable = new Mock<IOutlookItemFlaggable>();
            flaggable.SetupGet(x => x.InnerObject).Returns(new Mock<TaskItem>().Object);
            flaggable.SetupGet(x => x.ItemType).Returns(typeof(TaskItem));
            flaggable.SetupGet(x => x.Args).Returns(Array.Empty<object>());
            flaggable.SetupGet(x => x.Complete).Returns(true);

            var result = flaggable.Object.Try();

            result.Should().BeOfType<OutlookItemFlaggableTry>();
            result.Complete.Should().BeTrue();
        }

        [TestMethod]
        public void TryGet_WhenWrappingOutlookItem_ShouldReturnTryGetWrapper()
        {
            var outlookItem = new UtilitiesCS.OutlookItem(new SubjectOnlyItem { Subject = "Quarterly review" });

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
            var outlookItem = new UtilitiesCS.OutlookItem(new Mock<MailItem>().Object);

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
        [DataRow(typeof(MailItem), OlItemType.olMailItem)]
        [DataRow(typeof(TaskItem), OlItemType.olTaskItem)]
        [DataRow(typeof(MeetingItem), OlItemType.olAppointmentItem)]
        public void GetOlItemType_WhenInnerObjectIsSupported_ShouldReturnMappedValue(Type outlookType, OlItemType expected)
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

            act.Should().Throw<ArgumentException>()
                .WithMessage("Object is not a supported type*");
        }

        private static object CreateInteropMock(Type outlookType)
        {
            if (outlookType == typeof(MailItem))
            {
                return new Mock<MailItem>().Object;
            }

            if (outlookType == typeof(TaskItem))
            {
                return new Mock<TaskItem>().Object;
            }

            if (outlookType == typeof(MeetingItem))
            {
                return new Mock<MeetingItem>().Object;
            }

            throw new ArgumentOutOfRangeException(nameof(outlookType));
        }

        private sealed class SubjectOnlyItem
        {
            public string Subject { get; set; }
        }
    }
}