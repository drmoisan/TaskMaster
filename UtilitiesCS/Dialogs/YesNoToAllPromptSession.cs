#nullable enable
using System;

namespace UtilitiesCS
{
    /// <summary>
    /// Holds the answer to one Yes/No/YesToAll/NoToAll prompt across calls, so that a "ToAll"
    /// answer is reused without asking again while a single answer is used once. The prompt is
    /// supplied as a delegate, which lets a caller replace the modal dialog (for example, a unit
    /// test that must not show a form). Instances are not synchronized; one session serves one
    /// caller at a time.
    /// </summary>
    internal sealed class YesNoToAllPromptSession
    {
        private readonly Func<string, YesNoToAllResponse> _showDialog;

        /// <summary>
        /// Creates a session whose prompts are answered by <paramref name="showDialog"/>.
        /// </summary>
        /// <param name="showDialog">Shows the prompt message and returns the answer.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="showDialog"/> is null.
        /// </exception>
        internal YesNoToAllPromptSession(Func<string, YesNoToAllResponse> showDialog)
        {
            _showDialog = showDialog ?? throw new ArgumentNullException(nameof(showDialog));
        }

        /// <summary>
        /// Gets the answer the session holds. <see cref="YesNoToAllResponse.Empty"/> means that
        /// the next call to <see cref="Ask"/> shows the prompt.
        /// </summary>
        internal YesNoToAllResponse Response { get; private set; }

        /// <summary>
        /// Returns the held answer, showing the prompt first when the session holds no answer.
        /// </summary>
        /// <param name="message">The prompt text, shown only when no answer is held.</param>
        /// <returns>The held answer, or the answer the prompt returned.</returns>
        internal YesNoToAllResponse Ask(string message)
        {
            if (Response == YesNoToAllResponse.Empty)
            {
                Response = _showDialog(message);
            }

            return Response;
        }

        /// <summary>
        /// Releases a single-use answer (<see cref="YesNoToAllResponse.Yes"/> or
        /// <see cref="YesNoToAllResponse.No"/>) and keeps a "ToAll" answer.
        /// </summary>
        internal void ReleaseSingleAnswer()
        {
            if (Response == YesNoToAllResponse.Yes || Response == YesNoToAllResponse.No)
            {
                Response = YesNoToAllResponse.Empty;
            }
        }

        /// <summary>
        /// Clears any held answer, including a "ToAll" answer.
        /// </summary>
        internal void Reset()
        {
            Response = YesNoToAllResponse.Empty;
        }
    }
}
