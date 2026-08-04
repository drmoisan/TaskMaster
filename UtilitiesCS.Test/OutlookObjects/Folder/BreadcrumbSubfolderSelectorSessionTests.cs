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
        private const string TargetRowIdentity = "suggestion:zeus:1";
        private const string ParentPath = "\\Inbox\\Projects\\Apollo";
        private const string TargetParentPath = "\\Inbox\\Projects\\Zeus";
        private const string TargetSubfolderPath = TargetParentPath + "\\Delta";

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
            const string plainIdentity = "plain:recent-without-children:1";
            harness.Model.AddPlainRow(plainIdentity, "Recent without children", true);
            harness.Session.Open().Should().BeTrue();

            // Act
            var effects = new[]
            {
                harness.Session.ActivateSubfolder("missing-row", 0),
                harness.Session.ActivateSubfolder(RowIdentity, -1),
                harness.Session.ActivateSubfolder(RowIdentity, 2),
                harness.Session.ActivateSubfolder(plainIdentity, 0),
            };

            // Assert
            effects.Should().OnlyContain(effect => effect == BreadcrumbSelectionEffects.None);
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
            harness.Model.SelectedIndex.Should().Be(0);

            // Act
            BreadcrumbSelectionEffects effects = harness.Session.ActivateSubfolder(
                TargetRowIdentity,
                1
            );
            bool openAfterActivation = harness.Session.IsOpen;
            string? readbackAfterActivation = BreadcrumbSelectionMap.GetSelectedFolder(
                harness.Model
            );
            bool followupHandled = ApplyFollowup(harness.Session, followup);

            // Assert
            effects
                .Should()
                .Be(
                    BreadcrumbSelectionEffects.Handled
                        | BreadcrumbSelectionEffects.SelectionChanged
                        | BreadcrumbSelectionEffects.OpenStateChanged
                        | BreadcrumbSelectionEffects.RenderRequired
                );
            readbackAfterActivation.Should().Be(TargetSubfolderPath);
            openAfterActivation
                .Should()
                .BeFalse("subfolder activation is an immediate explicit commit");
            followupHandled
                .Should()
                .BeFalse("the completed selector session makes later close actions no-ops");
            BreadcrumbSelectionMap
                .GetSelectedFolder(harness.Model)
                .Should()
                .Be(TargetSubfolderPath);
            harness.Model.SelectedIndex.Should().Be(1);
            harness.Model.SelectedSubfolderIndex.Should().Be(1);
            harness.Session.CommittedIdentity.Should().Be(TargetRowIdentity);
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
            var targetParentKey = new FolderTreeNodeKey("store-a", "zeus", TargetParentPath);
            model.AddSuggestionRow(
                RowIdentity,
                new[] { new FolderBreadcrumbSegment(parentKey, "Apollo", ParentPath, true) },
                0.73
            );
            model.AddSuggestionRow(
                TargetRowIdentity,
                new[]
                {
                    new FolderBreadcrumbSegment(targetParentKey, "Zeus", TargetParentPath, true),
                },
                0.61
            );
            model.SelectRow(0);
            model.RightArrow().Should().BeTrue();
            model
                .Rows[0]
                .SetSubfolders(
                    new[] { Child("alpha", ParentPath, "Alpha"), Child("beta", ParentPath, "Beta") }
                );
            model.Rows[1].TryExpandLeaf().Should().BeTrue();
            model
                .Rows[1]
                .SetSubfolders(
                    new[]
                    {
                        Child("gamma", TargetParentPath, "Gamma"),
                        Child("delta", TargetParentPath, "Delta"),
                    }
                );
            return new SessionHarness(model, new BreadcrumbSelectionSession(model));
        }

        private static FolderBreadcrumbSegment Child(string entryId, string parentPath, string name)
        {
            string path = parentPath + "\\" + name;
            return new FolderBreadcrumbSegment(
                new FolderTreeNodeKey("store-a", entryId, path),
                name,
                path,
                false
            );
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
