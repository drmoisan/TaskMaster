#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.EmailIntelligence;

namespace UtilitiesCS
{
    public static partial class SortEmail
    {
        // Production answer state of the three attachment prompts. Tests pass their own sessions
        // to the seamed cores below, so no test reads or writes these instances. Cleanup_Files
        // resets them after each filing operation.
        private static readonly YesNoToAllPromptSession AttachmentsOverwritePrompt = new(
            YesNoToAll.ShowDialog
        );
        private static readonly YesNoToAllPromptSession PicturesOverwritePrompt = new(
            YesNoToAll.ShowDialog
        );
        private static readonly YesNoToAllPromptSession AttachmentsAltNamePrompt = new(
            YesNoToAll.ShowDialog
        );

        // A property, not an array field: a static field initializer in this partial could run
        // before the RemoveReadOnlyPrompt initializer in SortEmail.TrySaveAttachment.cs and
        // capture null, because the initialization order across partial files is unspecified.
        private static YesNoToAllPromptSession[] AllPromptSessions =>
            new[]
            {
                AttachmentsOverwritePrompt,
                PicturesOverwritePrompt,
                AttachmentsAltNamePrompt,
                RemoveReadOnlyPrompt,
            };

        public static void Cleanup_Files()
        {
            foreach (var prompt in AllPromptSessions)
            {
                prompt.Reset();
            }
        }

        internal static IEnumerable<AttachmentHelper> GetAttachmentsInfo(
            MailItem mailItem,
            string saveFsPath,
            string? deleteFsPath,
            bool saveAttachments,
            bool savePictures
        )
        {
            var attachments = mailItem
                .Attachments.Cast<Attachment>()
                .Where(x => x.Type != OlAttachmentType.olOLE)
                .Select(x => new AttachmentHelper(x, mailItem.SentOn, saveFsPath, deleteFsPath!));
            if (!saveAttachments)
            {
                attachments = attachments.Where(x => x.AttachmentInfo.IsImage);
            }

            if (!savePictures)
            {
                attachments = attachments.Where(x => !x.AttachmentInfo.IsImage);
            }
            return attachments;
        }

        internal static IAsyncEnumerable<AttachmentHelper> GetAttachmentsInfoAsync(
            MailItem mailItem,
            string saveFsPath,
            string? deleteFsPath,
            bool saveAttachments,
            bool savePictures
        )
        {
            //TraceUtility.LogMethodCall(mailItem, saveFsPath, deleteFsPath, saveAttachments, savePictures);
            // SelectAwait is obsolete (CS0618) per the framework's migration guidance ("Use
            // Select ... overloads of Select"), but the replacement overload requires adding a
            // CancellationToken parameter to the lambda. Suppressing narrowly preserves the
            // exact pre-existing behavior (no behavior change per AC7).
#pragma warning disable CS0618
            var attachments = mailItem
                .Attachments.Cast<Attachment>()
                .Where(x => x.Type != OlAttachmentType.olOLE)
                .ToAsyncEnumerable()
                .SelectAwait(async x =>
                    await AttachmentHelper.CreateAsync(
                        x,
                        mailItem.SentOn,
                        saveFsPath,
                        deleteFsPath!
                    )
                );
#pragma warning restore CS0618
            if (!saveAttachments)
            {
                attachments = attachments.Where(x => x.AttachmentInfo.IsImage);
            }

            if (!savePictures)
            {
                attachments = attachments.Where(x => !x.AttachmentInfo.IsImage);
            }
            return attachments;
        }

        // Excluded from coverage: wiring only. Calling it from a test would run the real
        // file-existence check and show the production dialogs (UT4).
        [ExcludeFromCodeCoverage]
        public static void SaveAttachment(this AttachmentHelper attachmentHelper)
        {
            SaveAttachment(
                attachmentHelper,
                File.Exists,
                PicturesOverwritePrompt,
                AttachmentsOverwritePrompt
            );
        }

        /// <summary>
        /// Saves the attachment synchronously through injected seams. When the primary save path
        /// exists, the overwrite prompt that matches the attachment kind is asked (the pictures
        /// session for an image, the attachments session otherwise), the answer is applied by the
        /// synchronous save switch, and a single answer is released afterwards.
        /// </summary>
        internal static void SaveAttachment(
            AttachmentHelper attachmentHelper,
            Func<string, bool> fileExists,
            YesNoToAllPromptSession picturesOverwritePrompt,
            YesNoToAllPromptSession attachmentsOverwritePrompt
        )
        {
            if (!fileExists(attachmentHelper.FilePathSave))
            {
                attachmentHelper.Attachment.SaveAsFile(attachmentHelper.FilePathSave);
                return;
            }

            var overwritePrompt = attachmentHelper.AttachmentInfo.IsImage
                ? picturesOverwritePrompt
                : attachmentsOverwritePrompt;
            var answer = overwritePrompt.Ask(
                $"The file {attachmentHelper.FilePathSave} already exists. Overwrite?"
            );
            SaveCase(
                answer,
                attachmentHelper.Attachment,
                attachmentHelper.FilePathSave,
                attachmentHelper.FilePathSaveAlt
            );
            overwritePrompt.ReleaseSingleAnswer();
        }

        // Excluded from coverage: wiring only. Calling it from a test would run the real
        // file-existence check, show the production dialogs and create a real directory through
        // the two-argument try-save overload (UT4).
        [ExcludeFromCodeCoverage]
        public static Task SaveAttachmentAsync(this AttachmentHelper attachmentHelper)
        {
            return SaveAttachmentAsync(
                attachmentHelper,
                File.Exists,
                PicturesOverwritePrompt,
                AttachmentsOverwritePrompt,
                AttachmentsAltNamePrompt,
                TrySaveAttachmentAsync
            );
        }

        /// <summary>
        /// Saves the attachment through injected seams. When the primary save path exists, the
        /// overwrite prompt that matches the attachment kind is asked, the answer is applied by
        /// the asynchronous save switch with the alternate-name session, and a single answer is
        /// released afterwards. The result of <paramref name="trySave"/> is not inspected: a
        /// persistent failure surfaces by exception from the try-save path.
        /// </summary>
        internal static async Task SaveAttachmentAsync(
            AttachmentHelper attachmentHelper,
            Func<string, bool> fileExists,
            YesNoToAllPromptSession picturesOverwritePrompt,
            YesNoToAllPromptSession attachmentsOverwritePrompt,
            YesNoToAllPromptSession altNamePrompt,
            Func<Attachment, string, Task<bool>> trySave
        )
        {
            if (!fileExists(attachmentHelper.FilePathSave))
            {
                await trySave(attachmentHelper.Attachment, attachmentHelper.FilePathSave);
                return;
            }

            var overwritePrompt = attachmentHelper.AttachmentInfo.IsImage
                ? picturesOverwritePrompt
                : attachmentsOverwritePrompt;
            var answer = overwritePrompt.Ask(
                $"The file {attachmentHelper.FilePathSave} already exists. Overwrite?"
            );
            await SaveCaseAsync(
                answer,
                attachmentHelper.Attachment,
                attachmentHelper.FilePathSave,
                attachmentHelper.FilePathSaveAlt,
                altNamePrompt,
                trySave
            );
            overwritePrompt.ReleaseSingleAnswer();
        }

        // Excluded from coverage: wiring only; re-roots the helper and forwards to the excluded
        // parameterless wrapper above.
        [ExcludeFromCodeCoverage]
        public static Task SaveAttachmentAsync(
            this AttachmentHelper attachmentHelper,
            string destinationPath
        )
        {
            RedirectSaveFolder(attachmentHelper, destinationPath);
            return SaveAttachmentAsync(attachmentHelper);
        }

        /// <summary>
        /// Re-roots both the primary and the alternate save path to the destination folder,
        /// keeping their file names. The helpers are built with the mail's Outlook folder name,
        /// so both paths must move; moving only the primary path left the alternate path relative
        /// to the process working directory (issue #959).
        /// </summary>
        internal static void RedirectSaveFolder(
            AttachmentHelper attachmentHelper,
            string destinationPath
        )
        {
            attachmentHelper.FolderPathSave = destinationPath;
        }

        internal static async Task SaveCaseAsync(
            YesNoToAllResponse response,
            Attachment attachment,
            string filePathSave,
            string filePathSaveAlt,
            YesNoToAllPromptSession altNamePrompt,
            Func<Attachment, string, Task<bool>> trySave
        )
        {
            switch (response)
            {
                case YesNoToAllResponse r
                    when (r == YesNoToAllResponse.NoToAll || r == YesNoToAllResponse.No):
                    var altAnswer = altNamePrompt.Ask(
                        $"The file {filePathSave} already exists. Save with an alternate name?"
                    );
                    if (
                        altAnswer == YesNoToAllResponse.Yes
                        || altAnswer == YesNoToAllResponse.YesToAll
                    )
                    {
                        await trySave(attachment, filePathSaveAlt);
                    }
                    altNamePrompt.ReleaseSingleAnswer();
                    break;

                case YesNoToAllResponse r
                    when (r == YesNoToAllResponse.YesToAll || r == YesNoToAllResponse.Yes):
                    await trySave(attachment, filePathSave);
                    break;

                default:
                    break;
            }
        }

        internal static void SaveCase(
            YesNoToAllResponse response,
            Attachment attachment,
            string filePathSave,
            string filePathSaveAlt
        )
        {
            switch (response)
            {
                case YesNoToAllResponse.NoToAll:
                case YesNoToAllResponse.No:
                    attachment.SaveAsFile(filePathSaveAlt);
                    break;
                case YesNoToAllResponse.Yes:
                case YesNoToAllResponse.YesToAll:
                    attachment.SaveAsFile(filePathSave);
                    break;
                default:
                    break;
            }
        }

        internal static async Task SaveMessageAsMsgAsync(MailItem mailItem, string fsLocation)
        {
            //TraceUtility.LogMethodCall(mailItem, fsLocation);

            var filenameSeed = FolderConverter.SanitizeFilename(mailItem.Subject);

            var strPath = AttachmentHelper.AdjustForMaxPath(fsLocation, filenameSeed, "msg", "");
            await Task.Run(() => mailItem.SaveAs(strPath, OlSaveAsType.olMSG));
        }

        internal static void SaveMessageAsMSG(MailItem mailItem, string fsLocation)
        {
            var filenameSeed = FolderConverter.SanitizeFilename(mailItem.Subject);

            var strPath = AttachmentHelper.AdjustForMaxPath(fsLocation, filenameSeed, "msg", "");
            mailItem.SaveAs(strPath, OlSaveAsType.olMSG);
        }
    }
}
