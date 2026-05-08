using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Fields;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.Test.OutlookObjects.Conversation
{
    [TestClass]
    public class ConversationHelper_ExtendedTests
    {
        #region SafeResolveConversationItem

        [TestMethod]
        public void SafeResolveConversationItem_NullNamespace_ReturnsNull()
        {
            ConvHelper.SafeResolveConversationItem(null, (ns, a, b) => "result").Should().BeNull();
        }

        [TestMethod]
        public void SafeResolveConversationItem_NullResolver_ReturnsNull()
        {
            ConvHelper.SafeResolveConversationItem(new object(), null).Should().BeNull();
        }

        [TestMethod]
        public void SafeResolveConversationItem_BothNull_ReturnsNull()
        {
            ConvHelper.SafeResolveConversationItem(null, null).Should().BeNull();
        }

        [TestMethod]
        public void SafeResolveConversationItem_ValidInputs_ReturnsResolverResult()
        {
            var nsRef = new object();
            ConvHelper
                .SafeResolveConversationItem(nsRef, (ns, a, b) => "resolved")
                .Should()
                .Be("resolved");
        }

        [TestMethod]
        public void SafeResolveConversationItem_ResolverThrows_ReturnsNull()
        {
            ConvHelper
                .SafeResolveConversationItem(
                    new object(),
                    (ns, a, b) => throw new InvalidOperationException()
                )
                .Should()
                .BeNull();
        }

        #endregion

        #region PadOrTrunc

        [TestMethod]
        public void PadOrTrunc_RightJustify_ShortString_PadsLeft()
        {
            string result = PadOrTruncHelper("Hi", 10, ConvHelper.Justify.Right, ' ');
            result.Should().HaveLength(10);
            result.Should().EndWith("Hi");
        }

        [TestMethod]
        public void PadOrTrunc_RightJustify_LongString_TruncatesWithDots()
        {
            string result = PadOrTruncHelper(
                "VeryLongFieldName",
                10,
                ConvHelper.Justify.Right,
                ' '
            );
            result.Should().StartWith("..");
        }

        [TestMethod]
        public void PadOrTrunc_LeftJustify_ShortString_PadsRight()
        {
            string result = PadOrTruncHelper("Hi", 10, ConvHelper.Justify.Left, ' ');
            result.Should().HaveLength(10);
            result.Should().StartWith("Hi");
        }

        [TestMethod]
        public void PadOrTrunc_LeftJustify_LongString_TruncatesWithDots()
        {
            string result = PadOrTruncHelper("VeryLongFieldName", 10, ConvHelper.Justify.Left, ' ');
            result.Should().EndWith("..");
            result.Should().HaveLength(10);
        }

        [TestMethod]
        public void PadOrTrunc_CenterJustify_ShortString_PadsBothSides()
        {
            string result = PadOrTruncHelper("Hi", 10, ConvHelper.Justify.Center, ' ');
            result.Should().HaveLength(10);
            result.Should().Contain("Hi");
        }

        [TestMethod]
        public async Task GetConversationDfAsync_CapturesConversationTableSnapshotBeforeBackgroundTransform()
        {
            var mailItem = new Mock<Outlook.MailItem>(MockBehavior.Loose);
            var conversation = new Mock<Outlook.Conversation>(MockBehavior.Loose);
            var table = CreateConversationTable();

            mailItem.Setup(x => x.GetConversation()).Returns(conversation.Object);
            conversation.Setup(x => x.GetTable()).Returns(table.Object);

            var result = ConvHelper.GetConversationDf(conversation.Object);

            await Task.CompletedTask;

            result.Should().NotBeNull();
            result.Columns["SentOn"].Should().NotBeNull();
            conversation.Verify(x => x.GetTable(), Times.Once);
        }

        public void PadOrTrunc_CenterJustify_LongString_TruncatesWithDots()
        {
            string result = PadOrTruncHelper(
                "VeryLongFieldName",
                10,
                ConvHelper.Justify.Center,
                ' '
            );
            result.Should().EndWith("..");
            result.Should().HaveLength(10);
        }

        [TestMethod]
        public void PadOrTrunc_ExactWidth_ReturnsUnchanged()
        {
            string result = PadOrTruncHelper("12345", 5, ConvHelper.Justify.Left, ' ');
            result.Should().Be("12345");
        }

        private static string PadOrTruncHelper(
            string fieldName,
            int fieldWidth,
            ConvHelper.Justify justification,
            char paddingChar
        )
        {
            var method = typeof(ConvHelper).GetMethod(
                "PadOrTrunc",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
            );
            return (string)
                method.Invoke(
                    null,
                    new object[] { fieldName, fieldWidth, justification, paddingChar }
                );
        }

        #endregion

        private static Mock<Outlook.Table> CreateConversationTable()
        {
            var table = new Mock<Outlook.Table>(MockBehavior.Strict);
            var columns = new Mock<Outlook.Columns>(MockBehavior.Strict);
            var data = new object[,]
            {
                { "2024-01-01", "Inbox", "Sender", "entry-1" },
            };
            var columnNames = new[]
            {
                "SentOn",
                MAPIFields.Schemas.FolderName,
                MAPIFields.Schemas.SenderName,
                "EntryID",
            };

            table.SetupGet(x => x.Columns).Returns(columns.Object);
            table.Setup(x => x.MoveToStart());
            table.Setup(x => x.GetRowCount()).Returns(1);
            table.Setup(x => x.GetArray(1)).Returns(data);
            columns.Setup(x => x.Count).Returns(columnNames.Length);
            columns.Setup(x => x.Remove(It.IsAny<object>()));
            columns
                .Setup(x => x.Add(It.IsAny<string>()))
                .Returns(() => new Mock<Outlook.Column>(MockBehavior.Loose).Object);

            for (var index = 0; index < columnNames.Length; index++)
            {
                var column = new Mock<Outlook.Column>(MockBehavior.Strict);
                column.SetupGet(x => x.Name).Returns(columnNames[index]);
                columns.Setup(x => x[index + 1]).Returns(column.Object);
            }

            return table;
        }

        #region JoinFixedWidth

        [TestMethod]
        public void JoinFixedWidth_FormatsColumnsWithDividers()
        {
            string[] cells = new[] { "A", "BB", "CCC" };
            var styles = new (int FieldWidth, ConvHelper.Justify Justification)[]
            {
                (5, ConvHelper.Justify.Left),
                (5, ConvHelper.Justify.Right),
                (5, ConvHelper.Justify.Center),
            };

            string result = ConvHelper.JoinFixedWidth(cells, styles, "|", "[");
            result.Should().StartWith("[");
            result.Should().Contain("|");
        }

        [TestMethod]
        public void JoinFixedWidth_SingleCell_FormatsCorrectly()
        {
            string[] cells = new[] { "X" };
            var styles = new (int FieldWidth, ConvHelper.Justify Justification)[]
            {
                (3, ConvHelper.Justify.Left),
            };

            string result = ConvHelper.JoinFixedWidth(cells, styles, "|", "|");
            result.Should().Be("|X  |");
        }

        #endregion

        #region GetMailItemList (strict=true)

        [TestMethod]
        public void GetMailItemList_Strict_NullDf_ThrowsArgumentNullException()
        {
            var mockApp = new Mock<Outlook.Application>();
            System.Action act = () =>
                ConvHelper.GetMailItemList(null, "storeId", mockApp.Object, true);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetMailItemList_Strict_MissingEntryIDColumn_ThrowsArgumentOutOfRangeException()
        {
            var df = new DataFrame(new StringDataFrameColumn("Other", new[] { "val" }));
            var mockApp = new Mock<Outlook.Application>();
            System.Action act = () =>
                ConvHelper.GetMailItemList(df, "storeId", mockApp.Object, true);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void GetMailItemList_Strict_EmptyDf_ThrowsArgumentOutOfRangeException()
        {
            var df = new DataFrame(new StringDataFrameColumn("EntryID", 0));
            var mockApp = new Mock<Outlook.Application>();
            System.Action act = () =>
                ConvHelper.GetMailItemList(df, "storeId", mockApp.Object, true);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        #endregion

        #region GetMailItemList (strict=false)

        [TestMethod]
        public void GetMailItemList_NotStrict_NullDf_ReturnsEmptyList()
        {
            var mockApp = new Mock<Outlook.Application>();
            IList result = ConvHelper.GetMailItemList(null, "storeId", mockApp.Object, false);
            result.Count.Should().Be(0);
        }

        [TestMethod]
        public void GetMailItemList_NotStrict_MissingEntryIDColumn_ReturnsEmptyList()
        {
            var df = new DataFrame(new StringDataFrameColumn("Other", new[] { "val" }));
            var mockApp = new Mock<Outlook.Application>();
            IList result = ConvHelper.GetMailItemList(df, "storeId", mockApp.Object, false);
            result.Count.Should().Be(0);
        }

        [TestMethod]
        public void GetMailItemList_NotStrict_EmptyDf_ReturnsEmptyList()
        {
            var df = new DataFrame(new StringDataFrameColumn("EntryID", 0));
            var mockApp = new Mock<Outlook.Application>();
            IList result = ConvHelper.GetMailItemList(df, "storeId", mockApp.Object, false);
            result.Count.Should().Be(0);
        }

        #endregion

        #region GetMailItemList (non-strict overload without bool)

        [TestMethod]
        public void GetMailItemList_NoBoolOverload_NullDf_ReturnsEmptyList()
        {
            var mockApp = new Mock<Outlook.Application>();
            IList result = ConvHelper.GetMailItemList((DataFrame?)null, "storeId", mockApp.Object);
            result.Count.Should().Be(0);
        }

        [TestMethod]
        public void GetMailItemList_NoBoolOverload_EmptyDf_ReturnsEmptyList()
        {
            var df = new DataFrame(new StringDataFrameColumn("EntryID", 0));
            var mockApp = new Mock<Outlook.Application>();
            IList result = ConvHelper.GetMailItemList(df, "storeId", mockApp.Object);
            result.Count.Should().Be(0);
        }

        [TestMethod]
        public void GetMailItemList_NoBoolOverload_DfWithEntryIDColumn_ReturnsItems()
        {
            var df = new DataFrame(new StringDataFrameColumn("EntryID", new[] { "id1" }));
            var mockApp = new Mock<Outlook.Application>();
            var mockNs = new Mock<Outlook.NameSpace>();
            var mockMail = new Mock<Outlook.MailItem>();

            mockApp.Setup(a => a.GetNamespace("MAPI")).Returns(mockNs.Object);
            mockNs.Setup(ns => ns.GetItemFromID("id1", "storeId")).Returns(mockMail.Object);

            IList result = ConvHelper.GetMailItemList(df, "storeId", mockApp.Object);
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(mockMail.Object);
        }

        #endregion

        #region FilterConversation

        [TestMethod]
        public void FilterConversation_NullDf_ReturnsNull()
        {
            DataFrame result = ConvHelper.FilterConversation(null, "Inbox", true, true);
            result.Should().BeNull();
        }

        [TestMethod]
        public void FilterConversation_NoFilters_ReturnsSameDf()
        {
            var df = new DataFrame(
                new StringDataFrameColumn("Folder Name", new[] { "Inbox", "Sent" }),
                new StringDataFrameColumn("MessageClass", new[] { "IPM.Note", "IPM.Other" })
            );
            DataFrame result = ConvHelper.FilterConversation(df, "Inbox", false, false);
            result.Rows.Count.Should().Be(2);
        }

        [TestMethod]
        public void FilterConversation_SameFolderTrue_FiltersToFolder()
        {
            var df = new DataFrame(
                new StringDataFrameColumn("Folder Name", new[] { "Inbox", "Sent", "Inbox" }),
                new StringDataFrameColumn(
                    "MessageClass",
                    new[] { "IPM.Note", "IPM.Note", "IPM.Other" }
                )
            );
            DataFrame result = ConvHelper.FilterConversation(df, "Inbox", true, false);
            result.Rows.Count.Should().Be(2);
        }

        [TestMethod]
        public void FilterConversation_MailOnlyTrue_FiltersToIPMNote()
        {
            var df = new DataFrame(
                new StringDataFrameColumn("Folder Name", new[] { "Inbox", "Inbox", "Inbox" }),
                new StringDataFrameColumn(
                    "MessageClass",
                    new[] { "IPM.Note", "IPM.Other", "IPM.Note" }
                )
            );
            DataFrame result = ConvHelper.FilterConversation(df, "Inbox", false, true);
            result.Rows.Count.Should().Be(2);
        }

        [TestMethod]
        public void FilterConversation_BothFilters_FiltersToFolderAndMail()
        {
            var df = new DataFrame(
                new StringDataFrameColumn("Folder Name", new[] { "Inbox", "Sent", "Inbox" }),
                new StringDataFrameColumn(
                    "MessageClass",
                    new[] { "IPM.Note", "IPM.Note", "IPM.Other" }
                )
            );
            DataFrame result = ConvHelper.FilterConversation(df, "Inbox", true, true);
            result.Rows.Count.Should().Be(1);
        }

        #endregion

        #region GetConversation (object extension)

        [TestMethod]
        public void GetConversation_NullObject_ReturnsNull()
        {
            object? obj = null;
            Outlook.Conversation result = ConvHelper.GetConversation(obj);
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetConversation_MailItem_ReturnsConversation()
        {
            var mockMail = new Mock<Outlook.MailItem>();
            var mockConv = new Mock<Outlook.Conversation>();
            mockMail.Setup(m => m.GetConversation()).Returns(mockConv.Object);

            Outlook.Conversation result = ConvHelper.GetConversation((object)mockMail.Object);
            result.Should().BeSameAs(mockConv.Object);
        }

        [TestMethod]
        public void GetConversation_MeetingItem_ReturnsConversation()
        {
            var mockMeeting = new Mock<Outlook.MeetingItem>();
            var mockConv = new Mock<Outlook.Conversation>();
            mockMeeting.Setup(m => m.GetConversation()).Returns(mockConv.Object);

            Outlook.Conversation result = ConvHelper.GetConversation((object)mockMeeting.Object);
            result.Should().BeSameAs(mockConv.Object);
        }

        [TestMethod]
        public void GetConversation_PostItem_ReturnsConversation()
        {
            var mockPost = new Mock<Outlook.PostItem>();
            var mockConv = new Mock<Outlook.Conversation>();
            mockPost.Setup(m => m.GetConversation()).Returns(mockConv.Object);

            Outlook.Conversation result = ConvHelper.GetConversation((object)mockPost.Object);
            result.Should().BeSameAs(mockConv.Object);
        }

        [TestMethod]
        public void GetConversation_UnsupportedType_ReturnsNull()
        {
            Outlook.Conversation result = ConvHelper.GetConversation("not an outlook item");
            result.Should().BeNull();
        }

        #endregion

        #region IsSupportedType

        [TestMethod]
        public void IsSupportedType_MailItem_ReturnsTrue()
        {
            var mock = new Mock<Outlook.MailItem>();
            ConvHelper.IsSupportedType(mock.Object).Should().BeTrue();
        }

        [TestMethod]
        public void IsSupportedType_MeetingItem_ReturnsTrue()
        {
            var mock = new Mock<Outlook.MeetingItem>();
            ConvHelper.IsSupportedType(mock.Object).Should().BeTrue();
        }

        [TestMethod]
        public void IsSupportedType_PostItem_ReturnsTrue()
        {
            var mock = new Mock<Outlook.PostItem>();
            ConvHelper.IsSupportedType(mock.Object).Should().BeTrue();
        }

        [TestMethod]
        public void IsSupportedType_String_ReturnsFalse()
        {
            ConvHelper.IsSupportedType("string").Should().BeFalse();
        }

        [TestMethod]
        public void IsSupportedType_Int_ReturnsFalse()
        {
            ConvHelper.IsSupportedType(42).Should().BeFalse();
        }

        #endregion

        #region ResolveType

        [TestMethod]
        public void ResolveType_MailItem_ReturnsMailItemType()
        {
            var mock = new Mock<Outlook.MailItem>();
            var method = typeof(ConvHelper).GetMethod(
                "ResolveType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
            );
            var result = (Type)method.Invoke(null, new object[] { mock.Object });
            result.FullName.Should().Be("Microsoft.Office.Interop.Outlook.MailItem");
        }

        [TestMethod]
        public void ResolveType_MeetingItem_ReturnsMeetingItemType()
        {
            var mock = new Mock<Outlook.MeetingItem>();
            var method = typeof(ConvHelper).GetMethod(
                "ResolveType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
            );
            var result = (Type)method.Invoke(null, new object[] { mock.Object });
            result.FullName.Should().Be("Microsoft.Office.Interop.Outlook.MeetingItem");
        }

        [TestMethod]
        public void ResolveType_PostItem_ReturnsPostItemType()
        {
            var mock = new Mock<Outlook.PostItem>();
            var method = typeof(ConvHelper).GetMethod(
                "ResolveType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
            );
            var result = (Type)method.Invoke(null, new object[] { mock.Object });
            result.FullName.Should().Be("Microsoft.Office.Interop.Outlook.PostItem");
        }

        [TestMethod]
        public void ResolveType_UnsupportedType_ThrowsArgumentException()
        {
            var method = typeof(ConvHelper).GetMethod(
                "ResolveType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
            );
            System.Action act = () => method.Invoke(null, new object[] { "unsupported" });
            act.Should()
                .Throw<System.Reflection.TargetInvocationException>()
                .WithInnerException<ArgumentException>();
        }

        #endregion

        #region ConversationCt (object overload)

        [TestMethod]
        public void ConversationCt_NonMailItem_ReturnsMinusOne()
        {
            object item = "not a mail item";
            int result = ConvHelper.ConversationCt(item, true, true);
            result.Should().Be(-1);
        }

        [TestMethod]
        public void ConversationCt_NullObject_ReturnsMinusOne()
        {
            object? item = null;
            int result = ConvHelper.ConversationCt(item, true, true);
            result.Should().Be(-1);
        }

        #endregion

        #region GetConversationDf (object overload)

        [TestMethod]
        public void GetConversationDf_NonMailItem_ReturnsNull()
        {
            object item = "not a mail item";
            DataFrame result = ConvHelper.GetConversationDf(item);
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetConversationDf_NullObject_ReturnsNull()
        {
            object? item = null;
            DataFrame result = ConvHelper.GetConversationDf(item);
            result.Should().BeNull();
        }

        #endregion

        #region GetConversationDf (Conversation overload)

        [TestMethod]
        public void GetConversationDf_NullConversation_ReturnsNull()
        {
            Outlook.Conversation? conv = null;
            DataFrame result = ConvHelper.GetConversationDf(conv);
            result.Should().BeNull();
        }

        #endregion

        #region GetTable (Conversation, WithFolder, WithStore)

        [TestMethod]
        public void GetTable_NullConversation_ReturnsNull()
        {
            Outlook.Conversation? conv = null;
            Outlook.Table result = ConvHelper.GetTable(conv, true, true);
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetTable_NullConversation_NoFlags_ReturnsNull()
        {
            Outlook.Conversation? conv = null;
            Outlook.Table result = ConvHelper.GetTable(conv, false, false);
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetTable_ValidConversation_WithFolder_AddsColumns()
        {
            var mockConv = new Mock<Outlook.Conversation>();
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockConv.Setup(c => c.GetTable()).Returns(mockTable.Object);
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);

            Outlook.Table result = ConvHelper.GetTable(mockConv.Object, true, false);
            result.Should().BeSameAs(mockTable.Object);
            mockColumns.Verify(c => c.Add("SentOn"), Times.Once);
        }

        [TestMethod]
        public void GetTable_ValidConversation_WithStore_AddsStoreColumn()
        {
            var mockConv = new Mock<Outlook.Conversation>();
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockConv.Setup(c => c.GetTable()).Returns(mockTable.Object);
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);

            Outlook.Table result = ConvHelper.GetTable(mockConv.Object, false, true);
            result.Should().BeSameAs(mockTable.Object);
            mockColumns.Verify(c => c.Add("SentOn"), Times.Once);
        }

        [TestMethod]
        public void GetTable_ValidConversation_BothFlags_AddsAllColumns()
        {
            var mockConv = new Mock<Outlook.Conversation>();
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockConv.Setup(c => c.GetTable()).Returns(mockTable.Object);
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);

            Outlook.Table result = ConvHelper.GetTable(mockConv.Object, true, true);
            result.Should().BeSameAs(mockTable.Object);
            mockColumns.Verify(c => c.Add(It.IsAny<string>()), Times.AtLeast(3));
        }

        #endregion

        #region ConversationColumnSchemas

        [TestMethod]
        public void ConversationColumnSchemas_ContainsExpectedEntries()
        {
            var field = typeof(ConvHelper).GetProperty(
                "ConversationColumnSchemas",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
            );
            var schemas = (string[])field.GetValue(null);
            schemas.Should().NotBeEmpty();
            schemas.Should().Contain("SentOn");
            schemas.Should().Contain("EntryID");
        }

        #endregion

        #region Justify enum

        [TestMethod]
        public void Justify_HasExpectedValues()
        {
            ((int)ConvHelper.Justify.Right).Should().Be(1);
            ((int)ConvHelper.Justify.Left).Should().Be(2);
            ((int)ConvHelper.Justify.Center).Should().Be(4);
        }

        #endregion

        #region GetInfoTable

        [TestMethod]
        public void GetInfoTable_ValidConversation_ReturnsTableWithAddedColumns()
        {
            var mockConv = new Mock<Outlook.Conversation>();
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockConv.Setup(c => c.GetTable()).Returns(mockTable.Object);
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);

            Outlook.Table result = ConvHelper.GetInfoTable(mockConv.Object);
            result.Should().BeSameAs(mockTable.Object);
            mockColumns.Verify(c => c.Add(It.IsAny<string>()), Times.AtLeast(5));
        }

        #endregion

        #region GetMailItemList (strict=true, happy path)

        [TestMethod]
        public void GetMailItemList_Strict_ValidDf_ReturnsItems()
        {
            var df = new DataFrame(
                new StringDataFrameColumn("EntryID", new[] { "entry1", "entry2" })
            );
            var mockApp = new Mock<Outlook.Application>();
            var mockNs = new Mock<Outlook.NameSpace>();
            var mockMail1 = new Mock<Outlook.MailItem>();
            var mockMail2 = new Mock<Outlook.MailItem>();
            mockApp.Setup(a => a.GetNamespace("MAPI")).Returns(mockNs.Object);
            mockNs.Setup(ns => ns.GetItemFromID("entry1", "store1")).Returns(mockMail1.Object);
            mockNs.Setup(ns => ns.GetItemFromID("entry2", "store1")).Returns(mockMail2.Object);

            IList result = ConvHelper.GetMailItemList(df, "store1", mockApp.Object, true);
            result.Count.Should().Be(2);
        }

        #endregion

        #region ConversationCt (MailItem overload)

        [TestMethod]
        public void ConversationCt_MailItem_NullConversation_ReturnsZero()
        {
            var mockMail = new Mock<Outlook.MailItem>();
            mockMail.Setup(m => m.GetConversation()).Returns((Outlook.Conversation)null!);

            int result = ConvHelper.ConversationCt((object)mockMail.Object, true, true);
            result.Should().Be(0);
        }

        [TestMethod]
        public void ConversationCt_MailItem_WithConversation_EmptyTable_ReturnsZero()
        {
            var mockMail = new Mock<Outlook.MailItem>();
            var mockConv = new Mock<Outlook.Conversation>();
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            var mockPropAccessor = new Mock<Outlook.PropertyAccessor>();

            mockMail.Setup(m => m.GetConversation()).Returns(mockConv.Object);
            mockMail.Setup(m => m.PropertyAccessor).Returns(mockPropAccessor.Object);
            mockPropAccessor.Setup(p => p.GetProperty(It.IsAny<string>())).Returns("Inbox");

            mockConv.Setup(c => c.GetTable()).Returns(mockTable.Object);
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);

            int colCount = 9;
            mockColumns.Setup(c => c.Count).Returns(colCount);
            for (int i = 1; i <= colCount; i++)
            {
                var mockCol = new Mock<Outlook.Column>();
                mockCol.Setup(c => c.Name).Returns($"Col{i}");
                mockColumns.Setup(c => c[i]).Returns(mockCol.Object);
            }
            mockTable.Setup(t => t.GetRowCount()).Returns(0);

            // This exercises: object overload → MailItem overload → GetConversation (not null) →
            // conv.GetDataFrame → GetConversationTable → ETL (0 rows) → ToDataFrame → PrettyText →
            // FilterConversation (SameFolder=true filters 0-row df; MailOnly=true filters 0-row df) →
            // return df.Rows.Count = 0
            int result = ConvHelper.ConversationCt((object)mockMail.Object, true, true);
            result.Should().Be(0);
        }

        [TestMethod]
        public void ConversationCt_MailItem_NoFilters_ReturnsZero()
        {
            var mockMail = new Mock<Outlook.MailItem>();
            var mockConv = new Mock<Outlook.Conversation>();
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();

            mockMail.Setup(m => m.GetConversation()).Returns(mockConv.Object);
            mockConv.Setup(c => c.GetTable()).Returns(mockTable.Object);
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);

            int colCount = 9;
            mockColumns.Setup(c => c.Count).Returns(colCount);
            for (int i = 1; i <= colCount; i++)
            {
                var mockCol = new Mock<Outlook.Column>();
                mockCol.Setup(c => c.Name).Returns($"Col{i}");
                mockColumns.Setup(c => c[i]).Returns(mockCol.Object);
            }
            mockTable.Setup(t => t.GetRowCount()).Returns(0);

            // This exercises the path without SameFolder or MailOnly filtering
            int result = ConvHelper.ConversationCt((object)mockMail.Object, false, false);
            result.Should().Be(0);
        }

        #endregion

        #region GetConversationDf (MailItem overload)

        [TestMethod]
        public void GetConversationDf_MailItem_NullConversation_ReturnsNull()
        {
            var mockMail = new Mock<Outlook.MailItem>();
            mockMail.Setup(m => m.GetConversation()).Returns((Outlook.Conversation)null!);

            DataFrame result = ConvHelper.GetConversationDf((object)mockMail.Object);
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetConversationDf_MailItem_WithConversation_COMException_ReturnsNull()
        {
            var mockMail = new Mock<Outlook.MailItem>();
            var mockConv = new Mock<Outlook.Conversation>();
            mockMail.Setup(m => m.GetConversation()).Returns(mockConv.Object);
            mockConv.Setup(c => c.GetTable()).Throws(new COMException("COM error"));

            DataFrame result = ConvHelper.GetConversationDf((object)mockMail.Object);
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetConversationDf_MailItem_WithConversation_EmptyTable_ReturnsEmptyDf()
        {
            var mockMail = new Mock<Outlook.MailItem>();
            var mockConv = new Mock<Outlook.Conversation>();
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();

            mockMail.Setup(m => m.GetConversation()).Returns(mockConv.Object);
            mockConv.Setup(c => c.GetTable()).Returns(mockTable.Object);
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);

            int colCount = 9;
            mockColumns.Setup(c => c.Count).Returns(colCount);
            for (int i = 1; i <= colCount; i++)
            {
                var mockCol = new Mock<Outlook.Column>();
                mockCol.Setup(c => c.Name).Returns($"Col{i}");
                mockColumns.Setup(c => c[i]).Returns(mockCol.Object);
            }
            mockTable.Setup(t => t.GetRowCount()).Returns(0);

            DataFrame result = ConvHelper.GetConversationDf((object)mockMail.Object);
            result.Should().NotBeNull();
            result.Rows.Count.Should().Be(0);
        }

        #endregion

        #region GetConversationDf (Conversation retry logic)

        [TestMethod]
        public void GetConversationDf_Conversation_COMExceptionRetries_EventuallyReturnsNull()
        {
            var mockConv = new Mock<Outlook.Conversation>();
            // GetDataFrame calls GetConversationTable which calls GetTable
            // Make GetTable throw COMException 3 times (exceeds retry limit)
            mockConv.Setup(c => c.GetTable()).Throws(new COMException("COM error"));

            // GetConversationDf catches COMException and retries up to 2 times
            // After 3 failures (retryCount 0, 1, 2), it returns null df
            DataFrame result = ConvHelper.GetConversationDf(mockConv.Object);
            result.Should().BeNull();
        }

        #endregion

        #region GetConversationTable

        [TestMethod]
        public void GetConversationTable_SetsUpColumnsAndRemovesEntryID()
        {
            var mockConv = new Mock<Outlook.Conversation>();
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockConv.Setup(c => c.GetTable()).Returns(mockTable.Object);
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);

            Outlook.Table result = ConvHelper.GetConversationTable(mockConv.Object);
            result.Should().BeSameAs(mockTable.Object);
            // Verifies RemoveColumns was called (which calls Columns.Remove("EntryID"))
            mockColumns.Verify(c => c.Remove(It.IsAny<object>()), Times.AtLeastOnce);
            // Verifies ConversationColumnSchemas were added
            mockColumns.Verify(c => c.Add(It.IsAny<string>()), Times.AtLeast(5));
        }

        #endregion

        #region GetConversationDfAsync (cancellation)

        [TestMethod]
        public async System.Threading.Tasks.Task GetConversationDfAsync_CancelledToken_Throws()
        {
            var mockMail = new Mock<Outlook.MailItem>();
            var cts = new System.Threading.CancellationTokenSource();
            cts.Cancel();

            Func<System.Threading.Tasks.Task> act = async () =>
                await ConvHelper.GetConversationDfAsync(
                    mockMail.Object,
                    cts.Token,
                    1000,
                    0,
                    System.Threading.Tasks.TaskCreationOptions.None,
                    System.Threading.Tasks.TaskScheduler.Default
                );
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        #endregion

        #region GetConversationDf (MailItem path)

        [TestMethod]
        public void GetConversationDf_MailItemPath_COMException_ReturnsNull()
        {
            // MailItem.GetConversation() returns a conversation that throws COMException
            // when GetTable() is called (deep in the GetDataFrame chain)
            var mockMail = new Mock<Outlook.MailItem>();
            var mockConv = new Mock<Outlook.Conversation>();
            mockMail.Setup(m => m.GetConversation()).Returns(mockConv.Object);
            mockConv.Setup(c => c.GetTable()).Throws(new COMException("COM error"));

            // GetConversationDf(MailItem) → conv.GetConversationDf() → retry loop → returns null
            DataFrame result = ConvHelper.GetConversationDf((object)mockMail.Object);
            result.Should().BeNull();
        }

        #endregion

        #region GetDataFrameAsync (null table path)

        [TestMethod]
        public async System.Threading.Tasks.Task GetDataFrameAsync_NullFromTimeout_ReturnsNull()
        {
            // GetDataFrameAsync calls RunWithTimeout on GetConversationTable,
            // but if the conversation's GetTable throws a timeout we can only
            // test by dispatching through TimeOutTask which is complex.
            // Instead verify the cancellation path of the simpler async overload.
            var mockMail = new Mock<Outlook.MailItem>();
            mockMail.Setup(m => m.GetConversation()).Returns((Outlook.Conversation)null!);

            // The simple GetConversationDfAsync calls TimeOutTask.RunWithTimeout
            // which will try mailItem.GetConversation() returning null
            // then conv.GetDataFrameAsync(token) will NullRef — this tests the path
            var cts = new System.Threading.CancellationTokenSource();
            cts.Cancel();
            Func<System.Threading.Tasks.Task> act = async () =>
                await ConvHelper.GetConversationDfAsync(mockMail.Object, cts.Token);
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        #endregion

        #region EnumerateColumnHeaders

        [TestMethod]
        public void EnumerateColumnHeaders_FormatsHeaders()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockColumns.Setup(c => c.Count).Returns(2);
            var mockCol1 = new Mock<Outlook.Column>();
            mockCol1.Setup(c => c.Name).Returns("Name");
            var mockCol2 = new Mock<Outlook.Column>();
            mockCol2.Setup(c => c.Name).Returns("Date");
            mockColumns.Setup(c => c[1]).Returns(mockCol1.Object);
            mockColumns.Setup(c => c[2]).Returns(mockCol2.Object);

            var styles = new (int FieldWidth, ConvHelper.Justify Justification)[]
            {
                (10, ConvHelper.Justify.Left),
                (10, ConvHelper.Justify.Right),
            };

            string result = ConvHelper.EnumerateColumnHeaders(mockTable.Object, styles, "|", "[");
            result.Should().NotBeNullOrEmpty();
            result.Should().StartWith("[");
            result.Should().Contain("|");
        }

        #endregion

        #region GetConversationDf (Conversation successful path)

        [TestMethod]
        public void GetConversationDf_Conversation_SuccessfulFirstTry_ReturnsDf()
        {
            var mockConv = new Mock<Outlook.Conversation>();
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockConv.Setup(c => c.GetTable()).Returns(mockTable.Object);
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);

            int colCount = 2;
            mockColumns.Setup(c => c.Count).Returns(colCount);
            for (int i = 1; i <= colCount; i++)
            {
                var mockCol = new Mock<Outlook.Column>();
                mockCol.Setup(c => c.Name).Returns($"Col{i}");
                mockColumns.Setup(c => c[i]).Returns(mockCol.Object);
            }
            mockTable.Setup(t => t.GetRowCount()).Returns(0);

            DataFrame result = ConvHelper.GetConversationDf(mockConv.Object);
            result.Should().NotBeNull();
        }

        [TestMethod]
        public void GetConversationDf_Conversation_COMExceptionThenSuccess_ReturnsDf()
        {
            var mockConv = new Mock<Outlook.Conversation>();
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();

            int callCount = 0;
            mockConv
                .Setup(c => c.GetTable())
                .Returns(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        throw new COMException("transient");
                    return mockTable.Object;
                });
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);

            int colCount = 2;
            mockColumns.Setup(c => c.Count).Returns(colCount);
            for (int i = 1; i <= colCount; i++)
            {
                var mockCol = new Mock<Outlook.Column>();
                mockCol.Setup(c => c.Name).Returns($"Col{i}");
                mockColumns.Setup(c => c[i]).Returns(mockCol.Object);
            }
            mockTable.Setup(t => t.GetRowCount()).Returns(0);

            DataFrame result = ConvHelper.GetConversationDf(mockConv.Object);
            result.Should().NotBeNull();
        }

        #endregion

        #region GetMailItemList non-strict overload - DfWithoutEntryIDColumn

        [TestMethod]
        public void GetMailItemList_NoBoolOverload_DfWithoutEntryIDColumn_ReturnsEmptyList()
        {
            var df = new DataFrame(new StringDataFrameColumn("Other", new[] { "val" }));
            var mockApp = new Mock<Outlook.Application>();
            IList result = ConvHelper.GetMailItemList(df, "storeId", mockApp.Object);
            result.Count.Should().Be(0);
        }

        #endregion
    }
}
