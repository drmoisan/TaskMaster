using System.Windows.Forms;

namespace Tags
{
    /// <summary>
    /// Dialog seam over <c>MessageBox</c>/<c>InputBox</c> so <see cref="TagController"/> can be
    /// unit-tested without showing a live dialog or constructing an <c>InputBoxViewer</c>.
    /// Production uses <see cref="WinFormsUserPrompt"/>; tests inject a Moq mock.
    /// </summary>
    public interface IUserPrompt
    {
        /// <summary>Shows a Yes/No question and returns the user's choice.</summary>
        DialogResult ShowYesNo(string message, string title);

        /// <summary>Shows an informational message with no return value.</summary>
        void ShowMessage(string message);

        /// <summary>Prompts for a category name and returns the entered text (or null if cancelled).</summary>
        string GetCategoryInput(string prompt, string title, string defaultResponse);
    }
}
