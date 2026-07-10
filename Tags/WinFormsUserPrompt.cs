using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using UtilitiesCS;

namespace Tags
{
    /// <summary>
    /// Production <see cref="IUserPrompt"/> adapter over <see cref="MessageBox"/> and
    /// <see cref="InputBox"/>. Register E1: thin, host-bound realization of live dialog UI with no
    /// decision logic to unit-test; every method shows a real modal window (and, for
    /// <see cref="GetCategoryInput"/>, a popup), which the maintainer-ratified STA refinement never
    /// permits. Marked <see cref="ExcludeFromCodeCoverageAttribute"/> per the Coverage Exemption Register.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class WinFormsUserPrompt : IUserPrompt
    {
        public DialogResult ShowYesNo(string message, string title) =>
            MessageBox.Show(message, title, MessageBoxButtons.YesNo);

        public void ShowMessage(string message) => MessageBox.Show(message);

        public string GetCategoryInput(string prompt, string title, string defaultResponse) =>
            InputBox.ShowDialog(prompt, title, DefaultResponse: defaultResponse);
    }
}
