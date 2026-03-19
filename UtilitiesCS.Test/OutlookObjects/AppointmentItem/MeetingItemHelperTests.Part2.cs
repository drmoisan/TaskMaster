using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.HelperClasses;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookRecipient = Microsoft.Office.Interop.Outlook.Recipient;

namespace UtilitiesCS.Test.OutlookObjects.AppointmentItemCoverage
{
    public partial class MeetingItemHelperTests
    {
        [TestMethod]
        public void PropertySetters_ShouldRoundTripAssignedValues()
        {
            var helper = CreateHelper();
            var appointment = new Mock<Microsoft.Office.Interop.Outlook.AppointmentItem>().Object;
            var globals = CreateGlobals(
                new Mock<OutlookFolder>().Object,
                new Mock<OutlookFolder>().Object,
                "\\Archive"
            );
            var folderInfo = new Mock<IFolderWrapper>().Object;
            var item = new Mock<MeetingItem>().Object;
            var sender = new RecipientInfo("Ada Lovelace", "ada@example.com", "Ada Html");
            var sentDate = new DateTime(2026, 5, 2, 9, 30, 0);

            helper.Appointment = appointment;
            helper.Actionable = "Respond";
            helper.Body = "Body";
            helper.Categories = "Blue";
            helper.ConversationID = "conv-42";
            helper.EmailPrefixToStrip = "WARNING";
            helper.EntryId = "entry-id";
            helper.Globals = globals.Object;
            helper.StoreId = "store-id";
            helper.FolderInfo = folderInfo;
            helper.FolderName = "Calendar";
            helper.Item = item;
            helper.PlainTextOptions = IItemInfo.PlainTextOptionsEnum.StripFormatting;
            helper.SentOn = "5/2/2026 9:30 AM";
            helper.Subject = "Planning";
            helper.SenderHtml = "Ada Html";
            helper.SenderName = "Ada Lovelace";
            helper.Sender = sender;
            helper.Size = 42;
            helper.Triage = "Now";
            helper.SentDate = sentDate;
            helper.InternetCodepage = 65001;
            helper.IsTaskFlagSet = true;

            helper.Appointment.Should().BeSameAs(appointment);
            helper.Actionable.Should().Be("Respond");
            helper.Body.Should().Be("Body");
            helper.Categories.Should().Be("Blue");
            helper.ConversationID.Should().Be("conv-42");
            helper.EmailPrefixToStrip.Should().Be("WARNING");
            helper.EntryId.Should().Be("entry-id");
            helper.Globals.Should().BeSameAs(globals.Object);
            helper.StoreId.Should().Be("store-id");
            helper.FolderInfo.Should().BeSameAs(folderInfo);
            helper.FolderName.Should().Be("Calendar");
            helper.Item.Should().BeSameAs(item);
            helper.PlainTextOptions.Should().Be(IItemInfo.PlainTextOptionsEnum.StripFormatting);
            helper.SentOn.Should().Be("5/2/2026 9:30 AM");
            helper.Subject.Should().Be("Planning");
            helper.SenderHtml.Should().Be("Ada Html");
            helper.SenderName.Should().Be("Ada Lovelace");
            helper.Sender.Should().BeSameAs(sender);
            helper.Size.Should().Be(42);
            helper.Triage.Should().Be("Now");
            helper.SentDate.Should().Be(sentDate);
            helper.InternetCodepage.Should().Be(65001);
            helper.IsTaskFlagSet.Should().BeTrue();
        }

        [TestMethod]
        public void PropertyChangedSetters_WhenSubscriberPresent_ShouldRaiseNotifications()
        {
            var helper = CreateHelper();
            string? propertyName = null;
            helper.PropertyChanged += (_, args) => propertyName = args.PropertyName;

            helper.CcRecipientsName = "Alan Turing";

            propertyName.Should().Be(nameof(MeetingItemHelper.CcRecipientsName));
        }

        [TestMethod]
        public void GetHeadersExtendedMapi_ShouldReturnPropertyAccessorValue()
        {
            var propertyAccessor = new Mock<PropertyAccessor>();
            propertyAccessor
                .Setup(x => x.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x007D001F/"))
                .Returns("headers");
            var meetingItem = new Mock<MeetingItem>();
            meetingItem.SetupGet(x => x.PropertyAccessor).Returns(propertyAccessor.Object);
            var helper = CreateHelper();
            SetField(helper, "_item", meetingItem.Object);

            var result = helper.GetHeadersExtendedMapi();

            result.Should().Be("headers");
        }

        [TestMethod]
        public void Tokenizer_ShouldCacheResolvedInstance()
        {
            var helper = CreateHelper();

            var first = helper.Tokenizer;
            var second = helper.Tokenizer;

            first.Should().BeSameAs(second);
            first.Should().BeOfType<EmailTokenizer>();
        }

        [TestMethod]
        public void UnReadSetter_ShouldPersistValueToMeetingItem()
        {
            var meetingItem = new Mock<MeetingItem>();
            meetingItem.SetupProperty(x => x.UnRead, false);
            var helper = CreateHelper();
            SetField(helper, "_item", meetingItem.Object);

            helper.UnRead = true;

            helper.UnRead.Should().BeTrue();
            meetingItem.Object.UnRead.Should().BeTrue();
            meetingItem.Verify(x => x.Save(), Times.Once);
        }

        [TestMethod]
        public void LoadInternetCodepage_ShouldReturnAppointmentInternetCodepage()
        {
            var appointment = new Mock<Microsoft.Office.Interop.Outlook.AppointmentItem>();
            appointment.SetupGet(x => x.InternetCodepage).Returns(65001);
            var helper = CreateHelper();
            helper.Appointment = appointment.Object;

            var result = (int)
                typeof(MeetingItemHelper)
                    .GetMethod(
                        "LoadInternetCodepage",
                        BindingFlags.Instance | BindingFlags.NonPublic
                    )
                    .Invoke(helper, null);

            result.Should().Be(65001);
        }

        [TestMethod]
        public void Equals_ShouldThrowNotImplementedException()
        {
            var helper = CreateHelper();

            System.Action act = () => helper.Equals(new Mock<IItemInfo>().Object);

            act.Should().Throw<NotImplementedException>();
        }

        [TestMethod]
        public void ToMatchableObject_ShouldProjectMatchRelevantFields()
        {
            var sender = new Mock<IRecipientInfo>().Object;
            var cc = new[] { new Mock<IRecipientInfo>().Object };
            var to = new[] { new Mock<IRecipientInfo>().Object };
            var helper = CreateHelper();
            SetLazyField(helper, "_sender", sender);
            SetLazyField(helper, "_ccRecipients", cc);
            SetLazyField(helper, "_toRecipients", to);
            SetLazyField(helper, "_subject", "Planning");
            SetLazyField(helper, "_body", "Summary");
            SetLazyField(helper, "_entryId", "entry");
            SetLazyField(helper, "_storeId", "store");
            SetLazyField(helper, "_sentDate", new DateTime(2026, 5, 2));
            SetLazyField(helper, "_size", 128);

            var result = helper.ToMatchableObject();

            result.Sender.Should().BeSameAs(sender);
            result.CcRecipients.Should().HaveCount(1);
            result.CcRecipients[0].Should().BeSameAs(cc[0]);
            result.ToRecipients.Should().HaveCount(1);
            result.ToRecipients[0].Should().BeSameAs(to[0]);
            result.Subject.Should().Be("Planning");
            result.Body.Should().Be("Summary");
            result.EntryId.Should().Be("entry");
            result.StoreId.Should().Be("store");
            result.SentDate.Should().Be(new DateTime(2026, 5, 2));
            result.Size.Should().Be(128);
        }

        private static Mock<IApplicationGlobals> CreateGlobals(
            OutlookFolder archiveRoot,
            OutlookFolder inbox,
            string archiveRootPath
        )
        {
            var olObjects = new Mock<IOlObjects>();
            olObjects.SetupGet(x => x.ArchiveRoot).Returns(archiveRoot);
            olObjects.SetupGet(x => x.Inbox).Returns(inbox);
            olObjects.SetupGet(x => x.ArchiveRootPath).Returns(archiveRootPath);
            olObjects.SetupGet(x => x.EmailPrefixToStrip).Returns("WARNING");

            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.Ol).Returns(olObjects.Object);
            return globals;
        }

        private static (
            MeetingItemHelper Helper,
            Mock<IApplicationGlobals> Globals,
            Mock<OutlookFolder> ArchiveRoot
        ) CreateConfiguredLoadAllFixture()
        {
            var archiveRoot = new Mock<OutlookFolder>();
            archiveRoot.SetupGet(x => x.FolderPath).Returns("\\Archive");
            var inbox = new Mock<OutlookFolder>();
            inbox.SetupGet(x => x.FolderPath).Returns("\\Inbox");
            var parentFolder = new Mock<OutlookFolder>();
            parentFolder.SetupGet(x => x.StoreID).Returns("store-id");
            parentFolder.SetupGet(x => x.Name).Returns("Calendar");
            parentFolder.SetupGet(x => x.FolderPath).Returns("\\Archive\\Calendar");
            var userProperties = new Mock<UserProperties>();
            userProperties.Setup(x => x.Find("Triage", true)).Returns((UserProperty)null!);
            var recipients = CreateRecipientsCollection(
                CreateRecipient(
                    "Grace Hopper",
                    "grace@example.com",
                    OlMailRecipientType.olTo
                ).Object,
                CreateRecipient("Alan Turing", "alan@example.com", OlMailRecipientType.olCC).Object
            );
            var attachments = new Mock<Attachments>();
            attachments.SetupGet(x => x.Count).Returns(0);
            var meetingItem = new Mock<MeetingItem>();
            meetingItem.SetupGet(x => x.EntryID).Returns("entry-id");
            meetingItem.SetupGet(x => x.Categories).Returns("Blue");
            meetingItem.SetupGet(x => x.SenderName).Returns("Ada Lovelace");
            meetingItem.SetupGet(x => x.SenderEmailAddress).Returns("ada@example.com");
            meetingItem.SetupGet(x => x.Body).Returns("WARNING\r\nBody text");
            meetingItem.SetupGet(x => x.ConversationID).Returns("conversation-id");
            meetingItem.SetupGet(x => x.Parent).Returns(parentFolder.Object);
            meetingItem.SetupGet(x => x.Recipients).Returns(recipients.Object);
            meetingItem.SetupGet(x => x.SentOn).Returns(new DateTime(2026, 5, 2, 9, 30, 0));
            meetingItem.SetupGet(x => x.Subject).Returns("Planning");
            meetingItem
                .SetupGet(x => x.RTFBody)
                .Returns("<html><head></head><body>Original</body></html>");
            meetingItem.SetupGet(x => x.UserProperties).Returns(userProperties.Object);
            meetingItem.SetupGet(x => x.Attachments).Returns(attachments.Object);
            var globals = CreateGlobals(archiveRoot.Object, inbox.Object, "\\Archive");

            return (
                new MeetingItemHelper(meetingItem.Object, globals.Object),
                globals,
                archiveRoot
            );
        }

        private static object GetPrivateFieldValue(MeetingItemHelper helper, string fieldName)
        {
            return typeof(MeetingItemHelper)
                    .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(helper)
                ?? throw new MissingFieldException(typeof(MeetingItemHelper).FullName, fieldName);
        }

        private static Mock<OutlookRecipient> CreateRecipient(
            string name,
            string address,
            OlMailRecipientType recipientType
        )
        {
            var recipient = new Mock<OutlookRecipient>();
            var addressEntry = new Mock<AddressEntry>();

            recipient.SetupGet(x => x.Name).Returns(name);
            recipient.SetupGet(x => x.Address).Returns(address);
            recipient.SetupGet(x => x.Type).Returns((int)recipientType);
            recipient.SetupGet(x => x.AddressEntry).Returns(addressEntry.Object);
            addressEntry
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olOutlookContactAddressEntry);

            return recipient;
        }

        private static Mock<Recipients> CreateRecipientsCollection(
            params OutlookRecipient[] recipients
        )
        {
            var collection = new ArrayList(recipients);
            var recipientsCollection = new Mock<Recipients>();
            recipientsCollection
                .Setup(x => x.GetEnumerator())
                .Returns(() => collection.GetEnumerator());
            return recipientsCollection;
        }

        private static int CountOccurrences(string input, string value)
        {
            var count = 0;
            var index = 0;

            while ((index = input.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }

            return count;
        }

        private sealed class MeetingItemHelperCopyProbe : MeetingItemHelper
        {
            public MeetingItemHelperCopyProbe(IItemInfo itemInfo)
                : base(itemInfo) { }
        }

        private static MeetingItemHelper CreateHelper()
        {
#pragma warning disable SYSLIB0050
            return (MeetingItemHelper)
                FormatterServices.GetUninitializedObject(typeof(MeetingItemHelper));
#pragma warning restore SYSLIB0050
        }

        private static void SetField(MeetingItemHelper helper, string fieldName, object value)
        {
            var field =
                typeof(MeetingItemHelper).GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic
                ) ?? throw new MissingFieldException(typeof(MeetingItemHelper).FullName, fieldName);
            field.SetValue(helper, value);
        }

        private static void SetLazyField<T>(MeetingItemHelper helper, string fieldName, T value)
        {
            SetField(helper, fieldName, new Lazy<T>(() => value));
        }
    }
}
