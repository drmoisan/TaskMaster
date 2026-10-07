#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public static partial class SortEmail
    {
        // Production answer state of the read-only-removal prompt. Tests pass their own session to
        // the five-argument overload, so no test reads or writes this instance; nothing can
        // replace it. Cleanup_Files resets it after each filing operation.
        private static readonly YesNoToAllPromptSession RemoveReadOnlyPrompt = new(
            YesNoToAll.ShowDialog
        );

        // Excluded from coverage: the only behavior of this wrapper is wiring the real
        // directory-creation default, and calling it from a test would create a real directory,
        // which the unit-test policy prohibits (UT4).
        /// <summary>
        /// Saves the attachment to <paramref name="filePathSave"/> and creates the destination
        /// directory on disk. The overload that takes a directory-creation delegate is the test
        /// seam.
        /// </summary>
        [ExcludeFromCodeCoverage]
        internal static Task<bool> TrySaveAttachmentAsync(
            this Attachment attachment,
            string filePathSave
        )
        {
            return TrySaveAttachmentAsync(
                attachment,
                filePathSave,
                path => System.IO.Directory.CreateDirectory(path)
            );
        }

        /// <summary>
        /// Saves the attachment to <paramref name="filePathSave"/>. The
        /// <paramref name="createDirectory"/> delegate receives the destination directory before
        /// the attachment is saved, so a caller can replace directory creation (for example, a
        /// unit test that must not touch the file system). The read-only prompt uses the
        /// production session and the read-only attribute is cleared on disk.
        /// </summary>
        internal static Task<bool> TrySaveAttachmentAsync(
            this Attachment attachment,
            string filePathSave,
            Action<string> createDirectory
        )
        {
            return TrySaveAttachmentAsync(
                attachment,
                filePathSave,
                createDirectory,
                ClearReadOnlyAttributeOnDisk,
                RemoveReadOnlyPrompt
            );
        }

        /// <summary>
        /// Saves the attachment to <paramref name="filePathSave"/> through injected seams.
        /// <paramref name="createDirectory"/> receives the destination directory before each save
        /// attempt. When the save is denied, <paramref name="removeReadOnlyPrompt"/> supplies the
        /// answer to the read-only prompt, asking only while it holds no answer, and
        /// <paramref name="clearReadOnly"/> clears the read-only attribute of the destination
        /// directory before the save is retried. This overload forwards to the private core with
        /// the retry flag cleared; the core bounds the retry to one clear per call.
        /// </summary>
        /// <returns>
        /// True when the attachment was saved; false when the answer declined the change or the
        /// attribute could not be cleared. A cancelled prompt, and a denial that persists after
        /// the attribute was cleared under a held "to all" answer, rethrow the original exception.
        /// </returns>
        internal static Task<bool> TrySaveAttachmentAsync(
            this Attachment attachment,
            string filePathSave,
            Action<string> createDirectory,
            Action<string> clearReadOnly,
            YesNoToAllPromptSession removeReadOnlyPrompt
        )
        {
            return TrySaveAttachmentCoreAsync(
                attachment,
                filePathSave,
                createDirectory,
                clearReadOnly,
                removeReadOnlyPrompt,
                isRetryAfterClear: false
            );
        }

        /// <summary>
        /// The retrying save. The last parameter records whether this call is the retry that
        /// follows a successful attribute clear, which is what bounds the recursion.
        /// </summary>
        private static async Task<bool> TrySaveAttachmentCoreAsync(
            Attachment attachment,
            string filePathSave,
            Action<string> createDirectory,
            Action<string> clearReadOnly,
            YesNoToAllPromptSession removeReadOnlyPrompt,
            bool isRetryAfterClear
        )
        {
            try
            {
                createDirectory(Path.GetDirectoryName(filePathSave));
                await Task.Run(() => attachment.SaveAsFile(filePathSave));
                return true;
            }
            catch (System.UnauthorizedAccessException e)
            {
                logger.Warn(
                    $"Saving {filePathSave} was denied; the read-only prompt decides whether to retry.",
                    e
                );

                // The attribute was already cleared once in this call chain and a "to all" answer
                // is never asked again, so another clear-and-retry cannot change the outcome
                // (issue #959, L2): surface the denial to the caller instead of looping.
                if (
                    isRetryAfterClear
                    && removeReadOnlyPrompt.Response == YesNoToAllResponse.YesToAll
                )
                {
                    logger.Error(
                        $"The file {filePathSave} is still denied after the read-only attribute was cleared.",
                        e
                    );
                    throw;
                }

                // Exception usually is thrown when readonly folder attribute is set.
                // When the session holds no answer yet, ask whether the user wants to remove the
                // readonly attribute and retry saving.
                if (removeReadOnlyPrompt.Response == YesNoToAllResponse.Empty)
                {
                    var message =
                        $"The folder {Path.GetDirectoryName(filePathSave)} is read-only. Do you want to remove the readonly attribute?";
                    removeReadOnlyPrompt.Ask(message);
                }

                if (
                    (removeReadOnlyPrompt.Response == YesNoToAllResponse.Yes)
                    || (removeReadOnlyPrompt.Response == YesNoToAllResponse.YesToAll)
                )
                {
                    var directory = Path.GetDirectoryName(filePathSave);
                    try
                    {
                        clearReadOnly(directory);
                    }
                    catch (System.Exception inner)
                    {
                        logger.Error(
                            $"The read-only attribute of {directory} could not be cleared; {filePathSave} was not saved.",
                            inner
                        );
                        return false;
                    }
                    finally
                    {
                        removeReadOnlyPrompt.ReleaseSingleAnswer();
                    }
                    return await TrySaveAttachmentCoreAsync(
                        attachment,
                        filePathSave,
                        createDirectory,
                        clearReadOnly,
                        removeReadOnlyPrompt,
                        isRetryAfterClear: true
                    );
                }
                else if (
                    (removeReadOnlyPrompt.Response == YesNoToAllResponse.No)
                    || (removeReadOnlyPrompt.Response == YesNoToAllResponse.NoToAll)
                )
                {
                    logger.Warn(
                        $"The file {filePathSave} was not saved because the read-only change was declined."
                    );
                    removeReadOnlyPrompt.ReleaseSingleAnswer();
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        // Excluded from coverage: a file-system adapter whose execution requires the real file
        // system, which unit tests must not touch (UT4). Tests pass their own delegate to the
        // five-argument overload instead.
        [ExcludeFromCodeCoverage]
        private static void ClearReadOnlyAttributeOnDisk(string directoryPath)
        {
            var di = new DirectoryInfo(directoryPath);
            di.Attributes &= ~System.IO.FileAttributes.ReadOnly;
        }
    }
}
