using System;
using System.Threading;
using System.Windows.Forms;

namespace UtilitiesCS
{
    /// <summary>
    /// Static wrapper that presents a modal input-box dialog and returns the user's entry.
    ///
    /// Purpose:
    ///     Encapsulates InputBoxViewer lifecycle (create, configure, show, dispose) behind a
    ///     single static method so callers never touch the viewer form directly.
    ///
    /// Seam:
    ///     <see cref="DialogInvoker"/> replaces the real ShowDialog() call.  Test code swaps
    ///     this delegate to inject a controlled DialogResult without opening a real modal window.
    ///     Production callers leave the default in place.
    /// </summary>
    public static class InputBox
    {
        /// <summary>
        /// Replaceable dialog-invoker seam.
        ///
        /// Purpose:
        ///     Allows unit tests to inject a <see cref="DialogResult"/> without opening a real
        ///     modal window.  Production code uses the default delegate, which calls
        ///     <see cref="Form.ShowDialog()"/> on the viewer.
        ///
        /// Usage:
        ///     Tests set this before calling ShowDialog and restore it in cleanup.
        ///
        /// Returns:
        ///     The <see cref="DialogResult"/> reported by the actual or injected dialog.
        /// </summary>
        // Per-flow storage so parallel test classes (ClassLevel parallelization) do not race
        // on a single shared static seam. AsyncLocal flows the value with the logical call
        // context, isolating injected stubs across concurrently executing test classes.
        private static readonly AsyncLocal<Func<InputBoxViewer, DialogResult>> _dialogInvoker =
            new AsyncLocal<Func<InputBoxViewer, DialogResult>>();

        private static readonly Func<InputBoxViewer, DialogResult> RealDialogInvoker = viewer =>
            viewer.ShowDialog();

        internal static Func<InputBoxViewer, DialogResult> DialogInvoker
        {
            get => _dialogInvoker.Value ?? RealDialogInvoker;
            set => _dialogInvoker.Value = value;
        }

        /// <summary>
        /// Presents an input-box dialog and returns the entered text, or null if cancelled.
        ///
        /// Purpose:
        ///     Creates an <see cref="InputBoxViewer"/>, populates its controls with the supplied
        ///     arguments, invokes it through <see cref="DialogInvoker"/>, and returns the text
        ///     the user entered when they accepted, or null when they cancelled.
        ///
        /// Args:
        ///     Prompt (string): Label text shown above the input field.
        ///     Title (string): Window title bar text.
        ///     DefaultResponse (string): Pre-populated value in the input field.
        ///
        /// Returns:
        ///     The text the user entered and accepted, or null if the dialog was cancelled.
        ///
        /// Side Effects:
        ///     Disposes the viewer form before returning.
        /// </summary>
        public static string ShowDialog(
            string Prompt,
            string Title = "",
            string DefaultResponse = ""
        )
        {
            var viewer = new InputBoxViewer();
            viewer.AcceptButton = viewer.Ok;
            viewer.CancelButton = viewer.Cancel;
            viewer.Message.Text = Prompt;
            viewer.Text = Title;
            viewer.Input.Text = DefaultResponse;
            viewer.Input.Select();

            // Invoke through the seam so tests can replace this without a real modal.
            DialogResult result = DialogInvoker(viewer);
            if (result == DialogResult.OK)
            {
                string value = viewer.Input.Text;
                viewer.Dispose();
                return value;
            }
            else
            {
                viewer.Dispose();
                return null;
            }
        }
    }
}
