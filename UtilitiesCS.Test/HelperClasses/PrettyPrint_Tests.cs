using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Microsoft.Data.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.HelperClasses
{
    // [DoNotParallelize] — DataFramePrettyHelpers_RenderRowsMarkdownAndConsoleOutput
    // captures and restores Console.Out, which is process-wide state. Under the
    // class-level parallel scope set in TaskMaster.runsettings, a sibling test
    // class's Console.SetOut overrides this class's redirect mid-test, causing
    // PrettyPrint's Console.WriteLine output to land in the wrong writer.
    [DoNotParallelize]
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
            act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("width");
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
            string[][] rows = { new[] { "subtotal", "10" }, new[] { "item", "2" } };

            // Act
            var formatted = rows.ToFormattedText(
                headers: new[] { "Name", "Count" },
                justifications: new[] { Enums.Justification.Left, Enums.Justification.Right },
                title: "Summary Report"
            );

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
            var dict = new Dictionary<int, decimal> { [7] = 12.5m, [9] = 2m };

            // Act
            var formatted = dict.ToFormattedText(
                key => $"Key:{key}",
                value => value.ToString("0.0"),
                headers: new[] { "Id", "Amount" },
                justifications: new[] { Enums.Justification.Left, Enums.Justification.Right },
                title: "Nested Values"
            );

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
            object[,] values =
            {
                { 1, 2 },
            };
            Action act = () => PrettyPrinters.ArraytoDatatable(values, new[] { "OnlyOne" });

            // Assert
            act.Should()
                .Throw<ArgumentException>()
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

        [TestMethod]
        public void DataFramePrettyHelpers_RenderRowsMarkdownAndConsoleOutput()
        {
            // Arrange
            var frame = new DataFrame(
                new StringDataFrameColumn("Name", new string[] { "alpha", null }),
                new PrimitiveDataFrameColumn<int>("Count", new[] { 1, 2 })
            );
            var row = frame.Rows[1];
            var originalOut = Console.Out;
            using var writer = new StringWriter();
            Console.SetOut(writer);

            try
            {
                // Act
                var prettyText = frame.PrettyText();
                var prettyRow = row.Pretty();
                var markdown = frame.ToMarkdown();
                frame.PrettyPrint();
                row.PrettyPrint();

                // Assert
                prettyText.Should().Contain("Name");
                prettyText.Should().Contain("alpha");
                prettyRow.Should().Be(" 2");
                markdown.Should().Contain("Name");
                markdown.Should().Contain("alpha");
                writer.ToString().Should().Contain("Name");
                writer.ToString().Should().Contain(" 2");
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [TestMethod]
        public void ArrayToDatatable_WithoutHeaders_BuildsDefaultColumns()
        {
            // Arrange
            object[,] values =
            {
                { "alpha", 1 },
                { "beta", 2 },
            };

            // Act
            var table = PrettyPrinters.ArraytoDatatable(values);

            // Assert
            table
                .Columns.Cast<DataColumn>()
                .Select(column => column.ColumnName)
                .Should()
                .Equal("Column1", "Column2");
            table.Rows.Count.Should().Be(2);
            table.Rows[0][0].Should().Be("alpha");
            table.Rows[1][1].Should().Be("2");
        }

        [TestMethod]
        public void ToFormattedText_WithTitleOnlyAndLongTitle_WrapsInsertedEmptyMessageRow()
        {
            // Arrange
            var title =
                "This title is intentionally longer than the generated single-column table width";

            // Act
            var formatted = Array.Empty<string[]>().ToFormattedText(title: title);

            // Assert
            formatted.Should().Contain("Object is empty and has no headers");
            formatted.Should().Contain("This title is intentionally longer");
            formatted.Should().Contain("than the generated single-column");
            formatted.Should().Contain("table width");
        }

        [TestMethod]
        public void ToFormattedText_WithHeadersOnly_UsesWideEmptyMessageAndHeaderDivider()
        {
            // Arrange
            string[][] rows = Array.Empty<string[]>();
            var headers = new[] { "ExtremelyWideHeaderNameForEmptyMessageCoverage" };

            // Act
            var formatted = rows.ToFormattedText(headers: headers);

            // Assert
            formatted.Should().Contain("Object is empty and has no data");
            formatted.Should().Contain("ExtremelyWideHeaderNameForEmptyMessageCoverage");
            formatted.Should().Contain("===");
        }

        [TestMethod]
        public void ToFormattedText_WithNullCellsAndImplicitJustifications_CoversWidthNormalization()
        {
            // Arrange
            string[][] rows = { new string[] { null, "12.5" }, new[] { "gamma", "7" } };

            // Act
            var formatted = rows.ToFormattedText(headers: new[] { "LongerHeader", "B" });

            // Assert
            formatted.Should().Contain("LongerHeader");
            formatted.Should().Contain("gamma");
            formatted.Should().Contain("12.5");
        }

        [TestMethod]
        public void ReflectionHelpers_CoverInternalFormattingBranches()
        {
            // Arrange
            var dict = new Dictionary<string, int> { ["abc"] = 4000, ["d"] = 2 };
            var getMaxLengthsMethod = typeof(PrettyPrinters)
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .Single(method =>
                    method.Name == "GetMaxLengthsByColumn"
                    && method.IsGenericMethodDefinition
                    && method.GetParameters().Length == 1
                )
                .MakeGenericMethod(typeof(string), typeof(int));
            var inferDefaultMethod = typeof(PrettyPrinters).GetMethod(
                "InferDefaultJustifications",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            var formatJaggedCellMethod = typeof(PrettyPrinters).GetMethod(
                "FormatJaggedCell",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            var formatJaggedCell2Method = typeof(PrettyPrinters).GetMethod(
                "FormatJaggedCell2",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            var appendEmptyMessageMethod = typeof(PrettyPrinters).GetMethod(
                "AppendJaggedEmptyMessage",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            var centeredOverflow = (string)
                formatJaggedCellMethod.Invoke(
                    null,
                    new object[] { "alphabet", Enums.Justification.Center, 4 }
                );
            var centeredShort = (string)
                formatJaggedCellMethod.Invoke(
                    null,
                    new object[] { "mid", Enums.Justification.Center, 7 }
                );
            var justified = (string)
                formatJaggedCellMethod.Invoke(
                    null,
                    new object[] { "alpha beta", Enums.Justification.Justified, 12 }
                );
            var fallback = (string)
                formatJaggedCellMethod.Invoke(
                    null,
                    new object[] { "left", (Enums.Justification)999, 6 }
                );
            var builder = new StringBuilder();
            var row = new[] { "9", "alphabet", "plain" };
            var justifications = new[]
            {
                Enums.Justification.Right,
                Enums.Justification.Center,
                Enums.Justification.Left,
            };
            var widths = new[] { 3, 4, 5 };
            var emptyBuilder = new StringBuilder();
            var defaultEmptyBuilder = new StringBuilder();

            // Act
            var maxLengths = (int[])getMaxLengthsMethod.Invoke(null, new object[] { dict });
            var emptyJustifications = (Enums.Justification[])
                inferDefaultMethod.Invoke(null, new object[] { Array.Empty<string[]>(), 2 });
            formatJaggedCell2Method.Invoke(
                null,
                new object[] { row, justifications, widths, builder, 0 }
            );
            formatJaggedCell2Method.Invoke(
                null,
                new object[] { row, justifications, widths, builder, 1 }
            );
            formatJaggedCell2Method.Invoke(
                null,
                new object[] { row, justifications, widths, builder, 2 }
            );
            appendEmptyMessageMethod.Invoke(
                null,
                new object[] { emptyBuilder, Array.Empty<string[]>(), 40 }
            );
            appendEmptyMessageMethod.Invoke(
                null,
                new object[] { defaultEmptyBuilder, Array.Empty<string[]>(), 5 }
            );

            // Assert
            maxLengths.Should().Equal(3, 4);
            emptyJustifications.Should().Equal(Enums.Justification.Left, Enums.Justification.Left);
            centeredOverflow.Should().StartWith("alph");
            centeredShort.Trim().Should().Be("mid");
            justified.TrimEnd().Should().StartWith("alpha");
            justified.TrimEnd().Should().EndWith("beta");
            fallback.Should().StartWith("left");
            builder.ToString().Should().Contain("  9");
            builder.ToString().Should().Contain("alph");
            builder.ToString().Should().Contain("plain");
            emptyBuilder.ToString().Should().Contain("Object is empty and has no data");
            defaultEmptyBuilder.ToString().Should().Contain("| ");
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
