#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>Failure-first collapsed and probability projection contracts for issue #400.</summary>
    [TestClass]
    public sealed class BreadcrumbRenderProjectionSelectorTests
    {
        [TestMethod]
        public void Project_ScoredFallback_UsesFallbackTextAndUnchangedFormatterOutput()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            model.AddScoredFallbackRow("folder-a", "\\Inbox\\Apollo", 0.734);

            // Act
            BreadcrumbRowRender row = BreadcrumbRenderProjection.Project(model)[0];

            // Assert
            row.IsSuggestion.Should().BeTrue("a scored fallback is still a suggestion");
            row.PercentText.Should().Be(PercentageFormatter.FormatPercent(0.734));
            row.Cells[row.Cells.Count - 1].Text.Should().Be("Apollo");
        }

        [TestMethod]
        public void Project_ResolvedSuggestion_RetainsExactSuppliedPercentage()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            model.AddSuggestionRow("folder-a", Chain(), 0.734);

            // Act
            BreadcrumbRowRender row = BreadcrumbRenderProjection.Project(model)[0];

            // Assert
            row.PercentText.Should().Be(PercentageFormatter.FormatPercent(0.734));
        }

        [TestMethod]
        public void Project_GenuinelyPlainRow_LeavesPercentageBlank()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            model.AddPlainRow("prompt", "Choose a folder", false);

            // Act
            BreadcrumbRowRender row = BreadcrumbRenderProjection.Project(model)[0];

            // Assert
            row.IsSuggestion.Should().BeFalse();
            row.PercentText.Should().BeEmpty();
        }

        [TestMethod]
        public void ProjectCollapsed_ReturnsExactlyCommittedSelectedDataRow()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            model.AddScoredFallbackRow("folder-a", "\\Inbox\\A", 0.8);
            model.AddPlainRow("prompt", "Suggested folders", false);
            model.AddScoredFallbackRow("folder-b", "\\Inbox\\B", 0.6);
            model.SelectRow(2);

            // Act
            IReadOnlyList<BreadcrumbRowRender> rows = ProjectCollapsed(model);

            // Assert
            rows.Should().ContainSingle();
            rows[0].RowIndex.Should().Be(2);
            rows[0].Selected.Should().BeTrue();
            rows[0].PercentText.Should().Be(PercentageFormatter.FormatPercent(0.6));
        }

        [TestMethod]
        public void ProjectCollapsed_NoSelectionOrNonSelectableSelection_ReturnsNoDataRow()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            model.AddPlainRow("prompt", "Suggested folders", false);

            // Act and assert
            ProjectCollapsed(model).Should().BeEmpty();
            model.SelectRow(0);
            ProjectCollapsed(model).Should().BeEmpty();
        }

        [TestMethod]
        public void ProjectCollapsed_NullModel_RejectsExplicitly()
        {
            // Arrange
            MethodInfo method = typeof(BreadcrumbRenderProjection).GetMethod(
                "ProjectCollapsed",
                new[] { typeof(BreadcrumbStateModel) }
            )!;

            // Act
            Action act = () => method.Invoke(null, new object?[] { null });

            // Assert
            act.Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<ArgumentNullException>();
        }

        private static IReadOnlyList<BreadcrumbRowRender> ProjectCollapsed(
            BreadcrumbStateModel model
        )
        {
            MethodInfo? method = typeof(BreadcrumbRenderProjection).GetMethod(
                "ProjectCollapsed",
                new[] { typeof(BreadcrumbStateModel) }
            );
            method
                .Should()
                .NotBeNull("issue #400 requires a dedicated one-row collapsed projection");
            return (IReadOnlyList<BreadcrumbRowRender>)
                method!.Invoke(null, new object[] { model })!;
        }

        private static IReadOnlyList<FolderBreadcrumbSegment> Chain()
        {
            var key = new FolderTreeNodeKey("store", "leaf", "\\Inbox\\Apollo");
            return new[]
            {
                new FolderBreadcrumbSegment(key, "Inbox", "\\Inbox", true),
                new FolderBreadcrumbSegment(key, "Apollo", "\\Inbox\\Apollo", false),
            };
        }
    }
}
