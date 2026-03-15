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
            outlookItem.SetupSet(x => x.Body = It.IsAny<string>()).Throws(new InvalidOperationException("boom"));
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
