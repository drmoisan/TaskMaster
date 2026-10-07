using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for the read-only-folder handling of the five-argument attachment save overload
    /// of <see cref="SortEmail"/>. Every test passes its own recording seams: a directory-creation
    /// delegate, a read-only-clear delegate and a fresh <see cref="YesNoToAllPromptSession"/>
    /// whose prompt returns scripted answers. The paths are rooted in-memory literals; no test
    /// touches the file system, shows a dialog or reads a static member of the class under test.
    /// </summary>
    [TestClass]
    public class SortEmail_TrySaveAttachment_Tests
    {
        // Rooted literal paths used only as in-memory values. The injected delegates record them
        // instead of acting on them, so nothing is created or changed on disk.
        private const string SandboxDirectory = @"C:\Sortemail956Sandbox\attachments";
        private const string SandboxFilePath = @"C:\Sortemail956Sandbox\attachments\saved.txt";
        private const string ExpectedPrompt =
            @"The folder C:\Sortemail956Sandbox\attachments is read-only. Do you want to remove the readonly attribute?";

        /// <summary>
        /// T1. Scenario: the first save succeeds. Expected: true; no prompt and no read-only clear;
        /// the directory is created once; the session still holds no answer.
        /// </summary>
        [TestMethod]
        public async Task TrySaveAttachmentAsync_WhenSaveSucceeds_DoesNotPromptOrClearReadOnly()
        {
            // Arrange
            var seams = new Seams();
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            attachment.SetupSequence(x => x.SaveAsFile(SandboxFilePath)).Pass();

            // Act
            bool saved = await SaveAsync(attachment, seams);

            // Assert
            saved.Should().BeTrue();
            seams.PromptMessages.Should().BeEmpty();
            seams.ClearedDirectories.Should().BeEmpty();
            seams.CreatedDirectories.Should().Equal(SandboxDirectory);
            seams.Session.Response.Should().Be(YesNoToAllResponse.Empty);
            attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Once);
        }

        /// <summary>
        /// T2. Scenario: the first save is denied and the answer is Yes. Expected: true; one prompt
        /// naming the folder; the directory cleared once; two saves; the Yes answer released.
        /// </summary>
        [TestMethod]
        public async Task TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYes_ClearsRetriesAndReleasesAnswer()
        {
            // Arrange
            var seams = new Seams(YesNoToAllResponse.Yes);
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            attachment
                .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                .Throws(new UnauthorizedAccessException("denied"))
                .Pass();

            // Act
            bool saved = await SaveAsync(attachment, seams);

            // Assert
            saved.Should().BeTrue();
            seams.PromptMessages.Should().Equal(ExpectedPrompt);
            seams.ClearedDirectories.Should().Equal(SandboxDirectory);
            seams.CreatedDirectories.Should().Equal(SandboxDirectory, SandboxDirectory);
            seams.Session.Response.Should().Be(YesNoToAllResponse.Empty);
            attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Exactly(2));
        }

        /// <summary>
        /// T3. Scenario: the first save is denied and the answer is YesToAll. Expected: true; the
        /// directory cleared once; two saves; the YesToAll answer stays held.
        /// </summary>
        [TestMethod]
        public async Task TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYesToAll_RetriesAndKeepsAnswer()
        {
            // Arrange
            var seams = new Seams(YesNoToAllResponse.YesToAll);
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            attachment
                .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                .Throws(new UnauthorizedAccessException("denied"))
                .Pass();

            // Act
            bool saved = await SaveAsync(attachment, seams);

            // Assert
            saved.Should().BeTrue();
            seams.PromptMessages.Should().Equal(ExpectedPrompt);
            seams.ClearedDirectories.Should().Equal(SandboxDirectory);
            seams.Session.Response.Should().Be(YesNoToAllResponse.YesToAll);
            attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Exactly(2));
        }

        /// <summary>
        /// T4. Scenario: two denied saves on one session after a YesToAll answer. Expected: both
        /// calls return true and the prompt is shown once; the second call reuses the answer.
        /// </summary>
        [TestMethod]
        public async Task TrySaveAttachmentAsync_WhenYesToAllIsHeld_SecondCallRetriesWithoutPrompt()
        {
            // Arrange
            var seams = new Seams(YesNoToAllResponse.YesToAll);
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            attachment
                .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                .Throws(new UnauthorizedAccessException("denied"))
                .Pass()
                .Throws(new UnauthorizedAccessException("denied"))
                .Pass();

            // Act
            bool first = await SaveAsync(attachment, seams);
            bool second = await SaveAsync(attachment, seams);

            // Assert
            first.Should().BeTrue();
            second.Should().BeTrue();
            seams.PromptMessages.Should().Equal(ExpectedPrompt);
            seams.ClearedDirectories.Should().Equal(SandboxDirectory, SandboxDirectory);
            seams.Session.Response.Should().Be(YesNoToAllResponse.YesToAll);
            attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Exactly(4));
        }

        /// <summary>
        /// T5. Scenario: the first save is denied and the answer is No. Expected: false; no clear;
        /// one save; the No answer released.
        /// </summary>
        [TestMethod]
        public async Task TrySaveAttachmentAsync_WhenDeniedAndAnswerIsNo_ReturnsFalseAndReleasesAnswer()
        {
            // Arrange
            var seams = new Seams(YesNoToAllResponse.No);
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            attachment
                .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                .Throws(new UnauthorizedAccessException("denied"));

            // Act
            bool saved = await SaveAsync(attachment, seams);

            // Assert
            saved.Should().BeFalse();
            seams.PromptMessages.Should().Equal(ExpectedPrompt);
            seams.ClearedDirectories.Should().BeEmpty();
            seams.Session.Response.Should().Be(YesNoToAllResponse.Empty);
            attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Once);
        }

        /// <summary>
        /// T6. Scenario: two denied saves on one session after a NoToAll answer. Expected: both
        /// calls return false, the prompt is shown once and the NoToAll answer stays held.
        /// </summary>
        [TestMethod]
        public async Task TrySaveAttachmentAsync_WhenNoToAllIsHeld_SecondCallReturnsFalseWithoutPrompt()
        {
            // Arrange
            var seams = new Seams(YesNoToAllResponse.NoToAll);
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            attachment
                .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                .Throws(new UnauthorizedAccessException("denied"))
                .Throws(new UnauthorizedAccessException("denied"));

            // Act
            bool first = await SaveAsync(attachment, seams);
            bool second = await SaveAsync(attachment, seams);

            // Assert
            first.Should().BeFalse();
            second.Should().BeFalse();
            seams.PromptMessages.Should().Equal(ExpectedPrompt);
            seams.ClearedDirectories.Should().BeEmpty();
            seams.Session.Response.Should().Be(YesNoToAllResponse.NoToAll);
            attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Exactly(2));
        }

        /// <summary>
        /// T7. Scenario: the first save is denied and the prompt is cancelled (Empty). Expected:
        /// the access exception propagates; no clear; the session holds no answer.
        /// </summary>
        [TestMethod]
        public async Task TrySaveAttachmentAsync_WhenPromptIsCancelled_RethrowsUnauthorizedAccessException()
        {
            // Arrange
            var seams = new Seams(YesNoToAllResponse.Empty);
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            attachment
                .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                .Throws(new UnauthorizedAccessException("denied"));

            // Act
            Func<Task> act = () => SaveAsync(attachment, seams);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            seams.PromptMessages.Should().Equal(ExpectedPrompt);
            seams.ClearedDirectories.Should().BeEmpty();
            seams.Session.Response.Should().Be(YesNoToAllResponse.Empty);
        }

        /// <summary>
        /// T8. Scenario: Yes is answered but clearing the attribute throws. Expected: false; no
        /// retry; the Yes answer released.
        /// </summary>
        [TestMethod]
        public async Task TrySaveAttachmentAsync_WhenClearThrowsAfterYes_ReturnsFalseAndReleasesAnswer()
        {
            // Arrange
            var seams = new Seams(YesNoToAllResponse.Yes)
            {
                ClearException = new IOException("attribute locked"),
            };
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            attachment
                .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                .Throws(new UnauthorizedAccessException("denied"));

            // Act
            bool saved = await SaveAsync(attachment, seams);

            // Assert
            saved.Should().BeFalse();
            seams.PromptMessages.Should().Equal(ExpectedPrompt);
            seams.ClearedDirectories.Should().Equal(SandboxDirectory);
            seams.Session.Response.Should().Be(YesNoToAllResponse.Empty);
            attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Once);
        }

        /// <summary>
        /// T9. Scenario: YesToAll is answered but clearing the attribute throws. Expected: false;
        /// no retry; the YesToAll answer stays held.
        /// </summary>
        [TestMethod]
        public async Task TrySaveAttachmentAsync_WhenClearThrowsAfterYesToAll_ReturnsFalseAndKeepsAnswer()
        {
            // Arrange
            var seams = new Seams(YesNoToAllResponse.YesToAll)
            {
                ClearException = new IOException("attribute locked"),
            };
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            attachment
                .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                .Throws(new UnauthorizedAccessException("denied"));

            // Act
            bool saved = await SaveAsync(attachment, seams);

            // Assert
            saved.Should().BeFalse();
            seams.PromptMessages.Should().Equal(ExpectedPrompt);
            seams.ClearedDirectories.Should().Equal(SandboxDirectory);
            seams.Session.Response.Should().Be(YesNoToAllResponse.YesToAll);
            attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Once);
        }

        /// <summary>
        /// T10. Scenario: the save throws an exception other than an access denial. Expected: the
        /// exception propagates unchanged and no prompt or clear occurs.
        /// </summary>
        [TestMethod]
        public async Task TrySaveAttachmentAsync_WhenSaveThrowsOtherException_PropagatesWithoutPrompt()
        {
            // Arrange
            var seams = new Seams();
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            attachment
                .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                .Throws(new IOException("disk failure"));

            // Act
            Func<Task> act = () => SaveAsync(attachment, seams);

            // Assert
            await act.Should().ThrowAsync<IOException>();
            seams.PromptMessages.Should().BeEmpty();
            seams.ClearedDirectories.Should().BeEmpty();
            seams.Session.Response.Should().Be(YesNoToAllResponse.Empty);
        }

        /// <summary>
        /// T11. Scenario: Yes is answered, the retry is denied again and the second answer is No.
        /// Expected: false after two prompts, one clear and two saves.
        /// </summary>
        [TestMethod]
        public async Task TrySaveAttachmentAsync_WhenRetryIsDeniedAndSecondAnswerIsNo_ReturnsFalse()
        {
            // Arrange
            var seams = new Seams(YesNoToAllResponse.Yes, YesNoToAllResponse.No);
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            attachment
                .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                .Throws(new UnauthorizedAccessException("denied"))
                .Throws(new UnauthorizedAccessException("denied"));

            // Act
            bool saved = await SaveAsync(attachment, seams);

            // Assert
            saved.Should().BeFalse();
            seams.PromptMessages.Should().Equal(ExpectedPrompt, ExpectedPrompt);
            seams.ClearedDirectories.Should().Equal(SandboxDirectory);
            seams.Session.Response.Should().Be(YesNoToAllResponse.Empty);
            attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Exactly(2));
        }

        /// <summary>
        /// T12. Scenario: YesToAll is held, the attribute is cleared once and the retried save is
        /// denied again (issue #959, L2). Expected: the original access exception is rethrown
        /// after exactly one clear, two save attempts and one prompt; the YesToAll answer stays
        /// held. The directory-creation tripwire ends an unbounded retry deterministically.
        /// </summary>
        [TestMethod]
        public async Task TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear()
        {
            // Arrange
            var seams = new Seams(YesNoToAllResponse.YesToAll) { CreateDirectoryLimit = 3 };
            var denied = new UnauthorizedAccessException("denied");
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            attachment.Setup(x => x.SaveAsFile(SandboxFilePath)).Throws(denied);

            // Act
            Func<Task> act = () => SaveAsync(attachment, seams);

            // Assert
            (await act.Should().ThrowAsync<UnauthorizedAccessException>())
                .Which.Should()
                .BeSameAs(denied);
            seams.CreatedDirectories.Should().Equal(SandboxDirectory, SandboxDirectory);
            seams.ClearedDirectories.Should().Equal(SandboxDirectory);
            seams.PromptMessages.Should().Equal(ExpectedPrompt);
            seams.Session.Response.Should().Be(YesNoToAllResponse.YesToAll);
            attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Exactly(2));
        }

        /// <summary>
        /// Calls the five-argument overload under test with the sandbox file path and the seams
        /// of the given recorder.
        /// </summary>
        private static Task<bool> SaveAsync(Mock<Attachment> attachment, Seams seams)
        {
            return attachment.Object.TrySaveAttachmentAsync(
                SandboxFilePath,
                seams.CreateDirectory,
                seams.ClearReadOnly,
                seams.Session
            );
        }

        /// <summary>
        /// Records every seam call made by the method under test and answers each prompt with the
        /// next scripted response. Each test creates its own instance, so no state is shared
        /// between tests; an unscripted prompt fails the test through the empty queue.
        /// </summary>
        private sealed class Seams
        {
            private readonly Queue<YesNoToAllResponse> _answers;

            public Seams(params YesNoToAllResponse[] answers)
            {
                _answers = new Queue<YesNoToAllResponse>(answers);
                Session = new YesNoToAllPromptSession(Prompt);
            }

            public YesNoToAllPromptSession Session { get; }
            public List<string> CreatedDirectories { get; } = new List<string>();
            public List<string> ClearedDirectories { get; } = new List<string>();
            public List<string> PromptMessages { get; } = new List<string>();
            public System.Exception ClearException { get; set; }

            // Tripwire for an unbounded retry: the directory-creation seam runs once per save
            // attempt, so a limit on recorded calls ends a loop deterministically without a timer.
            public int CreateDirectoryLimit { get; set; } = int.MaxValue;

            public void CreateDirectory(string path)
            {
                if (CreatedDirectories.Count >= CreateDirectoryLimit)
                {
                    throw new InvalidOperationException("retry bound exceeded");
                }
                CreatedDirectories.Add(path);
            }

            public void ClearReadOnly(string path)
            {
                ClearedDirectories.Add(path);
                if (ClearException is not null)
                {
                    throw ClearException;
                }
            }

            private YesNoToAllResponse Prompt(string message)
            {
                PromptMessages.Add(message);
                return _answers.Dequeue();
            }
        }
    }
}
