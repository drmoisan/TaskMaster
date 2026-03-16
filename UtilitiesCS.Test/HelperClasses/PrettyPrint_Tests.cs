using System;
using System.Collections.Generic;
using System.Data;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class PrettyPrint_Tests
    {
        [TestMethod]
        public void ToJustifiedText_NullOrWhitespaceInput_ReturnsPaddedText()
        {
            // Arrange
            const int width = 6;

            // Act
            var nullResult = PrettyPrinters.ToJustifiedText(null, width);
            var whitespaceResult = "   ".ToJustifiedText(width);

            // Assert
            nullResult.Should().Be(new string(' ', width));
            whitespaceResult.Should().Be(new string(' ', width));
        }

        [TestMethod]
        public void ToJustifiedText_WithNonPositiveWidth_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            Action act = () => "value".ToJustifiedText(0);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("width");
        }

        [TestMethod]
        public void ToFormattedText_WithEmptyJaggedArrayAndNoHeadersOrTitle_ReturnsEmptyObjectMessage()
        {
            // Arrange
            string[][] rows = Array.Empty<string[]>();

            // Act
            var formatted = rows.ToFormattedText();

            // Assert
            formatted.Should().Be("Object is empty and has no headers or title");
        }

        [TestMethod]
        public void ToFormattedText_WithHeadersAndAggregatorRow_FormatsTitleHeadersAndDivider()
        {
            // Arrange
            string[][] rows =
            {
                new[] { "subtotal", "10" },
                new[] { "item", "2" },
            };

            // Act
            var formatted = rows.ToFormattedText(
                headers: new[] { "Name", "Count" },
                justifications: new[] { Enums.Justification.Left, Enums.Justification.Right },
                title: "Summary Report");

            // Assert
            formatted.Should().Contain("Summary Report");
            formatted.Should().Contain("Name");
            formatted.Should().Contain("Count");
            formatted.Should().Contain(new string('_', 17));
            formatted.Should().Contain("subtotal");
            formatted.Should().Contain("item");
        }

        [TestMethod]
        public void ToFormattedText_ForGenericDictionary_UsesConvertersAndTitle()
        {
            // Arrange
            var dict = new Dictionary<int, decimal>
            {
                [7] = 12.5m,
                [9] = 2m,
            };

            // Act
            var formatted = dict.ToFormattedText(
                key => $"Key:{key}",
                value => value.ToString("0.0"),
                headers: new[] { "Id", "Amount" },
                justifications: new[] { Enums.Justification.Left, Enums.Justification.Right },
                title: "Nested Values");

            // Assert
            formatted.Should().Contain("Nested Values");
            formatted.Should().Contain("Key:7");
            formatted.Should().Contain("12.5");
            formatted.Should().Contain("Key:9");
            formatted.Should().Contain("2.0");
        }

        [TestMethod]
        public void ToFormattedText_ForNumericDictionaries_FormatsExpectedDecimalPlaces()
        {
            // Arrange
            var floatDict = new Dictionary<string, float> { ["pi"] = 3.14159f };
            var longDict = new Dictionary<string, long> { ["count"] = 12345 };

            // Act
            var floatFormatted = floatDict.ToFormattedText(2);
            var longFormatted = longDict.ToFormattedText();

            // Assert
            floatFormatted.Should().Contain("3.14");
            longFormatted.Should().Contain("12,345");
        }

        [TestMethod]
        public void ArrayToDatatable_WithHeadersAndNestedValues_BuildsExpectedTable()
        {
            // Arrange
            var nested = new SampleNode("root");
            object[,] values =
            {
                { 1, nested },
                { 2, null },
            };

            // Act
            DataTable table = PrettyPrinters.ArraytoDatatable(values, new[] { "Id", "Node" });

            // Assert
            table.Columns.Count.Should().Be(2);
            table.Columns[0].ColumnName.Should().Be("Id");
            table.Columns[1].ColumnName.Should().Be("Node");
            table.Rows.Count.Should().Be(2);
            table.Rows[0][0].Should().Be("1");
            table.Rows[0][1].Should().Be("Node:root");
            table.Rows[1][1].Should().Be(DBNull.Value);
        }

        [TestMethod]
        public void ArrayToDatatable_WithHeaderLengthMismatch_ThrowsArgumentException()
        {
            // Arrange
            object[,] values = { { 1, 2 } };
            Action act = () => PrettyPrinters.ArraytoDatatable(values, new[] { "OnlyOne" });

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*must match number of columns 2*");
        }

        [TestMethod]
        public void ToFormattedTextAndMarkdown_ForTwoDimensionalArray_RenderAllPrimitiveCells()
        {
            // Arrange
            string[,] values =
            {
                { "Name", "Value" },
                { "alpha", "1" },
                { "beta", "200" },
            };

            // Act
            var formatted = PrettyPrinters.ToFormattedText(values);
            var markdown = PrettyPrinters.ToMarkdown(values);

            // Assert
            formatted.Should().Contain("Name");
            formatted.Should().Contain("beta");
            markdown.Should().Contain("Name  | Value");
            markdown.Should().Contain("alpha | 1");
            markdown.Should().Contain("beta  | 200");
        }

        private sealed class SampleNode
        {
            public SampleNode(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public override string ToString() => $"Node:{Name}";
        }
    }
}
