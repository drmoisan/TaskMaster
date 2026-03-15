using System;
using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;

namespace UtilitiesCS.Test.OutlookObjects.MailItemCoverage
{
    [TestClass]
    public class MailItemHelperCoreTests
    {
        [TestMethod]
        public void ResolveFolderRoot_WhenFolderPathContainsArchiveRoot_ShouldReturnArchiveRoot()
        {
            var archiveRoot = new Mock<OutlookFolder>();
            var inbox = new Mock<OutlookFolder>();
            var globals = CreateGlobals(archiveRoot.Object, inbox.Object, "\\Archive");

            var result = MailItemHelper.ResolveFolderRoot(globals.Object, "\\Archive\\Projects");

            result.Should().BeSameAs(archiveRoot.Object);
        }

        [TestMethod]
        public void CompressPlainText_ShouldStripConfiguredSectionsAndAppendEndMarker()
        {
            const string text = "WARNING\r\nHello <https://example.test>\r\nFrom: Person\r\nSubject: Re: Status\r\nOlder content";

            var result = MailItemHelper.CompressPlainText(
                text,
                IItemInfo.PlainTextOptionsEnum.StripWarning |
                IItemInfo.PlainTextOptionsEnum.StripLinks |
                IItemInfo.PlainTextOptionsEnum.StripReplyBody |
                IItemInfo.PlainTextOptionsEnum.StripFormatting,
                "WARNING");

            result.Should().StartWith("Hello");
            result.Should().NotContain("WARNING");
            result.Should().NotContain("https://example.test");
            result.Should().EndWith("<EOM>");
        }

        [TestMethod]
        public void ToggleDark_ShouldInjectAndThenRemoveDarkModeHeader()
        {
            var helper = CreateHelper();
            SetLazyField(helper, "_html", "<html><head></head><body>Body</body></html>");

            var darkHtml = helper.ToggleDark();
            var restoredHtml = helper.ToggleDark();

            darkHtml.Should().Contain("filter: invert(100%)");
            restoredHtml.Should().Be("<html><head></head><body>Body</body></html>");
        }

        [TestMethod]
        public void GetHtml_ShouldInjectEmailHeaderIntoBodyMarkup()
        {
            var mailItem = new Mock<MailItem>();
            mailItem.SetupGet(x => x.HTMLBody).Returns("<html><head></head><body>Original</body></html>");
            var helper = CreateHelper();
            SetField(helper, "_item", mailItem.Object);
            SetLazyField(helper, "_html", "<html><head></head><body>Original</body></html>");
            SetLazyField(helper, "_senderHtml", "Sender");
            SetLazyField(helper, "_sentOn", "5/2/2026 12:00 AM");
            SetLazyField(helper, "_toRecipientsHtml", "To User");
            SetLazyField(helper, "_ccRecipientsHtml", "Cc User");
            SetLazyField(helper, "_subject", "Planning");

            var result = helper.GetHtml("ignored");

            result.Should().Contain("<b>From:</b>Sender");
            result.Should().Contain("Original");
        }

        [TestMethod]
        public void RecipientsEquivalent_ShouldHandleNullAndMismatchedArrays()
        {
            var helper = CreateHelper();
            var recipient = new Mock<IRecipientInfo>().Object;

            helper.RecipientsEquivalent(null, null).Should().BeTrue();
            helper.RecipientsEquivalent(new[] { recipient }, null).Should().BeFalse();
            helper.RecipientsEquivalent(new[] { recipient }, Array.Empty<IRecipientInfo>()).Should().BeFalse();
        }

        private static Mock<IApplicationGlobals> CreateGlobals(OutlookFolder archiveRoot, OutlookFolder inbox, string archiveRootPath)
        {
            var olObjects = new Mock<IOlObjects>();
            olObjects.SetupGet(x => x.ArchiveRoot).Returns(archiveRoot);
            olObjects.SetupGet(x => x.Inbox).Returns(inbox);
            olObjects.SetupGet(x => x.ArchiveRootPath).Returns(archiveRootPath);

            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.Ol).Returns(olObjects.Object);
            return globals;
        }

        private static MailItemHelper CreateHelper()
        {
#pragma warning disable SYSLIB0050
            return (MailItemHelper)FormatterServices.GetUninitializedObject(typeof(MailItemHelper));
#pragma warning restore SYSLIB0050
        }

        private static void SetField(MailItemHelper helper, string fieldName, object value)
        {
            var field = typeof(MailItemHelper).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new MissingFieldException(typeof(MailItemHelper).FullName, fieldName);
            field.SetValue(helper, value);
        }

        private static void SetLazyField<T>(MailItemHelper helper, string fieldName, T value)
        {
            SetField(helper, fieldName, new Lazy<T>(() => value));
        }
    }
}