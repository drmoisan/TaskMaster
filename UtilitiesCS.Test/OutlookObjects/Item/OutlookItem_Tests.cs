using System;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Item
{
    [TestClass]
    public class OutlookItem_Tests
    {
        [TestMethod]
        public void Constructor_WithNullItem_DoesNotThrow()
        {
            // Arrange & Act
            System.Action act = () => new OutlookItem(null);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Constructor_WithObject_SetsItemAndType()
        {
            // Arrange
            var testObj = "test string";

            // Act
            var item = new OutlookItem(testObj);

            // Assert
            item.ItemType.Should().Be(typeof(string));
            item.Args.Should().NotBeNull();
            item.Args.Should().BeEmpty();
        }

        [TestMethod]
        public void InnerObject_ReturnsWrappedItem()
        {
            // Arrange
            var testObj = new object();
            var item = new OutlookItem(testObj);

            // Assert
            item.InnerObject.Should().BeSameAs(testObj);
        }

        [TestMethod]
        public void Subject_Get_WithNonOutlookItem_ShouldThrowMissingMemberException()
        {
            // Arrange — non-Outlook object has no Subject property
            var item = new OutlookItem(new object());

            // Act
            System.Action act = () => _ = item.Subject;

            // Assert
            act.Should().Throw<MissingMemberException>();
        }

        [TestMethod]
        public void EntryID_Get_WithNonOutlookItem_ShouldThrowMissingMemberException()
        {
            // Arrange
            var item = new OutlookItem(new object());

            // Act
            System.Action act = () => _ = item.EntryID;

            // Assert
            act.Should().Throw<MissingMemberException>();
        }

        [TestMethod]
        public void Body_Get_WithNonOutlookItem_ShouldThrowMissingMemberException()
        {
            // Arrange
            var item = new OutlookItem(new object());

            // Act
            System.Action act = () => _ = item.Body;

            // Assert
            act.Should().Throw<MissingMemberException>();
        }

        [TestMethod]
        public void Categories_Get_WithNonOutlookItem_ShouldThrowMissingMemberException()
        {
            // Arrange
            var item = new OutlookItem(new object());

            // Act
            System.Action act = () => _ = item.Categories;

            // Assert
            act.Should().Throw<MissingMemberException>();
        }

        [TestMethod]
        public void Item_Get_WithWrappedObject_ReturnsInnerObjectViaInternalProperty()
        {
            // Arrange
            var testObj = new object();
            var item = new OutlookItem(testObj);
            var property = typeof(OutlookItem).GetProperty(
                "Item",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
            );

            // Act
            var result = property!.GetValue(item);

            // Assert
            result.Should().BeSameAs(testObj);
        }

        [TestMethod]
        public void PropertyWrappers_WithReflectionFriendlyOutlookShape_ShouldReturnExpectedValues()
        {
            // Arrange
            var reminderTime = new DateTime(2026, 3, 21, 10, 30, 0, DateTimeKind.Utc);
            var creationTime = new DateTime(2026, 3, 20, 8, 0, 0, DateTimeKind.Utc);
            var lastModificationTime = new DateTime(2026, 3, 21, 9, 0, 0, DateTimeKind.Utc);
            var innerItem = new ReflectionFriendlyOutlookShape
            {
                Companies = "Contoso",
                ConversationIndex = "ABC123",
                DownloadState = OlDownloadState.olHeaderOnly,
                IsConflict = true,
                MarkForDownload = OlRemoteStatus.olMarkedForDownload,
                Mileage = "125",
                OutlookInternalVersion = 42L,
                OutlookVersion = "16.0",
                ReminderTime = reminderTime,
                Sensitivity = OlSensitivity.olConfidential,
                CreationTime = creationTime,
                LastModificationTime = lastModificationTime,
            };
            var item = new OutlookItem(innerItem);

            // Act / Assert
            item.Actions.Should().BeNull();
            item.Application.Should().BeNull();
            item.Companies.Should().Be("Contoso");
            item.ConversationIndex.Should().Be("ABC123");
            item.DownloadState.Should().Be(OlDownloadState.olHeaderOnly);
            item.FormDescription.Should().BeNull();
            item.Inspector.Should().BeNull();
            item.IsConflict.Should().BeTrue();
            item.ItemProperties.Should().BeNull();
            item.LastModificationTime.Should().Be(lastModificationTime);
            item.Links.Should().BeNull();
            item.MarkForDownload.Should().Be(OlRemoteStatus.olMarkedForDownload);
            item.Mileage.Should().Be("125");
            item.OutlookInternalVersion.Should().Be(42L);
            item.OutlookVersion.Should().Be("16.0");
            item.Parent.Should().BeNull();
            item.ReminderTime.Should().Be(reminderTime);
            item.Sensitivity.Should().Be(OlSensitivity.olConfidential);
            item.Session.Should().BeNull();
        }

        [TestMethod]
        public void SetterWrappers_WithReflectionFriendlyOutlookShape_ShouldUpdateUnderlyingValues()
        {
            // Arrange
            var reminderTime = new DateTime(2026, 3, 22, 6, 45, 0, DateTimeKind.Utc);
            var innerItem = new ReflectionFriendlyOutlookShape();
            var item = new OutlookItem(innerItem);

            // Act
            item.Companies = "Fabrikam";
            item.MarkForDownload = OlRemoteStatus.olMarkedForDownload;
            item.Mileage = "88";
            item.ReminderTime = reminderTime;
            item.Sensitivity = OlSensitivity.olPrivate;

            // Assert
            innerItem.Companies.Should().Be("Fabrikam");
            innerItem.MarkForDownload.Should().Be(OlRemoteStatus.olMarkedForDownload);
            innerItem.Mileage.Should().Be("88");
            innerItem.ReminderTime.Should().Be(reminderTime);
            innerItem.Sensitivity.Should().Be(OlSensitivity.olPrivate);
        }

        [TestMethod]
        public void Close_WithReflectionFriendlyOutlookShape_ShouldDelegateCall()
        {
            // Arrange
            var innerItem = new ReflectionFriendlyOutlookShape();
            var item = new OutlookItem(innerItem);

            // Act
            item.Close(OlInspectorClose.olSave);

            // Assert
            innerItem.CloseCalled.Should().BeTrue();
        }

        [TestMethod]
        public void SaveAs_WithReflectionFriendlyOutlookShape_ShouldPropagateMissingMethodException()
        {
            // Arrange
            var item = new OutlookItem(new ReflectionFriendlyOutlookShape());

            // Act
            System.Action act = () => item.SaveAs("c:/mail.msg", OlSaveAsType.olMSG);

            // Assert
            act.Should().Throw<MissingMethodException>();
        }

        [TestMethod]
        public void Move_WithNullDestinationFolder_ShouldPropagateMissingMethodException()
        {
            // Arrange
            var item = new OutlookItem(new ReflectionFriendlyOutlookShape());

            // Act
            System.Action act = () => item.Move(null);

            // Assert
            act.Should().Throw<MissingMethodException>();
        }

        private sealed class ReflectionFriendlyOutlookShape
        {
            public Actions Actions { get; set; }

            public Application Application { get; set; }

            public string Companies { get; set; }

            public string ConversationIndex { get; set; }

            public DateTime CreationTime { get; set; }

            public OlDownloadState DownloadState { get; set; }

            public FormDescription FormDescription { get; set; }

            public Inspector GetInspector { get; set; }

            public bool IsConflict { get; set; }

            public ItemProperties ItemProperties { get; set; }

            public DateTime LastModificationTime { get; set; }

            public Links Links { get; set; }

            public OlRemoteStatus MarkForDownload { get; set; }

            public string Mileage { get; set; }

            public long OutlookInternalVersion { get; set; }

            public string OutlookVersion { get; set; }

            public OutlookFolder Parent { get; set; }

            public DateTime ReminderTime { get; set; }

            public OlSensitivity Sensitivity { get; set; }

            public NameSpace Session { get; set; }

            public bool CloseCalled { get; private set; }

            public bool MoveCalled { get; private set; }

            public string LastSaveAsPath { get; private set; }

            public OlSaveAsType LastSaveAsType { get; private set; }

            public void Close()
            {
                CloseCalled = true;
            }

            public object Move(OutlookFolder destinationFolder)
            {
                MoveCalled = true;
                return "moved";
            }

            public void SaveAs(string path, OlSaveAsType type)
            {
                LastSaveAsPath = path;
                LastSaveAsType = type;
            }
        }
    }
}
