using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects
{
    [TestClass]
    public class DASLFilterParser_Tests
    {
        [TestMethod]
        public void Parse_WithNullOrEmptyFilter_ThrowsArgumentException()
        {
            // Arrange
            var parser = new DASLFilterParser();

            // Act
            Action nullAct = () => parser.Parse(null);
            Action emptyAct = () => parser.Parse(string.Empty);

            // Assert
            nullAct.Should().Throw<ArgumentException>().WithParameterName("daslFilter");
            emptyAct.Should().Throw<ArgumentException>().WithParameterName("daslFilter");
        }

        [TestMethod]
        public void Parse_WithSingleExpression_ReturnsLeafNode()
        {
            // Arrange
            var parser = new DASLFilterParser();

            // Act
            var tree = parser.Parse("urn:schemas:httpmail:subject LIKE '%status%'");

            // Assert
            tree.Value.Should().Be("urn:schemas:httpmail:subject LIKE '%status%'");
            tree.Children.Should().BeEmpty();
            parser.CombineTree(tree).Should().Be("urn:schemas:httpmail:subject LIKE '%status%'");
        }

        [TestMethod]
        public void Parse_WithTopLevelAndAndNestedOr_PreservesOperatorHierarchy()
        {
            // Arrange
            var parser = new DASLFilterParser();
            const string filter =
                "urn:schemas:httpmail:subject LIKE '%status%' AND (urn:schemas:httpmail:fromemail = 'a@b.com' OR urn:schemas:httpmail:fromemail = 'c@d.com')";

            // Act
            var tree = parser.Parse(filter);

            // Assert
            tree.Value.Should().Be("AND");
            tree.Children.Should().HaveCount(2);
            tree.Children[0].Value.Should().Be("urn:schemas:httpmail:subject LIKE '%status%'");
            tree.Children[1].Value.Should().Be("()");
            tree.Children[1].Children.Should().HaveCount(1);
            tree.Children[1].Children[0].Value.Should().Be("OR");
            parser.CombineTree(tree).Should().Be(filter);
        }

        [TestMethod]
        public void Parse_WithWrappedExpression_ReturnsParenthesisNode()
        {
            // Arrange
            var parser = new DASLFilterParser();

            // Act
            var tree = parser.Parse("(urn:schemas:httpmail:subject LIKE '%status%')");

            // Assert
            tree.Value.Should().Be("()");
            tree.Children.Should().ContainSingle();
            tree.Children[0].Value.Should().Be("urn:schemas:httpmail:subject LIKE '%status%'");
            parser.CombineTree(tree).Should().Be("(urn:schemas:httpmail:subject LIKE '%status%')");
        }

        [TestMethod]
        public void Parse_WithInvalidSyntaxThatNeverClosesParenthesis_ReturnsLeafNode()
        {
            // Arrange
            var parser = new DASLFilterParser();
            const string filter =
                "(urn:schemas:httpmail:subject LIKE '%status%' AND urn:schemas:httpmail:fromemail = 'a@b.com'";

            // Act
            var tree = parser.Parse(filter);

            // Assert
            tree.Value.Should().Be(filter);
            tree.Children.Should().BeEmpty();
        }

        [TestMethod]
        public void PrintTree_WritesIndentedTreeToConsole()
        {
            // Arrange
            var parser = new DASLFilterParser();
            var tree = parser.Parse("A AND B");
            using var writer = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(writer);

            try
            {
                // Act
                parser.PrintTree(tree, 0);
            }
            finally
            {
                Console.SetOut(originalOut);
            }

            // Assert
            writer.ToString().Should().Contain("AND").And.Contain("  A").And.Contain("  B");
        }
    }
}
