#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.OutlookExtensions;

namespace UtilitiesCS
{
    public static partial class SortEmail
    {
        [ExcludeFromCodeCoverage]
        public static async Task SortAsync(
            bool savePictures,
            string destinationFolderpath,
            bool saveMsg,
            bool saveAttachments,
            bool removeFlowFile,
            IApplicationGlobals appGlobals
        )
        {
            var mailItems = appGlobals
                .Ol.App.ActiveExplorer()
                .Selection.Cast<object>()
                .Where(x => x is MailItem)
                .Select(x => (MailItem)x)
                .ToList();
            if (mailItems.Count == 0)
            {
                MessageBox.Show("No mail items are selected.");
            }
            else
            {
                await SortAsync(
                    mailItems,
                    savePictures,
                    destinationFolderpath,
                    saveMsg,
                    saveAttachments,
                    removeFlowFile,
                    appGlobals
                );
            }
        }

        [ExcludeFromCodeCoverage]
        public static async Task SortAsync(
            IList<MailItem> mailItems,
            bool savePictures,
            string destinationFolderpath,
            bool saveMsg,
            bool saveAttachments,
            bool removeFlowFile,
            IApplicationGlobals appGlobals
        )
        {
            if (mailItems is null || mailItems.Count == 0)
            {
                throw new ArgumentNullException($"{mailItems} is null or empty");
            }
            var olAncestor = FolderConverter.ResolveOlRoot(
                ((Folder)mailItems[0].Parent).FolderPath,
                appGlobals
            );
            if (appGlobals.FS.SpecialFolders.TryGetValue("OneDrive", out var folderRoot))
            {
                var fsAncestorEquivalent = folderRoot;
                await SortAsync(
                    mailItems,
                    savePictures,
                    destinationFolderpath,
                    saveMsg,
                    saveAttachments,
                    removeFlowFile,
                    appGlobals,
                    olAncestor,
                    fsAncestorEquivalent
                );
            }
        }

        [ExcludeFromCodeCoverage]
        public static async Task SortAsync(
            IList<MailItem> mailItems,
            bool savePictures,
            string destinationOlStem,
            bool saveMsg,
            bool saveAttachments,
            bool removePreviousFsFiles,
            IApplicationGlobals appGlobals,
            string olAncestor,
            string fsAncestorEquivalent
        )
        {
            //TraceUtility.LogMethodCall(mailItems, savePictures, destinationOlStem, saveMsg,
            //    saveAttachments, removePreviousFsFiles, appGlobals, olAncestor, fsAncestorEquivalent);

            if (mailItems is null || mailItems.Count == 0)
            {
                throw new ArgumentNullException($"{mailItems} is null or empty");
            }

            var conversationID = mailItems.FirstOrDefault().ConversationID;

            ResolvePaths(
                mailItems,
                destinationOlStem,
                appGlobals,
                olAncestor,
                fsAncestorEquivalent,
                out string destinationOlPath,
                out string saveFsPath,
                out string? deleteFsPath
            );

            foreach (var mailItem in mailItems)
            {
                // If saveMsg is true, save the message as an .msg file
                if (saveMsg)
                {
                    await SaveMessageAsMsgAsync(mailItem, saveFsPath);
                }

                if (saveAttachments || savePictures)
                {
                    // Get attachments to save and necessary info
                    var attachments = GetAttachmentsInfoAsync(
                        mailItem,
                        saveFsPath,
                        deleteFsPath,
                        saveAttachments,
                        savePictures
                    );
                    // Save to the file system
                    //await foreach (var attachment in attachments) { await attachment.SaveAttachmentAsync(); }
                    // ForEachAsync is obsolete (CS0618); see the rationale in EmailFiler's
                    // ProcessMailHelperAsync-adjacent fix. Suppressing narrowly preserves the
                    // exact pre-existing behavior (no behavior change per AC7).
#pragma warning disable CS0618
                    await attachments.ForEachAsync(async x => await x.SaveAttachmentAsync());
#pragma warning restore CS0618
                    //attachments.ForEach(x => x.SaveAttachment());

                    // Delete the original attachments if removePreviousFsFiles is true
                    var toDelete = attachments.Where(x => !x.FilePathDelete.IsNullOrEmpty());
                    await foreach (var attachment in toDelete)
                    {
                        await Task.Run(() => File.Delete(attachment.FilePathDelete!));
                    }
                }

                // Label the email as autosorted
                await Task.Run(() => mailItem.SetUdf("AutoSorted", "Yes"));
                mailItem.UnRead = false;
                await Task.Run(() => mailItem.Save());

                // Update Subject Map and Subject Encoder
                appGlobals.AF.SubjectMap.Add(mailItem.Subject, destinationOlStem);

                // Move the email to the destination folder
                Folder? olDestination = null;
                try
                {
                    var folderHandler = new FolderPredictor(appGlobals);
                    olDestination = folderHandler.GetFolder(destinationOlPath, appGlobals.Ol.App);
                }
                catch (System.Exception e)
                {
                    logger.Error($"Error getting folder {destinationOlPath}", e);
                    // Hacky solve to determine at debug time if I want to continue or not
                    var stop = true;
                    if (stop)
                    {
                        throw e;
                    }
                }
                if (olDestination is null)
                {
                    //logger.Debug($"Folder with path {destinationOlPath} could not be resolved");
                }

                MailItem? mailItemTemp = null;

                try
                {
                    if (olDestination is not null)
                    {
                        mailItemTemp = await Task.Run(() => (MailItem)mailItem.Move(olDestination));
                    }
                    else
                    {
                        //logger.Debug($"Folder with path {destinationOlPath} could not be resolved so the mail cannot be moved");
                    }
                }
                catch (System.Exception e)
                {
                    // Hacky solve to determine at debug time if I want to continue or not
                    var stop = true;
                    if (stop)
                    {
                        throw e;
                    }
                }

                // Add the email to the Undo Stack
                PushToUndoStack(mailItem, mailItemTemp!, appGlobals);

                // Capture the move details in the log
                await Task.Run(() => CaptureMoveDetails(mailItem, mailItemTemp!, appGlobals))
                    .ConfigureAwait(false);
            }

            // Update the Recents list and save
            appGlobals.AF.RecentsList.AddOrMoveFirst(destinationOlStem, 5);
            appGlobals.AF.RecentsList.Serialize();

            // Update the CtfMap and save
            appGlobals.AF.CtfMap.Add(destinationOlStem, conversationID, mailItems.Count);

            // Serialize the data
            var tasks = new List<Task>
            {
                //appGlobals.AF.RecentsList.SerializeAsync(),
                appGlobals.AF.CtfMap.SerializeAsync(),
                appGlobals.AF.SubjectMap.SerializeAsync(),
                appGlobals.AF.MovedMails.SerializeAsync(),
            };

            await Task.WhenAll(tasks).ConfigureAwait(false);

            appGlobals.AF.Encoder.Encoder.Serialize();
        }

        [ExcludeFromCodeCoverage]
        public static void Sort(
            IList<MailItem> mailItems,
            bool savePictures,
            string destinationOlStem,
            bool saveMsg,
            bool saveAttachments,
            bool removePreviousFsFiles,
            IApplicationGlobals appGlobals,
            string olAncestor,
            string fsAncestorEquivalent
        )
        {
            if (mailItems is null || mailItems.Count == 0)
            {
                throw new ArgumentNullException($"{mailItems} is null or empty");
            }

            var conversationID = mailItems[0].ConversationID;

            ResolvePaths(
                mailItems,
                destinationOlStem,
                appGlobals,
                olAncestor,
                fsAncestorEquivalent,
                out string destinationOlPath,
                out string saveFsPath,
                out string? deleteFsPath
            );

            foreach (var mailItem in mailItems)
            {
                // If saveMsg is true, save the message as an .msg file
                if (saveMsg)
                {
                    SaveMessageAsMSG(mailItem, saveFsPath);
                }

                if (saveAttachments || savePictures)
                {
                    // Get attachments to save and necessary info
                    var attachments = GetAttachmentsInfo(
                        mailItem,
                        saveFsPath,
                        deleteFsPath,
                        saveAttachments,
                        savePictures
                    );
                    // Save to the file system
                    foreach (var attachment in attachments)
                    {
                        attachment.SaveAttachment();
                    }
                    //attachments.ForEach(x => x.SaveAttachment());

                    // Delete the original attachments if removePreviousFsFiles is true
                    var toDelete = attachments.Where(x => !x.FilePathDelete.IsNullOrEmpty());
                    foreach (var attachment in toDelete)
                    {
                        File.Delete(attachment.FilePathDelete);
                    }
                }

                // Label the email as autosorted
                mailItem.SetUdf("AutoSorted", "Yes");
                mailItem.UnRead = false;
                mailItem.Save();

                // Update Subject Map and Subject Encoder
                appGlobals.AF.SubjectMap.Add(mailItem.Subject, destinationOlStem);

                // Move the email to the destination folder
                var folderHandler = new FolderPredictor(appGlobals);
                var olDestination = folderHandler.GetFolder(destinationOlPath, appGlobals.Ol.App);
                var mailItemTemp = (MailItem)mailItem.Move(olDestination);

                // Add the email to the Undo Stack
                PushToUndoStack(mailItem, mailItemTemp, appGlobals);

                // Capture the move details in the log
                CaptureMoveDetails(mailItem, mailItemTemp, appGlobals);
            }

            // Update the Recents list and save
            appGlobals.AF.RecentsList.AddOrMoveFirst(destinationOlStem, 5);

            // Update the CtfMap and save
            appGlobals.AF.CtfMap.Add(destinationOlStem, conversationID, mailItems.Count);

            // Serialize the data

            appGlobals.AF.RecentsList.Serialize();
            appGlobals.AF.CtfMap.Serialize();
            appGlobals.AF.SubjectMap.Serialize();
            appGlobals.AF.MovedMails.Serialize();

            appGlobals.AF.Encoder.Encoder.Serialize();
        }

        //private static (string saveFsPath, string deleteFsPath) ResolvePaths(
        //    IList<MailItem> mailItems,
        //    string destinationOlPath,
        //    IApplicationGlobals appGlobals,
        //    string olAncestor,
        //    string fsAncestorEquivalent)
        [ExcludeFromCodeCoverage]
        private static void ResolvePaths(
            IList<MailItem> mailItems,
            string destinationOlStem,
            IApplicationGlobals appGlobals,
            string olAncestor,
            string fsAncestorEquivalent,
            out string destinationOlPath,
            out string saveFsPath,
            out string? deleteFsPath
        )
        {
            //TraceUtility.LogMethodCall(mailItems, destinationOlStem, appGlobals, olAncestor, fsAncestorEquivalent);

            destinationOlPath = $"{olAncestor}\\{destinationOlStem}";

            // Resolve the file system destination folder path
            saveFsPath = destinationOlPath.ToFsFolderpath(olAncestor, fsAncestorEquivalent);

            // Resolve the file system deletion folder path if relevant
            deleteFsPath = null;
            var currentFolder = (Folder)mailItems[0].Parent;
            if (
                (currentFolder.FolderPath != appGlobals.Ol.InboxPath)
                && (currentFolder.FolderPath.Contains(olAncestor))
                && (currentFolder.FolderPath != olAncestor)
            )
            {
                deleteFsPath = ((Folder)mailItems[0].Parent).ToFsFolderpath(
                    olAncestor,
                    fsAncestorEquivalent
                );
            }
        }
    }
}
