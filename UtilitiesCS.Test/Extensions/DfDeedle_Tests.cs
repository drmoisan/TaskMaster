using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class DfDeedle_Tests
    {
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

        [TestMethod]
        public void DateFrom2dPosition_NullDateSlot_ReturnsMaxValueWithoutThrowing()
        {
            // Arrange: obtain the private static date-extraction helper via reflection.
            // DateFrom2dPosition is private because it is an internal parsing detail
            // that shields callers from null/unparseable date values.
            MethodInfo method = typeof(DfDeedle).GetMethod(
                "DateFrom2dPosition",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            method.Should().NotBeNull("DateFrom2dPosition must exist as a private static method");

            // A 2-D array where the date slot is null — simulates a missing SentOn field.
            object[,] data =
            {
                { null },
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
