using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.Test.OutlookObjects.Table
{
    [TestClass]
    public class OlTableExtensions_Tests
    {
        #region GetColumnDictionary (static overload)

        [TestMethod]
        public void GetColumnDictionary_BothNull_ReturnsEmptyDictionary()
        {
            var result = OlTableExtensions.GetColumnDictionary(null, null);
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetColumnDictionary_NullNames_ReturnsEmptyDictionary()
        {
            var result = OlTableExtensions.GetColumnDictionary(null, new object[] { 1 });
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetColumnDictionary_NullValues_ReturnsEmptyDictionary()
        {
            var result = OlTableExtensions.GetColumnDictionary(new[] { "a" }, null);
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetColumnDictionary_MatchingArrays_ReturnsMappedPairs()
        {
            var names = new[] { "Col1", "Col2" };
            var values = new object[] { "v1", 42 };

            var result = OlTableExtensions.GetColumnDictionary(names, values);

            result.Should().HaveCount(2);
            result["Col1"].Should().Be("v1");
            result["Col2"].Should().Be(42);
        }

        [TestMethod]
        public void GetColumnDictionary_MismatchedLengths_UsesMinimum()
        {
            var names = new[] { "Col1", "Col2", "Col3" };
            var values = new object[] { "v1" };

            var result = OlTableExtensions.GetColumnDictionary(names, values);
            result.Should().HaveCount(1);
        }

        #endregion

        #region RunTableRetry

        [TestMethod]
        public void RunTableRetry_SucceedsFirstTry_ReturnsResult()
        {
            var result = OlTableExtensions.RunTableRetry(() => 42, 3);
            result.Should().Be(42);
        }

        [TestMethod]
        public void RunTableRetry_FailsAllAttempts_ReturnsDefault()
        {
            var result = OlTableExtensions.RunTableRetry<int>(() => throw new Exception("fail"), 3);
            result.Should().Be(0);
        }

        [TestMethod]
        public void RunTableRetry_FailsThenSucceeds_ReturnsResult()
        {
            int callCount = 0;
            var result = OlTableExtensions.RunTableRetry(
                () =>
                {
                    callCount++;
                    if (callCount < 3)
                        throw new Exception("fail");
                    return 99;
                },
                5
            );
            result.Should().Be(99);
        }

        [TestMethod]
        public void RunTableRetry_ZeroAttempts_TriesOnce()
        {
            var result = OlTableExtensions.RunTableRetry(() => 7, 0);
            result.Should().Be(7);
        }

        #endregion

        #region ToObjectRow

        [TestMethod]
        public void ToObjectRow_NullValues_ReturnsEmptyArray()
        {
            var result = OlTableExtensions.ToObjectRow(null);
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void ToObjectRow_ValidValues_ReturnsSameArray()
        {
            var input = new object[] { 1, "two", 3.0 };
            var result = OlTableExtensions.ToObjectRow(input);
            result.Should().BeSameAs(input);
        }

        #endregion

        #region RemoveColumns

        [TestMethod]
        public void RemoveColumns_NullTable_NoException()
        {
            System.Action act = () => OlTableExtensions.RemoveColumns(null, new[] { "col" });
            act.Should().NotThrow();
        }

        [TestMethod]
        public void RemoveColumns_NullColumnNames_NoException()
        {
            var mockTable = new Mock<Outlook.Table>();
            System.Action act = () => OlTableExtensions.RemoveColumns(mockTable.Object, null);
            act.Should().NotThrow();
        }

        [TestMethod]
        public void RemoveColumns_EmptyColumnNames_NoException()
        {
            var mockTable = new Mock<Outlook.Table>();
            System.Action act = () =>
                OlTableExtensions.RemoveColumns(mockTable.Object, Array.Empty<string>());
            act.Should().NotThrow();
        }

        [TestMethod]
        public void RemoveColumns_ValidColumn_CallsRemove()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);

            OlTableExtensions.RemoveColumns(mockTable.Object, new[] { "EntryID" });
            mockColumns.Verify(c => c.Remove("EntryID"), Times.Once);
        }

        [TestMethod]
        public void RemoveColumns_COMExceptionNotFound_LogsAndContinues()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockColumns
                .Setup(c => c.Remove(It.IsAny<object>()))
                .Throws(new COMException("not found", -2147221233));

            System.Action act = () =>
                OlTableExtensions.RemoveColumns(mockTable.Object, new[] { "Missing" });
            act.Should().NotThrow();
        }

        [TestMethod]
        public void RemoveColumns_COMExceptionReadOnly_LogsAndContinues()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockColumns
                .Setup(c => c.Remove(It.IsAny<object>()))
                .Throws(new COMException("read-only", -2147352567));

            System.Action act = () =>
                OlTableExtensions.RemoveColumns(mockTable.Object, new[] { "ReadOnly" });
            act.Should().NotThrow();
        }

        [TestMethod]
        public void RemoveColumns_COMExceptionTimeout_ThrowsTimeoutException()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockColumns
                .Setup(c => c.Remove(It.IsAny<object>()))
                .Throws(new COMException("timeout in operation", -555728891));

            System.Action act = () =>
                OlTableExtensions.RemoveColumns(mockTable.Object, new[] { "col" });
            act.Should().Throw<TimeoutException>();
        }

        [TestMethod]
        public void RemoveColumns_COMExceptionMessageTimeout_ThrowsTimeoutException()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockColumns
                .Setup(c => c.Remove(It.IsAny<object>()))
                .Throws(new COMException("timeout occurred", 0));

            System.Action act = () =>
                OlTableExtensions.RemoveColumns(mockTable.Object, new[] { "col" });
            act.Should().Throw<TimeoutException>();
        }

        #endregion

        #region RemoveColumns (parameterless)

        [TestMethod]
        public void RemoveColumns_Parameterless_NullTable_NoException()
        {
            System.Action act = () => OlTableExtensions.RemoveColumns(null);
            act.Should().NotThrow();
        }

        [TestMethod]
        public void RemoveColumns_Parameterless_ValidTable_CallsRemoveAll()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);

            OlTableExtensions.RemoveColumns(mockTable.Object);
            mockColumns.Verify(c => c.RemoveAll(), Times.Once);
        }

        #endregion

        #region AddColumns

        [TestMethod]
        public void AddColumns_NullTable_NoException()
        {
            System.Action act = () => OlTableExtensions.AddColumns(null, new[] { "col" });
            act.Should().NotThrow();
        }

        [TestMethod]
        public void AddColumns_ValidTable_AddsColumns()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);

            OlTableExtensions.AddColumns(mockTable.Object, new[] { "Col1", "Col2" });
            mockColumns.Verify(c => c.Add("Col1"), Times.Once);
            mockColumns.Verify(c => c.Add("Col2"), Times.Once);
        }

        #endregion

        #region ETL

        [TestMethod]
        public void ETL_NullTable_ReturnsNulls()
        {
            var (data, columnInfo) = OlTableExtensions.ETL(null);
            data.Should().BeNull();
            columnInfo.Should().BeNull();
        }

        [TestMethod]
        public void ETL_EmptyTable_ReturnsEmptyData()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockColumns.Setup(c => c.Count).Returns(2);

            var mockCol1 = new Mock<Outlook.Column>();
            mockCol1.Setup(c => c.Name).Returns("Col1");
            var mockCol2 = new Mock<Outlook.Column>();
            mockCol2.Setup(c => c.Name).Returns("Col2");
            mockColumns.Setup(c => c[1]).Returns(mockCol1.Object);
            mockColumns.Setup(c => c[2]).Returns(mockCol2.Object);

            mockTable.Setup(t => t.GetRowCount()).Returns(0);

            var (data, columnInfo) = OlTableExtensions.ETL(mockTable.Object);
            data.Should().NotBeNull();
            columnInfo.Should().HaveCount(2);
        }

        #endregion

        #region GetColumnDictionary (Table extension)

        [TestMethod]
        public void GetColumnDictionary_Table_ReturnsMappedColumns()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockColumns.Setup(c => c.Count).Returns(1);

            var mockCol = new Mock<Outlook.Column>();
            mockCol.Setup(c => c.Name).Returns("Subject");
            mockColumns.Setup(c => c[1]).Returns(mockCol.Object);

            var result = OlTableExtensions.GetColumnDictionary(mockTable.Object);
            result.Should().ContainKey("Subject");
        }

        [TestMethod]
        public void GetColumnDictionary_DuplicateColumnNames_HandlesGracefully()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockColumns.Setup(c => c.Count).Returns(2);

            var mockCol1 = new Mock<Outlook.Column>();
            mockCol1.Setup(c => c.Name).Returns("Subject");
            var mockCol2 = new Mock<Outlook.Column>();
            mockCol2.Setup(c => c.Name).Returns("Subject");
            mockColumns.Setup(c => c[1]).Returns(mockCol1.Object);
            mockColumns.Setup(c => c[2]).Returns(mockCol2.Object);

            var result = OlTableExtensions.GetColumnDictionary(mockTable.Object);
            result.Should().HaveCountGreaterThanOrEqualTo(2);
        }

        #endregion

        #region WriteValuesToData

        [TestMethod]
        public void WriteValuesToData_RawValues_WritesCorrectly()
        {
            var data = new object[1, 3];
            var columnDictionary = new Dictionary<string, int>
            {
                { "Col1", 0 },
                { "Col2", 1 },
                { "Col3", 2 },
            };
            var rawValues = new object[] { "a", "b", "c" };
            var emptyBinIndices = Enumerable.Empty<int>().OrderBy(x => x);

            OlTableExtensions.WriteValuesToData(
                ref data,
                columnDictionary,
                emptyBinIndices,
                0,
                new Dictionary<int, string>(),
                Enumerable.Empty<int>(),
                new Dictionary<int, string>(),
                rawValues
            );

            data[0, 0].Should().Be("a");
            data[0, 1].Should().Be("b");
            data[0, 2].Should().Be("c");
        }

        [TestMethod]
        public void WriteValuesToData_WithBinIndices_OverridesCorrectly()
        {
            var data = new object[1, 2];
            var columnDictionary = new Dictionary<string, int> { { "Col1", 0 }, { "Col2", 1 } };
            var rawValues = new object[] { "raw1", "raw2" };
            var binIndices = new[] { 0 }.OrderBy(x => x);
            var binStrings = new Dictionary<int, string> { { 0, "binVal" } };

            OlTableExtensions.WriteValuesToData(
                ref data,
                columnDictionary,
                binIndices,
                0,
                binStrings,
                Enumerable.Empty<int>(),
                new Dictionary<int, string>(),
                rawValues
            );

            data[0, 0].Should().Be("binVal");
            data[0, 1].Should().Be("raw2");
        }

        [TestMethod]
        public void WriteValuesToData_WithObjIndices_OverridesCorrectly()
        {
            var data = new object[1, 2];
            var columnDictionary = new Dictionary<string, int> { { "Col1", 0 }, { "Col2", 1 } };
            var rawValues = new object[] { "raw1", "raw2" };
            var emptyBinIndices = Enumerable.Empty<int>().OrderBy(x => x);
            var objIndices = new[] { 1 }.AsEnumerable();
            var objStrings = new Dictionary<int, string> { { 1, "objVal" } };

            OlTableExtensions.WriteValuesToData(
                ref data,
                columnDictionary,
                emptyBinIndices,
                0,
                new Dictionary<int, string>(),
                objIndices,
                objStrings,
                rawValues
            );

            data[0, 0].Should().Be("raw1");
            data[0, 1].Should().Be("objVal");
        }

        #endregion

        #region ToObjectRow (extension with indices)

        [TestMethod]
        public void ToObjectRow_WithBinIndices_OverridesValues()
        {
            var rawValues = new object[] { "original", "keep" };
            var binIndices = new[] { 0 }.OrderBy(x => x);
            var binStrings = new Dictionary<int, string> { { 0, "replaced" } };

            var result = rawValues.ToObjectRow(
                binIndices,
                binStrings,
                null,
                new Dictionary<int, string>()
            );

            result[0].Should().Be("replaced");
            result[1].Should().Be("keep");
        }

        [TestMethod]
        public void ToObjectRow_WithObjIndices_OverridesValues()
        {
            var rawValues = new object[] { "keep", "original" };
            var objIndices = new[] { 1 }.AsEnumerable();
            var objStrings = new Dictionary<int, string> { { 1, "replaced" } };

            var result = rawValues.ToObjectRow(
                null,
                new Dictionary<int, string>(),
                objIndices,
                objStrings
            );

            result[0].Should().Be("keep");
            result[1].Should().Be("replaced");
        }

        [TestMethod]
        public void ToObjectRow_NullIndices_ReturnsUnchanged()
        {
            var rawValues = new object[] { "a", "b" };

            var result = rawValues.ToObjectRow(
                null,
                new Dictionary<int, string>(),
                null,
                new Dictionary<int, string>()
            );

            result[0].Should().Be("a");
            result[1].Should().Be("b");
        }

        #endregion

        #region RemoveColumns COMException rethrow

        [TestMethod]
        public void RemoveColumns_COMExceptionUnknownCode_Rethrows()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockColumns
                .Setup(c => c.Remove(It.IsAny<object>()))
                .Throws(new COMException("unknown error", -99999));

            System.Action act = () =>
                OlTableExtensions.RemoveColumns(mockTable.Object, new[] { "col" });
            act.Should().Throw<COMException>();
        }

        #endregion

        #region ConvertObjectColumnsToString

        [TestMethod]
        public void ConvertObjectColumnsToString_NullConverters_ReturnsEmpty()
        {
            var row = new Mock<Outlook.Row>();
            var result = OlTableExtensions.ConvertObjectColumnsToString(
                row.Object,
                null,
                null,
                null
            );
            result.Should().BeEmpty();
        }

        #endregion

        #region GetTableInView

        [TestMethod]
        public void GetTableInView_NullView_ThrowsInvalidOperationException()
        {
            var mockExplorer = new Mock<Outlook.Explorer>();
            var mockView = new Mock<Outlook.View>();
            mockView.Setup(v => v.Name).Returns("TestView");
            mockExplorer.Setup(e => e.CurrentView).Returns(mockView.Object);

            System.Action act = () => OlTableExtensions.GetTableInView(mockExplorer.Object);
            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void GetTableInView_TableViewCurrent_ReturnsTable()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockTableView = new Mock<Outlook.TableView>();
            var mockExplorer = new Mock<Outlook.Explorer>();

            mockTableView.Setup(v => v.GetTable()).Returns(mockTable.Object);
            mockExplorer.Setup(e => e.CurrentView).Returns(mockTableView.Object);

            var result = OlTableExtensions.GetTableInView(mockExplorer.Object);
            result.Should().BeSameAs(mockTable.Object);
        }

        #endregion

        #region GetColumnHeaders

        [TestMethod]
        public void GetColumnHeaders_NullTable_ReturnsEmptyArray()
        {
            var result = OlTableExtensions.GetColumnHeaders(null);
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetColumnHeaders_NullColumns_ReturnsEmptyArray()
        {
            var mockTable = new Mock<Outlook.Table>();
            mockTable.Setup(t => t.Columns).Returns((Outlook.Columns)null);

            var result = OlTableExtensions.GetColumnHeaders(mockTable.Object);
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetColumnHeaders_ZeroCount_ReturnsEmptyArray()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockColumns.Setup(c => c.Count).Returns(0);

            var result = OlTableExtensions.GetColumnHeaders(mockTable.Object);
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetColumnHeaders_WithColumns_ReturnsNames()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockColumns.Setup(c => c.Count).Returns(2);

            var mockCol1 = new Mock<Outlook.Column>();
            mockCol1.Setup(c => c.Name).Returns("Subject");
            var mockCol2 = new Mock<Outlook.Column>();
            mockCol2.Setup(c => c.Name).Returns("EntryID");
            mockColumns.Setup(c => c[1]).Returns(mockCol1.Object);
            mockColumns.Setup(c => c[2]).Returns(mockCol2.Object);

            var result = OlTableExtensions.GetColumnHeaders(mockTable.Object);
            result.Should().HaveCount(2);
            result[0].Should().Be("Subject");
            result[1].Should().Be("EntryID");
        }

        [TestMethod]
        public void GetColumnHeaders_ColumnWithNullName_UsesEmptyString()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockColumns.Setup(c => c.Count).Returns(1);

            var mockCol = new Mock<Outlook.Column>();
            mockCol.Setup(c => c.Name).Returns((string)null);
            mockColumns.Setup(c => c[1]).Returns(mockCol.Object);

            var result = OlTableExtensions.GetColumnHeaders(mockTable.Object);
            result.Should().HaveCount(1);
            result[0].Should().BeEmpty();
        }

        #endregion

        #region GetRows

        [TestMethod]
        public void GetRows_EmptyTable_ReturnsNoRows()
        {
            var mockTable = new Mock<Outlook.Table>();
            mockTable.Setup(t => t.EndOfTable).Returns(true);

            var result = OlTableExtensions.GetRows(mockTable.Object).ToList();

            result.Should().BeEmpty();
            mockTable.Verify(t => t.MoveToStart(), Times.Once);
        }

        [TestMethod]
        public void GetRows_WithTwoRows_ReturnsBothRows()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockRow1 = new Mock<Outlook.Row>();
            var mockRow2 = new Mock<Outlook.Row>();

            mockTable.SetupSequence(t => t.EndOfTable).Returns(false).Returns(false).Returns(true);

            mockTable
                .SetupSequence(t => t.GetNextRow())
                .Returns(mockRow1.Object)
                .Returns(mockRow2.Object);

            var result = OlTableExtensions.GetRows(mockTable.Object).ToList();

            result.Should().HaveCount(2);
            result[0].Should().BeSameAs(mockRow1.Object);
            result[1].Should().BeSameAs(mockRow2.Object);
        }

        #endregion

        #region GetTable (Store overload)

        [TestMethod]
        public void GetTable_Store_NullStore_ThrowsArgumentNullException()
        {
            System.Action act = () =>
                OlTableExtensions.GetTable(
                    (Outlook.Store)null,
                    Outlook.OlDefaultFolders.olFolderInbox,
                    new[] { "col" },
                    new[] { "col2" }
                );
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetTable_Store_GetDefaultFolderThrows_ReturnsNull()
        {
            var mockStore = new Mock<Outlook.Store>();
            mockStore
                .Setup(s => s.GetDefaultFolder(It.IsAny<Outlook.OlDefaultFolders>()))
                .Throws(new COMException("folder not found"));

            var result = OlTableExtensions.GetTable(
                mockStore.Object,
                Outlook.OlDefaultFolders.olFolderInbox,
                null,
                null
            );
            result.Should().BeNull();
        }

        #endregion

        #region AddColumns exception handling

        [TestMethod]
        public void AddColumns_ExceptionDuringAdd_LogsAndContinues()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockColumns
                .Setup(c => c.Add(It.IsAny<string>()))
                .Throws(new COMException("add failed"));

            System.Action act = () =>
                OlTableExtensions.AddColumns(mockTable.Object, new[] { "col" });
            act.Should().NotThrow();
        }

        #endregion

        #region ConvertBinColumnsToString

        [TestMethod]
        public void ConvertBinColumnsToString_EmptyIndices_ReturnsEmptyDictionary()
        {
            var mockRow = new Mock<Outlook.Row>();
            var emptyIndices = Enumerable.Empty<int>().OrderBy(x => x);

            var result = OlTableExtensions.ConvertBinColumnsToString(mockRow.Object, emptyIndices);
            result.Should().BeEmpty();
        }

        #endregion

        #region ConvertObjectColumnsToString (non-null converters)

        [TestMethod]
        public void ConvertObjectColumnsToString_NullObjIndices_ReturnsEmpty()
        {
            var mockRow = new Mock<Outlook.Row>();
            var converters = new Dictionary<string, Func<object, string>>
            {
                { "Col1", o => o?.ToString() },
            };

            var result = OlTableExtensions.ConvertObjectColumnsToString(
                mockRow.Object,
                null,
                new[] { "Col1" },
                converters
            );
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void ConvertObjectColumnsToString_NullObjFields_ReturnsEmpty()
        {
            var mockRow = new Mock<Outlook.Row>();
            var converters = new Dictionary<string, Func<object, string>>
            {
                { "Col1", o => o?.ToString() },
            };

            var result = OlTableExtensions.ConvertObjectColumnsToString(
                mockRow.Object,
                new[] { 0 },
                null,
                converters
            );
            result.Should().BeEmpty();
        }

        #endregion
    }
}
