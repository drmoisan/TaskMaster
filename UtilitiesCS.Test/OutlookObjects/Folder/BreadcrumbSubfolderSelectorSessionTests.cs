#nullable enable
using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>Failure-first durable subfolder-selection session contracts for issue #400.</summary>
    [TestClass]
    public sealed class BreadcrumbSubfolderSelectorSessionTests
    {
        private const string RowIdentity = "suggestion:apollo:0";
        private const string ParentPath = "\\Inbox\\Projects\\Apollo";
        private const string SubfolderPath = ParentPath + "\\Alpha";

        [TestMethod]
        public void OpenSelector_SubfolderActivationThenEnter_PreservesCommittedFullPath()
        {
            AssertDurableSelection(FollowupAction.Enter);
        }

        [TestMethod]
        public void OpenSelector_SubfolderActivationThenEscape_PreservesCommittedFullPath()
        {
            AssertDurableSelection(FollowupAction.Escape);
        }

        [TestMethod]
        public void OpenSelector_SubfolderActivationThenAutomaticClose_PreservesCommittedFullPath()
        {
            AssertDurableSelection(FollowupAction.NativeAutomaticClose);
        }

        [TestMethod]
        public void OpenSelector_InvalidSubfolderIndexes_LeaveSessionAndParentSelectionUnchanged()
        {
            // Arrange
            SessionHarness harness = CreateHarness();
            harness.Session.Open().Should().BeTrue();

            // Act
            Action negative = () => harness.Model.SelectSubfolder(-1);
            Action outOfRange = () => harness.Model.SelectSubfolder(1);

            // Assert
            negative.Should().Throw<ArgumentOutOfRangeException>();
            outOfRange.Should().Throw<ArgumentOutOfRangeException>();
            harness.Session.IsOpen.Should().BeTrue();
            harness.Session.CommittedIdentity.Should().Be(RowIdentity);
            harness.Session.OriginalIdentity.Should().Be(RowIdentity);
            harness.Session.PendingIdentity.Should().Be(RowIdentity);
            harness.Model.SelectedSubfolderIndex.Should().Be(-1);
            BreadcrumbSelectionMap.GetSelectedFolder(harness.Model).Should().Be(ParentPath);
        }

        private static void AssertDurableSelection(FollowupAction followup)
        {
            // Arrange
            SessionHarness harness = CreateHarness();
            harness.Session.Open().Should().BeTrue();

            // Act: the router-owned transition will select the expanded subfolder before
            // reconciling the open selector session.
            harness.Model.SelectSubfolder(0);
            harness.Session.SynchronizeCommittedSelection();
            bool openAfterActivation = harness.Session.IsOpen;
            string? readbackAfterActivation = BreadcrumbSelectionMap.GetSelectedFolder(
                harness.Model
            );
            bool followupHandled = ApplyFollowup(harness.Session, followup);

            // Assert
            readbackAfterActivation.Should().Be(SubfolderPath);
            openAfterActivation
                .Should()
                .BeFalse("subfolder activation is an immediate explicit commit");
            followupHandled
                .Should()
                .BeFalse("the completed selector session makes later close actions no-ops");
            BreadcrumbSelectionMap.GetSelectedFolder(harness.Model).Should().Be(SubfolderPath);
            harness.Model.SelectedSubfolderIndex.Should().Be(0);
            harness.Session.CommittedIdentity.Should().Be(RowIdentity);
            harness.Session.OriginalIdentity.Should().BeNull();
            harness.Session.PendingIdentity.Should().BeNull();
        }

        private static bool ApplyFollowup(
            BreadcrumbSelectionSession session,
            FollowupAction followup
        )
        {
            switch (followup)
            {
                case FollowupAction.Enter:
                    return session.CommitPending();
                case FollowupAction.Escape:
                case FollowupAction.NativeAutomaticClose:
                    return session.Cancel();
                default:
                    throw new ArgumentOutOfRangeException(nameof(followup), followup, null);
            }
        }

        private static SessionHarness CreateHarness()
        {
            var model = new BreadcrumbStateModel();
            var parentKey = new FolderTreeNodeKey("store-a", "apollo", ParentPath);
            var subfolderKey = new FolderTreeNodeKey("store-a", "alpha", SubfolderPath);
            model.AddSuggestionRow(
                RowIdentity,
                new[] { new FolderBreadcrumbSegment(parentKey, "Apollo", ParentPath, true) },
                0.73
            );
            model.SelectRow(0);
            model.RightArrow().Should().BeTrue();
            model
                .Rows[0]
                .SetSubfolders(
                    new[]
                    {
                        new FolderBreadcrumbSegment(subfolderKey, "Alpha", SubfolderPath, false),
                    }
                );
            return new SessionHarness(model, new BreadcrumbSelectionSession(model));
        }

        private enum FollowupAction
        {
            Enter,
            Escape,
            NativeAutomaticClose,
        }

        private sealed class SessionHarness
        {
            internal SessionHarness(BreadcrumbStateModel model, BreadcrumbSelectionSession session)
            {
                Model = model;
                Session = session;
            }

            internal BreadcrumbStateModel Model { get; }
            internal BreadcrumbSelectionSession Session { get; }
        }
    }
}
