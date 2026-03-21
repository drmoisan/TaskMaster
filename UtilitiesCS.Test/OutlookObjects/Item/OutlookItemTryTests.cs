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

        [TestMethod]
        public void PropertyWrappers_WhenUnderlyingGettersSucceed_ShouldReturnExpectedValues()
        {
            var created = new DateTime(2026, 3, 10, 7, 0, 0, DateTimeKind.Utc);
            var modified = new DateTime(2026, 3, 11, 8, 0, 0, DateTimeKind.Utc);
            var reminder = new DateTime(2026, 3, 12, 9, 0, 0, DateTimeKind.Utc);
            var outlookItem = CreateBaseOutlookItem();
            outlookItem
                .SetupGet(x => x.Actions)
                .Returns((Microsoft.Office.Interop.Outlook.Actions)null);
            outlookItem
                .SetupGet(x => x.Application)
                .Returns((Microsoft.Office.Interop.Outlook.Application)null);
            outlookItem.SetupGet(x => x.Companies).Returns("Contoso");
            outlookItem
                .SetupGet(x => x.Class)
                .Returns(Microsoft.Office.Interop.Outlook.OlObjectClass.olMail);
            outlookItem.SetupGet(x => x.ConversationIndex).Returns("abc123");
            outlookItem.SetupGet(x => x.CreationTime).Returns(created);
            outlookItem
                .SetupGet(x => x.DownloadState)
                .Returns(Microsoft.Office.Interop.Outlook.OlDownloadState.olHeaderOnly);
            outlookItem
                .SetupGet(x => x.FormDescription)
                .Returns((Microsoft.Office.Interop.Outlook.FormDescription)null);
            outlookItem
                .SetupGet(x => x.Inspector)
                .Returns((Microsoft.Office.Interop.Outlook.Inspector)null);
            outlookItem
                .SetupGet(x => x.Importance)
                .Returns(Microsoft.Office.Interop.Outlook.OlImportance.olImportanceHigh);
            outlookItem.SetupGet(x => x.IsConflict).Returns(true);
            outlookItem
                .SetupGet(x => x.ItemProperties)
                .Returns((Microsoft.Office.Interop.Outlook.ItemProperties)null);
            outlookItem.SetupGet(x => x.LastModificationTime).Returns(modified);
            outlookItem
                .SetupGet(x => x.Links)
                .Returns((Microsoft.Office.Interop.Outlook.Links)null);
            outlookItem
                .SetupGet(x => x.MarkForDownload)
                .Returns(Microsoft.Office.Interop.Outlook.OlRemoteStatus.olMarkedForDownload);
            outlookItem.SetupGet(x => x.Mileage).Returns("101");
            outlookItem
                .Setup(x => x.Move(It.IsAny<Microsoft.Office.Interop.Outlook.Folder>()))
                .Returns("moved");
            outlookItem.SetupGet(x => x.OutlookInternalVersion).Returns(16L);
            outlookItem.SetupGet(x => x.OutlookVersion).Returns("16.0");
            outlookItem
                .SetupGet(x => x.Parent)
                .Returns((Microsoft.Office.Interop.Outlook.Folder)null);
            outlookItem
                .SetupGet(x => x.PropertyAccessor)
                .Returns((Microsoft.Office.Interop.Outlook.PropertyAccessor)null);
            outlookItem
                .SetupGet(x => x.Sensitivity)
                .Returns(Microsoft.Office.Interop.Outlook.OlSensitivity.olPrivate);
            outlookItem
                .SetupGet(x => x.Session)
                .Returns((Microsoft.Office.Interop.Outlook.NameSpace)null);
            outlookItem
                .SetupGet(x => x.UserProperties)
                .Returns((Microsoft.Office.Interop.Outlook.UserProperties)null);
            outlookItem.SetupGet(x => x.ReminderTime).Returns(reminder);
            outlookItem.SetupGet(x => x.TaskStartDate).Returns(created);
            var wrapper = new OutlookItemTry(outlookItem.Object);

            wrapper.Actions.Should().BeNull();
            wrapper.Application.Should().BeNull();
            wrapper.Companies.Should().Be("Contoso");
            wrapper
                .OlObjectClass.Should()
                .Be(Microsoft.Office.Interop.Outlook.OlObjectClass.olMail);
            wrapper.ConversationIndex.Should().Be("abc123");
            wrapper.CreationTime.Should().Be(created);
            wrapper
                .DownloadState.Should()
                .Be(Microsoft.Office.Interop.Outlook.OlDownloadState.olHeaderOnly);
            wrapper.FormDescription.Should().BeNull();
            wrapper.GetInspector.Should().BeNull();
            wrapper
                .Importance.Should()
                .Be(Microsoft.Office.Interop.Outlook.OlImportance.olImportanceHigh);
            wrapper.IsConflict.Should().BeTrue();
            wrapper.ItemProperties.Should().BeNull();
            wrapper.LastModificationTime.Should().Be(modified);
            wrapper.Links.Should().BeNull();
            wrapper
                .MarkForDownload.Should()
                .Be(Microsoft.Office.Interop.Outlook.OlRemoteStatus.olMarkedForDownload);
            wrapper.Mileage.Should().Be("101");
            wrapper.Move(null).Should().Be("moved");
            wrapper.OutlookInternalVersion.Should().Be(16L);
            wrapper.OutlookVersion.Should().Be("16.0");
            wrapper.Parent.Should().BeNull();
            wrapper.PropertyAccessor.Should().BeNull();
            wrapper
                .Sensitivity.Should()
                .Be(Microsoft.Office.Interop.Outlook.OlSensitivity.olPrivate);
            wrapper.Session.Should().BeNull();
            wrapper.UserProperties.Should().BeNull();
            wrapper.ReminderTime.Should().Be(reminder);
            wrapper.TaskStartDate.Should().Be(created);
            wrapper.Class.Should().Be(Microsoft.Office.Interop.Outlook.OlObjectClass.olMail);
            wrapper.Inspector.Should().BeNull();
        }

        [TestMethod]
        public void SetterAndMethodWrappers_WhenUnderlyingMembersSucceed_ShouldForwardValues()
        {
            var reminder = new DateTime(2026, 3, 13, 10, 0, 0, DateTimeKind.Utc);
            var outlookItem = CreateBaseOutlookItem();
            bool printed = false;
            bool savedAs = false;
            bool showedCategories = false;
            Microsoft.Office.Interop.Outlook.OlSaveAsType saveAsType = default;
            string saveAsPath = null;
            outlookItem.SetupProperty(x => x.BillingInformation);
            outlookItem.SetupProperty(x => x.Companies);
            outlookItem.SetupProperty(x => x.Importance);
            outlookItem.SetupProperty(x => x.MarkForDownload);
            outlookItem.SetupProperty(x => x.MessageClass);
            outlookItem.SetupProperty(x => x.Mileage);
            outlookItem.SetupProperty(x => x.Sensitivity);
            outlookItem.SetupProperty(x => x.UnRead);
            outlookItem.SetupProperty(x => x.NoAging);
            outlookItem.SetupProperty(x => x.ReminderTime);
            outlookItem.Setup(x => x.PrintOut()).Callback(() => printed = true);
            outlookItem
                .Setup(x =>
                    x.SaveAs(
                        It.IsAny<string>(),
                        It.IsAny<Microsoft.Office.Interop.Outlook.OlSaveAsType>()
                    )
                )
                .Callback<string, Microsoft.Office.Interop.Outlook.OlSaveAsType>(
                    (path, type) =>
                    {
                        savedAs = true;
                        saveAsPath = path;
                        saveAsType = type;
                    }
                );
            outlookItem
                .Setup(x => x.ShowCategoriesDialog())
                .Callback(() => showedCategories = true);
            var wrapper = new OutlookItemTry(outlookItem.Object);

            wrapper.BillingInformation = "B999";
            wrapper.Companies = "Fabrikam";
            wrapper.Importance = Microsoft.Office.Interop.Outlook.OlImportance.olImportanceHigh;
            wrapper.MarkForDownload = Microsoft
                .Office
                .Interop
                .Outlook
                .OlRemoteStatus
                .olMarkedForDownload;
            wrapper.MessageClass = "IPM.Note";
            wrapper.Mileage = "22";
            wrapper.Sensitivity = Microsoft.Office.Interop.Outlook.OlSensitivity.olConfidential;
            wrapper.UnRead = true;
            wrapper.NoAging = true;
            wrapper.ReminderTime = reminder;
            wrapper.PrintOut();
            wrapper.SaveAs("mail.msg", Microsoft.Office.Interop.Outlook.OlSaveAsType.olMSG);
            wrapper.ShowCategoriesDialog();
            wrapper
                .GetOlItemType()
                .Should()
                .Be(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);

            outlookItem.Object.BillingInformation.Should().Be("B999");
            outlookItem.Object.Companies.Should().Be("Fabrikam");
            outlookItem
                .Object.Importance.Should()
                .Be(Microsoft.Office.Interop.Outlook.OlImportance.olImportanceHigh);
            outlookItem
                .Object.MarkForDownload.Should()
                .Be(Microsoft.Office.Interop.Outlook.OlRemoteStatus.olMarkedForDownload);
            outlookItem.Object.MessageClass.Should().Be("IPM.Note");
            outlookItem.Object.Mileage.Should().Be("22");
            outlookItem
                .Object.Sensitivity.Should()
                .Be(Microsoft.Office.Interop.Outlook.OlSensitivity.olConfidential);
            outlookItem.Object.UnRead.Should().BeTrue();
            outlookItem.Object.NoAging.Should().BeTrue();
            outlookItem.Object.ReminderTime.Should().Be(reminder);
            printed.Should().BeTrue();
            savedAs.Should().BeTrue();
            saveAsPath.Should().Be("mail.msg");
            saveAsType.Should().Be(Microsoft.Office.Interop.Outlook.OlSaveAsType.olMSG);
            showedCategories.Should().BeTrue();
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
