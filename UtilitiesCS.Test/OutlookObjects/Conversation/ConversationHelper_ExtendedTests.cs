using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Data.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
            string result = PadOrTruncHelper("VeryLongFieldName", 10, ConvHelper.Justify.Right, ' ');
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
        public void PadOrTrunc_CenterJustify_LongString_TruncatesWithDots()
        {
            string result = PadOrTruncHelper("VeryLongFieldName", 10, ConvHelper.Justify.Center, ' ');
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
            return (string)method.Invoke(null, new object[] { fieldName, fieldWidth, justification, paddingChar });
        }

        #endregion

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
            System.Action act = () => ConvHelper.GetMailItemList(null, "storeId", mockApp.Object, true);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetMailItemList_Strict_MissingEntryIDColumn_ThrowsArgumentOutOfRangeException()
        {
            var df = new DataFrame(new StringDataFrameColumn("Other", new[] { "val" }));
            var mockApp = new Mock<Outlook.Application>();
            System.Action act = () => ConvHelper.GetMailItemList(df, "storeId", mockApp.Object, true);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void GetMailItemList_Strict_EmptyDf_ThrowsArgumentOutOfRangeException()
        {
            var df = new DataFrame(new StringDataFrameColumn("EntryID", 0));
            var mockApp = new Mock<Outlook.Application>();
            System.Action act = () => ConvHelper.GetMailItemList(df, "storeId", mockApp.Object, true);
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
            IList result = ConvHelper.GetMailItemList((DataFrame)null, "storeId", mockApp.Object);
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
        public void GetMailItemList_NoBoolOverload_DfWithEntryIDColumn_ReturnsEmptyList()
        {
            // The non-strict overload has a logic issue in its OR condition:
            // it returns empty when EntryID column IS present
            var df = new DataFrame(new StringDataFrameColumn("EntryID", new[] { "id1" }));
            var mockApp = new Mock<Outlook.Application>();
            IList result = ConvHelper.GetMailItemList(df, "storeId", mockApp.Object);
            result.Count.Should().Be(0);
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
                new StringDataFrameColumn("MessageClass", new[] { "IPM.Note", "IPM.Note", "IPM.Other" })
            );
            DataFrame result = ConvHelper.FilterConversation(df, "Inbox", true, false);
            result.Rows.Count.Should().Be(2);
        }

        [TestMethod]
        public void FilterConversation_MailOnlyTrue_FiltersToIPMNote()
        {
            var df = new DataFrame(
                new StringDataFrameColumn("Folder Name", new[] { "Inbox", "Inbox", "Inbox" }),
                new StringDataFrameColumn("MessageClass", new[] { "IPM.Note", "IPM.Other", "IPM.Note" })
            );
            DataFrame result = ConvHelper.FilterConversation(df, "Inbox", false, true);
            result.Rows.Count.Should().Be(2);
        }

        [TestMethod]
        public void FilterConversation_BothFilters_FiltersToFolderAndMail()
        {
            var df = new DataFrame(
                new StringDataFrameColumn("Folder Name", new[] { "Inbox", "Sent", "Inbox" }),
                new StringDataFrameColumn("MessageClass", new[] { "IPM.Note", "IPM.Note", "IPM.Other" })
            );
            DataFrame result = ConvHelper.FilterConversation(df, "Inbox", true, true);
            result.Rows.Count.Should().Be(1);
        }

        #endregion

        #region GetConversation (object extension)

        [TestMethod]
        public void GetConversation_NullObject_ReturnsNull()
        {
            object obj = null;
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
            act.Should().Throw<System.Reflection.TargetInvocationException>()
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
            object item = null;
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
            object item = null;
            DataFrame result = ConvHelper.GetConversationDf(item);
            result.Should().BeNull();
        }

        #endregion

        #region GetConversationDf (Conversation overload)

        [TestMethod]
        public void GetConversationDf_NullConversation_ReturnsNull()
        {
            Outlook.Conversation conv = null;
            DataFrame result = ConvHelper.GetConversationDf(conv);
            result.Should().BeNull();
        }

        #endregion

        #region GetTable (Conversation, WithFolder, WithStore)

        [TestMethod]
        public void GetTable_NullConversation_ReturnsNull()
        {
            Outlook.Conversation conv = null;
            Outlook.Table result = ConvHelper.GetTable(conv, true, true);
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetTable_NullConversation_NoFlags_ReturnsNull()
        {
            Outlook.Conversation conv = null;
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
    }
}
