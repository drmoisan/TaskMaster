using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Item
{
    [TestClass]
    public class OutlookItemTests
    {
        [TestMethod]
        public void Constructor_ShouldCaptureWrappedObjectMetadata()
        {
            // Arrange
            var innerItem = new ReflectionFriendlyItem();

            // Act
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            // Assert
            outlookItem.InnerObject.Should().BeSameAs(innerItem);
            outlookItem.ItemType.Should().Be(typeof(ReflectionFriendlyItem));
            outlookItem.Args.Should().NotBeNull().And.BeEmpty();
        }

        [TestMethod]
        public void GetPropertyValue_ShouldReturnWrappedPropertyValue()
        {
            // Arrange
            var innerItem = new ReflectionFriendlyItem { Subject = "Quarterly review" };
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            // Act
            var result = outlookItem.GetPropertyValue<string>("Subject");

            // Assert
            result.Should().Be("Quarterly review");
            outlookItem.Subject.Should().Be("Quarterly review");
        }

        [TestMethod]
        public void SubjectSetter_ShouldUpdateWrappedProperty()
        {
            // Arrange
            var innerItem = new ReflectionFriendlyItem();
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            // Act
            outlookItem.Subject = "Updated subject";

            // Assert
            innerItem.Subject.Should().Be("Updated subject");
        }

        [TestMethod]
        public void Class_ShouldConvertUnderlyingNumericValueToEnum()
        {
            // Arrange
            var innerItem = new ReflectionFriendlyItem { Class = (int)OlObjectClass.olMail };
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            // Act
            var result = outlookItem.Class;

            // Assert
            result.Should().Be(OlObjectClass.olMail);
        }

        [TestMethod]
        public void GetPropertyValue_WhenPropertyIsMissing_ShouldThrowMissingMemberException()
        {
            // Arrange
            var outlookItem = new UtilitiesCS.OutlookItem(new ReflectionFriendlyItem());

            // Act
            System.Action action = () => outlookItem.GetPropertyValue<string>("DoesNotExist");

            // Assert
            action
                .Should()
                .Throw<MissingMemberException>()
                .WithMessage("Member 'ReflectionFriendlyItem.DoesNotExist' not found.");
        }

        [TestMethod]
        public void GetPropertyValue_WhenGetterThrows_ShouldRethrowTargetInvocationException()
        {
            // Arrange
            var outlookItem = new UtilitiesCS.OutlookItem(new ThrowingPropertyItem());

            // Act
            System.Action action = () => outlookItem.GetPropertyValue<string>("Subject");

            // Assert
            action
                .Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<InvalidOperationException>();
        }

        [TestMethod]
        public void TaskStartDate_WhenPropertyIsMissing_ShouldReturnDefaultValue()
        {
            // Arrange
            var missingPropertyItem = new UtilitiesCS.OutlookItem(
                new ReflectionFriendlyItemWithoutTaskStartDate()
            );

            // Act
            var missingResult = missingPropertyItem.TaskStartDate;

            // Assert
            missingResult.Should().Be(default);
        }

        [TestMethod]
        public void TaskStartDate_WhenGetterThrows_ShouldRethrowTargetInvocationException()
        {
            // Arrange
            var throwingPropertyItem = new UtilitiesCS.OutlookItem(new ThrowingTaskStartDateItem());

            // Act
            System.Action action = () => _ = throwingPropertyItem.TaskStartDate;

            // Assert
            action
                .Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<InvalidOperationException>();
        }

        [TestMethod]
        public void WrappedMethods_ShouldInvokeUnderlyingMembers()
        {
            // Arrange
            var innerItem = new ReflectionFriendlyItem();
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            // Act
            var copyResult = outlookItem.Copy();
            outlookItem.Display();
            outlookItem.PrintOut();
            outlookItem.Save();
            outlookItem.ShowCategoriesDialog();

            // Assert
            copyResult.Should().Be("copied");
            innerItem.DisplayCalled.Should().BeTrue();
            innerItem.PrintOutCalled.Should().BeTrue();
            innerItem.SaveCalled.Should().BeTrue();
            innerItem.ShowCategoriesDialogCalled.Should().BeTrue();
        }

        [TestMethod]
        public void CallMethod_WithArguments_WhenMethodCannotBeBound_ShouldBubbleMissingMethodException()
        {
            // Arrange
            var innerItem = new ReflectionFriendlyItem();
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);
            var callMethod = typeof(UtilitiesCS.OutlookItem).GetMethod(
                "CallMethod",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(string), typeof(object[]) },
                null
            );

            // Act
            System.Action action = () =>
                callMethod!.Invoke(
                    outlookItem,
                    new object[] { "Combine", new object[] { "report", 2026 } }
                );

            // Assert
            action
                .Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<MissingMethodException>();
        }

        private sealed class ReflectionFriendlyItem
        {
            public string Subject { get; set; }

            public int Class { get; set; }

            public bool DisplayCalled { get; private set; }

            public bool PrintOutCalled { get; private set; }

            public bool SaveCalled { get; private set; }

            public bool ShowCategoriesDialogCalled { get; private set; }

            public object Copy()
            {
                return "copied";
            }

            public string Combine(string prefix, int year)
            {
                return $"{prefix}-{year}";
            }

            public void Display()
            {
                DisplayCalled = true;
            }

            public void PrintOut()
            {
                PrintOutCalled = true;
            }

            public void Save()
            {
                SaveCalled = true;
            }

            public void ShowCategoriesDialog()
            {
                ShowCategoriesDialogCalled = true;
            }
        }

        private sealed class ReflectionFriendlyItemWithoutTaskStartDate
        {
            public string Subject { get; set; }
        }

        private sealed class ThrowingPropertyItem
        {
            public string Subject => throw new InvalidOperationException("Subject access failed.");
        }

        private sealed class ThrowingTaskStartDateItem
        {
            public DateTime TaskStartDate =>
                throw new InvalidOperationException("TaskStartDate access failed.");
        }
    }
}
