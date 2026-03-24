using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.Dialogs
{
    /// <summary>
    /// Unit tests for <see cref="FolderNotFoundViewer"/>.
    ///
    /// Purpose:
    ///     Verify that each action button correctly sets the FolderAction property,
    ///     that the FolderName property round-trips text, and that action buttons
    ///     hide rather than dispose the viewer.
    ///
    /// Constraints:
    ///     All tests run on an STA thread (required by WinForms).
    ///     Click handlers are invoked via reflection because they are private.
    /// </summary>
    [TestClass]
    public class FolderNotFoundViewer_Tests
    {
        // ---------------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Invokes a private instance method on the viewer by name with no arguments.
        /// </summary>
        /// <param name="viewer">The viewer instance to invoke on.</param>
        /// <param name="methodName">The private method name to invoke.</param>
        private static void InvokeClickHandler(FolderNotFoundViewer viewer, string methodName)
        {
            MethodInfo method =
                typeof(FolderNotFoundViewer).GetMethod(
                    methodName,
                    BindingFlags.NonPublic | BindingFlags.Instance
                ) ?? throw new MissingMethodException(nameof(FolderNotFoundViewer), methodName);

            method.Invoke(viewer, new object[] { viewer, EventArgs.Empty });
        }

        // ---------------------------------------------------------------------------
        // P1-T1: Save-style button sets FolderAction to "Create"
        // ---------------------------------------------------------------------------

        [TestMethod]
        [STAThread]
        public void CreateFolder_Click_SetsFolderActionToCreate()
        {
            // Arrange — create viewer and confirm FolderAction starts null
            using var viewer = new FolderNotFoundViewer();
            viewer.FolderAction.Should().BeNull();

            // Act — invoke the save-style (CreateFolder) click handler
            InvokeClickHandler(viewer, "CreateFolder_Click");

            // Assert — FolderAction must equal the save/keep enum value
            viewer.FolderAction.Should().Be("Create");
        }

        // ---------------------------------------------------------------------------
        // P1-T2: Discard-style button sets FolderAction to "Cancel"
        // ---------------------------------------------------------------------------

        [TestMethod]
        [STAThread]
        public void Cancel_Click_SetsFolderActionToCancel()
        {
            // Arrange
            using var viewer = new FolderNotFoundViewer();

            // Act — invoke the discard/cancel click handler
            InvokeClickHandler(viewer, "Cancel_Click");

            // Assert
            viewer.FolderAction.Should().Be("Cancel");
        }

        // ---------------------------------------------------------------------------
        // Additional action buttons — extend coverage for OpenFolder and NoToAll
        // ---------------------------------------------------------------------------

        [TestMethod]
        [STAThread]
        public void OpenFolder_Click_SetsFolderActionToFind()
        {
            // Arrange
            using var viewer = new FolderNotFoundViewer();

            // Act
            InvokeClickHandler(viewer, "OpenFolder_Click");

            // Assert
            viewer.FolderAction.Should().Be("Find");
        }

        [TestMethod]
        [STAThread]
        public void NoToAll_Click_SetsFolderActionToNoToAll()
        {
            // Arrange
            using var viewer = new FolderNotFoundViewer();

            // Act
            InvokeClickHandler(viewer, "NoToAll_Click");

            // Assert
            viewer.FolderAction.Should().Be("NoToAll");
        }

        // ---------------------------------------------------------------------------
        // P1-T3: FolderName round-trips text correctly
        // ---------------------------------------------------------------------------

        [TestMethod]
        [STAThread]
        public void FolderName_ReturnsAssignedText()
        {
            // Arrange
            using var viewer = new FolderNotFoundViewer();
            const string expected = @"C:\TestFolder\Missing";

            // Act — set via the property setter, which writes to the backing TextBox
            viewer.FolderName = expected;

            // Assert — getter reads from the same TextBox
            viewer.FolderName.Should().Be(expected);
        }

        // ---------------------------------------------------------------------------
        // P1-T4: Action buttons call Hide, not Dispose
        // ---------------------------------------------------------------------------

        [TestMethod]
        [STAThread]
        public void CreateFolder_Click_DoesNotDisposeViewer()
        {
            // Arrange
            using var viewer = new FolderNotFoundViewer();

            // Act
            InvokeClickHandler(viewer, "CreateFolder_Click");

            // Assert — viewer must not be disposed (Hide was called, not Dispose/Close)
            viewer.IsDisposed.Should().BeFalse();
        }
    }
}
