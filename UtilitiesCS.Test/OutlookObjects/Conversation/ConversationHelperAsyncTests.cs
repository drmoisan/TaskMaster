using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Fields;

namespace UtilitiesCS.Test.OutlookObjects.Conversation
{
    [TestClass]
    public class ConversationHelperAsyncTests
    {
        [TestMethod]
        public async Task GetConversationDfAsync_WhenConversationSnapshotLoads_ReturnsDataFrame()
        {
            var mailItem = new Mock<Microsoft.Office.Interop.Outlook.MailItem>(MockBehavior.Loose);
            var conversation = new Mock<Microsoft.Office.Interop.Outlook.Conversation>(
                MockBehavior.Loose
            );
            var table = CreateConversationTable();

            mailItem.Setup(x => x.GetConversation()).Returns(conversation.Object);
            conversation.Setup(x => x.GetTable()).Returns(table.Object);

            var result = await ConvHelper.GetConversationDfAsync(
                mailItem.Object,
                CancellationToken.None
            );

            result.Should().NotBeNull();
            result.Rows.Count.Should().Be(1);
            result.Columns["SentOn"].Should().NotBeNull();
            conversation.Verify(x => x.GetTable(), Times.Once);
        }

        [TestMethod]
        public async Task GetConversationDfAsync_RetryableOverload_ReturnsDataFrameFromConversationSnapshot()
        {
            var mailItem = new Mock<Microsoft.Office.Interop.Outlook.MailItem>(MockBehavior.Loose);
            var conversation = new Mock<Microsoft.Office.Interop.Outlook.Conversation>(
                MockBehavior.Loose
            );
            var table = CreateConversationTable();

            mailItem.Setup(x => x.GetConversation()).Returns(conversation.Object);
            conversation.Setup(x => x.GetTable()).Returns(table.Object);

            var result = await ConvHelper.GetConversationDfAsync(
                mailItem.Object,
                CancellationToken.None,
                timeout: 1000,
                retryCount: 0,
                options: TaskCreationOptions.None,
                scheduler: TaskScheduler.Default
            );

            result.Should().NotBeNull();
            result.Rows.Count.Should().Be(1);
            result.Columns["EntryID"].Should().NotBeNull();
            conversation.Verify(x => x.GetTable(), Times.Once);
        }

        private static Mock<Microsoft.Office.Interop.Outlook.Table> CreateConversationTable()
        {
            var table = new Mock<Microsoft.Office.Interop.Outlook.Table>(MockBehavior.Strict);
            var columns = new Mock<Microsoft.Office.Interop.Outlook.Columns>(MockBehavior.Strict);
            var row = new Mock<Microsoft.Office.Interop.Outlook.Row>(MockBehavior.Strict);
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
                MAPIFields.Schemas.FolderName,
                MAPIFields.Schemas.SenderName,
                MAPIFields.Schemas.SenderSmtpAddress,
                MAPIFields.Schemas.SenderAddrType,
                "EntryID",
                MAPIFields.Schemas.MessageStore,
                MAPIFields.Schemas.ConversationDepth,
                MAPIFields.Schemas.ConversationIndex,
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
                .Returns(() =>
                    new Mock<Microsoft.Office.Interop.Outlook.Column>(MockBehavior.Loose).Object
                );
            row.Setup(x => x.GetValues()).Returns(rawValues);
            row.Setup(x => x.BinaryToString(7)).Returns("store-1");
            row.Setup(x => x.BinaryToString(9)).Returns("conv-1");

            for (var index = 0; index < columnNames.Length; index++)
            {
                var column = new Mock<Microsoft.Office.Interop.Outlook.Column>(MockBehavior.Strict);
                column.SetupGet(x => x.Name).Returns(columnNames[index]);
                columns.Setup(x => x[index + 1]).Returns(column.Object);
            }

            return table;
        }
    }
}
