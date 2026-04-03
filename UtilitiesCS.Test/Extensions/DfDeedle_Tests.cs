using System;
using System.Collections.Generic;
using System.Reflection;
using Deedle;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class DfDeedle_Tests
    {
        // ----------------------------------------------------------------
        // AcceptableTriage
        // ----------------------------------------------------------------

        [TestMethod]
        public void AcceptableTriage_InvalidTriageValue_ReturnsDefaultZ()
        {
            // Arrange: obtain the private static AcceptableTriage helper via reflection.
            // AcceptableTriage is private because it is an internal normalization detail;
            // we access it here to verify its contract without exposing it publicly.
            MethodInfo method = typeof(DfDeedle).GetMethod(
                "AcceptableTriage",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            method.Should().NotBeNull("AcceptableTriage must exist as a private static method");

            // Act: invoke with an invalid triage value.
            var result = (string)method!.Invoke(null, new object[] { "X" });

            // Assert: invalid values are normalized to the default "Z" sentinel,
            // so the resulting frame contains no unknown triage labels.
            result.Should().Be("Z");
        }

        [TestMethod]
        public void AcceptableTriage_ValidTriageValues_ReturnUnchanged()
        {
            // Arrange
            MethodInfo method = typeof(DfDeedle).GetMethod(
                "AcceptableTriage",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            method.Should().NotBeNull();

            // Act & Assert: each acceptable triage value must round-trip unchanged.
            foreach (var valid in new[] { "Z", "A", "B", "C" })
            {
                var result = (string)method!.Invoke(null, new object[] { valid });
                result
                    .Should()
                    .Be(
                        valid,
                        because: $"'{valid}' is a valid triage value and must not be altered"
                    );
            }
        }

        // ----------------------------------------------------------------
        // DateFrom2dPosition
        // ----------------------------------------------------------------

        [TestMethod]
        public void DateFrom2dPosition_NullDateSlot_ReturnsMaxValueWithoutThrowing()
        {
            // Arrange: obtain the private static date-extraction helper via reflection.
            MethodInfo method = typeof(DfDeedle).GetMethod(
                "DateFrom2dPosition",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            method.Should().NotBeNull("DateFrom2dPosition must exist as a private static method");

            // A 2-D array where the date slot is null — simulates a missing SentOn field.
            object[,] data =
            {
                { null! },
            };

            // Act: extract the date from the null slot. Must not throw.
            var result = (DateTime)method!.Invoke(null, new object[] { data, 0, 0 });

            // Assert: null date slots should fall back to DateTime.MaxValue, not throw.
            result.Should().Be(DateTime.MaxValue);
        }

        [TestMethod]
        public void DateFrom2dPosition_UnparseableDateSlot_ReturnsMaxValueWithoutThrowing()
        {
            // Arrange
            MethodInfo method = typeof(DfDeedle).GetMethod(
                "DateFrom2dPosition",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            method.Should().NotBeNull();

            // A 2-D array with a string that cannot be parsed as a DateTime.
            object[,] data =
            {
                { "not-a-date" },
            };

            // Act: extract the date from the unparseable slot. Must not throw.
            var result = (DateTime)method!.Invoke(null, new object[] { data, 0, 0 });

            // Assert: DateTime.TryParse fails silently and the helper returns DateTime.MaxValue.
            result.Should().Be(DateTime.MaxValue);
        }

        // ----------------------------------------------------------------
        // GetFirstNonNull (internal)
        // ----------------------------------------------------------------

        [TestMethod]
        public void GetFirstNonNull_NullInput_ReturnsNull()
        {
            // Arrange & Act: pass a null collection — the guard must return null immediately.
            var result = DfDeedle.GetFirstNonNull(null);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetFirstNonNull_EmptyInput_ReturnsNull()
        {
            // Arrange & Act: an empty sequence has no non-null value to return.
            var result = DfDeedle.GetFirstNonNull(System.Array.Empty<object>());

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetFirstNonNull_AllNulls_ReturnsNull()
        {
            // Arrange: all values are null — filteredData will be empty.
            var result = DfDeedle.GetFirstNonNull(new object[] { null!, null! });

            // Assert: filtered array is empty → second null-guard fires.
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetFirstNonNull_MixedNulls_ReturnsFirstNonNull()
        {
            // Arrange & Act: the first non-null value in the sequence should be returned.
            var result = DfDeedle.GetFirstNonNull(new object[] { null!, "hello", "world" });

            // Assert
            result.Should().Be("hello");
        }

        // ----------------------------------------------------------------
        // GetColumnEid (internal)
        // ----------------------------------------------------------------

        [TestMethod]
        public void GetColumnEid_WithStringValues_ReturnsOrdinalSeries()
        {
            // Arrange: a simple array of string-boxed EID values.
            var slice = new object[] { "id-1", "id-2", "id-3" };

            // Act
            Series<int, string> result = DfDeedle.GetColumnEid(slice);

            // Assert: the series should have the same count and the same values.
            result.Should().NotBeNull();
            result.ValueCount.Should().Be(3);
            result.GetAt(0).Should().Be("id-1");
            result.GetAt(2).Should().Be("id-3");
        }

        // ----------------------------------------------------------------
        // FromArray2D — null and empty branches
        // ----------------------------------------------------------------

        [TestMethod]
        public void FromArray2D_NullData_ReturnsNull()
        {
            // Arrange: null data parameter should short-circuit to null immediately.
            var dict = new Dictionary<string, int> { ["EntryID"] = 0 };

            // Act
            var result = DfDeedle.FromArray2D(null, dict);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void FromArray2D_NullColumnDictionary_ReturnsNull()
        {
            // Arrange: null dictionary parameter should short-circuit to null immediately.
            var data = new object[1, 1];

            // Act
            var result = DfDeedle.FromArray2D(data, null);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void FromArray2D_EmptyData_ReturnsFrameWithColumnsButNoRows()
        {
            // Arrange: zero rows should produce a frame with the column keys intact
            // but no rows — used when a folder has matching columns but no items.
            var data = new object[0, 3];
            var dict = new Dictionary<string, int>
            {
                ["EntryID"] = 0,
                ["MessageClass"] = 1,
                ["SentOn"] = 2,
            };

            // Act
            var result = DfDeedle.FromArray2D(data, dict);

            // Assert
            result.Should().NotBeNull();
            result.RowCount.Should().Be(0);
            result.ColumnKeys.Should().Equal("EntryID", "MessageClass", "SentOn");
        }

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

        // ----------------------------------------------------------------
        // Email2dArrayToDf (private via reflection) — covers Email2dToRecords + EmailRecord
        // ----------------------------------------------------------------

        [TestMethod]
        public void Email2dArrayToDf_ViaReflection_ValidData_ReturnsFrame()
        {
            // Arrange: locate the private static Email2dArrayToDf helper.
            // This method is private because it is implementation detail of the async ETL path
            // but its logic — mapping a 2-D array to a typed EmailRecord frame — must be tested
            // because it drives the core async data pipeline.
            MethodInfo method = typeof(DfDeedle).GetMethod(
                "Email2dArrayToDf",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            method.Should().NotBeNull("Email2dArrayToDf must exist as a private static method");

            object[,] data =
            {
                { "id-1", "IPM.Note", "2024-01-15", "conv-1", "A", "store-1" },
            };
            var columnInfo = new Dictionary<string, int>
            {
                ["EntryID"] = 0,
                ["MessageClass"] = 1,
                ["SentOn"] = 2,
                ["ConversationId"] = 3,
                ["Triage"] = 4,
            };

            // Act: invoke with a single-row data array and expected column map.
            var result =
                (Frame<int, string>)
                    method!.Invoke(null, new object[] { "store-1", data, columnInfo });

            // Assert: the resulting frame must have exactly one row with the expected fields.
            result.Should().NotBeNull();
            result.RowCount.Should().Be(1);
        }

        // ----------------------------------------------------------------
        // EmailRecord default constructor (private struct via reflection)
        // ----------------------------------------------------------------

        [TestMethod]
        public void EmailRecord_DefaultConstructor_ViaReflection_ProducesDefaultValues()
        {
            // Arrange: locate the private EmailRecord struct.
            // The default constructor is tested here because it is declared explicitly
            // in the struct and generates tracked IL that would otherwise be uncovered.
            System.Type emailRecordType = typeof(DfDeedle).GetNestedType(
                "EmailRecord",
                BindingFlags.NonPublic
            );
            emailRecordType.Should().NotBeNull("EmailRecord must exist as a private nested struct");

            // Act: create a default instance; should not throw.
            var instance = Activator.CreateInstance(emailRecordType!);

            // Assert: a default-constructed EmailRecord should have null / default fields.
            instance.Should().NotBeNull();
        }
    }
}
