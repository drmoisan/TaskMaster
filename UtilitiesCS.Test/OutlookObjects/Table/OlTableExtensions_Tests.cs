using System;
using System.Collections.Generic;
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
            var result = OlTableExtensions.RunTableRetry(() =>
            {
                callCount++;
                if (callCount < 3) throw new Exception("fail");
                return 99;
            }, 5);
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
            System.Action act = () => OlTableExtensions.RemoveColumns(mockTable.Object, Array.Empty<string>());
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
            mockColumns.Setup(c => c.Remove(It.IsAny<object>()))
                .Throws(new COMException("not found", -2147221233));

            System.Action act = () => OlTableExtensions.RemoveColumns(mockTable.Object, new[] { "Missing" });
            act.Should().NotThrow();
        }

        [TestMethod]
        public void RemoveColumns_COMExceptionReadOnly_LogsAndContinues()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockColumns.Setup(c => c.Remove(It.IsAny<object>()))
                .Throws(new COMException("read-only", -2147352567));

            System.Action act = () => OlTableExtensions.RemoveColumns(mockTable.Object, new[] { "ReadOnly" });
            act.Should().NotThrow();
        }

        [TestMethod]
        public void RemoveColumns_COMExceptionTimeout_ThrowsTimeoutException()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockColumns.Setup(c => c.Remove(It.IsAny<object>()))
                .Throws(new COMException("timeout in operation", -555728891));

            System.Action act = () => OlTableExtensions.RemoveColumns(mockTable.Object, new[] { "col" });
            act.Should().Throw<TimeoutException>();
        }

        [TestMethod]
        public void RemoveColumns_COMExceptionMessageTimeout_ThrowsTimeoutException()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockColumns.Setup(c => c.Remove(It.IsAny<object>()))
                .Throws(new COMException("timeout occurred", 0));

            System.Action act = () => OlTableExtensions.RemoveColumns(mockTable.Object, new[] { "col" });
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

        #endregion
    }
}
