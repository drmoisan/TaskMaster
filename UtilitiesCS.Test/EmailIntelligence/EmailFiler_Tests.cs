using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for <see cref="EmailFiler"/>, targeting the pure helper methods
    /// accessible without COM or external I/O: the open-folder guard, the
    /// tab/CRLF stripping helper, and the undo-capture data path.
    ///
    /// <para>
    /// Usage notes:
    /// <list type="bullet">
    ///   <item><see cref="OpenFileSystemFolder"/> is internal — accessible via
    ///         [InternalsVisibleTo("UtilitiesCS.Test")] in AssemblyInfo.cs.</item>
    ///   <item><see cref="StripTabsCrLf"/> is internal — same assembly-level grant.</item>
    ///   <item>P39-T3 exercises the undo-capture data path through
    ///         <see cref="MovedMailInfo"/> (no-arg constructor + property setters)
    ///         and <see cref="ScoStack{T}"/> without requiring live COM objects.</item>
    /// </list>
    /// </para>
    /// </summary>
    [TestClass]
    public class EmailFiler_Tests
    {
        /// <summary>
        /// Verifies that <c>OpenFileSystemFolder</c> returns early without throwing or
        /// opening a shell process when the supplied path does not exist on disk.
        ///
        /// <para>
        /// Purpose: exercise the false branch of Directory.Exists so the logger
        /// path is covered and no Process.Start side-effect is triggered.
        /// </para>
        /// </summary>
        [TestMethod]
        public void OpenFileSystemFolder_WhenPathDoesNotExist_CompletesWithoutThrowing()
        {
            // Arrange — path that will never exist on any CI agent or dev machine
            var filer = new EmailFiler();
            const string nonexistentPath = @"C:\__NonExistentTaskMasterTestPath_XYZ_99__";

            // Act & Assert — the method must not throw; it logs the error and returns
            filer.Invoking(f => f.OpenFileSystemFolder(nonexistentPath)).Should().NotThrow();
        }

        /// <summary>
        /// Verifies that <c>StripTabsCrLf</c> replaces sequences of tabs, carriage
        /// returns, and line feeds with single spaces, collapses multiple spaces,
        /// and trims leading and trailing whitespace.
        ///
        /// <para>
        /// Purpose: pure-function path — no I/O or COM required; covers the two
        /// regex passes and the final Trim call.
        /// </para>
        /// </summary>
        [TestMethod]
        public void StripTabsCrLf_WhenInputContainsTabsAndCrLf_ReturnsCleanTrimmedString()
        {
            // Arrange
            var filer = new EmailFiler();

            // Tab–CRLF run → one space; double-tab run → one space; no leading/trailing spaces
            const string input = "Hello\t\r\nWorld\t\t!";

            // Act
            var result = filer.StripTabsCrLf(input);

            // Assert — each run of [\t\n\r]+ collapses to a single space, then trimmed
            result.Should().Be("Hello World !");
        }

        /// <summary>
        /// Verifies that the undo-stack capture mechanism stores move details correctly.
        /// Uses <see cref="MovedMailInfo"/> with the no-arg public constructor and
        /// explicit property assignment (mirroring the COM-backed constructor path)
        /// to confirm that a <see cref="ScoStack{T}"/> Peek returns the expected
        /// source and destination folder paths after Push.
        ///
        /// <para>
        /// Purpose: exercise the Push→Peek round-trip used by EmailFiler.PushToUndoStack
        /// without requiring live COM objects (MailItem / Outlook.Application).
        /// </para>
        /// </summary>
        [TestMethod]
        public void ScoStack_WhenMovedMailInfoPushed_RecordsExpectedPathsOnPeek()
        {
            // Arrange — construct a move record the same way PushToUndoStack does
            // but using the no-arg constructor so no COM call is required
            var info = new MovedMailInfo
            {
                FolderPathOld = "Inbox",
                FolderPathNew = "Archive",
                EntryId = "entry-abc-123",
                StoreId = "store-xyz-456",
            };
            var stack = new ScoStack<IMovedMailInfo>();

            // Act — simulate the stack.Push call inside EmailFiler.PushToUndoStack
            stack.Push(info);

            // Assert — the captured entry must reflect the source and destination paths
            stack.Count.Should().Be(1);
            var captured = stack.Peek();
            captured.FolderPathOld.Should().Be("Inbox");
            captured.FolderPathNew.Should().Be("Archive");
        }
    }
}
