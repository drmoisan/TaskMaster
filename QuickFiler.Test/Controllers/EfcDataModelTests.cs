using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Helper_Classes;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class EfcDataModelTests
    {
        /// <summary>
        /// Locks in the Phase 4 refactor boundary for first-selection model creation.
        /// `CreateAsync` should make the UI-thread snapshot load explicit before any
        /// background-safe initialization stage begins.
        /// </summary>
        [TestMethod]
        public async Task CreateAsync_StagesSnapshotLoadBeforeBackgroundInitialization()
        {
            var globals = CreateGlobals();
            var firstMail = CreateMailItem("entry-1", "Subject 1");
            var secondMail = CreateMailItem("entry-2", "Subject 2");
            var replacementMail = CreateMailItem("entry-3", "Replacement");
            var liveSelection = new List<MailItem> { firstMail.Object, secondMail.Object };

            var createTask = EfcDataModel.CreateAsync(
                globals.Object,
                liveSelection,
                new CancellationTokenSource(),
                CancellationToken.None,
                loadAll: false
            );

            liveSelection.Clear();
            liveSelection.Add(replacementMail.Object);

            var dataModel = await createTask;

            dataModel.Mail.Should().BeSameAs(firstMail.Object);
            dataModel
                .ConversationResolver.ConversationItems.Expanded.Should()
                .Equal(firstMail.Object, secondMail.Object);
        }

        /// <summary>
        /// Covers the single-selection constructor path that routes through the one-mail
        /// conversation loader instead of the collection overload.
        /// </summary>
        [TestMethod]
        public async Task CreateAsync_WithSingleSelectedMail_UsesSingleMailConversationResolverPath()
        {
            var globals = CreateGlobals();
            var onlyMail = CreateMailItem("entry-1", "Subject 1");
            var folder = new Mock<Folder>(MockBehavior.Strict);
            var conversation = new Mock<Conversation>(MockBehavior.Loose);
            var table = CreateConversationTable();
            var selection = new List<MailItem> { onlyMail.Object };

            folder.SetupGet(x => x.Name).Returns("Inbox");
            onlyMail.SetupGet(x => x.Parent).Returns(folder.Object);
            onlyMail.Setup(x => x.GetConversation()).Returns(conversation.Object);
            conversation.Setup(x => x.GetTable()).Returns(table.Object);

            var dataModel = await EfcDataModel.CreateAsync(
                globals.Object,
                selection,
                new CancellationTokenSource(),
                CancellationToken.None,
                loadAll: false
            );

            dataModel.Mail.Should().BeSameAs(onlyMail.Object);
            dataModel.ConversationResolver.Should().NotBeNull();
            dataModel.ConversationResolver.Mail.Should().BeSameAs(onlyMail.Object);
        }

        [TestMethod]
        public async Task LoadConversationInfoAsync_WhenGlobalsDoNotExposeOutlookApp_DoesNotRequireApp()
        {
            var globals = CreateGlobals();
            var folder = new Mock<Folder>(MockBehavior.Strict);
            var conversation = new Mock<Conversation>(MockBehavior.Loose);
            var table = CreateConversationTable();
            var mailItem = CreateMailItem("entry-1", "Subject 1");

            folder.SetupGet(x => x.Name).Returns("Inbox");
            folder.SetupGet(x => x.FolderPath).Returns("\\Archive\\Inbox");
            mailItem.SetupGet(x => x.Parent).Returns(folder.Object);
            mailItem.Setup(x => x.GetConversation()).Returns(conversation.Object);
            conversation.Setup(x => x.GetTable()).Returns(table.Object);

            var helper = await MailItemHelper.FromMailItemAsync(
                mailItem.Object,
                globals.Object,
                CancellationToken.None,
                loadAll: false
            );
            var resolver = new ConversationResolver(globals.Object, mailItem.Object)
            {
                MailHelper = helper,
            };

            await resolver.LoadDfAsync(CancellationToken.None, backgroundLoad: false);

            var info = await resolver.LoadConversationInfoAsync(
                CancellationToken.None,
                backgroundLoad: false
            );

            info.Expanded.Should().ContainSingle();
            info.Expanded[0].Should().BeSameAs(helper);
        }

        [TestMethod]
        public async Task CreateAsync_WithSingleSelectedMail_LeavesBackgroundInitializationStaged()
        {
            var globals = CreateGlobals();
            var onlyMail = CreateMailItem("entry-1", "Subject 1");
            var folder = new Mock<Folder>(MockBehavior.Strict);
            var conversation = new Mock<Conversation>(MockBehavior.Loose);
            var table = CreateConversationTable();
            var selection = new List<MailItem> { onlyMail.Object };

            folder.SetupGet(x => x.Name).Returns("Inbox");
            folder.SetupGet(x => x.FolderPath).Returns("\\Archive\\Inbox");
            onlyMail.SetupGet(x => x.Parent).Returns(folder.Object);
            onlyMail.Setup(x => x.GetConversation()).Returns(conversation.Object);
            conversation.Setup(x => x.GetTable()).Returns(table.Object);

            var dataModel = await EfcDataModel.CreateAsync(
                globals.Object,
                selection,
                new CancellationTokenSource(),
                CancellationToken.None,
                loadAll: false
            );

            SpinWait
                .SpinUntil(() => dataModel.ConversationResolver.FullyLoaded, 250)
                .Should()
                .BeFalse();
            dataModel.ConversationResolver.FullyLoaded.Should().BeFalse();
        }

        [TestMethod]
        public void Constructor_WhenMailProvided_LeavesBackgroundInitializationStaged()
        {
            var globals = CreateGlobals();
            var folder = new Mock<Folder>(MockBehavior.Strict);
            var conversation = new Mock<Conversation>(MockBehavior.Loose);
            var table = CreateConversationTable();
            var mailItem = CreateMailItem("entry-1", "Subject 1");

            folder.SetupGet(x => x.Name).Returns("Inbox");
            folder.SetupGet(x => x.FolderPath).Returns("\\Archive\\Inbox");
            mailItem.SetupGet(x => x.Parent).Returns(folder.Object);
            mailItem.Setup(x => x.GetConversation()).Returns(conversation.Object);
            conversation.Setup(x => x.GetTable()).Returns(table.Object);

            var dataModel = new EfcDataModel(
                globals.Object,
                mailItem.Object,
                new CancellationTokenSource(),
                CancellationToken.None
            );

            SpinWait
                .SpinUntil(() => dataModel.ConversationResolver.FullyLoaded, 250)
                .Should()
                .BeFalse();
            dataModel.ConversationResolver.FullyLoaded.Should().BeFalse();
        }

        /// <summary>
        /// Locks in the synchronous constructor snapshot contract used by the first-selection
        /// controller path. Supplying a mail item should eagerly build a resolver and snapshot
        /// dataframe without deferring the initial conversation lookup.
        /// </summary>
        [TestMethod]
        public void Constructor_WhenMailProvided_LoadsConversationSnapshotSynchronously()
        {
            var globals = CreateGlobals();
            var folder = new Mock<Folder>(MockBehavior.Strict);
            folder.SetupGet(x => x.Name).Returns("Inbox");

            var mailItem = CreateMailItem("entry-1", "Subject 1");
            var conversation = new Mock<Conversation>(MockBehavior.Loose);
            var table = CreateConversationTable();

            mailItem.SetupGet(x => x.Parent).Returns(folder.Object);
            mailItem.Setup(x => x.GetConversation()).Returns(conversation.Object);
            conversation.Setup(x => x.GetTable()).Returns(table.Object);

            var dataModel = new EfcDataModel(
                globals.Object,
                mailItem.Object,
                new CancellationTokenSource(),
                CancellationToken.None
            );

            dataModel.Mail.Should().BeSameAs(mailItem.Object);
            dataModel.ConversationResolver.Should().NotBeNull();
            var dataframePair = GetPropertyValue(dataModel.ConversationResolver, "Df");
            var expanded = GetPropertyValue(dataframePair, "Expanded");
            var sameFolder = GetPropertyValue(dataframePair, "SameFolder");

            expanded.Should().NotBeNull();
            sameFolder.Should().NotBeNull();
            GetDataFrameRowCount(expanded).Should().Be(1);
            GetDataFrameRowCount(sameFolder).Should().Be(1);
            conversation.Verify(x => x.GetTable(), Times.Once);
        }

        private static Mock<IApplicationGlobals> CreateGlobals()
        {
            var olObjects = new Mock<IOlObjects>(MockBehavior.Strict);
            olObjects.SetupGet(x => x.EmailPrefixToStrip).Returns(string.Empty);

            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            globals.SetupGet(x => x.Ol).Returns(olObjects.Object);
            return globals;
        }

        private static Mock<MailItem> CreateMailItem(string entryId, string subject)
        {
            var sender = CreateAddressEntry("Ada Sender", "ada@example.com");
            var toRecipient = CreateRecipient(
                "To User",
                "to@example.com",
                (int)OlMailRecipientType.olTo
            );
            var recipients = CreateRecipients(toRecipient.Object);
            var attachments = CreateAttachments();

            var mailItem = new Mock<MailItem>(MockBehavior.Strict);
            mailItem.SetupGet(x => x.EntryID).Returns(entryId);
            mailItem.SetupGet(x => x.ConversationID).Returns("conversation-1");
            mailItem.SetupGet(x => x.Subject).Returns(subject);
            mailItem.SetupGet(x => x.Body).Returns("Body");
            mailItem.SetupGet(x => x.HTMLBody).Returns("<html><body>Body</body></html>");
            mailItem.SetupGet(x => x.SenderName).Returns("Ada Sender");
            mailItem.SetupGet(x => x.SenderEmailAddress).Returns("ada@example.com");
            mailItem.SetupGet(x => x.Sender).Returns(sender.Object);
            mailItem.SetupGet(x => x.Recipients).Returns(recipients.Object);
            mailItem.SetupGet(x => x.Attachments).Returns(attachments.Object);
            mailItem.SetupGet(x => x.InternetCodepage).Returns(65001);
            return mailItem;
        }

        private static Mock<AddressEntry> CreateAddressEntry(string name, string address)
        {
            var propertyAccessor = new Mock<PropertyAccessor>(MockBehavior.Loose);
            var addressEntry = new Mock<AddressEntry>(MockBehavior.Strict);
            addressEntry
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olSmtpAddressEntry);
            addressEntry.SetupGet(x => x.Name).Returns(name);
            addressEntry.SetupGet(x => x.Address).Returns(address);
            addressEntry.SetupGet(x => x.PropertyAccessor).Returns(propertyAccessor.Object);
            return addressEntry;
        }

        private static Mock<Recipient> CreateRecipient(string name, string address, int type)
        {
            var propertyAccessor = new Mock<PropertyAccessor>(MockBehavior.Loose);
            var addressEntry = CreateAddressEntry(name, address);
            var recipient = new Mock<Recipient>(MockBehavior.Strict);
            recipient.SetupGet(x => x.Name).Returns(name);
            recipient.SetupGet(x => x.Address).Returns(address);
            recipient.SetupGet(x => x.Type).Returns(type);
            recipient.SetupGet(x => x.AddressEntry).Returns(addressEntry.Object);
            recipient.SetupGet(x => x.PropertyAccessor).Returns(propertyAccessor.Object);
            return recipient;
        }

        private static Mock<Recipients> CreateRecipients(params Recipient[] recipients)
        {
            var recipientsMock = new Mock<Recipients>(MockBehavior.Strict);
            recipientsMock.SetupGet(x => x.Count).Returns(recipients.Length);
            recipientsMock
                .Setup(x => x.GetEnumerator())
                .Returns(() => ((IEnumerable)recipients).GetEnumerator());
            return recipientsMock;
        }

        private static Mock<Attachments> CreateAttachments()
        {
            var attachments = new Mock<Attachments>(MockBehavior.Strict);
            attachments.SetupGet(x => x.Count).Returns(0);
            attachments
                .Setup(x => x.GetEnumerator())
                .Returns(() => ((IEnumerable)Array.Empty<Attachment>()).GetEnumerator());
            return attachments;
        }

        private static Mock<Table> CreateConversationTable()
        {
            var table = new Mock<Table>(MockBehavior.Strict);
            var columns = new Mock<Columns>(MockBehavior.Strict);
            var row = new Mock<Row>(MockBehavior.Strict);
            var data = new object[,]
            {
                {
                    "2024-01-01",
                    "Inbox",
                    "Sender",
                    "sender@example.com",
                    "SMTP",
                    "entry-1",
                    "store-1",
                    0,
                    "conv-1",
                },
            };
            var rawValues = new object[]
            {
                "2024-01-01",
                "Inbox",
                "Sender",
                "sender@example.com",
                "SMTP",
                "entry-1",
                "store-raw",
                0,
                "conv-raw",
            };
            var columnNames = new[]
            {
                "SentOn",
                "Folder Name",
                "SenderName",
                "SenderSmtpAddress",
                "SenderAddrType",
                "EntryID",
                "Store",
                "ConversationDepth",
                "ConversationIndex",
            };

            table.SetupGet(x => x.Columns).Returns(columns.Object);
            var currentRow = 0;
            table.Setup(x => x.MoveToStart()).Callback(() => currentRow = 0);
            table.Setup(x => x.GetRowCount()).Returns(1);
            table.Setup(x => x.GetArray(1)).Returns(data);
            table.Setup(x => x.EndOfTable).Returns(() => currentRow >= 1);
            table
                .Setup(x => x.GetNextRow())
                .Returns(() =>
                {
                    currentRow++;
                    return row.Object;
                });
            columns.Setup(x => x.Count).Returns(columnNames.Length);
            columns.Setup(x => x.Remove(It.IsAny<object>()));
            columns
                .Setup(x => x.Add(It.IsAny<string>()))
                .Returns(() => new Mock<Column>(MockBehavior.Loose).Object);
            row.Setup(x => x.GetValues()).Returns(rawValues);
            row.Setup(x => x.BinaryToString(7)).Returns("store-1");
            row.Setup(x => x.BinaryToString(9)).Returns("conv-1");

            for (var index = 0; index < columnNames.Length; index++)
            {
                var column = new Mock<Column>(MockBehavior.Strict);
                column.SetupGet(x => x.Name).Returns(columnNames[index]);
                columns.Setup(x => x[index + 1]).Returns(column.Object);
            }

            return table;
        }

        private static object GetPropertyValue(object target, string propertyName)
        {
            var property = target.GetType().GetProperty(propertyName);
            property.Should().NotBeNull($"property '{propertyName}' should exist");
            if (property == null)
            {
                Assert.Fail($"Property '{propertyName}' should exist.");
            }

            return property.GetValue(target);
        }

        private static long GetDataFrameRowCount(object dataFrame)
        {
            var rows = GetPropertyValue(dataFrame, "Rows");
            var countProperty = rows.GetType().GetProperty("Count");
            if (countProperty != null)
            {
                return System.Convert.ToInt64(countProperty.GetValue(rows));
            }

            var countMethod = rows.GetType().GetMethod("Count", System.Type.EmptyTypes);
            countMethod.Should().NotBeNull("dataframe rows should expose Count semantics");
            if (countMethod == null)
            {
                Assert.Fail("Dataframe rows should expose Count semantics.");
            }

            return System.Convert.ToInt64(countMethod.Invoke(rows, null));
        }
    }
}
