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
            // Reset DisplayInvoker seam to the real implementation after each test to
            // prevent cross-test contamination from P2-T8 and P2-T9 seam mutations.
            NotImplementedDialog.DisplayInvoker = viewer => viewer.ShowDialog();
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
                )
                ?? throw new MissingMethodException(nameof(NotImplementedDialog), "ThrowException");

            // Act
            DialogResult result = (DialogResult)method.Invoke(null, Array.Empty<object>())!;

            // Assert
            result
                .Should()
                .Be(
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
            result
                .Should()
                .Be(
                    DialogResult.No,
                    "KeepRunning returns No, which StopAtNotImplemented maps to returning false (keep running)"
                );
        }

        // ---------------------------------------------------------------------------
        // P2-T8: StopAtNotImplemented returns true (throw) when seam returns Yes
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that StopAtNotImplemented returns true when the injected display seam
        /// returns DialogResult.Yes, exercising the throw-exception branch.
        ///
        /// Purpose:
        ///     Cover the StopAtNotImplemented wrapper body including the if (result == DialogResult.Yes)
        ///     branch that returns true.  Uses the DisplayInvoker seam instead of a real modal dialog.
        ///
        /// Returns:
        ///     true when seam returns DialogResult.Yes.
        /// </summary>
        [STATestMethod]
        public void StopAtNotImplemented_SeamReturnsYes_ReturnsTrueThrowPath()
        {
            // Arrange: inject seam that returns Yes (throw-exception decision)
            NotImplementedDialog.DisplayInvoker = _ => DialogResult.Yes;

            // Act: invoke the public entry point with a custom function name
            bool result = NotImplementedDialog.StopAtNotImplemented("MyCustomFunction");

            // Assert: Yes maps to the "throw exception" decision → returns true
            result.Should().BeTrue("DialogResult.Yes signals the throw-exception path");
        }

        // ---------------------------------------------------------------------------
        // P2-T9: StopAtNotImplemented returns false (keep running) when seam returns No
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that StopAtNotImplemented returns false when the injected display seam
        /// returns DialogResult.No, exercising the keep-running (default/else) branch.
        ///
        /// Purpose:
        ///     Cover the else branch of StopAtNotImplemented that returns false,
        ///     exercising the default/keep-running path via the DisplayInvoker seam.
        ///
        /// Returns:
        ///     false when seam returns DialogResult.No.
        /// </summary>
        [STATestMethod]
        public void StopAtNotImplemented_SeamReturnsNo_ReturnsFalseKeepRunningPath()
        {
            // Arrange: inject seam that returns No (keep-running decision)
            NotImplementedDialog.DisplayInvoker = _ => DialogResult.No;

            // Act: invoke the public entry point
            bool result = NotImplementedDialog.StopAtNotImplemented("AnotherFunction");

            // Assert: No maps to the "keep running" decision → returns false
            result.Should().BeFalse("DialogResult.No signals the keep-running (else) path");
        }
    }
}
