#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection
{
    /// <summary>
    /// Injectable filesystem seam used by <see cref="ConcurrentObservableCollection{T}"/> for
    /// its serialization/deserialization paths. Replaces the legacy
    /// <c>IScoCollectionFileSystem</c> seam that was removed with <c>ScoCollection.cs</c>.
    /// Production wiring is <see cref="ConcurrentObservableCollectionFileSystem"/>; tests swap in
    /// a mock so file IO can be exercised without touching the real filesystem.
    /// </summary>
    internal interface IConcurrentObservableCollectionFileSystem
    {
        bool Exists(string filePath);
        string ReadAllText(string filePath);
        StreamWriter CreateText(string filePath);
    }

    /// <summary>
    /// Injectable user-prompt seam used by <see cref="ConcurrentObservableCollection{T}"/> when a
    /// load error requires a create-or-abort decision. Replaces the legacy
    /// <c>IScoCollectionPrompt</c> seam.
    /// </summary>
    internal interface IConcurrentObservableCollectionPrompt
    {
        DialogResult ShowError(string messageText);
    }

    /// <summary>
    /// Default production filesystem implementation backed by <see cref="File"/>.
    /// </summary>
    [ExcludeFromCodeCoverage] // thin filesystem I/O passthrough (host-bound; see CLAUDE.md I/O-boundary exemption)
    internal sealed class ConcurrentObservableCollectionFileSystem
        : IConcurrentObservableCollectionFileSystem
    {
        public bool Exists(string filePath) => File.Exists(filePath);

        public string ReadAllText(string filePath) => File.ReadAllText(filePath);

        public StreamWriter CreateText(string filePath) => File.CreateText(filePath);
    }

    /// <summary>
    /// Default production prompt implementation backed by <see cref="MyBox"/>.
    /// </summary>
    [ExcludeFromCodeCoverage] // WinForms MessageBox prompt (host-bound UI; see CLAUDE.md WinForms exemption)
    internal sealed class ConcurrentObservableCollectionPrompt
        : IConcurrentObservableCollectionPrompt
    {
        public DialogResult ShowError(string messageText)
        {
            return MyBox.ShowDialog(
                messageText,
                "Error",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error
            );
        }
    }
}
