using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class DfDeedle_Tests
    {
        [TestMethod]
        public void FromArray2D_EmailLikeArray_ReturnsExpectedRowCountAndColumnLayout()
        {
            object[,] data =
            {
                { "id-1", "IPM.Note", "2024-01-01", "conv-1", "A", "store-1" },
                { "id-2", "IPM.Note", "2024-01-02", "conv-2", "B", "store-1" },
            };
            var columnDictionary = new Dictionary<string, int>
            {
                ["EntryID"] = 0,
                ["MessageClass"] = 1,
                ["SentOn"] = 2,
                ["ConversationId"] = 3,
                ["Triage"] = 4,
                ["StoreId"] = 5,
            };

            var df = DfDeedle.FromArray2D(data, columnDictionary);

            df.Should().NotBeNull();
            df.RowCount.Should().Be(2);
            df.ColumnKeys.Should()
                .Equal("EntryID", "MessageClass", "SentOn", "ConversationId", "Triage", "StoreId");
        }
    }
}
