using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Unit tests for the pure <see cref="QfcFormKeyHandler.IsAltKeyCommand(Keys)"/> predicate
    /// extracted from the form variants' <c>ProcessCmdKey</c> overrides (Seam A).
    /// </summary>
    [TestClass]
    public class QfcFormKeyHandlerTests
    {
        [TestMethod]
        public void IsAltKeyCommand_WithAltKey_ReturnsTrue()
        {
            // Arrange
            var keyData = Keys.Alt;

            // Act
            var result = QfcFormKeyHandler.IsAltKeyCommand(keyData);

            // Assert
            result.Should().BeTrue("the Alt modifier alone is an Alt-key command");
        }

        [TestMethod]
        public void IsAltKeyCommand_WithAltPlusOtherKey_ReturnsTrue()
        {
            // Arrange
            var keyData = Keys.Alt | Keys.Left;

            // Act
            var result = QfcFormKeyHandler.IsAltKeyCommand(keyData);

            // Assert
            result.Should().BeTrue("the Alt flag is set even when combined with another key");
        }

        [TestMethod]
        public void IsAltKeyCommand_WithControlKey_ReturnsFalse()
        {
            // Arrange
            var keyData = Keys.Control;

            // Act
            var result = QfcFormKeyHandler.IsAltKeyCommand(keyData);

            // Assert
            result.Should().BeFalse("the Control modifier is not an Alt-key command");
        }

        [TestMethod]
        public void IsAltKeyCommand_WithNone_ReturnsFalse()
        {
            // Arrange
            var keyData = Keys.None;

            // Act
            var result = QfcFormKeyHandler.IsAltKeyCommand(keyData);

            // Assert
            result.Should().BeFalse("no key data carries no Alt modifier");
        }

        // Issue #663. The QuickFiler form's ProcessCmdKey dispatches the parameterless
        // ToggleKeyboardDialogAsync() overload, which accepts no key data, so the only gesture that
        // dispatch can encode is a bare Alt press. ClaimsAltChord therefore accepts the Alt modifier
        // only when the key-code half of the value, keyData & Keys.KeyCode, is Keys.Menu or
        // Keys.None. Every other Alt chord is a mnemonic or a system chord and must fall through to
        // the base implementation.

        // Positive case, synthetic shape: the bare Keys.Alt value a unit test supplies masks to
        // Keys.None in its key-code half.
        [TestMethod]
        public void ClaimsAltChord_WithBareAltFlagAndHandler_ReturnsTrue()
        {
            // Arrange
            var handler = new Mock<IQfcKeyboardHandler>();

            // Act
            var result = QfcFormKeyHandler.ClaimsAltChord(handler.Object, Keys.Alt);

            // Assert
            result
                .Should()
                .BeTrue(
                    "bare Alt is the only chord the keyboard-navigation dialog toggle services"
                );
        }

        // Positive case, physical-keyboard shape: a real bare Alt press arrives with the Alt
        // modifier flag set and Keys.Menu, documented as "The ALT key", in its key-code half.
        [TestMethod]
        public void ClaimsAltChord_WithMenuKeyCodeAndAltFlag_ReturnsTrue()
        {
            // Arrange
            var handler = new Mock<IQfcKeyboardHandler>();

            // Act
            var result = QfcFormKeyHandler.ClaimsAltChord(handler.Object, Keys.Menu | Keys.Alt);

            // Assert
            result
                .Should()
                .BeTrue(
                    "a physical bare Alt press carries the Keys.Menu key code with the Alt flag"
                );
        }

        // Negative case, the one real mnemonic on this surface: the hosted ItemViewer and
        // ItemViewerExpanded controls each carry a "&Move Options" menu item.
        [TestMethod]
        public void ClaimsAltChord_WithAltM_ReturnsFalse()
        {
            // Arrange
            var handler = new Mock<IQfcKeyboardHandler>();

            // Act
            var result = QfcFormKeyHandler.ClaimsAltChord(handler.Object, Keys.Alt | Keys.M);

            // Assert
            result
                .Should()
                .BeFalse(
                    "Alt+M is the Move Options mnemonic on the hosted item viewers and must reach the base implementation"
                );
        }

        // Negative case, system chord: Alt+F4 reaches ProcessCmdKey as WM_SYSKEYDOWN before the
        // default window procedure can translate it into the close command.
        [TestMethod]
        public void ClaimsAltChord_WithAltF4_ReturnsFalse()
        {
            // Arrange
            var handler = new Mock<IQfcKeyboardHandler>();

            // Act
            var result = QfcFormKeyHandler.ClaimsAltChord(handler.Object, Keys.Alt | Keys.F4);

            // Assert
            result
                .Should()
                .BeFalse("Alt+F4 is the standard window-close chord and must not be consumed here");
        }

        // Negative case, vestigial chord: no keyboard registry on this surface is keyed on an
        // Alt-modified arrow value, so claiming Alt+arrow discards a key the form will not act on.
        [TestMethod]
        public void ClaimsAltChord_WithAltLeft_ReturnsFalse()
        {
            // Arrange
            var handler = new Mock<IQfcKeyboardHandler>();

            // Act
            var result = QfcFormKeyHandler.ClaimsAltChord(handler.Object, Keys.Alt | Keys.Left);

            // Assert
            result
                .Should()
                .BeFalse("Alt+arrow is vestigial on this surface and must fall through unclaimed");
        }

        // Negative case, no Alt modifier at all. Two inputs are asserted in one body: a bare letter
        // key, and Keys.Control, whose key-code half is Keys.None and which would be accepted by a
        // predicate that inspected only the key-code half without first testing the Alt flag.
        [TestMethod]
        public void ClaimsAltChord_WithoutAltFlag_ReturnsFalse()
        {
            // Arrange
            var handler = new Mock<IQfcKeyboardHandler>();

            // Act
            var withLetterKey = QfcFormKeyHandler.ClaimsAltChord(handler.Object, Keys.M);
            var withControlModifier = QfcFormKeyHandler.ClaimsAltChord(
                handler.Object,
                Keys.Control
            );

            // Assert
            withLetterKey
                .Should()
                .BeFalse("a bare letter key carries no Alt flag and is not the dialog gesture");
            withControlModifier
                .Should()
                .BeFalse(
                    "Keys.Control carries no Alt flag even though its key-code half is Keys.None"
                );
        }

        // Negative case, unwired handler: with nothing to dispatch to, the chord is not claimed and
        // reaches the base implementation unchanged.
        [TestMethod]
        public void ClaimsAltChord_WithNullHandler_ReturnsFalse()
        {
            // Arrange
            IQfcKeyboardHandler handler = null;

            // Act
            var result = QfcFormKeyHandler.ClaimsAltChord(handler, Keys.Alt);

            // Assert
            result
                .Should()
                .BeFalse("with no handler wired there is nothing to claim the chord for");
        }
    }
}
