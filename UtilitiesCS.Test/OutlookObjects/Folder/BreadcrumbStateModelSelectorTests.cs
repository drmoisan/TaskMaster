#nullable enable
using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>Failure-first scored-fallback and stable-row-state contracts for issue #400.</summary>
    [TestClass]
    public sealed class BreadcrumbStateModelSelectorTests
    {
        [TestMethod]
        public void AddScoredFallbackRow_RetainsIdentityTextAndSuppliedProbability()
        {
            // Arrange
            var model = new BreadcrumbStateModel();

            // Act
            AddScoredFallback(model, "folder-a", "\\Inbox\\A", 0.825);

            // Assert
            object row = model.Rows[0];
            Property<string>(row, "Identity").Should().Be("folder-a");
            Property<bool>(row, "IsSelectable").Should().BeTrue();
            Property<bool>(row, "IsScoredFallback").Should().BeTrue();
            Property<string>(row, "FallbackText").Should().Be("\\Inbox\\A");
            Property<double?>(row, "Probability").Should().Be(0.825);
        }

        [TestMethod]
        public void ExplicitPlainRow_RemainsNonScoredAndCanBeNonSelectable()
        {
            // Arrange
            var model = new BreadcrumbStateModel();

            // Act
            AddPlain(model, "prompt", "Choose a folder", false);

            // Assert
            object row = model.Rows[0];
            Property<string>(row, "Identity").Should().Be("prompt");
            Property<bool>(row, "IsSelectable").Should().BeFalse();
            Property<bool>(row, "IsScoredFallback").Should().BeFalse();
            Property<double?>(row, "Probability").Should().BeNull();
        }

        [TestMethod]
        public void ScoredFallback_SurvivesAtomicReplaceRowsWithoutLosingData()
        {
            // Arrange
            var source = new BreadcrumbStateModel();
            AddScoredFallback(source, "folder-a", "\\Inbox\\A", 0.61);
            var destination = new BreadcrumbStateModel();

            // Act
            destination.ReplaceRows(source.Rows);
            object row = destination.Rows[0];

            // Assert
            Property<string>(row, "Identity").Should().Be("folder-a");
            Property<string>(row, "FallbackText").Should().Be("\\Inbox\\A");
            Property<double?>(row, "Probability").Should().Be(0.61);
        }

        [TestMethod]
        public void ResolvedSuggestion_DerivesStableIdentityFromLeafKey()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            var key = new FolderTreeNodeKey("store", "entry", "\\Inbox\\A");

            // Act
            model.AddSuggestionRow(
                new[] { new FolderBreadcrumbSegment(key, "A", "\\Inbox\\A", false) },
                0.4
            );

            // Assert
            Property<string>(model.Rows[0], "Identity").Should().Be(key.ToString());
            Property<bool>(model.Rows[0], "IsSelectable").Should().BeTrue();
            Property<bool>(model.Rows[0], "IsScoredFallback").Should().BeFalse();
        }

        [TestMethod]
        public void AddSuggestionRow_EmptyOrNullSegmentChain_RejectsExplicitly()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            var nullSegmentChain = new FolderBreadcrumbSegment[] { null! };

            // Act
            Action emptyChain = () =>
                model.AddSuggestionRow("folder-a", Array.Empty<FolderBreadcrumbSegment>(), 0.4);
            Action nullSegment = () => model.AddSuggestionRow("folder-a", nullSegmentChain, 0.4);

            // Assert
            emptyChain.Should().Throw<ArgumentException>();
            nullSegment.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void AddRows_BlankIdentityOrNullPlainText_RejectsExplicitly()
        {
            // Arrange
            var model = new BreadcrumbStateModel();

            // Act
            Action blankIdentity = () => model.AddScoredFallbackRow(" ", "\\Inbox\\A", 0.4);
            Action nullPlainText = () => model.AddPlainRow(null!);

            // Assert
            blankIdentity.Should().Throw<ArgumentException>();
            nullPlainText.Should().Throw<ArgumentNullException>();
        }

        private static void AddScoredFallback(
            BreadcrumbStateModel model,
            string identity,
            string text,
            double probability
        )
        {
            MethodInfo? method = typeof(BreadcrumbStateModel).GetMethod(
                "AddScoredFallbackRow",
                new[] { typeof(string), typeof(string), typeof(double?) }
            );
            method
                .Should()
                .NotBeNull("issue #400 requires scored fallback rows before hierarchy resolution");
            method!.Invoke(model, new object?[] { identity, text, probability });
        }

        private static void AddPlain(
            BreadcrumbStateModel model,
            string identity,
            string text,
            bool selectable
        )
        {
            MethodInfo? method = typeof(BreadcrumbStateModel).GetMethod(
                "AddPlainRow",
                new[] { typeof(string), typeof(string), typeof(bool) }
            );
            method.Should().NotBeNull("issue #400 requires explicit selectable-row metadata");
            method!.Invoke(model, new object[] { identity, text, selectable });
        }

        private static T Property<T>(object target, string property) =>
            (T)target.GetType().GetProperty(property)!.GetValue(target)!;
    }
}
