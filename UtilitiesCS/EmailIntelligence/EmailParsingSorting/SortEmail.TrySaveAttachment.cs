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
        private static YesNoToAllResponse _removeReadOnly = YesNoToAllResponse.Empty;

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
        /// unit test that must not touch the file system).
        /// </summary>
        [ExcludeFromCodeCoverage]
        internal static async Task<bool> TrySaveAttachmentAsync(
            this Attachment attachment,
            string filePathSave,
            Action<string> createDirectory
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
                // Check if _removeReadOnly is empty.
                // If so, ask if the user wants to remove the readonly attribute and retry saving
                if (_removeReadOnly == YesNoToAllResponse.Empty)
                {
                    var message =
                        $"The folder {Path.GetDirectoryName(filePathSave)} is read-only. Do you want to remove the readonly attribute?";
                    _removeReadOnly = YesNoToAll.ShowDialog(message);
                }

                if (
                    (_removeReadOnly == YesNoToAllResponse.Yes)
                    || (_removeReadOnly == YesNoToAllResponse.YesToAll)
                )
                {
                    var di = new DirectoryInfo(Path.GetDirectoryName(filePathSave));
                    try
                    {
                        di.Attributes &= ~System.IO.FileAttributes.ReadOnly;
                    }
                    catch (System.Exception inner)
                    {
                        Debug.WriteLine(inner.Message);
                        return false;
                    }
                    finally
                    {
                        if (_removeReadOnly == YesNoToAllResponse.Yes)
                        {
                            _removeReadOnly = YesNoToAllResponse.Empty;
                        }
                    }
                    return await TrySaveAttachmentAsync(attachment, filePathSave, createDirectory);
                }
                else if (
                    (_removeReadOnly == YesNoToAllResponse.No)
                    || (_removeReadOnly == YesNoToAllResponse.NoToAll)
                )
                {
                    Debug.WriteLine($"The file {filePathSave} was not saved.");
                    if (_removeReadOnly == YesNoToAllResponse.No)
                    {
                        _removeReadOnly = YesNoToAllResponse.Empty;
                    }
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
    }
}
