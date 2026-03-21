using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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

        [TestMethod]
        public void WrapperMethods_WhenPropertiesExist_ShouldReturnExpectedResults()
        {
            var created = new DateTime(2026, 3, 1, 8, 30, 0, DateTimeKind.Utc);
            var modified = new DateTime(2026, 3, 2, 9, 45, 0, DateTimeKind.Utc);
            var inner = new TryGetRichItem
            {
                Companies = "Contoso",
                Class = (int)Microsoft.Office.Interop.Outlook.OlObjectClass.olMail,
                ConversationIndex = "abc123",
                CreationTime = created,
                DownloadState = Microsoft.Office.Interop.Outlook.OlDownloadState.olHeaderOnly,
                IsConflict = true,
                LastModificationTime = modified,
                MarkForDownload = Microsoft
                    .Office
                    .Interop
                    .Outlook
                    .OlRemoteStatus
                    .olMarkedForDownload,
                MessageClass = "IPM.Note",
                Mileage = "77",
                OutlookInternalVersion = 16,
                OutlookVersion = "16.0",
                Sensitivity = Microsoft.Office.Interop.Outlook.OlSensitivity.olPrivate,
            };
            var wrapper = new OutlookItemTryGet(new UtilitiesCS.OutlookItem(inner));

            wrapper.Actions(out Microsoft.Office.Interop.Outlook.Actions actions).Should().BeTrue();
            actions.Should().BeNull();
            wrapper
                .Application(out Microsoft.Office.Interop.Outlook.Application application)
                .Should()
                .BeTrue();
            application.Should().BeNull();
            wrapper
                .Attachments(out Microsoft.Office.Interop.Outlook.Attachments attachments)
                .Should()
                .BeTrue();
            attachments.Should().BeNull();
            wrapper.Companies(out string companies).Should().BeTrue();
            companies.Should().Be("Contoso");
            wrapper
                .OlObjectClass(out Microsoft.Office.Interop.Outlook.OlObjectClass objectClass)
                .Should()
                .BeTrue();
            objectClass.Should().Be(Microsoft.Office.Interop.Outlook.OlObjectClass.olMail);
            wrapper.ConversationIndex(out string conversationIndex).Should().BeTrue();
            conversationIndex.Should().Be("abc123");
            wrapper.CreationTime(out DateTime creationTime).Should().BeTrue();
            creationTime.Should().Be(created);
            wrapper
                .DownloadState(out Microsoft.Office.Interop.Outlook.OlDownloadState downloadState)
                .Should()
                .BeTrue();
            downloadState
                .Should()
                .Be(Microsoft.Office.Interop.Outlook.OlDownloadState.olHeaderOnly);
            wrapper
                .FormDescription(
                    out Microsoft.Office.Interop.Outlook.FormDescription formDescription
                )
                .Should()
                .BeTrue();
            formDescription.Should().BeNull();
            wrapper
                .GetInspector(out Microsoft.Office.Interop.Outlook.Inspector inspector)
                .Should()
                .BeTrue();
            inspector.Should().BeNull();
            wrapper.IsConflict(out bool isConflict).Should().BeTrue();
            isConflict.Should().BeTrue();
            wrapper
                .ItemProperties(out Microsoft.Office.Interop.Outlook.ItemProperties itemProperties)
                .Should()
                .BeTrue();
            itemProperties.Should().BeNull();
            wrapper.LastModificationTime(out DateTime lastModificationTime).Should().BeTrue();
            lastModificationTime.Should().Be(modified);
            wrapper.Links(out Microsoft.Office.Interop.Outlook.Links links).Should().BeTrue();
            links.Should().BeNull();
            wrapper
                .MarkForDownload(
                    out Microsoft.Office.Interop.Outlook.OlRemoteStatus markForDownload
                )
                .Should()
                .BeTrue();
            markForDownload
                .Should()
                .Be(Microsoft.Office.Interop.Outlook.OlRemoteStatus.olMarkedForDownload);
            wrapper.MessageClass(out string messageClass).Should().BeTrue();
            messageClass.Should().Be("IPM.Note");
            wrapper.Mileage(out string mileage).Should().BeTrue();
            mileage.Should().Be("77");
            wrapper.OutlookInternalVersion(out long internalVersion).Should().BeTrue();
            internalVersion.Should().Be(16);
            wrapper.OutlookVersion(out string outlookVersion).Should().BeTrue();
            outlookVersion.Should().Be("16.0");
            wrapper.Parent(out Microsoft.Office.Interop.Outlook.Folder parent).Should().BeTrue();
            parent.Should().BeNull();
            wrapper
                .PropertyAccessor(out Microsoft.Office.Interop.Outlook.PropertyAccessor accessor)
                .Should()
                .BeTrue();
            accessor.Should().BeNull();
            wrapper
                .Sensitivity(out Microsoft.Office.Interop.Outlook.OlSensitivity sensitivity)
                .Should()
                .BeTrue();
            sensitivity.Should().Be(Microsoft.Office.Interop.Outlook.OlSensitivity.olPrivate);
            wrapper
                .Session(out Microsoft.Office.Interop.Outlook.NameSpace session)
                .Should()
                .BeTrue();
            session.Should().BeNull();
            wrapper
                .UserProperties(out Microsoft.Office.Interop.Outlook.UserProperties userProperties)
                .Should()
                .BeTrue();
            userProperties.Should().BeNull();
        }

        [TestMethod]
        public void OlItemType_WhenWrappedObjectIsMailItem_ShouldReturnTrueAndValue()
        {
            var wrapper = new OutlookItemTryGet(
                new UtilitiesCS.OutlookItem(
                    new Mock<Microsoft.Office.Interop.Outlook.MailItem>().Object
                )
            );

            var success = wrapper.OlItemType(
                out Microsoft.Office.Interop.Outlook.OlItemType result
            );

            success.Should().BeTrue();
            result.Should().Be(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
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

        private sealed class TryGetRichItem
        {
            public Microsoft.Office.Interop.Outlook.Actions Actions { get; set; }

            public Microsoft.Office.Interop.Outlook.Application Application { get; set; }

            public Microsoft.Office.Interop.Outlook.Attachments Attachments { get; set; }

            public string Companies { get; set; }

            public int Class { get; set; }

            public string ConversationIndex { get; set; }

            public DateTime CreationTime { get; set; }

            public Microsoft.Office.Interop.Outlook.OlDownloadState DownloadState { get; set; }

            public Microsoft.Office.Interop.Outlook.FormDescription FormDescription { get; set; }

            public Microsoft.Office.Interop.Outlook.Inspector GetInspector { get; set; }

            public bool IsConflict { get; set; }

            public Microsoft.Office.Interop.Outlook.ItemProperties ItemProperties { get; set; }

            public DateTime LastModificationTime { get; set; }

            public Microsoft.Office.Interop.Outlook.Links Links { get; set; }

            public Microsoft.Office.Interop.Outlook.OlRemoteStatus MarkForDownload { get; set; }

            public string MessageClass { get; set; }

            public string Mileage { get; set; }

            public long OutlookInternalVersion { get; set; }

            public string OutlookVersion { get; set; }

            public Microsoft.Office.Interop.Outlook.Folder Parent { get; set; }

            public Microsoft.Office.Interop.Outlook.PropertyAccessor PropertyAccessor { get; set; }

            public Microsoft.Office.Interop.Outlook.OlSensitivity Sensitivity { get; set; }

            public Microsoft.Office.Interop.Outlook.NameSpace Session { get; set; }

            public Microsoft.Office.Interop.Outlook.UserProperties UserProperties { get; set; }
        }
    }
}
