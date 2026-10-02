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
        private static YesNoToAllResponse _responseSaveFile = YesNoToAllResponse.Empty;
        private static YesNoToAllResponse _attachmentsOverwrite = YesNoToAllResponse.Empty;
        private static YesNoToAllResponse _attachmentsAltName = YesNoToAllResponse.Empty;
        private static YesNoToAllResponse _picturesOverwrite = YesNoToAllResponse.Empty;

        public static void Cleanup_Files()
        {
            _responseSaveFile = YesNoToAllResponse.Empty;
            _attachmentsOverwrite = YesNoToAllResponse.Empty;
            _picturesOverwrite = YesNoToAllResponse.Empty;
            RemoveReadOnlyPrompt.Reset();
        }

        [ExcludeFromCodeCoverage]
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

        [ExcludeFromCodeCoverage]
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

        [ExcludeFromCodeCoverage]
        public static void SaveAttachment(this AttachmentHelper attachmentHelper)
        {
            if (File.Exists(attachmentHelper.FilePathSave))
            {
                if (attachmentHelper.AttachmentInfo.IsImage)
                {
                    if (_picturesOverwrite == YesNoToAllResponse.Empty)
                    {
                        _picturesOverwrite = YesNoToAll.ShowDialog(
                            $"The file {attachmentHelper.FilePathSave} already exists. Overwrite?"
                        );
                    }
                    SaveCase(
                        _picturesOverwrite,
                        attachmentHelper.Attachment,
                        attachmentHelper.FilePathSave,
                        attachmentHelper.FilePathSaveAlt
                    );

                    if (
                        _picturesOverwrite == YesNoToAllResponse.Yes
                        || _picturesOverwrite == YesNoToAllResponse.No
                    )
                    {
                        _picturesOverwrite = YesNoToAllResponse.Empty;
                    }
                }
                else
                {
                    if (_attachmentsOverwrite == YesNoToAllResponse.Empty)
                    {
                        _attachmentsOverwrite = YesNoToAll.ShowDialog(
                            $"The file {attachmentHelper.FilePathSave} already exists. Overwrite?"
                        );
                    }
                    SaveCase(
                        _attachmentsOverwrite,
                        attachmentHelper.Attachment,
                        attachmentHelper.FilePathSave,
                        attachmentHelper.FilePathSaveAlt
                    );

                    // Reset response about overwriting attachments when it is not "ToAll"
                    if (
                        _attachmentsOverwrite == YesNoToAllResponse.Yes
                        || _attachmentsOverwrite == YesNoToAllResponse.No
                    )
                    {
                        _attachmentsOverwrite = YesNoToAllResponse.Empty;
                    }
                }
            }
            else
            {
                //attachmentInfo.Attachment.SaveAsFile(attachmentInfo.FolderPathSave);
                attachmentHelper.Attachment.SaveAsFile(attachmentHelper.FilePathSave);
                //await Task.Run(() => attachmentInfo.Attachment.SaveAsFile(attachmentInfo.FilePathSave));
            }
        }

        [ExcludeFromCodeCoverage]
        public static async Task SaveAttachmentAsync(this AttachmentHelper attachmentHelper)
        {
            //TraceUtility.LogMethodCall(attachmentHelper);

            if (File.Exists(attachmentHelper.FilePathSave))
            {
                if (attachmentHelper.AttachmentInfo.IsImage)
                {
                    if (_picturesOverwrite == YesNoToAllResponse.Empty)
                    {
                        _picturesOverwrite = YesNoToAll.ShowDialog(
                            $"The file {attachmentHelper.FilePathSave} already exists. Overwrite?"
                        );
                    }
                    await SaveCaseAsync(
                        _picturesOverwrite,
                        attachmentHelper.Attachment,
                        attachmentHelper.FilePathSave,
                        attachmentHelper.FilePathSaveAlt
                    );

                    if (
                        _picturesOverwrite == YesNoToAllResponse.Yes
                        || _picturesOverwrite == YesNoToAllResponse.No
                    )
                    {
                        _picturesOverwrite = YesNoToAllResponse.Empty;
                    }
                }
                else
                {
                    if (_attachmentsOverwrite == YesNoToAllResponse.Empty)
                    {
                        _attachmentsOverwrite = YesNoToAll.ShowDialog(
                            $"The file {attachmentHelper.FilePathSave} already exists. Overwrite?"
                        );
                    }

                    await SaveCaseAsync(
                        _attachmentsOverwrite,
                        attachmentHelper.Attachment,
                        attachmentHelper.FilePathSave,
                        attachmentHelper.FilePathSaveAlt
                    );
                    if (
                        _attachmentsOverwrite == YesNoToAllResponse.Yes
                        || _attachmentsOverwrite == YesNoToAllResponse.No
                    )
                    {
                        _attachmentsOverwrite = YesNoToAllResponse.Empty;
                    }
                }
            }
            else
            {
                //await Task.Run(() => attachmentInfo.Attachment.SaveAsFile(attachmentInfo.FolderPathSave));
                await attachmentHelper.Attachment.TrySaveAttachmentAsync(
                    attachmentHelper.FilePathSave
                );
            }
        }

        [ExcludeFromCodeCoverage]
        public static async Task SaveAttachmentAsync(
            this AttachmentHelper attachmentHelper,
            string destinationPath
        )
        {
            //TraceUtility.LogMethodCall(attachmentHelper);
            //logger.Debug($"Original Destination Path {attachmentHelper.FolderPathSave}");
            attachmentHelper.FolderPathSave = destinationPath;
            //logger.Debug($"New Destination Path {attachmentHelper.FolderPathSave}");

            await SaveAttachmentAsync(attachmentHelper);
        }

        [ExcludeFromCodeCoverage]
        internal static async Task SaveCaseAsync(
            YesNoToAllResponse response,
            Attachment attachment,
            string filePathSave,
            string filePathSaveAlt
        )
        {
            switch (response)
            {
                case YesNoToAllResponse r
                    when (r == YesNoToAllResponse.NoToAll || r == YesNoToAllResponse.No):
                    if (_attachmentsAltName == YesNoToAllResponse.Empty)
                    {
                        _attachmentsAltName = YesNoToAll.ShowDialog(
                            $"The file {filePathSave} already exists. Save with an alternate name?"
                        );
                        //await UIThreadExtensions.UiDispatcher.InvokeAsync(()=>_attachmentsAltName = YesNoToAll.ShowDialog($"The file {filePathSave} already exists. Save with an alternate name?"));
                    }

                    if (
                        _attachmentsAltName == YesNoToAllResponse.Yes
                        || _attachmentsAltName == YesNoToAllResponse.YesToAll
                    )
                    {
                        await attachment.TrySaveAttachmentAsync(filePathSaveAlt);
                    }

                    // Reset the Alt name response if it is not set "ToAll"
                    if (
                        _attachmentsAltName == YesNoToAllResponse.Yes
                        || _attachmentsAltName == YesNoToAllResponse.No
                    )
                    {
                        _attachmentsAltName = YesNoToAllResponse.Empty;
                    }
                    break;

                case YesNoToAllResponse r
                    when (r == YesNoToAllResponse.YesToAll || r == YesNoToAllResponse.Yes):
                    await attachment.TrySaveAttachmentAsync(filePathSave);
                    break;

                default:
                    await Task.CompletedTask;
                    break;
            }
        }

        [ExcludeFromCodeCoverage]
        internal static void SaveCase(
            YesNoToAllResponse response,
            Attachment attachment,
            string filePathSave,
            string filePathSaveAlt
        )
        {
            switch (response)
            {
                case (YesNoToAllResponse.NoToAll | YesNoToAllResponse.No):
                    attachment.SaveAsFile(filePathSaveAlt);
                    break;
                case (YesNoToAllResponse.Yes | YesNoToAllResponse.YesToAll):
                    attachment.SaveAsFile(filePathSave);
                    break;
                default:
                    break;
            }
        }

        [ExcludeFromCodeCoverage]
        internal static bool IsPicture(this Attachment attachment)
        {
            var extension = Path.GetExtension(attachment.FileName);
            return extension == ".jpg"
                || extension == ".jpeg"
                || extension == ".png"
                || extension == ".gif"
                || extension == ".bmp";
        }

        [ExcludeFromCodeCoverage]
        internal static async Task SaveMessageAsMsgAsync(MailItem mailItem, string fsLocation)
        {
            //TraceUtility.LogMethodCall(mailItem, fsLocation);

            var filenameSeed = FolderConverter.SanitizeFilename(mailItem.Subject);

            var strPath = AttachmentHelper.AdjustForMaxPath(fsLocation, filenameSeed, "msg", "");
            await Task.Run(() => mailItem.SaveAs(strPath, OlSaveAsType.olMSG));
        }

        [ExcludeFromCodeCoverage]
        internal static void SaveMessageAsMSG(MailItem mailItem, string fsLocation)
        {
            var filenameSeed = FolderConverter.SanitizeFilename(mailItem.Subject);

            var strPath = AttachmentHelper.AdjustForMaxPath(fsLocation, filenameSeed, "msg", "");
            mailItem.SaveAs(strPath, OlSaveAsType.olMSG);
        }
    }
}
