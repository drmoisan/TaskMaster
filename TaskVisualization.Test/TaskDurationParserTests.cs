using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskVisualization;

namespace TaskVisualization.Test
{
    [TestClass]
    public class TaskDurationParserTests
    {
        [TestMethod]
        public void Parse_PositiveInteger_ReturnsOkWithMinutes()
        {
            var (ok, minutes, error) = TaskDurationParser.Parse("15");

            ok.Should().BeTrue();
            minutes.Should().Be(15);
            error.Should().BeEmpty();
        }

        [TestMethod]
        public void Parse_Zero_ReturnsOkWithZero()
        {
            var (ok, minutes, error) = TaskDurationParser.Parse("0");

            ok.Should().BeTrue();
            minutes.Should().Be(0);
            error.Should().BeEmpty();
        }

        [TestMethod]
        public void Parse_NegativeInteger_ReturnsNotOkWithNegativeMessage()
        {
            var (ok, minutes, error) = TaskDurationParser.Parse("-3");

            ok.Should().BeFalse();
            minutes.Should().Be(0);
            error
                .Should()
                .Be(new ArgumentOutOfRangeException("Duration cannot be negative").Message);
        }

        [TestMethod]
        public void Parse_NonInteger_PropagatesFormatException()
        {
            Action act = () => TaskDurationParser.Parse("abc");

            act.Should().Throw<FormatException>();
        }

        [TestMethod]
        public void Parse_EmptyString_PropagatesFormatException()
        {
            Action act = () => TaskDurationParser.Parse("");

            act.Should().Throw<FormatException>();
        }

        [TestMethod]
        public void Parse_Whitespace_PropagatesFormatException()
        {
            Action act = () => TaskDurationParser.Parse("   ");

            act.Should().Throw<FormatException>();
        }
    }
}
