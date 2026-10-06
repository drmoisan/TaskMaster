using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for the SaveCase switch and the seamed SaveCaseAsync core of
    /// <see cref="SortEmail"/> (issue #959: defect L1 and the alternate-name prompt session).
    /// Each test passes a Moq attachment, rooted literal paths, its own scripted prompt session
    /// and a recording try-save delegate; no file is created, no dialog is shown and no
    /// production session is read or written.
    /// </summary>
    [TestClass]
    public class SortEmail_SaveCase_Tests
    {
        // Rooted literal paths used only as in-memory values; SaveAsFile is a mock.
        private const string RequestedPath = @"C:\Sortemail959Sandbox\attachments\report.pdf";
        private const string AlternatePath = @"C:\Sortemail959Sandbox\attachments\report_alt.pdf";
        private const string AltNamePrompt =
            "The file " + RequestedPath + " already exists. Save with an alternate name?";

        /// <summary>
        /// L1-A. Scenario: the overwrite answer is No or NoToAll. Expected: one save to the
        /// alternate path and none to the requested path.
        /// </summary>
        [DataTestMethod]
        [DataRow(
            YesNoToAllResponse.No,
            DisplayName = "SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [No]"
        )]
        [DataRow(
            YesNoToAllResponse.NoToAll,
            DisplayName = "SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [NoToAll]"
        )]
        public void SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath(YesNoToAllResponse answer)
        {
            // Arrange
            var attachment = new Mock<Attachment>(MockBehavior.Loose);

            // Act
            SortEmail.SaveCase(answer, attachment.Object, RequestedPath, AlternatePath);

            // Assert
            attachment.Verify(x => x.SaveAsFile(AlternatePath), Times.Once);
            attachment.Verify(x => x.SaveAsFile(RequestedPath), Times.Never);
        }

        /// <summary>
        /// L1-B. Scenario: the overwrite answer is Yes or YesToAll. Expected: one save to the
        /// requested path and none to the alternate path.
        /// </summary>
        [DataTestMethod]
        [DataRow(
            YesNoToAllResponse.Yes,
            DisplayName = "SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [Yes]"
        )]
        [DataRow(
            YesNoToAllResponse.YesToAll,
            DisplayName = "SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [YesToAll]"
        )]
        public void SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath(
            YesNoToAllResponse answer
        )
        {
            // Arrange
            var attachment = new Mock<Attachment>(MockBehavior.Loose);

            // Act
            SortEmail.SaveCase(answer, attachment.Object, RequestedPath, AlternatePath);

            // Assert
            attachment.Verify(x => x.SaveAsFile(RequestedPath), Times.Once);
            attachment.Verify(x => x.SaveAsFile(AlternatePath), Times.Never);
        }

        /// <summary>
        /// L1-C (control). Scenario: the answer is Empty (a cancelled prompt). Expected: no save
        /// on either path.
        /// </summary>
        [TestMethod]
        public void SaveCase_WhenAnswerIsEmpty_DoesNotSave()
        {
            // Arrange
            var attachment = new Mock<Attachment>(MockBehavior.Loose);

            // Act
            SortEmail.SaveCase(
                YesNoToAllResponse.Empty,
                attachment.Object,
                RequestedPath,
                AlternatePath
            );

            // Assert
            attachment.Verify(x => x.SaveAsFile(It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// SC1. Scenario: the overwrite answer is Yes or YesToAll. Expected: one try-save to the
        /// requested path; the alternate-name session is never asked.
        /// </summary>
        [DataTestMethod]
        [DataRow(
            YesNoToAllResponse.Yes,
            DisplayName = "SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt [Yes]"
        )]
        [DataRow(
            YesNoToAllResponse.YesToAll,
            DisplayName = "SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt [YesToAll]"
        )]
        public async Task SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt(
            YesNoToAllResponse answer
        )
        {
            // Arrange
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            var altName = new ScriptedPrompt();
            var saves = new List<string>();

            // Act
            await SortEmail.SaveCaseAsync(
                answer,
                attachment.Object,
                RequestedPath,
                AlternatePath,
                altName.Session,
                RecordingSave(saves)
            );

            // Assert
            saves.Should().Equal(RequestedPath);
            altName.Messages.Should().BeEmpty();
            altName.Session.Response.Should().Be(YesNoToAllResponse.Empty);
        }

        /// <summary>
        /// SC2. Scenario: the overwrite answer is No and the alternate-name answer is Yes.
        /// Expected: the alternate-name prompt is shown once with the requested path, one
        /// try-save goes to the alternate path, and the single Yes answer is released.
        /// </summary>
        [TestMethod]
        public async Task SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePathAndReleasesAnswer()
        {
            // Arrange
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            var altName = new ScriptedPrompt(YesNoToAllResponse.Yes);
            var saves = new List<string>();

            // Act
            await SortEmail.SaveCaseAsync(
                YesNoToAllResponse.No,
                attachment.Object,
                RequestedPath,
                AlternatePath,
                altName.Session,
                RecordingSave(saves)
            );

            // Assert
            altName.Messages.Should().Equal(AltNamePrompt);
            saves.Should().Equal(AlternatePath);
            altName.Session.Response.Should().Be(YesNoToAllResponse.Empty);
        }

        /// <summary>
        /// SC3. Scenario: the overwrite answer is NoToAll and the alternate-name answer is
        /// YesToAll, over two calls. Expected: one prompt, two alternate-path saves, the
        /// YesToAll answer kept.
        /// </summary>
        [TestMethod]
        public async Task SaveCaseAsync_WhenAnswerIsNoToAllAndAltNameAnswerIsYesToAll_KeepsAnswerAcrossCalls()
        {
            // Arrange
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            var altName = new ScriptedPrompt(YesNoToAllResponse.YesToAll);
            var saves = new List<string>();

            // Act
            await SortEmail.SaveCaseAsync(
                YesNoToAllResponse.NoToAll,
                attachment.Object,
                RequestedPath,
                AlternatePath,
                altName.Session,
                RecordingSave(saves)
            );
            await SortEmail.SaveCaseAsync(
                YesNoToAllResponse.NoToAll,
                attachment.Object,
                RequestedPath,
                AlternatePath,
                altName.Session,
                RecordingSave(saves)
            );

            // Assert
            altName.Messages.Should().ContainSingle();
            saves.Should().Equal(AlternatePath, AlternatePath);
            altName.Session.Response.Should().Be(YesNoToAllResponse.YesToAll);
        }

        /// <summary>
        /// SC4. Scenario: the overwrite answer is No and the alternate-name answer is NoToAll,
        /// over two calls. Expected: one prompt, no save, the NoToAll answer kept.
        /// </summary>
        [TestMethod]
        public async Task SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsNoToAll_DoesNotSaveAndKeepsAnswer()
        {
            // Arrange
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            var altName = new ScriptedPrompt(YesNoToAllResponse.NoToAll);
            var saves = new List<string>();

            // Act
            await SortEmail.SaveCaseAsync(
                YesNoToAllResponse.No,
                attachment.Object,
                RequestedPath,
                AlternatePath,
                altName.Session,
                RecordingSave(saves)
            );
            await SortEmail.SaveCaseAsync(
                YesNoToAllResponse.No,
                attachment.Object,
                RequestedPath,
                AlternatePath,
                altName.Session,
                RecordingSave(saves)
            );

            // Assert
            altName.Messages.Should().ContainSingle();
            saves.Should().BeEmpty();
            altName.Session.Response.Should().Be(YesNoToAllResponse.NoToAll);
        }

        /// <summary>
        /// SC5. Scenario: the overwrite answer is No and the alternate-name prompt is cancelled
        /// (Empty), over two calls. Expected: no save, the session holds no answer, and the
        /// second call prompts again.
        /// </summary>
        [TestMethod]
        public async Task SaveCaseAsync_WhenAnswerIsNoAndAltNamePromptIsCancelled_DoesNotSaveAndStaysAskable()
        {
            // Arrange
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            var altName = new ScriptedPrompt(YesNoToAllResponse.Empty, YesNoToAllResponse.Empty);
            var saves = new List<string>();

            // Act
            await SortEmail.SaveCaseAsync(
                YesNoToAllResponse.No,
                attachment.Object,
                RequestedPath,
                AlternatePath,
                altName.Session,
                RecordingSave(saves)
            );
            await SortEmail.SaveCaseAsync(
                YesNoToAllResponse.No,
                attachment.Object,
                RequestedPath,
                AlternatePath,
                altName.Session,
                RecordingSave(saves)
            );

            // Assert
            altName.Messages.Should().Equal(AltNamePrompt, AltNamePrompt);
            saves.Should().BeEmpty();
            altName.Session.Response.Should().Be(YesNoToAllResponse.Empty);
        }

        /// <summary>
        /// SC6. Scenario: the overwrite answer is Empty. Expected: no prompt and no save.
        /// </summary>
        [TestMethod]
        public async Task SaveCaseAsync_WhenAnswerIsEmpty_DoesNothing()
        {
            // Arrange
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            var altName = new ScriptedPrompt();
            var saves = new List<string>();

            // Act
            await SortEmail.SaveCaseAsync(
                YesNoToAllResponse.Empty,
                attachment.Object,
                RequestedPath,
                AlternatePath,
                altName.Session,
                RecordingSave(saves)
            );

            // Assert
            altName.Messages.Should().BeEmpty();
            saves.Should().BeEmpty();
        }

        /// <summary>
        /// Returns a try-save delegate that records the requested path and reports success
        /// without touching any file.
        /// </summary>
        private static SortEmail.TrySaveAttachmentDelegate RecordingSave(List<string> saves)
        {
            return (attachment, path) =>
            {
                saves.Add(path);
                return Task.FromResult(true);
            };
        }

        /// <summary>
        /// Owns one prompt session whose answers are scripted; an unscripted prompt throws from
        /// the empty queue, which is how a test proves that a session was not asked.
        /// </summary>
        private sealed class ScriptedPrompt
        {
            private readonly Queue<YesNoToAllResponse> _answers;

            public ScriptedPrompt(params YesNoToAllResponse[] answers)
            {
                _answers = new Queue<YesNoToAllResponse>(answers);
                Session = new YesNoToAllPromptSession(Prompt);
            }

            public YesNoToAllPromptSession Session { get; }
            public List<string> Messages { get; } = new List<string>();

            private YesNoToAllResponse Prompt(string message)
            {
                Messages.Add(message);
                return _answers.Dequeue();
            }
        }
    }
}
