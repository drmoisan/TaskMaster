using System;
using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.Test.OutlookObjects.MailItem
{
    [TestClass]
    public class MailItemHelper_ExtendedTests
    {
        #region Constructors

        [TestMethod]
        public void DefaultConstructor_CreatesInstance()
        {
            var helper = new MailItemHelper();
            helper.Should().NotBeNull();
        }

        #endregion

        #region TryProjectMailItemMembers

        [TestMethod]
        public void TryProjectMailItemMembers_NullSource_ReturnsSafeDefaults()
        {
            var result = MailItemHelper.TryProjectMailItemMembers(null);
            result.Subject.Should().BeEmpty();
            result.EntryId.Should().BeEmpty();
        }

        [TestMethod]
        public void TryProjectMailItemMembers_WithNullProperties_ReturnsSafeDefaults()
        {
            var source = new { Subject = (string)null, EntryID = (string)null };
            var result = MailItemHelper.TryProjectMailItemMembers(source);
            result.Subject.Should().BeEmpty();
            result.EntryId.Should().BeEmpty();
        }

        #endregion

        #region Property Setters and Getters

        [TestMethod]
        public void Properties_SetAndGet_RoundTrip()
        {
            var helper = new MailItemHelper();
            helper.Subject = "Test Subject";
            helper.Subject.Should().Be("Test Subject");

            helper.Body = "Test Body";
            helper.Body.Should().Be("Test Body");

            helper.Categories = "Cat1";
            helper.Categories.Should().Be("Cat1");

            helper.ConversationID = "conv123";
            helper.ConversationID.Should().Be("conv123");

            helper.EntryId = "entry1";
            helper.EntryId.Should().Be("entry1");

            helper.StoreId = "store1";
            helper.StoreId.Should().Be("store1");

            helper.Actionable = "Yes";
            helper.Actionable.Should().Be("Yes");

            helper.SentOn = "3/20/2026";
            helper.SentOn.Should().Be("3/20/2026");

            helper.Triage = "High";
            helper.Triage.Should().Be("High");
        }

        [TestMethod]
        public void SenderProperties_SetAndGet()
        {
            var helper = new MailItemHelper();
            helper.SenderName = "John";
            helper.SenderName.Should().Be("John");

            helper.SenderHtml = "<b>John</b>";
            helper.SenderHtml.Should().Be("<b>John</b>");
        }

        [TestMethod]
        public void RecipientProperties_SetAndGet()
        {
            var helper = new MailItemHelper();
            helper.ToRecipientsName = "Alice; Bob";
            helper.ToRecipientsName.Should().Be("Alice; Bob");

            helper.ToRecipientsHtml = "<b>Alice</b>";
            helper.ToRecipientsHtml.Should().Be("<b>Alice</b>");

            helper.CcRecipientsName = "Charlie";
            helper.CcRecipientsName.Should().Be("Charlie");

            helper.CcRecipientsHtml = "<b>Charlie</b>";
            helper.CcRecipientsHtml.Should().Be("<b>Charlie</b>");
        }

        [TestMethod]
        public void FolderName_SetAndGet()
        {
            var helper = CreateHelper();
            SetLazyField(helper, "_folderName", "Inbox");
            helper.FolderName.Should().Be("Inbox");
        }

        [TestMethod]
        public void Size_SetAndGet()
        {
            var helper = new MailItemHelper();
            helper.Size = 42;
            helper.Size.Should().Be(42);
        }

        [TestMethod]
        public void SentDate_SetAndGet()
        {
            var helper = new MailItemHelper();
            var dt = new DateTime(2026, 3, 20);
            helper.SentDate = dt;
            helper.SentDate.Should().Be(dt);
        }

        [TestMethod]
        public void UnRead_SetAndGet_ViaReflection()
        {
            var helper = CreateHelper();
            SetLazyField(helper, "_unread", true);
            helper.UnRead.Should().BeTrue();
        }

        [TestMethod]
        public void IsTaskFlagSet_SetAndGet()
        {
            var helper = new MailItemHelper();
            helper.IsTaskFlagSet = true;
            helper.IsTaskFlagSet.Should().BeTrue();
        }

        [TestMethod]
        public void InternetCodepage_SetAndGet()
        {
            var helper = new MailItemHelper();
            helper.InternetCodepage = 65001;
            helper.InternetCodepage.Should().Be(65001);
        }

        [TestMethod]
        public void PlainTextOptions_Default_IsStripAll()
        {
            var helper = new MailItemHelper();
            helper.PlainTextOptions.Should().Be(IItemInfo.PlainTextOptionsEnum.StripAll);
        }

        [TestMethod]
        public void PlainTextOptions_SetAndGet()
        {
            var helper = new MailItemHelper();
            helper.PlainTextOptions = IItemInfo.PlainTextOptionsEnum.StripLinks;
            helper.PlainTextOptions.Should().Be(IItemInfo.PlainTextOptionsEnum.StripLinks);
        }

        #endregion

        #region SetSender

        [TestMethod]
        public void SetSender_SetsAllSenderFields()
        {
            var helper = new MailItemHelper();
            var mockSender = new Mock<IRecipientInfo>();
            mockSender.Setup(s => s.Name).Returns("Test Sender");
            mockSender.Setup(s => s.Html).Returns("<b>Test</b>");

            helper.SetSender(mockSender.Object);

            helper.Sender.Should().BeSameAs(mockSender.Object);
            helper.SenderName.Should().Be("Test Sender");
            helper.SenderHtml.Should().Be("<b>Test</b>");
        }

        #endregion

        #region Equals

        [TestMethod]
        public void Equals_NullOther_ReturnsFalse()
        {
            var helper = CreatePopulatedHelper();
            helper.Equals((IItemInfo)null).Should().BeFalse();
        }

        [TestMethod]
        public void Equals_SameReference_ReturnsTrue()
        {
            var helper = CreatePopulatedHelper();
            helper.Equals((IItemInfo)helper).Should().BeTrue();
        }

        [TestMethod]
        public void Equals_DifferentSize_ReturnsFalse()
        {
            var a = CreatePopulatedHelper();
            var b = CreatePopulatedHelper();
            b.Size = 999;
            a.Equals((IItemInfo)b).Should().BeFalse();
        }

        [TestMethod]
        public void Equals_DifferentSubject_ReturnsFalse()
        {
            var a = CreatePopulatedHelper();
            var b = CreatePopulatedHelper();
            b.Subject = "Different";
            a.Equals((IItemInfo)b).Should().BeFalse();
        }

        [TestMethod]
        public void Equals_DifferentSentDate_ReturnsFalse()
        {
            var a = CreatePopulatedHelper();
            var b = CreatePopulatedHelper();
            b.SentDate = new DateTime(2020, 1, 1);
            a.Equals((IItemInfo)b).Should().BeFalse();
        }

        [TestMethod]
        public void Equals_MatchingValues_ReturnsTrue()
        {
            var a = CreatePopulatedHelper();
            var b = CreatePopulatedHelper();
            a.Equals((IItemInfo)b).Should().BeTrue();
        }

        #endregion

        #region RecipientsEquivalent

        [TestMethod]
        public void RecipientsEquivalent_BothNull_ReturnsTrue()
        {
            var helper = new MailItemHelper();
            helper.RecipientsEquivalent(null, null).Should().BeTrue();
        }

        [TestMethod]
        public void RecipientsEquivalent_SourceNull_ReturnsFalse()
        {
            var helper = new MailItemHelper();
            var r = new Mock<IRecipientInfo>().Object;
            helper.RecipientsEquivalent(null, new[] { r }).Should().BeFalse();
        }

        [TestMethod]
        public void RecipientsEquivalent_OtherNull_ReturnsFalse()
        {
            var helper = new MailItemHelper();
            var r = new Mock<IRecipientInfo>().Object;
            helper.RecipientsEquivalent(new[] { r }, null).Should().BeFalse();
        }

        [TestMethod]
        public void RecipientsEquivalent_DifferentLength_ReturnsFalse()
        {
            var helper = new MailItemHelper();
            var r = new Mock<IRecipientInfo>().Object;
            helper
                .RecipientsEquivalent(new[] { r }, Array.Empty<IRecipientInfo>())
                .Should()
                .BeFalse();
        }

        [TestMethod]
        public void RecipientsEquivalent_SameElements_ReturnsTrue()
        {
            var helper = new MailItemHelper();
            var r = new Mock<IRecipientInfo>().Object;
            helper.RecipientsEquivalent(new[] { r }, new[] { r }).Should().BeTrue();
        }

        #endregion

        #region ToMatchableObject

        [TestMethod]
        public void ToMatchableObject_CopiesRelevantFields()
        {
            var helper = CreatePopulatedHelper();
            var result = helper.ToMatchableObject();

            result.Should().NotBeNull();
            result.Size.Should().Be(helper.Size);
            result.Subject.Should().Be(helper.Subject);
            result.SentDate.Should().Be(helper.SentDate);
            result.Body.Should().Be(helper.Body);
            result.EntryId.Should().Be(helper.EntryId);
            result.StoreId.Should().Be(helper.StoreId);
        }

        #endregion

        #region ToSerializableObject

        [TestMethod]
        public void ToSerializableObject_ReturnsItemInfo()
        {
            var helper = CreatePopulatedHelper();
            var result = helper.ToSerializableObject();
            result.Should().NotBeNull();
        }

        #endregion

        #region CompressPlainText additional paths

        [TestMethod]
        public void CompressPlainText_StripReplyHeader_RemovesReplyHeaderOnly()
        {
            const string text =
                "Hello\r\nFrom: Person\r\nSent: Date\r\nTo: Other\r\nSubject: Re: Topic\r\nOlder text";

            var result = MailItemHelper.CompressPlainText(
                text,
                IItemInfo.PlainTextOptionsEnum.StripReplyHeader,
                ""
            );

            result.Should().Contain("Hello");
        }

        [TestMethod]
        public void CompressPlainText_StripReplyBody_RemovesBody()
        {
            const string text =
                "Hello\r\nFrom: Person\r\nSent: Date\r\nTo: Other\r\nSubject: Re: Topic\r\nOlder text";

            var result = MailItemHelper.CompressPlainText(
                text,
                IItemInfo.PlainTextOptionsEnum.StripReplyBody,
                ""
            );

            result.Should().Contain("Hello");
        }

        [TestMethod]
        public void CompressPlainText_ShowStripped_InsertsTags()
        {
            const string text = "Hello <https://test.example>";

            var result = MailItemHelper.CompressPlainText(
                text,
                IItemInfo.PlainTextOptionsEnum.StripLinks
                    | IItemInfo.PlainTextOptionsEnum.ShowStripped,
                ""
            );

            result.Should().Contain("<link>");
        }

        #endregion

        #region ToggleDark explicit state

        [TestMethod]
        public void ToggleDark_WithExplicitState_On_AddsDarkStyle()
        {
            var helper = CreateHelper();
            SetLazyField(helper, "_html", "<html><head></head><body>Body</body></html>");

            var result = helper.ToggleDark(Enums.ToggleState.On);
            result.Should().Contain("filter: invert(100%)");
        }

        [TestMethod]
        public void ToggleDark_WithExplicitState_Off_WhenAlreadyOff_NoChange()
        {
            var helper = CreateHelper();
            SetLazyField(helper, "_html", "<html><head></head><body>Body</body></html>");

            var result = helper.ToggleDark(Enums.ToggleState.Off);
            result.Should().NotContain("filter: invert(100%)");
        }

        [TestMethod]
        public void ToggleDark_WithExplicitState_OnThenOff_RestoresOriginal()
        {
            var helper = CreateHelper();
            var original = "<html><head></head><body>Body</body></html>";
            SetLazyField(helper, "_html", original);

            helper.ToggleDark(Enums.ToggleState.On);
            var result = helper.ToggleDark(Enums.ToggleState.Off);
            result.Should().Be(original);
        }

        #endregion

        #region ResolveFolderRoot

        [TestMethod]
        public void ResolveFolderRoot_WhenPathDoesNotContainArchive_ReturnsInbox()
        {
            var archiveRoot = new Mock<Outlook.Folder>();
            var inbox = new Mock<Outlook.Folder>();
            var globals = CreateGlobals(archiveRoot.Object, inbox.Object, "\\Archive");

            var result = MailItemHelper.ResolveFolderRoot(globals.Object, "\\Inbox\\Projects");
            result.Should().BeSameAs(inbox.Object);
        }

        #endregion

        #region PropertyChanged

        [TestMethod]
        public void PropertyChanged_RaisedWhenRecipientNameSet()
        {
            var helper = new MailItemHelper();
            bool raised = false;
            helper.PropertyChanged += (s, e) => raised = true;

            helper.ToRecipientsName = "Test";
            raised.Should().BeTrue();
        }

        [TestMethod]
        public void PropertyChanged_RaisedWhenCcNameSet()
        {
            var helper = new MailItemHelper();
            bool raised = false;
            helper.PropertyChanged += (s, e) => raised = true;

            helper.CcRecipientsName = "Test";
            raised.Should().BeTrue();
        }

        #endregion

        #region Helpers

        private static MailItemHelper CreateHelper()
        {
#pragma warning disable SYSLIB0050
            return (MailItemHelper)FormatterServices.GetUninitializedObject(typeof(MailItemHelper));
#pragma warning restore SYSLIB0050
        }

        private static MailItemHelper CreatePopulatedHelper()
        {
            var helper = new MailItemHelper();
            helper.Size = 100;
            helper.SentDate = new DateTime(2026, 3, 20);
            helper.Subject = "Test";
            helper.Body = "Body text";
            helper.EntryId = "entry1";
            helper.StoreId = "store1";
            return helper;
        }

        private static Mock<IApplicationGlobals> CreateGlobals(
            Outlook.Folder archiveRoot,
            Outlook.Folder inbox,
            string archiveRootPath
        )
        {
            var olObjects = new Mock<IOlObjects>();
            olObjects.SetupGet(x => x.ArchiveRoot).Returns(archiveRoot);
            olObjects.SetupGet(x => x.Inbox).Returns(inbox);
            olObjects.SetupGet(x => x.ArchiveRootPath).Returns(archiveRootPath);

            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.Ol).Returns(olObjects.Object);
            return globals;
        }

        private static void SetField(MailItemHelper helper, string fieldName, object value)
        {
            var field =
                typeof(MailItemHelper).GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic
                ) ?? throw new MissingFieldException(typeof(MailItemHelper).FullName, fieldName);
            field.SetValue(helper, value);
        }

        private static void SetLazyField<T>(MailItemHelper helper, string fieldName, T value)
        {
            SetField(helper, fieldName, new Lazy<T>(() => value));
        }

        #endregion
    }
}
