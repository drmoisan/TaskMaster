using System;
using System.Reflection;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.Dialogs
{
    /// <summary>
    /// Unit tests for <see cref="NotImplementedDialog"/>.
    ///
    /// Purpose:
    ///     NotImplementedDialog is a static class whose public entry point creates a blocking
    ///     WinForms dialog, making it non-testable directly. The two private helper methods
    ///     that encode the "throw" and "keep running" decisions ARE unit-testable via reflection.
    ///
    /// Coverage strategy:
    ///     - ThrowException() → verifies the "throw" decision path by asserting DialogResult.Yes
    ///     - KeepRunning() → verifies the "keep running" decision path by asserting DialogResult.No
    ///     - Both paths drive the bool return value of StopAtNotImplemented via the dialog result.
    /// </summary>
    [TestClass]
    public class NotImplementedDialog_Tests
    {
        // ---------------------------------------------------------------------------
        // Null static-state capture/restore (P5-T3): no settable static bool exists on
        // NotImplementedDialog, so cleanup is a no-op; the attribute is present to satisfy
        // test-isolation requirements in case the production class gains static state later.
        // ---------------------------------------------------------------------------

        [TestInitialize]
        public void TestInitialize()
        {
            // No static boolean state to capture on the current implementation.
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // No static boolean state to restore on the current implementation.
        }

        // ---------------------------------------------------------------------------
        // P5-T1: ThrowException path returns the "throw" signal (DialogResult.Yes)
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void ThrowException_ReturnsDialogResultYes()
        {
            // Arrange — ThrowException() is private; invoke via reflection to cover the throw decision branch.
            // Returning DialogResult.Yes is the contract that StopAtNotImplemented interprets as "throw".
            MethodInfo method =
                typeof(NotImplementedDialog).GetMethod(
                    "ThrowException",
                    BindingFlags.NonPublic | BindingFlags.Static
                ) ?? throw new MissingMethodException(nameof(NotImplementedDialog), "ThrowException");

            // Act
            DialogResult result = (DialogResult)method.Invoke(null, Array.Empty<object>())!;

            // Assert
            result.Should().Be(
                DialogResult.Yes,
                "ThrowException returns Yes, which StopAtNotImplemented maps to returning true (throw)"
            );
        }

        // ---------------------------------------------------------------------------
        // P5-T2: KeepRunning path returns the "keep running" signal (DialogResult.No)
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void KeepRunning_ReturnsDialogResultNo()
        {
            // Arrange — KeepRunning() is private; invoke via reflection to cover the keep-running decision branch.
            // Returning DialogResult.No is the contract that StopAtNotImplemented interprets as "do not throw".
            MethodInfo method =
                typeof(NotImplementedDialog).GetMethod(
                    "KeepRunning",
                    BindingFlags.NonPublic | BindingFlags.Static
                ) ?? throw new MissingMethodException(nameof(NotImplementedDialog), "KeepRunning");

            // Act
            DialogResult result = (DialogResult)method.Invoke(null, Array.Empty<object>())!;

            // Assert
            result.Should().Be(
                DialogResult.No,
                "KeepRunning returns No, which StopAtNotImplemented maps to returning false (keep running)"
            );
        }
    }
}
