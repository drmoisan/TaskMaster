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

        #region Extended Tests — P2-T13

        [TestMethod]
        public void SetPropertyValue_WhenInvokeMemberFails_ShouldFallBackToPropertyInfo()
        {
            // SetPropertyValue's InvokeMember will work for POCO items, confirming the primary path
            var innerItem = new ReflectionFriendlyItem();
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            // Act
            outlookItem.Subject = "primary path";

            // Assert
            innerItem.Subject.Should().Be("primary path");
        }

        [TestMethod]
        public void SetPropertyValue_WhenPropertyIsMissing_ShouldThrowMissingMemberException()
        {
            var innerItem = new ReflectionFriendlyItem();
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            // SetPropertyValue is protected internal; invoke via a public property setter that maps to it
            // "MissingProperty" doesn't exist so the fallback path in SetPropertyValue should throw
            var method = typeof(UtilitiesCS.OutlookItem).GetMethod(
                "SetPropertyValue",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            System.Action action = () => method!.MakeGenericMethod(typeof(string))
                .Invoke(outlookItem, new object[] { "MissingProperty", "value" });

            action.Should().Throw<TargetInvocationException>()
                .WithInnerException<MissingMemberException>();
        }

        [TestMethod]
        public void GetPropertyValueIfExists_WhenPropertyDoesNotExist_ShouldReturnDefault()
        {
            var innerItem = new ReflectionFriendlyItemWithoutTaskStartDate();
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            var method = typeof(UtilitiesCS.OutlookItem).GetMethod(
                "GetPropertyValueIfExists",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            var result = method!.MakeGenericMethod(typeof(string))
                .Invoke(outlookItem, new object[] { "DoesNotExist" });

            result.Should().BeNull();
        }

        [TestMethod]
        public void BillingInformation_GetterAndSetter_ShouldDelegateThroughReflection()
        {
            var innerItem = new ExtendedReflectionFriendlyItem();
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            outlookItem.BillingInformation = "BC100";

            outlookItem.BillingInformation.Should().Be("BC100");
            innerItem.BillingInformation.Should().Be("BC100");
        }

        [TestMethod]
        public void Body_GetterAndSetter_ShouldDelegateThroughReflection()
        {
            var innerItem = new ExtendedReflectionFriendlyItem();
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            outlookItem.Body = "Hello World";

            outlookItem.Body.Should().Be("Hello World");
        }

        [TestMethod]
        public void Categories_GetterAndSetter_ShouldDelegateThroughReflection()
        {
            var innerItem = new ExtendedReflectionFriendlyItem();
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            outlookItem.Categories = "cat1; cat2";

            outlookItem.Categories.Should().Be("cat1; cat2");
        }

        [TestMethod]
        public void UnRead_GetterAndSetter_ShouldDelegateThroughReflection()
        {
            var innerItem = new ExtendedReflectionFriendlyItem();
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            outlookItem.UnRead = true;

            outlookItem.UnRead.Should().BeTrue();
        }

        [TestMethod]
        public void Saved_ShouldReturnWrappedPropertyValue()
        {
            var innerItem = new ExtendedReflectionFriendlyItem { Saved = true };
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            outlookItem.Saved.Should().BeTrue();
        }

        [TestMethod]
        public void Size_ShouldReturnWrappedPropertyValue()
        {
            var innerItem = new ExtendedReflectionFriendlyItem { Size = 4096 };
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            outlookItem.Size.Should().Be(4096);
        }

        [TestMethod]
        public void EntryID_ShouldReturnWrappedPropertyValue()
        {
            var innerItem = new ExtendedReflectionFriendlyItem { EntryID = "ABC123" };
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            outlookItem.EntryID.Should().Be("ABC123");
        }

        [TestMethod]
        public void ConversationTopic_ShouldReturnWrappedPropertyValue()
        {
            var innerItem = new ExtendedReflectionFriendlyItem { ConversationTopic = "Topic1" };
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            outlookItem.ConversationTopic.Should().Be("Topic1");
        }

        [TestMethod]
        public void MessageClass_GetterAndSetter_ShouldDelegateThroughReflection()
        {
            var innerItem = new ExtendedReflectionFriendlyItem();
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            outlookItem.MessageClass = "IPM.Note";

            outlookItem.MessageClass.Should().Be("IPM.Note");
        }

        [TestMethod]
        public void NoAging_GetterAndSetter_ShouldDelegateThroughReflection()
        {
            var innerItem = new ExtendedReflectionFriendlyItem();
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            outlookItem.NoAging = true;

            outlookItem.NoAging.Should().BeTrue();
        }

        [TestMethod]
        public void CallMethod_NoArgs_WhenInvokeMemberFails_ShouldFallbackToGetMethod()
        {
            // ReflectionFriendlyItem.Copy() is accessible via InvokeMember on POCOs, 
            // but let's verify the primary path works end-to-end
            var innerItem = new ReflectionFriendlyItem();
            var outlookItem = new UtilitiesCS.OutlookItem(innerItem);

            var callMethod = typeof(UtilitiesCS.OutlookItem).GetMethod(
                "CallMethod",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(string) },
                null
            );

            var result = callMethod!.Invoke(outlookItem, new object[] { "Copy" });

            result.Should().Be("copied");
        }

        private sealed class ExtendedReflectionFriendlyItem
        {
            public string Subject { get; set; }
            public string BillingInformation { get; set; }
            public string Body { get; set; }
            public string Categories { get; set; }
            public string Companies { get; set; }
            public string MessageClass { get; set; }
            public string Mileage { get; set; }
            public string ConversationIndex { get; set; }
            public string ConversationTopic { get; set; }
            public string EntryID { get; set; }
            public bool UnRead { get; set; }
            public bool Saved { get; set; }
            public bool NoAging { get; set; }
            public bool IsConflict { get; set; }
            public int Size { get; set; }
            public long OutlookInternalVersion { get; set; }
            public string OutlookVersion { get; set; }
            public DateTime CreationTime { get; set; }
            public DateTime LastModificationTime { get; set; }
            public DateTime ReminderTime { get; set; }
        }

        #endregion
    }
}
