using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for the seamed attachment-saving cores of <see cref="SortEmail"/> (issue #959:
    /// the prompt sessions, the re-rooting defect and the cleanup invariant). Every test owns its
    /// prompt sessions, its recording delegates and its mocks; the rooted literal paths are
    /// never touched on disk, no dialog is shown and no production session is read or written.
    /// </summary>
    [TestClass]
    public class SortEmail_AttachmentSaving_Tests
    {
        private const string SandboxFolder = @"C:\Sortemail959Sandbox\attachments";
        private const string OriginFolder = @"C:\Sortemail959Sandbox\origin";
        private const string DestinationFolder = @"C:\Sortemail959Sandbox\destination";
        private static readonly DateTime SentOn = new DateTime(2026, 4, 3, 9, 30, 0);

        /// <summary>
        /// AS1. Scenario: the destination file does not exist. Expected: the existence check is
        /// asked for the primary path, one try-save goes to the primary path, no session is asked.
        /// </summary>
        [TestMethod]
        public async Task SaveAttachmentAsync_WhenFileDoesNotExist_SavesWithoutPrompting()
        {
            // Arrange
            var helper = CreateHelper(CreateAttachmentMock("photo.jpg"), SandboxFolder);
            var queried = new List<string>();
            var saves = new List<string>();
            var pictures = new ScriptedPrompt();
            var attachments = new ScriptedPrompt();
            var altName = new ScriptedPrompt();

            // Act
            await SortEmail.SaveAttachmentAsync(
                helper,
                Exists(false, queried),
                pictures.Session,
                attachments.Session,
                altName.Session,
                RecordingSave(saves)
            );

            // Assert
            queried.Should().Equal(helper.FilePathSave);
            saves.Should().Equal(helper.FilePathSave);
            pictures.Messages.Should().BeEmpty();
            attachments.Messages.Should().BeEmpty();
            altName.Messages.Should().BeEmpty();
        }

        /// <summary>
        /// AS2. Scenario: the file exists and the attachment is an image. Expected: only the
        /// pictures session is asked, with the overwrite text, and the Yes answer saves to the
        /// primary path.
        /// </summary>
        [TestMethod]
        public async Task SaveAttachmentAsync_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly()
        {
            // Arrange
            var helper = CreateHelper(CreateAttachmentMock("photo.jpg"), SandboxFolder);
            var saves = new List<string>();
            var pictures = new ScriptedPrompt(YesNoToAllResponse.Yes);
            var attachments = new ScriptedPrompt();
            var altName = new ScriptedPrompt();

            // Act
            await SortEmail.SaveAttachmentAsync(
                helper,
                Exists(true, new List<string>()),
                pictures.Session,
                attachments.Session,
                altName.Session,
                RecordingSave(saves)
            );

            // Assert
            pictures.Messages.Should().Equal(OverwritePrompt(helper));
            attachments.Messages.Should().BeEmpty();
            altName.Messages.Should().BeEmpty();
            saves.Should().Equal(helper.FilePathSave);
        }

        /// <summary>
        /// AS3. Scenario: the file exists and the attachment is a document. Expected: only the
        /// attachments session is asked and the Yes answer saves to the primary path.
        /// </summary>
        [TestMethod]
        public async Task SaveAttachmentAsync_WhenFileExistsAndAttachmentIsDocument_AsksAttachmentsPromptOnly()
        {
            // Arrange
            var helper = CreateHelper(CreateAttachmentMock("report.pdf"), SandboxFolder);
            var saves = new List<string>();
            var pictures = new ScriptedPrompt();
            var attachments = new ScriptedPrompt(YesNoToAllResponse.Yes);
            var altName = new ScriptedPrompt();

            // Act
            await SortEmail.SaveAttachmentAsync(
                helper,
                Exists(true, new List<string>()),
                pictures.Session,
                attachments.Session,
                altName.Session,
                RecordingSave(saves)
            );

            // Assert
            attachments.Messages.Should().Equal(OverwritePrompt(helper));
            pictures.Messages.Should().BeEmpty();
            altName.Messages.Should().BeEmpty();
            saves.Should().Equal(helper.FilePathSave);
        }

        /// <summary>
        /// AS4. Scenario: the overwrite answer is Yes. Expected: the answer is released after the
        /// save, so the session holds no answer.
        /// </summary>
        [TestMethod]
        public async Task SaveAttachmentAsync_WhenOverwriteAnswerIsYes_ReleasesAnswerAfterSave()
        {
            // Arrange
            var helper = CreateHelper(CreateAttachmentMock("report.pdf"), SandboxFolder);
            var saves = new List<string>();
            var attachments = new ScriptedPrompt(YesNoToAllResponse.Yes);

            // Act
            await SortEmail.SaveAttachmentAsync(
                helper,
                Exists(true, new List<string>()),
                new ScriptedPrompt().Session,
                attachments.Session,
                new ScriptedPrompt().Session,
                RecordingSave(saves)
            );

            // Assert
            saves.Should().Equal(helper.FilePathSave);
            attachments.Session.Response.Should().Be(YesNoToAllResponse.Empty);
        }

        /// <summary>
        /// AS5. Scenario: the overwrite answer is YesToAll and a second attachment follows.
        /// Expected: one prompt, two primary-path saves, the YesToAll answer kept.
        /// </summary>
        [TestMethod]
        public async Task SaveAttachmentAsync_WhenOverwriteAnswerIsYesToAll_KeepsAnswerAndDoesNotAskAgain()
        {
            // Arrange
            var first = CreateHelper(CreateAttachmentMock("report.pdf"), SandboxFolder);
            var second = CreateHelper(CreateAttachmentMock("notes.pdf"), SandboxFolder);
            var saves = new List<string>();
            var attachments = new ScriptedPrompt(YesNoToAllResponse.YesToAll);
            var altName = new ScriptedPrompt();

            // Act
            await SortEmail.SaveAttachmentAsync(
                first,
                Exists(true, new List<string>()),
                new ScriptedPrompt().Session,
                attachments.Session,
                altName.Session,
                RecordingSave(saves)
            );
            await SortEmail.SaveAttachmentAsync(
                second,
                Exists(true, new List<string>()),
                new ScriptedPrompt().Session,
                attachments.Session,
                altName.Session,
                RecordingSave(saves)
            );

            // Assert
            attachments.Messages.Should().ContainSingle();
            saves.Should().Equal(first.FilePathSave, second.FilePathSave);
            attachments.Session.Response.Should().Be(YesNoToAllResponse.YesToAll);
        }

        /// <summary>
        /// AS6. Scenario: the overwrite answer is No and the alternate-name answer is Yes.
        /// Expected: one try-save to the alternate path and both single answers released.
        /// </summary>
        [TestMethod]
        public async Task SaveAttachmentAsync_WhenOverwriteAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePath()
        {
            // Arrange
            var helper = CreateHelper(CreateAttachmentMock("report.pdf"), SandboxFolder);
            var saves = new List<string>();
            var attachments = new ScriptedPrompt(YesNoToAllResponse.No);
            var altName = new ScriptedPrompt(YesNoToAllResponse.Yes);

            // Act
            await SortEmail.SaveAttachmentAsync(
                helper,
                Exists(true, new List<string>()),
                new ScriptedPrompt().Session,
                attachments.Session,
                altName.Session,
                RecordingSave(saves)
            );

            // Assert
            altName.Messages.Should().Equal(AltNamePrompt(helper));
            saves.Should().Equal(helper.FilePathSaveAlt);
            attachments.Session.Response.Should().Be(YesNoToAllResponse.Empty);
            altName.Session.Response.Should().Be(YesNoToAllResponse.Empty);
        }

        /// <summary>
        /// SS1. Scenario: the synchronous core is called and the file does not exist. Expected:
        /// one direct SaveAsFile to the primary path and no prompt.
        /// </summary>
        [TestMethod]
        public void SaveAttachment_WhenFileDoesNotExist_SavesDirectly()
        {
            // Arrange
            var attachment = CreateAttachmentMock("report.pdf");
            var helper = CreateHelper(attachment, SandboxFolder);
            var pictures = new ScriptedPrompt();
            var attachments = new ScriptedPrompt();

            // Act
            SortEmail.SaveAttachment(
                helper,
                Exists(false, new List<string>()),
                pictures.Session,
                attachments.Session
            );

            // Assert
            attachment.Verify(x => x.SaveAsFile(helper.FilePathSave), Times.Once);
            pictures.Messages.Should().BeEmpty();
            attachments.Messages.Should().BeEmpty();
        }

        /// <summary>
        /// SS2. Scenario: the file exists and the answer is Yes. Expected: one SaveAsFile to the
        /// primary path and the answer released.
        /// </summary>
        [TestMethod]
        public void SaveAttachment_WhenFileExistsAndAnswerIsYes_OverwritesAndReleasesAnswer()
        {
            // Arrange
            var attachment = CreateAttachmentMock("report.pdf");
            var helper = CreateHelper(attachment, SandboxFolder);
            var attachments = new ScriptedPrompt(YesNoToAllResponse.Yes);

            // Act
            SortEmail.SaveAttachment(
                helper,
                Exists(true, new List<string>()),
                new ScriptedPrompt().Session,
                attachments.Session
            );

            // Assert
            attachment.Verify(x => x.SaveAsFile(helper.FilePathSave), Times.Once);
            attachments.Session.Response.Should().Be(YesNoToAllResponse.Empty);
        }

        /// <summary>
        /// SS3. Scenario: the file exists and the answer is NoToAll. Expected: one SaveAsFile to
        /// the alternate path through the corrected SaveCase labels (L1), none to the primary
        /// path, and the NoToAll answer kept.
        /// </summary>
        [TestMethod]
        public void SaveAttachment_WhenFileExistsAndAnswerIsNoToAll_SavesToAlternatePathAndKeepsAnswer()
        {
            // Arrange
            var attachment = CreateAttachmentMock("report.pdf");
            var helper = CreateHelper(attachment, SandboxFolder);
            var attachments = new ScriptedPrompt(YesNoToAllResponse.NoToAll);

            // Act
            SortEmail.SaveAttachment(
                helper,
                Exists(true, new List<string>()),
                new ScriptedPrompt().Session,
                attachments.Session
            );

            // Assert
            attachment.Verify(x => x.SaveAsFile(helper.FilePathSaveAlt), Times.Once);
            attachment.Verify(x => x.SaveAsFile(helper.FilePathSave), Times.Never);
            attachments.Session.Response.Should().Be(YesNoToAllResponse.NoToAll);
        }

        /// <summary>
        /// SS4. Scenario: the synchronous core is called, the file exists and the attachment is
        /// an image. Expected: only the pictures session is asked, with the overwrite text, the
        /// Yes answer gives one SaveAsFile to the primary path and none to the alternate path,
        /// and the answer is released (CR-1 of the 2026-10-06 code review: the image arm of the
        /// synchronous session selection; the asynchronous arm is pinned by AS2).
        /// </summary>
        [TestMethod]
        public void SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly()
        {
            // Arrange
            var attachment = CreateAttachmentMock("photo.jpg");
            var helper = CreateHelper(attachment, SandboxFolder);
            var pictures = new ScriptedPrompt(YesNoToAllResponse.Yes);
            var attachments = new ScriptedPrompt();

            // Act
            SortEmail.SaveAttachment(
                helper,
                Exists(true, new List<string>()),
                pictures.Session,
                attachments.Session
            );

            // Assert
            pictures.Messages.Should().Equal(OverwritePrompt(helper));
            attachments.Messages.Should().BeEmpty();
            attachment.Verify(x => x.SaveAsFile(helper.FilePathSave), Times.Once);
            attachment.Verify(x => x.SaveAsFile(helper.FilePathSaveAlt), Times.Never);
            pictures.Session.Response.Should().Be(YesNoToAllResponse.Empty);
        }

        /// <summary>
        /// RR. Scenario: a helper built under an origin folder is redirected to a destination
        /// folder. Expected: both the primary and the alternate save path move to the
        /// destination and both file names are unchanged (the re-rooting defect).
        /// </summary>
        [TestMethod]
        public void RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths()
        {
            // Arrange
            var helper = CreateHelper(CreateAttachmentMock("photo.jpg"), OriginFolder);
            var primaryName = Path.GetFileName(helper.FilePathSave);
            var alternateName = Path.GetFileName(helper.FilePathSaveAlt);

            // Act
            SortEmail.RedirectSaveFolder(helper, DestinationFolder);

            // Assert
            Path.GetDirectoryName(helper.FilePathSave).Should().Be(DestinationFolder);
            Path.GetDirectoryName(helper.FilePathSaveAlt).Should().Be(DestinationFolder);
            Path.GetFileName(helper.FilePathSave).Should().Be(primaryName);
            Path.GetFileName(helper.FilePathSaveAlt).Should().Be(alternateName);
        }

        /// <summary>
        /// CF. Scenario: the static prompt sessions of SortEmail are enumerated by reflection.
        /// Expected: every static session field is contained by reference in the reset list the
        /// cleanup iterates, the list has no duplicate and no stray entry, and there are exactly
        /// four production sessions. The test reads static state only.
        /// </summary>
        [TestMethod]
        public void Cleanup_Files_ResetsEveryPromptSession()
        {
            // Arrange
            var sessionFields = typeof(SortEmail)
                .GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(field => field.FieldType == typeof(YesNoToAllPromptSession))
                .ToList();
            var property = typeof(SortEmail).GetProperty(
                "AllPromptSessions",
                BindingFlags.NonPublic | BindingFlags.Static
            );

            // Act
            var resetTargets = property?.GetValue(null) as YesNoToAllPromptSession[];

            // Assert
            property.Should().NotBeNull();
            resetTargets.Should().NotBeNull();
            sessionFields.Should().HaveCount(4);
            resetTargets.Should().HaveCount(4);
            resetTargets.Should().OnlyHaveUniqueItems();
            foreach (var field in sessionFields)
            {
                var session = field.GetValue(null);
                resetTargets.Should().Contain(target => ReferenceEquals(target, session));
            }
        }

        private static Mock<Attachment> CreateAttachmentMock(string fileName)
        {
            var attachment = new Mock<Attachment>(MockBehavior.Loose);
            attachment.SetupGet(x => x.Type).Returns(OlAttachmentType.olByValue);
            attachment.SetupGet(x => x.BlockLevel).Returns((OlAttachmentBlockLevel)0);
            attachment.SetupGet(x => x.Class).Returns(OlObjectClass.olAttachment);
            attachment.SetupGet(x => x.DisplayName).Returns(fileName);
            attachment.SetupGet(x => x.FileName).Returns(fileName);
            attachment.SetupGet(x => x.Index).Returns(1);
            attachment.SetupGet(x => x.PathName).Returns(Path.Combine(@"C:\temp", fileName));
            attachment.SetupGet(x => x.Position).Returns(2);
            attachment.SetupGet(x => x.Size).Returns(1);
            return attachment;
        }

        private static AttachmentHelper CreateHelper(Mock<Attachment> attachment, string folder)
        {
            return new AttachmentHelper(attachment.Object, SentOn, folder);
        }

        private static string OverwritePrompt(AttachmentHelper helper)
        {
            return $"The file {helper.FilePathSave} already exists. Overwrite?";
        }

        private static string AltNamePrompt(AttachmentHelper helper)
        {
            return $"The file {helper.FilePathSave} already exists. Save with an alternate name?";
        }

        /// <summary>
        /// Returns a file-existence predicate that records the queried path and answers with the
        /// scripted value without touching any file.
        /// </summary>
        private static Func<string, bool> Exists(bool exists, List<string> queried)
        {
            return path =>
            {
                queried.Add(path);
                return exists;
            };
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
