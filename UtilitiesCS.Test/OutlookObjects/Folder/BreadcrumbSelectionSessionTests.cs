#nullable enable
using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>Failure-first selector-session contracts for issue #400.</summary>
    [TestClass]
    public sealed class BreadcrumbSelectionSessionTests
    {
        [TestMethod]
        public void ClosedNavigation_CommitsSelectableRows_SkipsLabelsAndStopsAtBoundaries()
        {
            // Arrange
            var model = CreateModel();
            model.SelectRow(0);
            object session = CreateSession(model);

            // Act and assert
            InvokeBool(session, "MoveNext").Should().BeTrue();
            model.SelectedIndex.Should().Be(2);
            Property<string>(session, "CommittedIdentity").Should().Be("folder-b");
            InvokeBool(session, "MoveNext").Should().BeFalse("the last row must not wrap");
            InvokeBool(session, "MovePrevious").Should().BeTrue();
            model.SelectedIndex.Should().Be(0);
            InvokeBool(session, "MovePrevious").Should().BeFalse("the first row must not wrap");
        }

        [TestMethod]
        public void OpenNavigation_ChangesPendingWithoutChangingCommittedOrModelSelection()
        {
            // Arrange
            var model = CreateModel();
            model.SelectRow(0);
            object session = CreateSession(model);

            // Act
            Invoke(session, "Open");
            bool moved = InvokeBool(session, "MoveNext");

            // Assert
            moved.Should().BeTrue();
            model.SelectedIndex.Should().Be(0);
            Property<string>(session, "CommittedIdentity").Should().Be("folder-a");
            Property<string>(session, "OriginalIdentity").Should().Be("folder-a");
            Property<string>(session, "PendingIdentity").Should().Be("folder-b");
        }

        [TestMethod]
        public void CommitPending_CommitsOnceClosesAndClearsTheOpenSnapshot()
        {
            // Arrange
            var model = CreateModel();
            model.SelectRow(0);
            object session = CreateSession(model);
            Invoke(session, "Open");
            InvokeBool(session, "MoveNext").Should().BeTrue();

            // Act
            bool changed = InvokeBool(session, "CommitPending");
            bool secondCommit = InvokeBool(session, "CommitPending");

            // Assert
            changed.Should().BeTrue();
            secondCommit.Should().BeFalse("an explicit commit closes the session");
            model.SelectedIndex.Should().Be(2);
            Property<bool>(session, "IsOpen").Should().BeFalse();
            Property<string?>(session, "OriginalIdentity").Should().BeNull();
            Property<string?>(session, "PendingIdentity").Should().BeNull();
        }

        [TestMethod]
        public void Cancel_RestoresOriginalAndNeverCommitsPending()
        {
            // Arrange
            var model = CreateModel();
            model.SelectRow(0);
            object session = CreateSession(model);
            Invoke(session, "Open");
            InvokeBool(session, "MoveNext").Should().BeTrue();

            // Act
            bool closed = InvokeBool(session, "Cancel");

            // Assert
            closed.Should().BeTrue();
            model.SelectedIndex.Should().Be(0);
            Property<string>(session, "CommittedIdentity").Should().Be("folder-a");
            Property<bool>(session, "IsOpen").Should().BeFalse();
            InvokeBool(session, "Cancel").Should().BeFalse("a closed session is a no-op");
        }

        [TestMethod]
        public void Activate_OpenSelectableIdentity_CommitsAndCloses()
        {
            // Arrange
            var model = CreateModel();
            model.SelectRow(0);
            object session = CreateSession(model);
            Invoke(session, "Open");

            // Act
            bool changed = InvokeBool(session, "Activate", "folder-b");

            // Assert
            changed.Should().BeTrue();
            model.SelectedIndex.Should().Be(2);
            Property<string>(session, "CommittedIdentity").Should().Be("folder-b");
            Property<bool>(session, "IsOpen").Should().BeFalse();
        }

        [TestMethod]
        public void Activate_InvalidAndClosedSelectableIdentities_ReturnExpectedChangeState()
        {
            // Arrange
            var model = CreateModel();
            model.SelectRow(0);
            object session = CreateSession(model);

            // Act
            bool invalidChanged = InvokeBool(session, "Activate", "missing-folder");
            bool selectedChanged = InvokeBool(session, "Activate", "folder-b");
            bool sameSelectionChanged = InvokeBool(session, "Activate", "folder-b");

            // Assert
            invalidChanged.Should().BeFalse();
            selectedChanged.Should().BeTrue();
            sameSelectionChanged.Should().BeFalse();
            model.SelectedIndex.Should().Be(2);
            Property<string>(session, "CommittedIdentity").Should().Be("folder-b");
        }

        [TestMethod]
        public void Cancel_WhenOpenedWithoutACommittedSelection_RestoresNoSelection()
        {
            // Arrange
            var model = CreateModel();
            object session = CreateSession(model);
            InvokeBool(session, "Open").Should().BeTrue();
            InvokeBool(session, "MoveNext").Should().BeTrue();

            // Act
            bool closed = InvokeBool(session, "Cancel");

            // Assert
            closed.Should().BeTrue();
            model.SelectedIndex.Should().Be(-1);
            Property<string?>(session, "CommittedIdentity").Should().BeNull();
            Property<bool>(session, "IsOpen").Should().BeFalse();
        }

        [TestMethod]
        public void EmptyAndNoSelectableModels_HaveDeterministicNoOpNavigation()
        {
            // Arrange
            var empty = new BreadcrumbStateModel();
            object emptySession = CreateSession(empty);
            var labelsOnly = new BreadcrumbStateModel();
            AddRow(labelsOnly, "label", "Choose a folder", false);
            object labelsSession = CreateSession(labelsOnly);

            // Act and assert
            InvokeBool(emptySession, "MoveNext").Should().BeFalse();
            Invoke(emptySession, "Open");
            InvokeBool(emptySession, "MovePrevious").Should().BeFalse();
            InvokeBool(emptySession, "CommitPending").Should().BeFalse();
            InvokeBool(labelsSession, "MoveNext").Should().BeFalse();
            labelsOnly.SelectedIndex.Should().Be(-1);
        }

        [TestMethod]
        public void BreadcrumbLeftAndRightTransitions_DoNotMutateSelectorSession()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            var key = new FolderTreeNodeKey("store", "entry", "\\Inbox");
            model.AddSuggestionRow(
                new[] { new FolderBreadcrumbSegment(key, "Inbox", "\\Inbox", true) },
                0.6
            );
            model.SelectRow(0);
            object session = CreateSession(model);
            Invoke(session, "Open");

            // Act
            model.RightArrow().Should().BeTrue();
            model.LeftArrow().Should().BeTrue();

            // Assert
            Property<string>(session, "CommittedIdentity").Should().Be(key.ToString());
            Property<string>(session, "OriginalIdentity").Should().Be(key.ToString());
            Property<string>(session, "PendingIdentity").Should().Be(key.ToString());
        }

        private static BreadcrumbStateModel CreateModel()
        {
            var model = new BreadcrumbStateModel();
            AddRow(model, "folder-a", "\\Inbox\\A", true);
            AddRow(model, "label", "Suggested folders", false);
            AddRow(model, "folder-b", "\\Inbox\\B", true);
            return model;
        }

        private static void AddRow(
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
            method
                .Should()
                .NotBeNull("issue #400 requires stable identity and selectable-row metadata");
            method!.Invoke(model, new object[] { identity, text, selectable });
        }

        private static object CreateSession(BreadcrumbStateModel model)
        {
            Type? type = typeof(BreadcrumbStateModel).Assembly.GetType(
                "UtilitiesCS.OutlookObjects.Folder.BreadcrumbSelectionSession",
                false
            );
            type.Should().NotBeNull("issue #400 requires a committed/original/pending session");
            return Activator.CreateInstance(type!, model)!;
        }

        private static void Invoke(object target, string method) =>
            target.GetType().GetMethod(method)!.Invoke(target, null);

        private static bool InvokeBool(object target, string method, params object[] arguments) =>
            (bool)target.GetType().GetMethod(method)!.Invoke(target, arguments)!;

        private static T Property<T>(object target, string property) =>
            (T)target.GetType().GetProperty(property)!.GetValue(target)!;
    }
}
