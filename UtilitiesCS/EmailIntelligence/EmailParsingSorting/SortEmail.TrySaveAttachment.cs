#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Deedle;
using Microsoft.Office.Interop.Outlook;
using SDILReader;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;
using Outlook = Microsoft.Office.Interop.Outlook;

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
        /// directory before the save is retried.
        /// </summary>
        /// <returns>
        /// True when the attachment was saved; false when the answer declined the change or the
        /// attribute could not be cleared. A cancelled prompt rethrows the original exception.
        /// </returns>
        internal static async Task<bool> TrySaveAttachmentAsync(
            this Attachment attachment,
            string filePathSave,
            Action<string> createDirectory,
            Action<string> clearReadOnly,
            YesNoToAllPromptSession removeReadOnlyPrompt
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
                Debug.WriteLine(e.Message);

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
                        Debug.WriteLine(inner.Message);
                        return false;
                    }
                    finally
                    {
                        removeReadOnlyPrompt.ReleaseSingleAnswer();
                    }
                    return await TrySaveAttachmentAsync(
                        attachment,
                        filePathSave,
                        createDirectory,
                        clearReadOnly,
                        removeReadOnlyPrompt
                    );
                }
                else if (
                    (removeReadOnlyPrompt.Response == YesNoToAllResponse.No)
                    || (removeReadOnlyPrompt.Response == YesNoToAllResponse.NoToAll)
                )
                {
                    Debug.WriteLine($"The file {filePathSave} was not saved.");
                    removeReadOnlyPrompt.ReleaseSingleAnswer();
                    return false;
                }
                else
                {
                    throw;
                }
            }
            catch (System.Exception)
            {
                throw;
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
