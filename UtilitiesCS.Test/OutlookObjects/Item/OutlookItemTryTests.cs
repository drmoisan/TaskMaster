using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookExtensions;

namespace UtilitiesCS.Test.OutlookObjects.Item
{
    [TestClass]
    public class OutlookItemTryTests
    {
        [TestMethod]
        public void MetadataProperties_ShouldReturnWrappedValues()
        {
            // Arrange
            var innerObject = new object();
            var args = new object[] { "arg" };
            var outlookItem = new Mock<IOutlookItem>();
            outlookItem.SetupGet(x => x.InnerObject).Returns(innerObject);
            outlookItem.SetupGet(x => x.ItemType).Returns(typeof(string));
            outlookItem.SetupGet(x => x.Args).Returns(args);
            var wrapper = new OutlookItemTry(outlookItem.Object);

            // Act / Assert
            wrapper.InnerObject.Should().BeSameAs(innerObject);
            wrapper.ItemType.Should().Be(typeof(string));
            wrapper.Args.Should().BeSameAs(args);
        }

        [TestMethod]
        public void Body_WhenUnderlyingGetterSucceeds_ShouldReturnValue()
        {
            // Arrange
            var outlookItem = CreateBaseOutlookItem();
            outlookItem.SetupGet(x => x.Body).Returns("Wrapped body");
            var wrapper = new OutlookItemTry(outlookItem.Object);

            // Act
            var result = wrapper.Body;

            // Assert
            result.Should().Be("Wrapped body");
        }

        [TestMethod]
        public void Body_WhenUnderlyingGetterThrowsSystemException_ShouldReturnNull()
        {
            // Arrange
            var outlookItem = CreateBaseOutlookItem();
            outlookItem.SetupGet(x => x.Body).Throws(new InvalidOperationException("boom"));
            var wrapper = new OutlookItemTry(outlookItem.Object);

            // Act
            var result = wrapper.Body;

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void BodySetter_WhenUnderlyingSetterThrowsSystemException_ShouldNotThrow()
        {
            // Arrange
            var outlookItem = CreateBaseOutlookItem();
            outlookItem
                .SetupSet(x => x.Body = It.IsAny<string>())
                .Throws(new InvalidOperationException("boom"));
            var wrapper = new OutlookItemTry(outlookItem.Object);

            // Act
            System.Action action = () => wrapper.Body = "Updated body";

            // Assert
            action.Should().NotThrow();
        }

        [TestMethod]
        public void Copy_WhenUnderlyingCallThrowsSystemException_ShouldReturnNull()
        {
            // Arrange
            var outlookItem = CreateBaseOutlookItem();
            outlookItem.Setup(x => x.Copy()).Throws(new InvalidOperationException("boom"));
            var wrapper = new OutlookItemTry(outlookItem.Object);

            // Act
            var result = wrapper.Copy();

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void Save_WhenUnderlyingCallThrowsSystemException_ShouldNotThrow()
        {
            // Arrange
            var outlookItem = CreateBaseOutlookItem();
            outlookItem.Setup(x => x.Save()).Throws(new InvalidOperationException("boom"));
            var wrapper = new OutlookItemTry(outlookItem.Object);

            // Act
            System.Action action = wrapper.Save;

            // Assert
            action.Should().NotThrow();
        }

        [TestMethod]
        public void GetPropertyValue_WhenUnderlyingCallThrowsSystemException_ShouldReturnDefaultValue()
        {
            // Arrange
            var outlookItem = CreateBaseOutlookItem();
            outlookItem
                .Setup(x => x.GetPropertyValue<string>("Subject"))
                .Throws(new InvalidOperationException("boom"));
            var wrapper = new OutlookItemTry(outlookItem.Object);

            // Act
            var result = wrapper.GetPropertyValue<string>("Subject");

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void SubjectSetter_WhenUnderlyingSetterSucceeds_ShouldForwardValue()
        {
            // Arrange
            string assignedValue = null;
            var outlookItem = CreateBaseOutlookItem();
            outlookItem
                .SetupSet(x => x.Subject = It.IsAny<string>())
                .Callback<string>(value => assignedValue = value);
            var wrapper = new OutlookItemTry(outlookItem.Object);

            // Act
            wrapper.Subject = "Forwarded subject";

            // Assert
            assignedValue.Should().Be("Forwarded subject");
        }

        [TestMethod]
        public void Categories_WhenUnderlyingGetterSucceeds_ShouldReturnValue()
        {
            var outlookItem = CreateBaseOutlookItem();
            outlookItem.SetupGet(x => x.Categories).Returns("cat1; cat2");
            var wrapper = new OutlookItemTry(outlookItem.Object);

            wrapper.Categories.Should().Be("cat1; cat2");
        }

        [TestMethod]
        public void CategoriesSetter_WhenUnderlyingSetterSucceeds_ShouldForwardValue()
        {
            string assigned = null;
            var outlookItem = CreateBaseOutlookItem();
            outlookItem
                .SetupSet(x => x.Categories = It.IsAny<string>())
                .Callback<string>(v => assigned = v);
            var wrapper = new OutlookItemTry(outlookItem.Object);

            wrapper.Categories = "NewCat";

            assigned.Should().Be("NewCat");
        }

        [TestMethod]
        public void UnRead_WhenUnderlyingGetterSucceeds_ShouldReturnValue()
        {
            var outlookItem = CreateBaseOutlookItem();
            outlookItem.SetupGet(x => x.UnRead).Returns(true);
            var wrapper = new OutlookItemTry(outlookItem.Object);

            wrapper.UnRead.Should().BeTrue();
        }

        [TestMethod]
        public void UnRead_WhenUnderlyingGetterThrowsSystemException_ShouldReturnFalse()
        {
            var outlookItem = CreateBaseOutlookItem();
            outlookItem.SetupGet(x => x.UnRead).Throws(new InvalidOperationException("boom"));
            var wrapper = new OutlookItemTry(outlookItem.Object);

            wrapper.UnRead.Should().BeFalse();
        }

        [TestMethod]
        public void Size_WhenUnderlyingGetterSucceeds_ShouldReturnValue()
        {
            var outlookItem = CreateBaseOutlookItem();
            outlookItem.SetupGet(x => x.Size).Returns(42);
            var wrapper = new OutlookItemTry(outlookItem.Object);

            wrapper.Size.Should().Be(42);
        }

        [TestMethod]
        public void EntryID_WhenUnderlyingGetterSucceeds_ShouldReturnValue()
        {
            var outlookItem = CreateBaseOutlookItem();
            outlookItem.SetupGet(x => x.EntryID).Returns("ABCD1234");
            var wrapper = new OutlookItemTry(outlookItem.Object);

            wrapper.EntryID.Should().Be("ABCD1234");
        }

        [TestMethod]
        public void ConversationTopic_WhenUnderlyingGetterSucceeds_ShouldReturnValue()
        {
            var outlookItem = CreateBaseOutlookItem();
            outlookItem.SetupGet(x => x.ConversationTopic).Returns("RE: Meeting");
            var wrapper = new OutlookItemTry(outlookItem.Object);

            wrapper.ConversationTopic.Should().Be("RE: Meeting");
        }

        [TestMethod]
        public void MessageClass_WhenUnderlyingGetterSucceeds_ShouldReturnValue()
        {
            var outlookItem = CreateBaseOutlookItem();
            outlookItem.SetupGet(x => x.MessageClass).Returns("IPM.Note");
            var wrapper = new OutlookItemTry(outlookItem.Object);

            wrapper.MessageClass.Should().Be("IPM.Note");
        }

        [TestMethod]
        public void Saved_WhenUnderlyingGetterSucceeds_ShouldReturnValue()
        {
            var outlookItem = CreateBaseOutlookItem();
            outlookItem.SetupGet(x => x.Saved).Returns(true);
            var wrapper = new OutlookItemTry(outlookItem.Object);

            wrapper.Saved.Should().BeTrue();
        }

        [TestMethod]
        public void Close_WhenUnderlyingCallSucceeds_ShouldNotThrow()
        {
            var outlookItem = CreateBaseOutlookItem();
            var wrapper = new OutlookItemTry(outlookItem.Object);

            System.Action action = () =>
                wrapper.Close(Microsoft.Office.Interop.Outlook.OlInspectorClose.olDiscard);

            action.Should().NotThrow();
        }

        [TestMethod]
        public void Display_WhenUnderlyingCallThrowsSystemException_ShouldNotThrow()
        {
            var outlookItem = CreateBaseOutlookItem();
            outlookItem.Setup(x => x.Display()).Throws(new InvalidOperationException("boom"));
            var wrapper = new OutlookItemTry(outlookItem.Object);

            System.Action action = () => wrapper.Display();

            action.Should().NotThrow();
        }

        [TestMethod]
        public void NoAging_WhenUnderlyingGetterSucceeds_ShouldReturnValue()
        {
            var outlookItem = CreateBaseOutlookItem();
            outlookItem.SetupGet(x => x.NoAging).Returns(true);
            var wrapper = new OutlookItemTry(outlookItem.Object);

            wrapper.NoAging.Should().BeTrue();
        }

        [TestMethod]
        public void BillingInformation_WhenUnderlyingGetterSucceeds_ShouldReturnValue()
        {
            var outlookItem = CreateBaseOutlookItem();
            outlookItem.SetupGet(x => x.BillingInformation).Returns("B123");
            var wrapper = new OutlookItemTry(outlookItem.Object);

            wrapper.BillingInformation.Should().Be("B123");
        }

        [TestMethod]
        public void OlItemType_WhenInnerObjectIsMailItem_ShouldReturnMailItem()
        {
            // GetOlItemType is an extension method — mock the InnerObject type instead
            var outlookItem = CreateBaseOutlookItem();
            var mockMailItem = new Mock<Microsoft.Office.Interop.Outlook.MailItem>();
            outlookItem.SetupGet(x => x.InnerObject).Returns(mockMailItem.Object);
            var wrapper = new OutlookItemTry(outlookItem.Object);

            wrapper.OlItemType.Should().Be(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
        }

        private static Mock<IOutlookItem> CreateBaseOutlookItem()
        {
            var outlookItem = new Mock<IOutlookItem>();
            outlookItem.SetupGet(x => x.InnerObject).Returns(new object());
            outlookItem.SetupGet(x => x.ItemType).Returns(typeof(object));
            outlookItem.SetupGet(x => x.Args).Returns(Array.Empty<object>());
            return outlookItem;
        }
    }
}
