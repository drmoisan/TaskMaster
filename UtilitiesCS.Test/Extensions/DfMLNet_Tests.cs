using System;
using System.Data;
using System.Reflection;
using FluentAssertions;
using Microsoft.Data.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class DfMLNet_Tests
    {
        // -----------------------------------------------------------------------
        // P48-T1 — ToDataFrame converts an object sequence to a DataFrame with
        //          the expected columns and types
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that ToDataFrame produces a DataFrame whose column names and
        /// element types match the supplied column-name array and source data types.
        ///
        /// Purpose:
        ///     Confirm that column dispatch (string vs. numeric) is exercised and that
        ///     GetNames / GetTypes extension helpers return the expected metadata.
        ///
        /// Returns:
        ///     Passes when the resulting DataFrame has two columns with the correct
        ///     names and element types (string and int).
        /// </summary>
        [TestMethod]
        public void ToDataFrame_WithStringAndIntColumns_HasCorrectNamesAndTypes()
        {
            // Arrange: 2×2 object array with one string column and one int column
            var data = new object[2, 2]
            {
                { "Alice", 1 },
                { "Bob", 2 },
            };

            // Act
            var df = data.ToDataFrame(new[] { "Name", "Score" });

            // Assert: column names and element types are correct
            df.Columns.GetNames().Should().Equal("Name", "Score");
            df.Columns["Name"].DataType.Should().Be(typeof(string));
            df.Columns["Score"].DataType.Should().Be(typeof(int));
            df.Rows.Count.Should().Be(2);
        }

        // -----------------------------------------------------------------------
        // P48-T2 — First-non-null column selector returns the correct column from
        //          mixed-null inputs
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that GetFirstNonNull skips leading nulls and returns the first
        /// non-null element, enabling correct type dispatch in GetDfColumn.
        ///
        /// Purpose:
        ///     Confirm that a column whose first entries are null is still correctly
        ///     typed by inspecting the first non-null value.
        ///
        /// Returns:
        ///     Passes when GetFirstNonNull returns the expected non-null value.
        /// </summary>
        [TestMethod]
        public void GetFirstNonNull_WithLeadingNulls_ReturnsFirstNonNullValue()
        {
            // Arrange: array where first two entries are null
            var data = new object[] { null, null, 42, 99 };

            // Act
            var result = DfMLNet.GetFirstNonNull(data);

            // Assert
            result.Should().Be(42);
        }

        // -----------------------------------------------------------------------
        // P48-T3 — ToDataTable conversion preserves the row count
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that converting a DataFrame to a DataTable produces a table
        /// whose row count matches the source frame row count.
        ///
        /// Purpose:
        ///     Confirm that no rows are dropped or duplicated during the conversion
        ///     and that all column values are carried over correctly.
        ///
        /// Returns:
        ///     Passes when the DataTable row count equals the DataFrame row count.
        /// </summary>
        [TestMethod]
        public void ToDataTable_PreservesRowCount()
        {
            // Arrange: create a 3-row DataFrame
            var data = new object[3, 1]
            {
                { "x" },
                { "y" },
                { "z" },
            };
            var df = data.ToDataFrame(new[] { "Label" });

            // Act
            DataTable table = df.ToDataTable();

            // Assert: same number of rows
            table.Rows.Count.Should().Be(3);
            table.Columns["Label"].Should().NotBeNull();
        }

        [TestMethod]
        public void ToDataFrame_WhenColumnCountsDiffer_ThrowsArgumentException()
        {
            var data = new object[1, 2]
            {
                { "A", 1 },
            };

            Action act = () => data.ToDataFrame(new[] { "OnlyOne" });

            act.Should().Throw<ArgumentException>().WithMessage("*They must be of the same size*");
        }

        [TestMethod]
        public void GetDfColumn_CoversRemainingPrimitiveAndFallbackBranches()
        {
            var fallbackSeed = new FallbackValue("fallback");
            var cases = new (object[] Data, Type ExpectedType, string Name)[]
            {
                ([true, false], typeof(bool), "bool"),
                ([(byte)1, (byte)2], typeof(byte), "byte"),
                ([(sbyte)1, (sbyte)2], typeof(sbyte), "sbyte"),
                (['a', 'b'], typeof(char), "char"),
                ([1.5m, 2.5m], typeof(decimal), "decimal"),
                ([1.5d, 2.5d], typeof(double), "double"),
                ([1.5f, 2.5f], typeof(float), "float"),
                ([(uint)1, (uint)2], typeof(uint), "uint"),
                ([(nint)1, (nint)2], typeof(nint), "nint"),
                ([(nuint)1, (nuint)2], typeof(nuint), "nuint"),
                ([(long)1, (long)2], typeof(long), "long"),
                ([(ulong)1, (ulong)2], typeof(ulong), "ulong"),
                ([(short)1, (short)2], typeof(short), "short"),
                ([(ushort)1, (ushort)2], typeof(ushort), "ushort"),
                ([fallbackSeed, null], typeof(string), "fallback"),
            };

            foreach (var (data, expectedType, name) in cases)
            {
                var column = DfMLNet.GetDfColumn(name, data);

                column.DataType.Should().Be(expectedType, because: name);
                column.Name.Should().Be(name);
            }
        }

        [TestMethod]
        public void GetFirstNonNull_WhenInputIsNullEmptyOrAllNull_ReturnsNull()
        {
            DfMLNet.GetFirstNonNull(null).Should().BeNull();
            DfMLNet.GetFirstNonNull(System.Array.Empty<object>()).Should().BeNull();
            DfMLNet.GetFirstNonNull([null, null]).Should().BeNull();
        }

        [TestMethod]
        public void ToDataTable_PreservesCellValues()
        {
            var data = new object[2, 2]
            {
                { "Alice", 10 },
                { "Bob", 20 },
            };
            var df = data.ToDataFrame(new[] { "Name", "Score" });

            var table = df.ToDataTable();

            table.Rows[0]["Name"].Should().Be("Alice");
            table.Rows[0]["Score"].Should().Be(10);
            table.Rows[1]["Name"].Should().Be("Bob");
            table.Rows[1]["Score"].Should().Be(20);
        }

        [TestMethod]
        public void MakeDataTableAndDisplay_PrivateHelper_CompletesWithoutThrowing()
        {
            var method = typeof(DfMLNet).GetMethod(
                "MakeDataTableAndDisplay",
                BindingFlags.NonPublic | BindingFlags.Static
            );

            method.Should().NotBeNull();
            Action act = () => method.Invoke(null, null);

            act.Should().NotThrow();
        }

        private sealed class FallbackValue(string text)
        {
            public override string ToString() => text;
        }
    }
}
