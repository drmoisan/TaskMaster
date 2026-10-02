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
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        public static void InitializeSortToExisting(
            string InitType = "Sort",
            bool QuickLoad = false,
            bool WholeConversation = true,
            string strSeed = "",
            object? objItem = null
        )
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public static async Task SortAsync(
            IList<MailItemHelper> mailHelpers,
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
            //TraceUtility.LogMethodCall(mailHelpers, savePictures, destinationOlStem, saveMsg,
            //    saveAttachments, removePreviousFsFiles, appGlobals, olAncestor, fsAncestorEquivalent);

            if (mailHelpers is null || mailHelpers.Count == 0)
            {
                throw new ArgumentNullException($"{mailHelpers} is null or empty");
            }

            var conversationID = mailHelpers.FirstOrDefault().ConversationID;

            // Resolve the paths for the emails
            ResolvePaths(
                (Folder)mailHelpers.FirstOrDefault()!.FolderInfo!.OlFolder!,
                destinationOlStem,
                appGlobals,
                olAncestor,
                fsAncestorEquivalent,
                out string destinationOlPath,
                out string saveFsPath,
                out string? deleteFsPath,
                out Folder? destinationFolder
            );

            // Exit if the destination folder cannot be resolved
            if (destinationFolder is null)
            {
                //logger.Debug($"Folder with path {destinationOlPath} could not be resolved. Emails will not be moved");
                return;
            }

            // Process each email
            foreach (var mailHelper in mailHelpers)
            {
                await ProcessMailItemAsync(
                        savePictures,
                        destinationOlStem,
                        saveMsg,
                        saveAttachments,
                        appGlobals,
                        saveFsPath,
                        destinationFolder,
                        mailHelper
                    )
                    .ConfigureAwait(false);
            }

            // Update Predictive Engine
            await UpdatePredictiveEngineAsync(
                    mailHelpers,
                    destinationOlStem,
                    appGlobals,
                    conversationID
                )
                .ConfigureAwait(false);
        }

        [ExcludeFromCodeCoverage]
        public static async Task UpdatePredictiveEngineAsync(
            IList<MailItemHelper> mailHelpers,
            string destinationOlStem,
            IApplicationGlobals appGlobals,
            string conversationID
        )
        {
            // Update the Recents list and save
            appGlobals.AF.RecentsList.AddOrMoveFirst(destinationOlStem, 5);
            appGlobals.AF.RecentsList.Serialize();

            // Update the CtfMap and save
            appGlobals.AF.CtfMap.Add(destinationOlStem, conversationID, mailHelpers.Count);

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
        public static async Task ProcessMailItemAsync(
            bool savePictures,
            string destinationOlStem,
            bool saveMsg,
            bool saveAttachments,
            IApplicationGlobals appGlobals,
            string saveFsPath,
            Folder destinationFolder,
            MailItemHelper mailHelper
        )
        {
            // If saveMsg is true, save the message as an .msg file
            if (saveMsg)
            {
                await SaveMessageAsMsgAsync(mailHelper.Item, saveFsPath);
            }

            if (saveAttachments || savePictures)
            {
                var attachments = mailHelper.AttachmentsHelper.ToAsyncEnumerable();
                // ForEachAsync is obsolete (CS0618); see the rationale in EmailFiler's
                // ProcessMailHelperAsync-adjacent fix. Suppressing narrowly preserves the
                // exact pre-existing behavior (no behavior change per AC7).
#pragma warning disable CS0618
                await attachments.ForEachAsync(async x =>
                {
                    await x.SaveAttachmentAsync(saveFsPath);
                });
#pragma warning restore CS0618

                // Delete the original attachments if removePreviousFsFiles is true
                var toDelete = attachments.Where(x => !x.FilePathDelete.IsNullOrEmpty());
                await foreach (var attachment in toDelete)
                {
                    await Task.Run(() => File.Delete(attachment.FilePathDelete));
                }
            }

            // Label the email as autosorted
            await Task.Run(() =>
            {
                mailHelper.Item.SetUdf("AutoSorted", "Yes");
                mailHelper.Item.UnRead = false;
                mailHelper.Item.Save();
            });

            var bayesianTask = Task.Run(async () =>
                (await new OlFolderClassifierGroup(appGlobals).GetFolderPredictorAsync()).Train(
                    destinationOlStem,
                    mailHelper.Tokens,
                    1
                )
            );
            // Update Subject Map and Subject Encoder
            var subjectMapTask = Task.Run(() =>
                appGlobals.AF.SubjectMap.Add(mailHelper.Subject, destinationOlStem)
            );

            // Move the email to the destination folder

            MailItem? mailItemNew = null;
            MailItem mailItemOriginal = mailHelper.Item;

            try
            {
                mailItemNew = await Task.Run(() =>
                    (MailItem)mailHelper.Item.Move(destinationFolder)
                );
                mailHelper.Item = mailItemNew;
            }
            catch (System.Exception e)
            {
                logger.Error(
                    $"Error moving email {mailHelper.Subject} to {destinationFolder.FolderPath}\n{e.Message}",
                    e
                );
            }

            await bayesianTask;
            await subjectMapTask;

            // Add the email to the Undo Stack
            if (mailItemNew is not null)
            {
                PushToUndoStack(mailItemOriginal, mailItemNew, appGlobals);
                // Capture the move details in the log
                await Task.Run(() => CaptureMoveDetails(mailItemOriginal, mailItemNew, appGlobals))
                    .ConfigureAwait(false);
                //await Task.Run(() => CaptureMoveDetails(mailHelper, appGlobals)).ConfigureAwait(false);
            }
        }

        [ExcludeFromCodeCoverage]
        private static void ResolvePaths(
            Folder currentFolder,
            string destinationOlStem,
            IApplicationGlobals appGlobals,
            string olAncestor,
            string fsAncestorEquivalent,
            out string destinationOlPath,
            out string saveFsPath,
            out string? deleteFsPath,
            out Folder? destinationFolder
        )
        {
            //TraceUtility.LogMethodCall(currentFolder, destinationOlStem, appGlobals, olAncestor, fsAncestorEquivalent);

            destinationOlPath = $"{olAncestor}\\{destinationOlStem}";

            // Resolve the file system destination folder path
            saveFsPath = destinationOlPath.ToFsFolderpath(olAncestor, fsAncestorEquivalent);

            // Resolve the file system deletion folder path if relevant
            deleteFsPath = null;
            if (
                (currentFolder.FolderPath != appGlobals.Ol.InboxPath)
                && (currentFolder.FolderPath.Contains(olAncestor))
                && (currentFolder.FolderPath != olAncestor)
            )
            {
                deleteFsPath = currentFolder.ToFsFolderpath(olAncestor, fsAncestorEquivalent);
            }

            destinationFolder = null;
            try
            {
                destinationFolder = new FolderPredictor(appGlobals).GetFolder(
                    destinationOlPath,
                    appGlobals.Ol.App
                );
            }
            catch (System.Exception e)
            {
                //logger.Debug($"Cannot grab handle on Folder {destinationOlPath}. Emails will not be moved");
                logger.Error(e);
            }
        }
    }
}
