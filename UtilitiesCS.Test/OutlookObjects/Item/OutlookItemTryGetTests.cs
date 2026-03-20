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
            var wrapper = new OutlookItemTryGet(
                new UtilitiesCS.OutlookItem(new TryGetFriendlyItem { Subject = "Project update" })
            );

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
            var wrapper = new OutlookItemTryGet(
                new UtilitiesCS.OutlookItem(new MissingSubjectItem())
            );

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
            var wrapper = new OutlookItemTryGet(
                new UtilitiesCS.OutlookItem(new TryGetFriendlyItem { Size = 42 })
            );

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

        [TestMethod]
        public void Body_WhenPropertyExists_ShouldReturnTrueAndValue()
        {
            var wrapper = new OutlookItemTryGet(
                new UtilitiesCS.OutlookItem(new TryGetFriendlyItem { Body = "Hello world" })
            );

            var success = wrapper.Body(out string result);

            success.Should().BeTrue();
            result.Should().Be("Hello world");
        }

        [TestMethod]
        public void Categories_WhenPropertyExists_ShouldReturnTrueAndValue()
        {
            var wrapper = new OutlookItemTryGet(
                new UtilitiesCS.OutlookItem(new TryGetFriendlyItem { Categories = "cat1; cat2" })
            );

            var success = wrapper.Categories(out string result);

            success.Should().BeTrue();
            result.Should().Be("cat1; cat2");
        }

        [TestMethod]
        public void BillingInformation_WhenPropertyExists_ShouldReturnTrueAndValue()
        {
            var wrapper = new OutlookItemTryGet(
                new UtilitiesCS.OutlookItem(new TryGetFriendlyItem { BillingInformation = "B999" })
            );

            var success = wrapper.BillingInformation(out string result);

            success.Should().BeTrue();
            result.Should().Be("B999");
        }

        [TestMethod]
        public void TryGet_WhenGetterThrowsSystemException_ShouldReturnFalseAndDefault()
        {
            var wrapper = new OutlookItemTryGet(
                new UtilitiesCS.OutlookItem(new TryGetFriendlyItem())
            );

            var success = wrapper.TryGet<string>(
                () => throw new InvalidOperationException("boom"),
                out string result
            );

            success.Should().BeFalse();
            result.Should().BeNull();
        }

        [TestMethod]
        public void TrySet_WhenSetterSucceeds_ShouldReturnTrue()
        {
            var wrapper = new OutlookItemTryGet(
                new UtilitiesCS.OutlookItem(new TryGetFriendlyItem())
            );
            string captured = null;

            var success = wrapper.TrySet<string>(v => captured = v, "hello");

            success.Should().BeTrue();
            captured.Should().Be("hello");
        }

        [TestMethod]
        public void TrySet_WhenSetterThrowsSystemException_ShouldReturnFalse()
        {
            var wrapper = new OutlookItemTryGet(
                new UtilitiesCS.OutlookItem(new TryGetFriendlyItem())
            );

            var success = wrapper.TrySet<string>(
                _ => throw new InvalidOperationException("boom"),
                "hello"
            );

            success.Should().BeFalse();
        }

        [TestMethod]
        public void TryCall_Action_WhenSucceeds_ShouldReturnTrue()
        {
            var wrapper = new OutlookItemTryGet(
                new UtilitiesCS.OutlookItem(new TryGetFriendlyItem())
            );
            bool called = false;

            var success = wrapper.TryCall(() => called = true);

            success.Should().BeTrue();
            called.Should().BeTrue();
        }

        [TestMethod]
        public void TryCall_Action_WhenThrowsSystemException_ShouldReturnFalse()
        {
            var wrapper = new OutlookItemTryGet(
                new UtilitiesCS.OutlookItem(new TryGetFriendlyItem())
            );

            var success = wrapper.TryCall(
                (System.Action)(() => throw new InvalidOperationException("boom"))
            );

            success.Should().BeFalse();
        }

        [TestMethod]
        public void TryCall_Func_WhenSucceeds_ShouldReturnTrueAndValue()
        {
            var wrapper = new OutlookItemTryGet(
                new UtilitiesCS.OutlookItem(new TryGetFriendlyItem())
            );

            var success = wrapper.TryCall(() => 42, out int result);

            success.Should().BeTrue();
            result.Should().Be(42);
        }

        [TestMethod]
        public void TryCall_Func_WhenThrowsSystemException_ShouldReturnFalseAndDefault()
        {
            var wrapper = new OutlookItemTryGet(
                new UtilitiesCS.OutlookItem(new TryGetFriendlyItem())
            );

            var success = wrapper.TryCall<int>(
                () => throw new InvalidOperationException("boom"),
                out int result
            );

            success.Should().BeFalse();
            result.Should().Be(0);
        }

        [TestMethod]
        public void UnRead_WhenPropertyExists_ShouldReturnTrueAndValue()
        {
            var wrapper = new OutlookItemTryGet(
                new UtilitiesCS.OutlookItem(new TryGetFriendlyItem { UnRead = true })
            );

            var success = wrapper.UnRead(out bool result);

            success.Should().BeTrue();
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Saved_WhenPropertyExists_ShouldReturnTrueAndValue()
        {
            var wrapper = new OutlookItemTryGet(
                new UtilitiesCS.OutlookItem(new TryGetFriendlyItem { Saved = true })
            );

            var success = wrapper.Saved(out bool result);

            success.Should().BeTrue();
            result.Should().BeTrue();
        }

        private sealed class TryGetFriendlyItem
        {
            public string Subject { get; set; }

            public int Size { get; set; }

            public string Body { get; set; }

            public string Categories { get; set; }

            public string BillingInformation { get; set; }

            public bool UnRead { get; set; }

            public bool Saved { get; set; }
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
