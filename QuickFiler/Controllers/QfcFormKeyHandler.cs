using System.Windows.Forms;
using QuickFiler.Interfaces;

namespace QuickFiler.Controllers
{
    /// <summary>
    /// Pure routing predicates extracted from the QuickFiler form variants'
    /// <c>ProcessCmdKey</c> overrides so the key-command logic can be unit tested
    /// without a live <see cref="Form"/> window handle.
    /// </summary>
    internal static class QfcFormKeyHandler
    {
        /// <summary>
        /// Returns <see langword="true"/> when the supplied key combination should be
        /// handled as an Alt-key shortcut command (i.e. the Alt modifier is set).
        /// </summary>
        /// <param name="keyData">The key data reported by <c>ProcessCmdKey</c>.</param>
        /// <returns><see langword="true"/> if the Alt flag is present; otherwise <see langword="false"/>.</returns>
        internal static bool IsAltKeyCommand(Keys keyData) => keyData.HasFlag(Keys.Alt);

        /// <summary>
        /// Decides whether the QuickFiler form's <c>ProcessCmdKey</c> override should claim the
        /// supplied key chord for the keyboard-navigation dialog.
        /// </summary>
        /// <param name="handler">The keyboard handler the form dispatches to, or <see langword="null"/>.</param>
        /// <param name="keyData">The key data reported by <c>ProcessCmdKey</c>.</param>
        /// <returns><see langword="true"/> when the chord is claimed; otherwise <see langword="false"/>.</returns>
        internal static bool ClaimsAltChord(IQfcKeyboardHandler handler, Keys keyData)
        {
            if (handler is null || !keyData.HasFlag(Keys.Alt))
            {
                return false;
            }

            Keys keyCode = keyData & Keys.KeyCode;
            return keyCode == Keys.Menu || keyCode == Keys.None;
        }
    }
}
