using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookExtensions;

namespace UtilitiesCS.Test.OutlookObjects.Item
{
    [TestClass]
    public class OutlookItemTryGetTests
    {
        [TestMethod]
        public void Subject_WhenWrappedPropertyExists_ShouldReturnTrueAndValue()
        {
            // Arrange
            var wrapper = new OutlookItemTryGet(new UtilitiesCS.OutlookItem(new TryGetFriendlyItem
            {
                Subject = "Project update"
            }));

            // Act
            var success = wrapper.Subject(out string result);

            // Assert
            success.Should().BeTrue();
            result.Should().Be("Project update");
        }

        [TestMethod]
        public void Subject_WhenWrappedPropertyIsMissing_ShouldReturnFalseAndNull()
        {
            // Arrange
            var wrapper = new OutlookItemTryGet(new UtilitiesCS.OutlookItem(new MissingSubjectItem()));

            // Act
            var success = wrapper.Subject(out string result);

            // Assert
            success.Should().BeFalse();
            result.Should().BeNull();
        }

        [TestMethod]
        public void Size_WhenWrappedPropertyExists_ShouldReturnTrueAndValue()
        {
            // Arrange
            var wrapper = new OutlookItemTryGet(new UtilitiesCS.OutlookItem(new TryGetFriendlyItem
            {
                Size = 42
            }));

            // Act
            var success = wrapper.Size(out long result);

            // Assert
            success.Should().BeTrue();
            result.Should().Be(42);
        }

        [TestMethod]
        public void Size_WhenWrappedPropertyIsMissing_ShouldReturnFalseAndDefaultValue()
        {
            // Arrange
            var wrapper = new OutlookItemTryGet(new UtilitiesCS.OutlookItem(new MissingSizeItem()));

            // Act
            var success = wrapper.Size(out long result);

            // Assert
            success.Should().BeFalse();
            result.Should().Be(default);
        }

        [TestMethod]
        public void InnerObject_WhenWrappedObjectExists_ShouldReturnTrueAndSameObject()
        {
            // Arrange
            var innerObject = new TryGetFriendlyItem();
            var wrapper = new OutlookItemTryGet(new UtilitiesCS.OutlookItem(innerObject));

            // Act
            var success = wrapper.InnerObject(out object result);

            // Assert
            success.Should().BeTrue();
            result.Should().BeSameAs(innerObject);
        }

        private sealed class TryGetFriendlyItem
        {
            public string Subject { get; set; }

            public int Size { get; set; }
        }

        private sealed class MissingSubjectItem
        {
            public int Size { get; set; }
        }

        private sealed class MissingSizeItem
        {
            public string Subject { get; set; }
        }
    }
}
