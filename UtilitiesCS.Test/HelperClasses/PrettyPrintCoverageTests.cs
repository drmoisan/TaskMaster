using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class PrettyPrintCoverageTests
    {
        [TestMethod]
        public void ToJustifiedText_WithScalarInputs_HandlesPaddingTruncationAndInvalidWidth()
        {
            // Arrange
            const string value = "alpha beta";

            // Act
            string padded = value.ToJustifiedText(14);
            string truncated = value.ToJustifiedText(5);
            string nullText = PrettyPrinters.ToJustifiedText(null, 4);
            Action invalidWidth = () => value.ToJustifiedText(0);

            // Assert
            padded.Length.Should().Be(14);
            padded.TrimEnd().Should().StartWith("alpha");
            padded.TrimEnd().Should().EndWith("beta");
            truncated.Should().Be("alpha");
            nullText.Should().Be(new string(' ', 4));
            invalidWidth
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .Which.ParamName.Should()
                .Be("width");
        }

        [TestMethod]
        public void ToFormattedText_WithCollectionRows_FormatsHeadersAggregatorsAndBoundaries()
        {
            // Arrange
            string[][] rows = { new[] { "subtotal", "100" }, new[] { "item", "7" } };

            // Act
            string formatted = rows.ToFormattedText(
                headers: new[] { "Name", "Count" },
                justifications: new[] { Enums.Justification.Left, Enums.Justification.Right },
                title: "Summary"
            );

            // Assert
            formatted.Should().Contain("Summary");
            formatted.Should().Contain("Name");
            formatted.Should().Contain("Count");
            formatted.Should().Contain("subtotal");
            formatted.Should().Contain("item");
            formatted.Should().Contain("_");
            formatted.Should().Contain("100");
        }

        [TestMethod]
        public void ToFormattedText_WithNestedDictionaryValues_UsesConverters()
        {
            // Arrange
            var values = new Dictionary<NestedKey, NestedValue>
            {
                [new("alpha")] = new(3),
                [new("beta")] = new(9),
            };

            // Act
            string formatted = values.ToFormattedText(
                key => key.Name,
                value => $"Score:{value.Score}",
                headers: new[] { "Name", "Score" },
                justifications: new[] { Enums.Justification.Left, Enums.Justification.Right }
            );

            // Assert
            formatted.Should().Contain("alpha");
            formatted.Should().Contain("Score:3");
            formatted.Should().Contain("beta");
            formatted.Should().Contain("Score:9");
        }

        [TestMethod]
        public void ToFormattedText_WithNullCells_NormalizesToEmptyText()
        {
            // Arrange
            string[][] rows = { new string[] { null, "12" }, new[] { "name", null } };

            // Act
            string formatted = rows.ToFormattedText(headers: new[] { "Label", "Value" });

            // Assert
            formatted.Should().Contain("Label");
            formatted.Should().Contain("Value");
            formatted.Should().Contain("name");
            formatted.Should().Contain("12");
        }

        [TestMethod]
        public void ToFormattedText_WithEmptyInputs_ReturnsBoundaryMessages()
        {
            // Arrange
            string[][] empty = Array.Empty<string[]>();

            // Act
            string noHeadersOrTitle = empty.ToFormattedText();
            string headersOnly = empty.ToFormattedText(headers: new[] { "OnlyHeader" });
            string titleOnly = empty.ToFormattedText(title: "Empty Title");

            // Assert
            noHeadersOrTitle.Should().Be("Object is empty and has no headers or title");
            headersOnly.Should().Contain("Empty");
            headersOnly.Should().Contain("OnlyHeader");
            titleOnly.Should().Contain("Empty Title");
            titleOnly.Should().Contain("Object is empty and has no headers");
        }

        [TestMethod]
        public void ArrayToDatatable_WithNestedAndNullValues_BuildsExpectedRows()
        {
            // Arrange
            object[,] values =
            {
                { new NestedValue(1), null },
                { new NestedValue(2), "ready" },
            };

            // Act
            DataTable table = PrettyPrinters.ArraytoDatatable(values, new[] { "Node", "Status" });

            // Assert
            table
                .Columns.Cast<DataColumn>()
                .Select(column => column.ColumnName)
                .Should()
                .Equal("Node", "Status");
            table.Rows.Count.Should().Be(2);
            table.Rows[0]["Node"].Should().Be("Score:1");
            table.Rows[0]["Status"].Should().Be(DBNull.Value);
            table.Rows[1]["Node"].Should().Be("Score:2");
            table.Rows[1]["Status"].Should().Be("ready");
        }

        [TestMethod]
        public void ArrayToDatatable_WithHeaderMismatch_ThrowsArgumentException()
        {
            // Arrange
            object[,] values =
            {
                { 1, 2 },
            };

            // Act
            Action action = () => PrettyPrinters.ArraytoDatatable(values, new[] { "Only" });

            // Assert
            action
                .Should()
                .Throw<ArgumentException>()
                .WithMessage("*must match number of columns 2*");
        }

        [TestMethod]
        public void ToFormattedTextAndMarkdown_WithTwoDimensionalArray_RenderCollectionBoundaries()
        {
            // Arrange
            string[,] values =
            {
                { "Name", "Value" },
                { "alpha", "1" },
                { "beta", "200" },
            };

            // Act
            string formatted = PrettyPrinters.ToFormattedText(values);
            string markdown = PrettyPrinters.ToMarkdown(values);

            // Assert
            formatted.Should().Contain("Name");
            formatted.Should().Contain("alpha");
            formatted.Should().Contain("beta");
            markdown.Should().Contain("Name");
            markdown.Should().Contain("Value");
            markdown.Should().Contain("|");
            markdown.Should().Contain("---");
        }

        private sealed class NestedKey
        {
            public NestedKey(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        private sealed class NestedValue
        {
            public NestedValue(int score)
            {
                Score = score;
            }

            public int Score { get; }

            public override string ToString() => $"Score:{Score}";
        }
    }
}
