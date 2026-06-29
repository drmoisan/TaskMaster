using System.Windows.Forms;

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
    }
}
