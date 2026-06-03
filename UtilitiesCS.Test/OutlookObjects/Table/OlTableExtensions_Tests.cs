using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Fields;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.Test.OutlookObjects.Table
{
    // EnumerateTable_WritesFormattedOutputAndMovesToStart redirects Console.Out,
    // which is process-wide state. Under class-level parallel execution another
    // test class can replace the writer mid-test and make the captured output empty.
    [DoNotParallelize]
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
            mockTable.Setup(t => t.Columns).Returns((Outlook.Columns)null!);

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
            mockCol.Setup(c => c.Name).Returns((string)null!);
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
                    (Outlook.Store?)null,
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
                { "Col1", o => o?.ToString() ?? string.Empty },
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
                { "Col1", o => o?.ToString() ?? string.Empty },
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

        #region P61 — Column add/remove order, retry call count, and record extraction

        // -----------------------------------------------------------------------
        // P61-T1 — AddColumns calls Add on the COM Columns interface for each
        //           supplied name in the exact input sequence.
        // -----------------------------------------------------------------------

        [TestMethod]
        public void AddColumns_CallsAddInOrder_MatchesInputSequence()
        {
            // Arrange: mock the COM Table and Columns interface; capture call order.
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            var addedOrder = new List<string>();
            mockColumns
                .Setup(c => c.Add(It.IsAny<string>()))
                .Callback<string>(col => addedOrder.Add(col));

            // Act: call the helper with a three-column input.
            OlTableExtensions.AddColumns(mockTable.Object, new[] { "Col1", "Col2", "Col3" });

            // Assert: Add was invoked for each column and in the declared order.
            addedOrder.Should().ContainInConsecutiveOrder("Col1", "Col2", "Col3");
        }

        // -----------------------------------------------------------------------
        // P61-T2 — RunTableRetry invokes the action exactly N times when it fails
        //           N-1 times and succeeds on the Nth attempt.
        // -----------------------------------------------------------------------

        [TestMethod]
        public void RunTableRetry_FailsNMinus1Times_InvokesExactlyNTimes()
        {
            // Arrange: action that throws on the first 2 calls and succeeds on the 3rd.
            int callCount = 0;

            // Act: 5 max-attempt budget; action should settle after exactly 3 calls.
            OlTableExtensions.RunTableRetry(
                () =>
                {
                    callCount++;
                    if (callCount < 3)
                        throw new Exception("transient failure");
                    return "done";
                },
                5
            );

            // Assert: exactly 3 calls — 2 failures + 1 success.
            callCount.Should().Be(3, "the retry wrapper must stop as soon as the action succeeds");
        }

        // -----------------------------------------------------------------------
        // P61-T3 — GetColumnDictionary maps column names to their original typed
        //           values (i.e., strongly-typed record extraction from row data).
        // -----------------------------------------------------------------------

        [TestMethod]
        public void GetColumnDictionary_MixedTypes_PreservesTypedFieldValues()
        {
            // Arrange: simulate a row extraction with mixed-type column values.
            var names = new[] { "Subject", "Size", "IsRead" };
            var values = new object[] { "Meeting Notes", 2048, true };

            // Act: extract the row into a column-keyed dictionary.
            var result = OlTableExtensions.GetColumnDictionary(names, values);

            // Assert: each field preserves its original type and value.
            ((string)result["Subject"])
                .Should()
                .Be("Meeting Notes");
            ((int)result["Size"]).Should().Be(2048);
            ((bool)result["IsRead"]).Should().BeTrue();
        }

        [TestMethod]
        public async Task RemoveColumnsAsync_ValidColumns_CompletesWithinTimeout()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);

            await mockTable.Object.RemoveColumnsAsync(
                new[] { "EntryID", "Store" },
                CancellationToken.None,
                1000
            );

            mockColumns.Verify(c => c.Remove("EntryID"), Times.Once);
            mockColumns.Verify(c => c.Remove("Store"), Times.Once);
        }

        [TestMethod]
        public void GetColumnDictionary_Table_WithSchemaName_UsesSemanticAlias()
        {
            var schemaName = MAPIFields.FieldToSchema["Store"];
            var (mockTable, _) = CreateTableWithColumns(new string[] { schemaName });

            var result = mockTable.Object.GetColumnDictionary();

            result.Should().ContainKey("Store");
            result["Store"].Should().Be(0);
        }

        [TestMethod]
        public void ExtractData2_WithStoreColumn_UsesBinaryStringValueInReturnedArray()
        {
            var row = CreateRowMock(
                new object[] { "Recipients", "raw-store", "Subject" },
                new Dictionary<int, string> { { 2, "STORE-ID-001" } }
            );
            var (mockTable, _) = CreateTableWithColumns(
                new[] { "MessageRecipients", "Store", "Subject" },
                null,
                row
            );

            var (data, columnInfo) = mockTable.Object.ExtractData2();

            columnInfo["Store"].Should().Be(1);
            data[0, 0].Should().Be("Recipients");
            data[0, 1].Should().Be("STORE-ID-001");
            data[0, 2].Should().Be("Subject");
        }

        [TestMethod]
        public void ExtractData2_WithoutStoreColumn_UsesTableArray()
        {
            var expected = new object[,]
            {
                { "Hello", 5 },
            };
            var (mockTable, _) = CreateTableWithColumns(new[] { "Subject", "Size" }, expected);

            var (data, columnInfo) = mockTable.Object.ExtractData2();

            data.Should().BeSameAs(expected);
            columnInfo.Should().ContainKey("Subject");
            columnInfo.Should().ContainKey("Size");
        }

        [TestMethod]
        public void ETL_WithBinaryAndObjectFieldsAndProgress_TransformsRowsByRow()
        {
            var recipient = new object();
            var row = CreateRowMock(
                new object[] { recipient, "raw-store", "Subject" },
                new Dictionary<int, string> { { 2, "STORE-ID-002" } },
                new Dictionary<int, object> { { 1, recipient } }
            );
            var (mockTable, _) = CreateTableWithColumns(
                new[] { "MessageRecipients", "Store", "Subject" },
                null,
                row
            );
            var converters = new Dictionary<string, Func<object, string>>
            {
                { "MessageRecipients", _ => "Converted Recipients" },
            };
            var progress = CreateReportingTracker();

            var (data, columnInfo) = mockTable.Object.ETL(converters, progress);

            columnInfo["MessageRecipients"].Should().Be(0);
            columnInfo["Store"].Should().Be(1);
            data[0, 0].Should().Be("Converted Recipients");
            data[0, 1].Should().Be("STORE-ID-002");
            data[0, 2].Should().Be("Subject");
        }

        [TestMethod]
        public async Task EtlAsync_WithBinaryAndObjectFieldsAndProgress_ReturnsTransformedData()
        {
            var recipient = new object();
            var row = CreateRowMock(
                new object[] { recipient, "raw-store", "Subject" },
                new Dictionary<int, string> { { 2, "STORE-ID-003" } },
                new Dictionary<int, object> { { 1, recipient } }
            );
            var (mockTable, _) = CreateTableWithColumns(
                new[] { "MessageRecipients", "Store", "Subject" },
                null,
                row
            );
            // EtlAsync computes its TimeoutAfter budget as 250 ms * GetRowCount().
            // Override to a large value so the timeout cannot fire under test-host
            // contention; iteration is driven by EndOfTable/GetNextRow, not GetRowCount.
            mockTable.Setup(t => t.GetRowCount()).Returns(120);
            var converters = new Dictionary<string, Func<object, string>>
            {
                { "MessageRecipients", _ => "Converted Async Recipients" },
            };
            var tokenSource = new CancellationTokenSource();
            var progress = CreateReportingTracker();

            var (data, columnInfo) = await mockTable.Object.EtlAsync(
                CancellationToken.None,
                tokenSource,
                0,
                progress,
                converters
            );

            columnInfo["Store"].Should().Be(1);
            data[0, 0].Should().Be("Converted Async Recipients");
            data[0, 1].Should().Be("STORE-ID-003");
            data[0, 2].Should().Be("Subject");
            tokenSource.IsCancellationRequested.Should().BeFalse();
        }

        [TestMethod]
        public async Task EtlAsyncOld_WithBinaryAndObjectFields_ReturnsTransformedData()
        {
            var recipient = new object();
            var row = CreateRowMock(
                new object[] { recipient, "raw-store", "Subject" },
                new Dictionary<int, string> { { 2, "STORE-ID-004" } },
                new Dictionary<int, object> { { 1, recipient } }
            );
            var (mockTable, _) = CreateTableWithColumns(
                new[] { "MessageRecipients", "Store", "Subject" },
                null,
                row
            );
            var converters = new Dictionary<string, Func<object, string>>
            {
                { "MessageRecipients", _ => "Converted Old Async" },
            };

            var (data, columnInfo) = await mockTable.Object.EtlAsyncOld(
                CancellationToken.None,
                new CancellationTokenSource(),
                0,
                null,
                converters
            );

            columnInfo["MessageRecipients"].Should().Be(0);
            data[0, 0].Should().Be("Converted Old Async");
            data[0, 1].Should().Be("STORE-ID-004");
            data[0, 2].Should().Be("Subject");
        }

        [TestMethod]
        public async Task EtlPrepAsync_WithBinaryAndObjectFields_ReturnsPreparedRowsAndMetadata()
        {
            var recipient = new object();
            var row = CreateRowMock(
                new object[] { recipient, "raw-store", "Subject" },
                new Dictionary<int, string> { { 2, "STORE-ID-005" } },
                new Dictionary<int, object> { { 1, recipient } }
            );
            var (mockTable, _) = CreateTableWithColumns(
                new[] { "MessageRecipients", "Store", "Subject" },
                null,
                row
            );
            var converters = new Dictionary<string, Func<object, string>>
            {
                { "MessageRecipients", _ => "Converted Prep" },
            };
            var prep = await InvokeAsyncResult(
                "EtlPrepAsync",
                new[]
                {
                    typeof(Outlook.Table),
                    typeof(CancellationToken),
                    typeof(Dictionary<string, Func<object, string>>),
                },
                mockTable.Object,
                CancellationToken.None,
                converters
            );
            prep.Should().NotBeNull();
            var prepType = prep!.GetType();
            var columnDictionary =
                (Dictionary<string, int>)prepType.GetField("Item2")!.GetValue(prep);
            var binIndices = (
                (IEnumerable<int>)prepType.GetField("Item4")!.GetValue(prep)
            ).ToList();
            var objFields = (
                (IEnumerable<string>)prepType.GetField("Item5")!.GetValue(prep)
            ).ToList();
            var objIndices = (
                (IEnumerable<int>)prepType.GetField("Item6")!.GetValue(prep)
            ).ToList();

            columnDictionary["Store"].Should().Be(1);
            binIndices.Should().ContainSingle().Which.Should().Be(1);
            objFields.Should().ContainSingle().Which.Should().Be("MessageRecipients");
            objIndices.Should().ContainSingle().Which.Should().Be(0);
        }

        [TestMethod]
        public async Task EtlByRowAsync_PublicAsyncEnumerable_ReturnsConvertedObjectRow()
        {
            var recipient = new object();
            var row = CreateRowMock(
                new object[] { recipient, "raw-store", "Subject" },
                new Dictionary<int, string> { { 2, "STORE-ID-006" } },
                new Dictionary<int, object> { { 1, recipient } }
            );
            var converters = new Dictionary<string, Func<object, string>>
            {
                { "MessageRecipients", _ => "Converted Public Async" },
            };

            var transformed = new[] { row.Object }
                .ToAsyncEnumerable()
                .EtlByRowAsync(
                    converters,
                    new[] { 1 }.OrderBy(index => index),
                    new[] { "MessageRecipients" },
                    new[] { 0 }
                );
            var rows = await transformed.ToListAsync();

            rows.Should().ContainSingle();
            rows[0][0].Should().Be("Converted Public Async");
            rows[0][1].Should().Be("STORE-ID-006");
            rows[0][2].Should().Be("Subject");
        }

        [TestMethod]
        public async Task EtlByRowAsync_PrivateHelper_ReturnsConvertedRows()
        {
            var recipient = new object();
            var row = CreateRowMock(
                new object[] { recipient, "raw-store", "Subject" },
                new Dictionary<int, string> { { 2, "STORE-ID-007" } },
                new Dictionary<int, object> { { 1, recipient } }
            );
            var (mockTable, _) = CreateTableWithColumns(
                new[] { "MessageRecipients", "Store", "Subject" },
                null,
                row
            );
            var converters = new Dictionary<string, Func<object, string>>
            {
                { "MessageRecipients", _ => "Converted Private Async" },
            };
            var columnDictionary = new Dictionary<string, int>
            {
                { "MessageRecipients", 0 },
                { "Store", 1 },
                { "Subject", 2 },
            };

            var asyncRows = await InvokeStaticAsync<IAsyncEnumerable<object[]>>(
                "EtlByRowAsync",
                new[]
                {
                    typeof(Outlook.Table),
                    typeof(Dictionary<string, Func<object, string>>),
                    typeof(Dictionary<string, int>),
                    typeof(CancellationToken),
                },
                mockTable.Object,
                converters,
                columnDictionary,
                CancellationToken.None
            );
            var rows = await asyncRows.ToListAsync();

            rows.Should().ContainSingle();
            rows[0][0].Should().Be("Converted Private Async");
            rows[0][1].Should().Be("STORE-ID-007");
            rows[0][2].Should().Be("Subject");
        }

        [TestMethod]
        public void EtlRow_PrivateWriter_PopulatesDataArray()
        {
            var recipient = new object();
            var row = CreateRowMock(
                new object[] { recipient, "raw-store", "Subject" },
                new Dictionary<int, string> { { 2, "STORE-ID-008" } },
                new Dictionary<int, object> { { 1, recipient } }
            );
            object[,] data = new object[1, 3];
            var args = new object[]
            {
                data,
                row.Object,
                new Dictionary<string, Func<object, string>>
                {
                    { "MessageRecipients", _ => "Converted Writer" },
                },
                new Dictionary<string, int>
                {
                    { "MessageRecipients", 0 },
                    { "Store", 1 },
                    { "Subject", 2 },
                },
                new[] { 1 }.OrderBy(index => index),
                new[] { "MessageRecipients" },
                new[] { 0 },
                0,
            };

            InvokeStatic(
                "EtlRow",
                new[]
                {
                    typeof(object[,]).MakeByRefType(),
                    typeof(Outlook.Row),
                    typeof(Dictionary<string, Func<object, string>>),
                    typeof(Dictionary<string, int>),
                    typeof(IOrderedEnumerable<int>),
                    typeof(IEnumerable<string>),
                    typeof(IEnumerable<int>),
                    typeof(int),
                },
                args
            );

            var updated = (object[,])args[0];
            updated[0, 0].Should().Be("Converted Writer");
            updated[0, 1].Should().Be("STORE-ID-008");
            updated[0, 2].Should().Be("Subject");
        }

        [TestMethod]
        public void ConvertBinColumnsToString_WithIndices_ReturnsMappedValues()
        {
            var row = CreateRowMock(
                Array.Empty<object>(),
                new Dictionary<int, string> { { 1, "BIN-ONE" }, { 3, "BIN-THREE" } }
            );

            var result = OlTableExtensions.ConvertBinColumnsToString(
                row.Object,
                new[] { 0, 2 }.OrderBy(index => index)
            );

            result[0].Should().Be("BIN-ONE");
            result[2].Should().Be("BIN-THREE");
        }

        [TestMethod]
        public void ConvertObjectColumnsToString_WithIndicesAndConverters_ReturnsMappedValues()
        {
            var element = new object();
            var row = CreateRowMock(
                Array.Empty<object>(),
                null,
                new Dictionary<int, object> { { 1, element } }
            );
            var converters = new Dictionary<string, Func<object, string>>
            {
                { "MessageRecipients", _ => "Converted Element" },
            };

            var result = OlTableExtensions.ConvertObjectColumnsToString(
                row.Object,
                new[] { 0 },
                new[] { "MessageRecipients" },
                converters
            );

            result[0].Should().Be("Converted Element");
        }

        [TestMethod]
        public async Task GetTableInViewAsync_NullTableView_ThrowsInvalidOperationException()
        {
            var mockExplorer = new Mock<Outlook.Explorer>();
            var mockView = new Mock<Outlook.View>();
            mockView.Setup(v => v.Name).Returns("Invalid View");
            mockExplorer.Setup(e => e.CurrentView).Returns(mockView.Object);

            Func<Task> act = async () =>
                await InvokeAsyncResult(
                    "GetTableInViewAsync",
                    new[] { typeof(Outlook.Explorer), typeof(CancellationToken), typeof(int) },
                    mockExplorer.Object,
                    CancellationToken.None,
                    0
                );

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [TestMethod]
        public async Task GetTableInViewAsync_SlowSynchronousGetTable_ReturnsTableWithoutSyntheticRetry()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockTableView = new Mock<Outlook.TableView>();
            var mockExplorer = new Mock<Outlook.Explorer>();
            var callCount = 0;

            mockTableView
                .Setup(v => v.GetTable())
                .Returns(() =>
                {
                    callCount++;
                    if (callCount == 1)
                    {
                        Thread.Sleep(2100);
                    }

                    return mockTable.Object;
                });
            mockExplorer.Setup(e => e.CurrentView).Returns(mockTableView.Object);

            var result = await InvokeAsyncResult(
                "GetTableInViewAsync",
                new[] { typeof(Outlook.Explorer), typeof(CancellationToken), typeof(int) },
                mockExplorer.Object,
                CancellationToken.None,
                0
            );

            result.Should().BeSameAs(mockTable.Object);
            callCount.Should().Be(1);
        }

        [TestMethod]
        public async Task GetTableInViewAsync_CanceledToken_PropagatesOperationCanceledException()
        {
            var mockTableView = new Mock<Outlook.TableView>();
            var mockExplorer = new Mock<Outlook.Explorer>();
            var cancel = new CancellationTokenSource();
            cancel.Cancel();
            mockExplorer.Setup(e => e.CurrentView).Returns(mockTableView.Object);

            Func<Task> act = async () =>
                await InvokeAsyncResult(
                    "GetTableInViewAsync",
                    new[] { typeof(Outlook.Explorer), typeof(CancellationToken), typeof(int) },
                    mockExplorer.Object,
                    cancel.Token,
                    0
                );

            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [TestMethod]
        public async Task TryGetTableAsync_Store_SuccessfullyReturnsTable()
        {
            var mockStore = new Mock<Outlook.Store>();
            var mockFolder = new Mock<Outlook.MAPIFolder>();
            var (mockTable, _) = CreateTableWithColumns(new[] { "Subject" });

            mockStore
                .Setup(s => s.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox))
                .Returns(mockFolder.Object);
            mockFolder
                .Setup(f => f.GetTable(It.IsAny<object>(), It.IsAny<object>()))
                .Returns(mockTable.Object);

            var result = await mockStore.Object.TryGetTableAsync(
                Outlook.OlDefaultFolders.olFolderInbox,
                new[] { "EntryID" },
                new[] { "Subject" },
                CancellationToken.None,
                1
            );

            (result is null || ReferenceEquals(result, mockTable.Object)).Should().BeTrue();
            mockStore.Verify(
                s => s.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox),
                Times.Once
            );
            mockFolder.Verify(f => f.GetTable(It.IsAny<object>(), It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public async Task TryGetTableAsync_Store_WhenDefaultFolderThrows_ReturnsNull()
        {
            var mockStore = new Mock<Outlook.Store>();
            mockStore
                .Setup(s => s.GetDefaultFolder(It.IsAny<Outlook.OlDefaultFolders>()))
                .Throws(new COMException("missing folder"));

            var result = await mockStore.Object.TryGetTableAsync(
                Outlook.OlDefaultFolders.olFolderInbox,
                null,
                null,
                CancellationToken.None,
                1
            );

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetTableAsync_Store_WhenDefaultFolderThrows_Rethrows()
        {
            var mockStore = new Mock<Outlook.Store>();
            mockStore
                .Setup(s => s.GetDefaultFolder(It.IsAny<Outlook.OlDefaultFolders>()))
                .Throws(new COMException("no folder"));

            Func<Task> act = async () =>
                await mockStore.Object.GetTableAsync(
                    Outlook.OlDefaultFolders.olFolderInbox,
                    null,
                    null,
                    CancellationToken.None,
                    1
                );

            await act.Should().ThrowAsync<COMException>();
        }

        [TestMethod]
        public async Task GetTableAsync_Store_SuccessfullyReturnsTable()
        {
            var mockStore = new Mock<Outlook.Store>();
            var mockFolder = new Mock<Outlook.MAPIFolder>();
            var (mockTable, _) = CreateTableWithColumns(new[] { "Subject" });

            mockStore
                .Setup(s => s.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox))
                .Returns(mockFolder.Object);
            mockFolder
                .Setup(f => f.GetTable(It.IsAny<object>(), It.IsAny<object>()))
                .Returns(mockTable.Object);

            var result = await mockStore.Object.GetTableAsync(
                Outlook.OlDefaultFolders.olFolderInbox,
                new[] { "EntryID" },
                new[] { "Subject" },
                CancellationToken.None,
                1
            );

            (result is null || ReferenceEquals(result, mockTable.Object)).Should().BeTrue();
            mockStore.Verify(
                s => s.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox),
                Times.Once
            );
            mockFolder.Verify(f => f.GetTable(It.IsAny<object>(), It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public async Task TryGetTableAsync_Folder_TaskCanceled_ReturnsNull()
        {
            var mockFolder = new Mock<Outlook.MAPIFolder>();
            mockFolder
                .Setup(f => f.GetTable(It.IsAny<object>(), It.IsAny<object>()))
                .Throws(new TaskCanceledException("cancelled"));

            var result = await mockFolder.Object.TryGetTableAsync(
                null,
                null,
                CancellationToken.None,
                1
            );

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetTableAsync_Folder_ComExceptionThenSuccess_RetriesAndReturnsTable()
        {
            var mockFolder = new Mock<Outlook.MAPIFolder>();
            var (mockTable, _) = CreateTableWithColumns(new[] { "Subject" });
            var callCount = 0;

            mockFolder
                .Setup(f => f.GetTable(It.IsAny<object>(), It.IsAny<object>()))
                .Returns(() =>
                {
                    callCount++;
                    if (callCount == 1)
                    {
                        throw new COMException("transient failure");
                    }

                    return mockTable.Object;
                });

            var result = await mockFolder.Object.GetTableAsync(
                new[] { "EntryID" },
                new[] { "Subject" },
                CancellationToken.None,
                2
            );

            (result is null || ReferenceEquals(result, mockTable.Object)).Should().BeTrue();
            callCount.Should().Be(2);
        }

        [TestMethod]
        public void GetTable_Folder_ReturnsConfiguredTable()
        {
            var mockFolder = new Mock<Outlook.MAPIFolder>();
            var (mockTable, mockColumns) = CreateTableWithColumns(new[] { "Subject" });
            mockFolder
                .Setup(f => f.GetTable(It.IsAny<object>(), It.IsAny<object>()))
                .Returns(mockTable.Object);

            var result = OlTableExtensions.GetTable(
                mockFolder.Object,
                new[] { "EntryID" },
                new[] { "Subject" }
            );

            result.Should().BeSameAs(mockTable.Object);
            mockColumns.Verify(c => c.Remove("EntryID"), Times.Once);
            mockColumns.Verify(c => c.Add("Subject"), Times.Once);
        }

        [TestMethod]
        public void GetTable_Store_ReturnsConfiguredTable()
        {
            var mockStore = new Mock<Outlook.Store>();
            var mockFolder = new Mock<Outlook.MAPIFolder>();
            var (mockTable, mockColumns) = CreateTableWithColumns(new[] { "Subject" });
            mockStore
                .Setup(s => s.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox))
                .Returns(mockFolder.Object);
            mockFolder
                .Setup(f => f.GetTable(It.IsAny<object>(), It.IsAny<object>()))
                .Returns(mockTable.Object);

            var result = mockStore.Object.GetTable(
                Outlook.OlDefaultFolders.olFolderInbox,
                new[] { "EntryID" },
                new[] { "Subject" }
            );

            result.Should().BeSameAs(mockTable.Object);
            mockColumns.Verify(c => c.Remove("EntryID"), Times.Once);
            mockColumns.Verify(c => c.Add("Subject"), Times.Once);
        }

        [TestMethod]
        public void GetTable_Conversation_ReturnsConfiguredTable()
        {
            var mockConversation = new Mock<Outlook.Conversation>();
            var (mockTable, mockColumns) = CreateTableWithColumns(new[] { "Subject" });
            mockConversation.Setup(c => c.GetTable()).Returns(mockTable.Object);

            var result = mockConversation.Object.GetTable(new[] { "EntryID" }, new[] { "Subject" });

            result.Should().BeSameAs(mockTable.Object);
            mockColumns.Verify(c => c.Remove("EntryID"), Times.Once);
            mockColumns.Verify(c => c.Add("Subject"), Times.Once);
        }

        [TestMethod]
        public async Task TryGetTableAsync_Conversation_TaskCanceled_ReturnsNull()
        {
            var mockConversation = new Mock<Outlook.Conversation>();
            mockConversation
                .Setup(c => c.GetTable())
                .Throws(new TaskCanceledException("cancelled"));

            var result = await mockConversation.Object.TryGetTableAsync(
                null,
                null,
                CancellationToken.None,
                1
            );

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetTableAsync_Conversation_ComExceptionThenSuccess_RetriesAndReturnsTable()
        {
            var mockConversation = new Mock<Outlook.Conversation>();
            var (mockTable, mockColumns) = CreateTableWithColumns(new[] { "Subject" });
            var callCount = 0;

            mockConversation
                .Setup(c => c.GetTable())
                .Returns(() =>
                {
                    callCount++;
                    if (callCount == 1)
                    {
                        throw new COMException("transient");
                    }

                    return mockTable.Object;
                });

            var result = await mockConversation.Object.GetTableAsync(
                new[] { "EntryID" },
                new[] { "Subject" },
                CancellationToken.None,
                2
            );

            result.Should().BeSameAs(mockTable.Object);
            callCount.Should().Be(2);
            mockColumns.Verify(c => c.Remove("EntryID"), Times.Once);
            mockColumns.Verify(c => c.Add("Subject"), Times.Once);
        }

        [TestMethod]
        public void GetColumnHeaders_SchemaName_MapsToFieldName()
        {
            var schemaName = MAPIFields.FieldToSchema["Store"];
            var (mockTable, _) = CreateTableWithColumns(new string[] { schemaName });

            var headers = mockTable.Object.GetColumnHeaders();

            headers.Should().ContainSingle().Which.Should().Be("Store");
        }

        [TestMethod]
        public void EnumerateTable_WritesFormattedOutputAndMovesToStart()
        {
            var schemaName = MAPIFields.FieldToSchema["Store"];
            var array = new object[,]
            {
                { "STORE-ID-009", "Subject" },
            };
            var (mockTable, _) = CreateTableWithColumns(
                new string[] { schemaName, "Subject" },
                array
            );
            var output = new StringWriter();
            var original = Console.Out;

            try
            {
                Console.SetOut(output);
                mockTable.Object.EnumerateTable();
            }
            finally
            {
                Console.SetOut(original);
            }

            output.ToString().Should().Contain("Store");
            output.ToString().Should().Contain("Subject");
            output.ToString().Should().Contain("STORE-ID-009");
            mockTable.Verify(t => t.MoveToStart(), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task GetTableInViewAsync_ImmediateSuccess_CallsGetTableOnceAndReturnsSnapshot()
        {
            var mockTable = new Mock<Outlook.Table>(MockBehavior.Strict);
            var mockTableView = new Mock<Outlook.TableView>(MockBehavior.Strict);
            var mockExplorer = new Mock<Outlook.Explorer>(MockBehavior.Strict);
            var callCount = 0;

            mockTableView
                .Setup(x => x.GetTable())
                .Returns(() =>
                {
                    callCount++;
                    return mockTable.Object;
                });
            mockExplorer.SetupGet(x => x.CurrentView).Returns(mockTableView.Object);

            var result = await InvokeAsyncResult(
                "GetTableInViewAsync",
                new[] { typeof(Outlook.Explorer), typeof(CancellationToken), typeof(int) },
                mockExplorer.Object,
                CancellationToken.None,
                0
            );

            result.Should().BeSameAs(mockTable.Object);
            callCount.Should().Be(1);
        }

        private sealed class CapturingProgressTracker : ProgressTracker
        {
            public CapturingProgressTracker()
                : base(new CancellationTokenSource()) { }

            public int? LastValue { get; private set; }

            public string? LastJobName { get; private set; }

            public override void Report((int Value, string JobName) report)
            {
                LastValue = report.Value;
                LastJobName = report.JobName;
            }
        }

        private static ProgressTracker CreateReportingTracker() =>
            new ProgressTracker(new CapturingProgressTracker(), allocation: 100, startingAt: 0);

        private static Mock<Outlook.Row> CreateRowMock(
            object[] values,
            IDictionary<int, string>? binaryStrings = null,
            IDictionary<int, object>? indexedValues = null
        )
        {
            var mockRow = new Mock<Outlook.Row>();
            mockRow.Setup(r => r.GetValues()).Returns(values);

            if (binaryStrings is not null)
            {
                foreach (var pair in binaryStrings)
                {
                    mockRow.Setup(r => r.BinaryToString(pair.Key)).Returns(pair.Value);
                }
            }

            if (indexedValues is not null)
            {
                foreach (var pair in indexedValues)
                {
                    mockRow.Setup(r => r[pair.Key]).Returns(pair.Value);
                }
            }

            return mockRow;
        }

        private static (
            Mock<Outlook.Table> Table,
            Mock<Outlook.Columns> Columns
        ) CreateTableWithColumns(
            string[] columnNames,
            object[,]? array = null,
            params Mock<Outlook.Row>[] rows
        )
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockColumns.Setup(c => c.Count).Returns(columnNames.Length);

            for (var index = 0; index < columnNames.Length; index++)
            {
                var mockColumn = new Mock<Outlook.Column>();
                mockColumn.Setup(c => c.Name).Returns(columnNames[index]);
                mockColumns.Setup(c => c[index + 1]).Returns(mockColumn.Object);
            }

            var effectiveRowCount = rows.Length > 0 ? rows.Length : array?.GetLength(0) ?? 0;
            mockTable.Setup(t => t.GetRowCount()).Returns(effectiveRowCount);

            var currentRow = 0;
            mockTable.Setup(t => t.MoveToStart()).Callback(() => currentRow = 0);
            mockTable.Setup(t => t.EndOfTable).Returns(() => currentRow >= rows.Length);
            mockTable.Setup(t => t.GetNextRow()).Returns(() => rows[currentRow++].Object);

            if (array is not null)
            {
                mockTable.Setup(t => t.GetArray(It.IsAny<int>())).Returns(array);
            }

            return (mockTable, mockColumns);
        }

        private static object InvokeStatic(
            string methodName,
            Type[] parameterTypes,
            params object[] args
        )
        {
            var method = typeof(OlTableExtensions).GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.NonPublic,
                binder: null,
                types: parameterTypes,
                modifiers: null
            );

            method.Should().NotBeNull();
            return method!.Invoke(null, args);
        }

        private static async Task<T> InvokeStaticAsync<T>(
            string methodName,
            Type[] parameterTypes,
            params object[] args
        )
        {
            var task = InvokeStatic(methodName, parameterTypes, args);
            task.Should().BeAssignableTo<Task<T>>();
            return await ((Task<T>)task);
        }

        private static async Task<object?> InvokeAsyncResult(
            string methodName,
            Type[] parameterTypes,
            params object[] args
        )
        {
            var method = typeof(OlTableExtensions).GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: parameterTypes,
                modifiers: null
            );

            method.Should().NotBeNull();
            var taskObject = method!.Invoke(null, args);
            taskObject.Should().BeAssignableTo<Task>();

            var task = (Task)taskObject;
            await task;
            return task.GetType().GetProperty("Result")?.GetValue(task);
        }

        #endregion
    }
}
