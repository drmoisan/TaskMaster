using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Interfaces;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class EmailSorterTests
    {
        [TestMethod]
        public void Constructor_Default_UsesDefaultSortOptions()
        {
            // Arrange and Act
            var sorter = new EmailSorter();

            // Assert
            sorter.Options.Should().Be(SortOptionsEnum.Default);
        }

        [TestMethod]
        public void Constructor_WithOptions_UsesProvidedSortOptions()
        {
            // Arrange
            const SortOptionsEnum options =
                SortOptionsEnum.TriageImportantFirst | SortOptionsEnum.DateRecentFirst;

            // Act
            var sorter = new EmailSorter(options);

            // Assert
            sorter.Options.Should().Be(options);
        }

        [TestMethod]
        public void GetDateKey_WithKnownDate_ReturnsSortableTimestampKey()
        {
            // Arrange
            var sorter = new EmailSorter();
            var sentOn = new DateTime(2026, 7, 6, 18, 7, 5);

            // Act
            long key = sorter.GetDateKey(sentOn);

            // Assert
            key.Should().Be(20260706180705);
        }

        [TestMethod]
        [DataRow("A", 420260706180705L)]
        [DataRow("B", 320260706180705L)]
        [DataRow("C", 220260706180705L)]
        [DataRow("Z", 120260706180705L)]
        public void GetSortKey_WithSupportedTriage_ReturnsExpectedCompositeKey(
            string triage,
            long expected
        )
        {
            // Arrange
            var sorter = new EmailSorter(
                SortOptionsEnum.TriageImportantFirst | SortOptionsEnum.DateRecentFirst
            );
            var sentOn = new DateTime(2026, 7, 6, 18, 7, 5);

            // Act
            long key = sorter.GetSortKey(triage, sentOn);

            // Assert
            key.Should().Be(expected);
        }

        [TestMethod]
        public void GetSortKey_WithUnsupportedTriage_PropagatesKeyNotFoundException()
        {
            // Arrange
            var sorter = new EmailSorter(
                SortOptionsEnum.TriageImportantFirst | SortOptionsEnum.DateRecentFirst
            );
            var sentOn = new DateTime(2026, 7, 6, 18, 7, 5);

            // Act
            Action act = () => sorter.GetSortKey("X", sentOn);

            // Assert
            act.Should().Throw<KeyNotFoundException>();
        }
    }
}
