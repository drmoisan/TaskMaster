using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Regression tests for issue #467 — <c>EfcViewer.ProcessCmdKey</c> claiming every
    /// Alt-modified key and so swallowing the <c>Alt+F</c> and <c>Alt+M</c> menu mnemonics —
    /// and for the <c>EfcViewer</c> half of issue #466, the dead <c>SetController</c> /
    /// <c>_formController</c> / viewer-side <c>EditFiltersMenuItem_Click</c> trap.
    /// </summary>
    /// <remarks>
    /// This fixture does not derive from, construct, or show any
    /// <c>System.Windows.Forms.Form</c>. The input-routing logic is exercised through the
    /// extracted <c>internal static</c> predicate, which needs no window handle, following the
    /// pattern of <c>QfcFormKeyHandlerTests.cs</c>.
    ///
    /// The file is deliberately placed under <c>Controllers/</c> rather than <c>Viewers/</c>;
    /// the deviation from the mirrored test layout is recorded in the plan task that created it.
    /// </remarks>
    [TestClass]
    public class EfcViewerTests
    {
        /// <summary>
        /// #466 A. <c>EfcFormController</c> never calls <c>SetController</c>, unlike its QuickFiler
        /// twin, so <c>_formController</c> is permanently null. Both are removed rather than wired
        /// up, which disarms the trap without adding behaviour.
        /// </summary>
        [TestMethod]
        public void SetControllerAndFormControllerField_AreAbsentFromEfcViewerMetadata()
        {
            // Arrange / Act
            MethodInfo setController = typeof(EfcViewer).GetMethod(
                "SetController",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            FieldInfo formController = typeof(EfcViewer).GetField(
                "_formController",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            // Assert
            setController
                .Should()
                .BeNull("SetController has no call site, so #466 A closes it by removal");
            formController
                .Should()
                .BeNull(
                    "_formController is permanently null once SetController is gone and is removed with it"
                );
        }

        /// <summary>
        /// #466 A. The viewer-side handler dereferenced the permanently null
        /// <c>_formController</c>. It is unreachable today only because the Designer never wires
        /// <c>EditFiltersMenuItem.Click</c>; a routine Designer regeneration would arm it.
        /// </summary>
        [TestMethod]
        public void EditFiltersMenuItemClick_IsAbsentFromEfcViewerMetadata()
        {
            // Arrange
            MethodInfo[] declared = typeof(EfcViewer).GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            );

            // Act
            MethodInfo[] viewerSideHandlers = declared
                .Where(candidate => candidate.Name == "EditFiltersMenuItem_Click")
                .ToArray();

            // Assert
            viewerSideHandlers
                .Should()
                .BeEmpty(
                    "the viewer-side Edit Filters handler is a latent null-dereference trap and #466 A removes it"
                );
        }

        /// <summary>
        /// #466 A, stated positively. The Edit Filters command is not broken and is not being
        /// repaired: the form controller subscribes to the Designer control directly and its own
        /// handler is the live route. Removing the viewer-side duplicate must not disturb it.
        /// </summary>
        [TestMethod]
        public void FormEditFiltersMenuItemClick_IsStillDeclaredOnEfcFormController()
        {
            // Arrange / Act
            MethodInfo liveHandler = typeof(EfcFormController).GetMethod(
                "EditFiltersMenuItem_Click",
                BindingFlags.Public | BindingFlags.Instance
            );

            // Assert
            liveHandler
                .Should()
                .NotBeNull(
                    "EfcFormController.EditFiltersMenuItem_Click is the live Edit Filters route and must survive #466 A"
                );
        }

        // #467 (RC10). ToggleKeyboardDialogAsync never inspects the key data, so the claim the
        // handler actually services is bare Alt. Any Alt-plus-key chord is a WinForms mnemonic and
        // must reach base.ProcessCmdKey. The predicate is scoped to EfcViewer and narrows only
        // what EfcViewer claims; the Alt-mnemonic route to CharActions stays reachable.
        [TestMethod]
        public void ClaimsAltChord_WithBareAltAndHandler_ReturnsTrue()
        {
            var handler = new Mock<IQfcKeyboardHandler>();

            EfcViewer
                .ClaimsAltChord(handler.Object, Keys.Alt)
                .Should()
                .BeTrue("bare Alt is the chord the keyboard dialog services");
        }

        [TestMethod]
        public void ClaimsAltChord_WithAltF_ReturnsFalse()
        {
            var handler = new Mock<IQfcKeyboardHandler>();

            EfcViewer
                .ClaimsAltChord(handler.Object, Keys.Alt | Keys.F)
                .Should()
                .BeFalse("Alt+F is the Filters menu mnemonic and must reach base.ProcessCmdKey");
        }

        [TestMethod]
        public void ClaimsAltChord_WithAltM_ReturnsFalse()
        {
            var handler = new Mock<IQfcKeyboardHandler>();

            EfcViewer
                .ClaimsAltChord(handler.Object, Keys.Alt | Keys.M)
                .Should()
                .BeFalse("Alt+M is the Move Options menu mnemonic and must not be swallowed");
        }

        [TestMethod]
        public void ClaimsAltChord_WithNonAltChord_ReturnsFalse()
        {
            var handler = new Mock<IQfcKeyboardHandler>();

            EfcViewer
                .ClaimsAltChord(handler.Object, Keys.F)
                .Should()
                .BeFalse("a chord without the Alt flag is not the keyboard dialog gesture");
        }

        [TestMethod]
        public void ClaimsAltChord_WithNullHandler_ReturnsFalse()
        {
            EfcViewer
                .ClaimsAltChord(null, Keys.Alt)
                .Should()
                .BeFalse("with no handler wired there is nothing to claim the chord for");
        }
    }
}
