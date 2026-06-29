using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Controllers;

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
    }
}
