using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.EmailIntelligence;
using System.Collections.Generic;
using Deedle;
using System.Threading.Tasks;

namespace UtilitiesCS
{

    public static class SortEmail
    {
        #region Public Methods

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void InitializeSortToExisting(string InitType = "Sort", bool QuickLoad = false, bool WholeConversation = true, string strSeed = "", object objItem = null)
        {
            throw new NotImplementedException();
        }

        async public static Task SortAsync(bool savePictures,
                               string destinationFolderpath,
                               bool saveMsg,
                               bool saveAttachments,
                               bool removeFlowFile,
                               IApplicationGlobals appGlobals)
        {
            var mailItems = appGlobals.Ol.App.ActiveExplorer()
                                             .Selection
                                             .Cast<object>()
                                             .Where(x => x is MailItem)
                                             .Select(x => (MailItem)x)
                                             .ToList();
            if (mailItems.Count == 0)
            {
                MessageBox.Show("No mail items are selected.");
            }
            else { await SortAsync(mailItems, savePictures, destinationFolderpath, saveMsg, saveAttachments, removeFlowFile, appGlobals); }
        }
        
        async public static Task SortAsync(IList<MailItem> mailItems,
                               bool savePictures,
                               string destinationFolderpath,
                               bool saveMsg,
                               bool saveAttachments,
                               bool removeFlowFile,
                               IApplicationGlobals appGlobals)
        {
            if (mailItems is null || mailItems.Count == 0) { throw new ArgumentNullException($"{mailItems} is null or empty"); }
            var olAncestor = FolderConverter.ResolveOlRoot(((Folder)mailItems[0].Parent).FolderPath, appGlobals);
            var fsAncestorEquivalent = appGlobals.FS.FldrOneDrive;
            await SortAsync(mailItems, savePictures, destinationFolderpath, saveMsg, saveAttachments, removeFlowFile, appGlobals, olAncestor, fsAncestorEquivalent);
        }

        async public static Task SortAsync(IList<MailItemHelper> mailHelpers,
                                           bool savePictures,
                                           string destinationOlStem,
                                           bool saveMsg,
                                           bool saveAttachments,
                                           bool removePreviousFsFiles,
                                           IApplicationGlobals appGlobals,
                                           string olAncestor,
                                           string fsAncestorEquivalent)
        {
            TraceUtility.LogMethodCall(mailHelpers, savePictures, destinationOlStem, saveMsg,
                saveAttachments, removePreviousFsFiles, appGlobals, olAncestor, fsAncestorEquivalent);

            if (mailHelpers is null || mailHelpers.Count == 0)
            { throw new ArgumentNullException($"{mailHelpers} is null or empty"); }

            var conversationID = mailHelpers.FirstOrDefault().ConversationID;

            // Resolve the paths for the emails
            ResolvePaths((Folder)mailHelpers.FirstOrDefault().FolderInfo.OlFolder,
                destinationOlStem, appGlobals, olAncestor, fsAncestorEquivalent,
                out string destinationOlPath, out string saveFsPath, out string deleteFsPath, out Folder destinationFolder);

            // Exit if the destination folder cannot be resolved
            if (destinationFolder is null)
            {
                logger.Debug($"Folder with path {destinationOlPath} could not be resolved. Emails will not be moved");
                return;
            }

            // Process each email
            foreach (var mailHelper in mailHelpers)
            {
                await ProcessMailItemAsync(savePictures, destinationOlStem, saveMsg, saveAttachments, appGlobals, saveFsPath, destinationFolder, mailHelper).ConfigureAwait(false);
            }

            // Update Predictive Engine
            await UpdatePredictiveEngineAsync(mailHelpers, destinationOlStem, appGlobals, conversationID).ConfigureAwait(false);

        }

        public static async Task UpdatePredictiveEngineAsync(IList<MailItemHelper> mailHelpers, string destinationOlStem, IApplicationGlobals appGlobals, string conversationID)
        {
            // Update the Recents list and save
            appGlobals.AF.RecentsList.Add(destinationOlStem);

            // Update the CtfMap and save
            appGlobals.AF.CtfMap.Add(destinationOlStem, conversationID, mailHelpers.Count);

            // Serialize the data
            var tasks = new List<Task>
            {
                appGlobals.AF.RecentsList.SerializeAsync(),
                appGlobals.AF.CtfMap.SerializeAsync(),
                appGlobals.AF.SubjectMap.SerializeAsync(),
                appGlobals.AF.MovedMails.SerializeAsync()
            };

            await Task.WhenAll(tasks).ConfigureAwait(false);

            await appGlobals.AF.Encoder.Encoder.SerializeAsync();
        }

        public static async Task ProcessMailItemAsync(bool savePictures, string destinationOlStem, bool saveMsg, bool saveAttachments, IApplicationGlobals appGlobals, string saveFsPath, Folder destinationFolder, MailItemHelper mailHelper)
        {
            // If saveMsg is true, save the message as an .msg file
            if (saveMsg) { await SaveMessageAsMsgAsync(mailHelper.Item, saveFsPath); }

            if (saveAttachments || savePictures)
            {

                var attachments = mailHelper.AttachmentsHelper.ToAsyncEnumerable();
                await attachments.ForEachAsync(async x => 
                { 
                    
                    await x.SaveAttachmentAsync(saveFsPath); 
                });

                // Delete the original attachments if removePreviousFsFiles is true
                var toDelete = attachments.Where(x => !x.FilePathDelete.IsNullOrEmpty());
                await foreach (var attachment in toDelete) { await Task.Run(() => File.Delete(attachment.FilePathDelete)); }
            }

            // Label the email as autosorted
            await Task.Run(() =>
            {
                mailHelper.Item.SetUdf("AutoSorted", "Yes");
                mailHelper.Item.UnRead = false;
                mailHelper.Item.Save();
            });

            var bayesianTask = Task.Run(() => appGlobals.AF.Manager["Folder"].AddOrUpdateClassifier(destinationOlStem, mailHelper.Tokens, 1));
            // Update Subject Map and Subject Encoder
            var subjectMapTask = Task.Run(() => appGlobals.AF.SubjectMap.Add(mailHelper.Subject, destinationOlStem));

            // Move the email to the destination folder

            MailItem mailItemTemp = null;

            try
            {
                mailItemTemp = await Task.Run(() => (MailItem)mailHelper.Item.Move(destinationFolder));
            }
            catch (System.Exception e)
            {
                logger.Error($"Error moving email {mailHelper.Subject} to {destinationFolder.FolderPath}\n{e.Message}", e);
            }

            await bayesianTask;
            await subjectMapTask;
            
            // Add the email to the Undo Stack
            if (mailItemTemp is not null)
            {
                PushToUndoStack(mailHelper.Item, mailItemTemp, appGlobals);
                // Capture the move details in the log
                await Task.Run(() => CaptureMoveDetails(mailHelper.Item, mailItemTemp, appGlobals)).ConfigureAwait(false);
            }
        }

        async public static Task SortAsync(IList<MailItem> mailItems,
                                     bool savePictures,
                                     string destinationOlStem,
                                     bool saveMsg,
                                     bool saveAttachments,
                                     bool removePreviousFsFiles,
                                     IApplicationGlobals appGlobals,
                                     string olAncestor,
                                     string fsAncestorEquivalent)
        {
            TraceUtility.LogMethodCall(mailItems, savePictures, destinationOlStem, saveMsg, 
                saveAttachments, removePreviousFsFiles, appGlobals, olAncestor, fsAncestorEquivalent);

            if (mailItems is null || mailItems.Count == 0) 
            { throw new ArgumentNullException($"{mailItems} is null or empty"); }

            var conversationID = mailItems.FirstOrDefault().ConversationID;

            ResolvePaths(mailItems, destinationOlStem, appGlobals, olAncestor, fsAncestorEquivalent,
                out string destinationOlPath, out string saveFsPath, out string deleteFsPath);

            foreach (var mailItem in mailItems)
            {
                // If saveMsg is true, save the message as an .msg file
                if (saveMsg) { await SaveMessageAsMsgAsync(mailItem, saveFsPath); }

                if (saveAttachments || savePictures)
                {
                    // Get attachments to save and necessary info
                    var attachments = GetAttachmentsInfoAsync(mailItem,
                                                              saveFsPath,
                                                              deleteFsPath,
                                                              saveAttachments,
                                                              savePictures);
                    // Save to the file system
                    //await foreach (var attachment in attachments) { await attachment.SaveAttachmentAsync(); }
                    await attachments.ForEachAsync(async x => await x.SaveAttachmentAsync());
                    //attachments.ForEach(x => x.SaveAttachment());

                    // Delete the original attachments if removePreviousFsFiles is true
                    var toDelete = attachments.Where(x => !x.FilePathDelete.IsNullOrEmpty());
                    await foreach (var attachment in toDelete) { await Task.Run(() => File.Delete(attachment.FilePathDelete)); }
                }

                // Label the email as autosorted
                await Task.Run(() => mailItem.SetUdf("AutoSorted", "Yes"));
                mailItem.UnRead = false;
                await Task.Run(() => mailItem.Save());

                // Update Subject Map and Subject Encoder
                appGlobals.AF.SubjectMap.Add(mailItem.Subject, destinationOlStem);

                // Move the email to the destination folder
                Folder olDestination = null;
                try
                {
                    var folderHandler = new OlFolderHelper(appGlobals);
                    olDestination = folderHandler.GetFolder(destinationOlPath, appGlobals.Ol.App);
                }
                catch (System.Exception e)
                {
                    logger.Error($"Error getting folder {destinationOlPath}", e);
                    // Hacky solve to determine at debug time if I want to continue or not
                    var stop = true;
                    if (stop) { throw e; }
                }
                if (olDestination is null)
                {
                    logger.Debug($"Folder with path {destinationOlPath} could not be resolved");
                }
                
                MailItem mailItemTemp = null;
                try
                {
                    if (olDestination is not null)
                    {
                        mailItemTemp = await Task.Run(() => (MailItem)mailItem.Move(olDestination));
                    }
                    else 
                    { 
                        logger.Debug($"Folder with path {destinationOlPath} could not be resolved so the mail cannot be moved");
                    }
                    
                }
                catch (System.Exception e)
                {
                    // Hacky solve to determine at debug time if I want to continue or not
                    var stop = true;
                    if (stop) { throw e; }
                }
                

                // Add the email to the Undo Stack
                PushToUndoStack(mailItem, mailItemTemp, appGlobals);

                // Capture the move details in the log
                await Task.Run(()=>CaptureMoveDetails(mailItem, mailItemTemp, appGlobals)).ConfigureAwait(false);
                
            }

            // Update the Recents list and save
            appGlobals.AF.RecentsList.Add(destinationOlStem);

            // Update the CtfMap and save
            appGlobals.AF.CtfMap.Add(destinationOlStem, conversationID, mailItems.Count);

            // Serialize the data
            var tasks = new List<Task> 
            { 
                appGlobals.AF.RecentsList.SerializeAsync(),
                appGlobals.AF.CtfMap.SerializeAsync(),
                appGlobals.AF.SubjectMap.SerializeAsync(),
                appGlobals.AF.MovedMails.SerializeAsync() 
            };

            await Task.WhenAll(tasks).ConfigureAwait(false);
            
            await appGlobals.AF.Encoder.Encoder.SerializeAsync();
                        
        }

        public static void Sort(IList<MailItem> mailItems,
                                     bool savePictures,
                                     string destinationOlStem,
                                     bool saveMsg,
                                     bool saveAttachments,
                                     bool removePreviousFsFiles,
                                     IApplicationGlobals appGlobals,
                                     string olAncestor,
                                     string fsAncestorEquivalent)
        {
            if (mailItems is null || mailItems.Count == 0) { throw new ArgumentNullException($"{mailItems} is null or empty"); }

            var conversationID = mailItems[0].ConversationID;

            ResolvePaths(mailItems,destinationOlStem,appGlobals,olAncestor,fsAncestorEquivalent, 
                out string destinationOlPath,out string saveFsPath, out string deleteFsPath);

            foreach (var mailItem in mailItems)
            {
                // If saveMsg is true, save the message as an .msg file
                if (saveMsg) { SaveMessageAsMSG(mailItem, saveFsPath); }

                if (saveAttachments || savePictures)
                {
                    // Get attachments to save and necessary info
                    var attachments = GetAttachmentsInfo(mailItem,
                                                         saveFsPath,
                                                         deleteFsPath,
                                                         saveAttachments,
                                                         savePictures);
                    // Save to the file system
                    foreach (var attachment in attachments) { attachment.SaveAttachment(); }
                    //attachments.ForEach(x => x.SaveAttachment());

                    // Delete the original attachments if removePreviousFsFiles is true
                    var toDelete = attachments.Where(x => !x.FilePathDelete.IsNullOrEmpty());
                    foreach (var attachment in toDelete) { File.Delete(attachment.FilePathDelete); }
                }

                // Label the email as autosorted
                mailItem.SetUdf("AutoSorted", "Yes");
                mailItem.UnRead = false;
                mailItem.Save();

                // Update Subject Map and Subject Encoder
                appGlobals.AF.SubjectMap.Add(mailItem.Subject, destinationOlStem);

                // Move the email to the destination folder
                var folderHandler = new OlFolderHelper(appGlobals);
                var olDestination = folderHandler.GetFolder(destinationOlPath, appGlobals.Ol.App);
                var mailItemTemp = (MailItem)mailItem.Move(olDestination);

                // Add the email to the Undo Stack
                PushToUndoStack(mailItem, mailItemTemp, appGlobals);

                // Capture the move details in the log
                CaptureMoveDetails(mailItem, mailItemTemp, appGlobals);

            }

            // Update the Recents list and save
            appGlobals.AF.RecentsList.Add(destinationOlStem);

            // Update the CtfMap and save
            appGlobals.AF.CtfMap.Add(destinationOlStem, conversationID, mailItems.Count);

            // Serialize the data


            appGlobals.AF.RecentsList.Serialize();
            appGlobals.AF.CtfMap.Serialize();
            appGlobals.AF.SubjectMap.Serialize();
            appGlobals.AF.MovedMails.Serialize();
            
            appGlobals.AF.Encoder.Encoder.Serialize();

        }


        public static void Cleanup_Files()
        {
            _responseSaveFile = YesNoToAllResponse.Empty;
            _attachmentsOverwrite = YesNoToAllResponse.Empty;
            _picturesOverwrite = YesNoToAllResponse.Empty;
            _removeReadOnly = YesNoToAllResponse.Empty;
        }

        // Duplicative with QuickFiler but it is still mapped to main menu so I need to take it out
        public static async Task UndoAsync(ScoStack<IMovedMailInfo> movedStack, IApplicationGlobals globals) 
        {
            DialogResult repeatResponse = DialogResult.Yes;
            var i = 0;

            while (i < movedStack.Count && repeatResponse == DialogResult.Yes)
            {
                var message = movedStack[i].UndoMoveMessage(globals.Ol.App);
                if (message is null) { i++; }
                else
                {
                    var undoResponse = MessageBox.Show(message, "Undo Dialog", MessageBoxButtons.YesNo);
                    if (undoResponse == DialogResult.Yes)
                    {
                        var helper = await MailItemHelper.FromMailItemAsync(movedStack[i].MailItem, globals, default, true);
                        globals.AF.Manager["Folder"].UnTrain(helper.FolderInfo.RelativePath, helper.Tokens, 1);
                        movedStack[i].UndoMove();
                        movedStack.Pop(i);
                    }
                    else { i++;}
                    repeatResponse = MessageBox.Show("Continue Undoing Moves?", "Undo Dialog", MessageBoxButtons.YesNo);   
                } 
            }
            
            if (repeatResponse == DialogResult.Yes) { MessageBox.Show("Nothing to undo"); }
            movedStack.Serialize();
        }

        #endregion

        #region Private Static Variables

        private static YesNoToAllResponse _responseSaveFile = YesNoToAllResponse.Empty;
        private static YesNoToAllResponse _attachmentsOverwrite = YesNoToAllResponse.Empty;
        private static YesNoToAllResponse _attachmentsAltName = YesNoToAllResponse.Empty;
        private static YesNoToAllResponse _picturesOverwrite = YesNoToAllResponse.Empty;
        private static YesNoToAllResponse _removeReadOnly = YesNoToAllResponse.Empty;


        private const int MAX_PATH = 256;

        #endregion

        #region Helper Methods

        internal static IEnumerable<AttachmentHelper> GetAttachmentsInfo(
            MailItem mailItem,
            string saveFsPath,
            string deleteFsPath,
            bool saveAttachments,
            bool savePictures)
        {
            var attachments = mailItem.Attachments
                                      .Cast<Attachment>()
                                      .Where(x => x.Type != OlAttachmentType.olOLE)
                                      .Select(x => new AttachmentHelper(x, mailItem.SentOn, saveFsPath, deleteFsPath));
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
            string deleteFsPath,
            bool saveAttachments,
            bool savePictures)
        {
            TraceUtility.LogMethodCall(mailItem, saveFsPath, deleteFsPath, saveAttachments, savePictures);
            var attachments = mailItem.Attachments
                                      .Cast<Attachment>()
                                      .Where(x => x.Type != OlAttachmentType.olOLE)
                                      .ToAsyncEnumerable()
                                      .SelectAwait(async x => await AttachmentHelper.CreateAsync(x, mailItem.SentOn, saveFsPath, deleteFsPath));
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

        public static void SaveAttachment(this AttachmentHelper attachmentHelper)
        {
            if (File.Exists(attachmentHelper.FilePathSave))
            {
                if (attachmentHelper.AttachmentInfo.IsImage)
                {
                    if (_picturesOverwrite == YesNoToAllResponse.Empty)
                    {
                        _picturesOverwrite = YesNoToAll.ShowDialog($"The file {attachmentHelper.FilePathSave} already exists. Overwrite?");
                    }
                    SaveCase(_picturesOverwrite, attachmentHelper.Attachment, attachmentHelper.FilePathSave, attachmentHelper.FilePathSaveAlt);

                    if (_picturesOverwrite == YesNoToAllResponse.Yes || _picturesOverwrite == YesNoToAllResponse.No)
                    {
                        _picturesOverwrite = YesNoToAllResponse.Empty;
                    }
                }
                else
                {
                    if (_attachmentsOverwrite == YesNoToAllResponse.Empty)
                    {
                        _attachmentsOverwrite = YesNoToAll.ShowDialog($"The file {attachmentHelper.FilePathSave} already exists. Overwrite?");
                    }
                    SaveCase(_attachmentsOverwrite, attachmentHelper.Attachment, attachmentHelper.FilePathSave, attachmentHelper.FilePathSaveAlt);
                    
                    // Reset response about overwriting attachments when it is not "ToAll"
                    if (_attachmentsOverwrite == YesNoToAllResponse.Yes || _attachmentsOverwrite == YesNoToAllResponse.No)
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

        async public static Task SaveAttachmentAsync(this AttachmentHelper attachmentHelper)
        {
            TraceUtility.LogMethodCall(attachmentHelper);

            if (File.Exists(attachmentHelper.FilePathSave))
            {
                if (attachmentHelper.AttachmentInfo.IsImage)
                {
                    if (_picturesOverwrite == YesNoToAllResponse.Empty)
                    {
                        _picturesOverwrite = YesNoToAll.ShowDialog($"The file {attachmentHelper.FilePathSave} already exists. Overwrite?");
                    }
                    await SaveCaseAsync(_picturesOverwrite, attachmentHelper.Attachment, attachmentHelper.FilePathSave, attachmentHelper.FilePathSaveAlt);

                    if (_picturesOverwrite == YesNoToAllResponse.Yes || _picturesOverwrite == YesNoToAllResponse.No)
                    {
                        _picturesOverwrite = YesNoToAllResponse.Empty;
                    }
                }
                else
                {
                    if (_attachmentsOverwrite == YesNoToAllResponse.Empty)
                    {
                        _attachmentsOverwrite = YesNoToAll.ShowDialog($"The file {attachmentHelper.FilePathSave} already exists. Overwrite?");
                    }
                    
                    await SaveCaseAsync(_attachmentsOverwrite, attachmentHelper.Attachment, attachmentHelper.FilePathSave, attachmentHelper.FilePathSaveAlt);
                    if (_attachmentsOverwrite == YesNoToAllResponse.Yes || _attachmentsOverwrite == YesNoToAllResponse.No)
                    {
                        _attachmentsOverwrite = YesNoToAllResponse.Empty;
                    }
                }
            }
            else 
            { 
                //await Task.Run(() => attachmentInfo.Attachment.SaveAsFile(attachmentInfo.FolderPathSave));
                await attachmentHelper.Attachment.TrySaveAttachmentAsync(attachmentHelper.FilePathSave);
            }
        }

        async public static Task SaveAttachmentAsync(this AttachmentHelper attachmentHelper, string destinationPath)
        {
            TraceUtility.LogMethodCall(attachmentHelper);
            logger.Debug($"Original Destination Path {attachmentHelper.FolderPathSave}");
            attachmentHelper.FolderPathSave = destinationPath;
            logger.Debug($"New Destination Path {attachmentHelper.FolderPathSave}");


            await SaveAttachmentAsync(attachmentHelper);
        }

        async internal static Task SaveCaseAsync(
            YesNoToAllResponse response,
            Attachment attachment,
            string filePathSave,
            string filePathSaveAlt)
        {
            switch (response)
            {
                case YesNoToAllResponse r when (r == YesNoToAllResponse.NoToAll || r == YesNoToAllResponse.No):
                    if (_attachmentsAltName == YesNoToAllResponse.Empty)
                    {
                        _attachmentsAltName = YesNoToAll.ShowDialog($"The file {filePathSave} already exists. Save with an alternate name?");
                        //await UIThreadExtensions.UiDispatcher.InvokeAsync(()=>_attachmentsAltName = YesNoToAll.ShowDialog($"The file {filePathSave} already exists. Save with an alternate name?"));
                    }
                    
                    if (_attachmentsAltName == YesNoToAllResponse.Yes || _attachmentsAltName == YesNoToAllResponse.YesToAll)
                    {
                        await attachment.TrySaveAttachmentAsync(filePathSaveAlt);
                    }
                    
                    // Reset the Alt name response if it is not set "ToAll"
                    if (_attachmentsAltName == YesNoToAllResponse.Yes || _attachmentsAltName == YesNoToAllResponse.No)
                    {
                        _attachmentsAltName = YesNoToAllResponse.Empty;
                    }
                    break;

                case YesNoToAllResponse r when (r == YesNoToAllResponse.YesToAll || r == YesNoToAllResponse.Yes):
                    await attachment.TrySaveAttachmentAsync(filePathSave);
                    break;

                default:
                    await Task.CompletedTask;
                    break;
            }
        }

        async internal static Task<bool> TrySaveAttachmentAsync(
            this Attachment attachment,
            string filePathSave)
        {
            try
            {
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(filePathSave));
                await Task.Run(()=>attachment.SaveAsFile(filePathSave));
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
                    var message = $"The folder {Path.GetDirectoryName(filePathSave)} is read-only. Do you want to remove the readonly attribute?";
                    _removeReadOnly = YesNoToAll.ShowDialog(message);
                }

                if ((_removeReadOnly == YesNoToAllResponse.Yes) || (_removeReadOnly == YesNoToAllResponse.YesToAll))
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
                    return await TrySaveAttachmentAsync(attachment, filePathSave);
                }
                else if ((_removeReadOnly == YesNoToAllResponse.No) || (_removeReadOnly == YesNoToAllResponse.NoToAll))
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
        
        internal static void SaveCase(
            YesNoToAllResponse response,
            Attachment attachment,
            string filePathSave,
            string filePathSaveAlt)
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

        internal static bool IsPicture(this Attachment attachment)
        {
            var extension = Path.GetExtension(attachment.FileName);
            return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif" || extension == ".bmp";
        }

        //private static (string saveFsPath, string deleteFsPath) ResolvePaths(
        //    IList<MailItem> mailItems,
        //    string destinationOlPath,
        //    IApplicationGlobals appGlobals,
        //    string olAncestor,
        //    string fsAncestorEquivalent)
        private static void ResolvePaths(
            IList<MailItem> mailItems,
            string destinationOlStem,
            IApplicationGlobals appGlobals,
            string olAncestor,
            string fsAncestorEquivalent,
            out string destinationOlPath,
            out string saveFsPath, 
            out string deleteFsPath)
        {
            TraceUtility.LogMethodCall(mailItems, destinationOlStem, appGlobals, olAncestor, fsAncestorEquivalent);

            destinationOlPath = $"{olAncestor}\\{destinationOlStem}";

            // Resolve the file system destination folder path 
            saveFsPath = destinationOlPath.ToFsFolderpath(olAncestor, fsAncestorEquivalent);

            // Resolve the file system deletion folder path if relevant
            deleteFsPath = null;
            var currentFolder = (Folder)mailItems[0].Parent;
            if ((currentFolder.FolderPath != appGlobals.Ol.EmailRootPath)&&
                (currentFolder.FolderPath.Contains(olAncestor))&&
                (currentFolder.FolderPath != olAncestor))
            {
                deleteFsPath = ((Folder)mailItems[0].Parent).ToFsFolderpath(olAncestor, fsAncestorEquivalent);
            }

        }

        private static void ResolvePaths(
            Folder currentFolder,
            string destinationOlStem,
            IApplicationGlobals appGlobals,
            string olAncestor,
            string fsAncestorEquivalent,
            out string destinationOlPath,
            out string saveFsPath,
            out string deleteFsPath,
            out Folder destinationFolder)
        {
            TraceUtility.LogMethodCall(currentFolder, destinationOlStem, appGlobals, olAncestor, fsAncestorEquivalent);

            destinationOlPath = $"{olAncestor}\\{destinationOlStem}";
            
            // Resolve the file system destination folder path 
            saveFsPath = destinationOlPath.ToFsFolderpath(olAncestor, fsAncestorEquivalent);

            // Resolve the file system deletion folder path if relevant
            deleteFsPath = null;
            if ((currentFolder.FolderPath != appGlobals.Ol.EmailRootPath) &&
                (currentFolder.FolderPath.Contains(olAncestor)) &&
                (currentFolder.FolderPath != olAncestor))
            {
                deleteFsPath = currentFolder.ToFsFolderpath(olAncestor, fsAncestorEquivalent);
            }

            destinationFolder = null;
            try
            {
                destinationFolder = new OlFolderHelper(appGlobals).GetFolder(destinationOlPath, appGlobals.Ol.App);
            }
            catch (System.Exception e)
            {
                logger.Debug($"Cannot grab handle on Folder {destinationOlPath}. Emails will not be moved");
                logger.Error(e);
            }
            
        }

        async internal static Task SaveMessageAsMsgAsync(
            MailItem mailItem,
            string fsLocation)
        {
            TraceUtility.LogMethodCall(mailItem, fsLocation);

            var filenameSeed = FolderConverter.SanitizeFilename(mailItem.Subject);
            
            var strPath = AttachmentHelper.AdjustForMaxPath(fsLocation, filenameSeed, "msg", "");
            await Task.Run(()=>mailItem.SaveAs(strPath, OlSaveAsType.olMSG));
        }

        internal static void SaveMessageAsMSG(
            MailItem mailItem,
            string fsLocation)
        {
            var filenameSeed = FolderConverter.SanitizeFilename(mailItem.Subject);

            var strPath = AttachmentHelper.AdjustForMaxPath(fsLocation, filenameSeed, "msg", "");
            mailItem.SaveAs(strPath, OlSaveAsType.olMSG);
        }

        internal static void SaveAttachmentsOld(
            MailItem mailItem,
            string fsLocation,
            string DteString,
            string DteString2,
            bool save_images,
            bool DELFILE,
            bool Verify_Action)
        {

            #region tocollapse
            int atmtct = 0;
            bool AlreadyExists;
            string strAtmtFullName;
            bool FileExtExists;
            string[] strAtmtName = new string[2];
            string strAtmtPath;
            string strAtmtPath2;
            bool blnIsSave;
            YesNoToAllResponse response;
            #endregion
            var lCountEachItem = mailItem.Attachments.Count;
            if (lCountEachItem > 0)
            {
                foreach (Attachment attachment in mailItem.Attachments)
                {
                    #region Hide
                    atmtct = atmtct + 1;

                    AlreadyExists = false;
                    
                    // Get the full name of the current attachment.
                    if (attachment.Type != OlAttachmentType.olOLE)
                    {
                        strAtmtFullName = attachment.FileName;
                    }
                    else
                    {
                        strAtmtFullName = "NOTHING";
                    }

                    // Is there a dot in the file extension?
                    if (strAtmtFullName.Contains("."))
                    {
                        FileExtExists = true;

                        // Find the dot postion in atmtFullName.
                        int intDotPosition = strAtmtFullName.IndexOf(".");

                        // Get the name.
                        strAtmtName[0] = strAtmtFullName.Substring(0, intDotPosition - 1);

                        // Get the file extension.
                        strAtmtName[1] = strAtmtFullName.Substring(strAtmtFullName.Length - intDotPosition);
                    }

                    else
                    {
                        FileExtExists = false;
                        strAtmtName[0] = strAtmtFullName;
                        strAtmtName[1] = "NONE";
                    }


                    // Get the full saving path of the current attachment.
                    strAtmtPath = fsLocation + DteString + " " + strAtmtFullName;
                    strAtmtPath2 = fsLocation + DteString2 + " " + strAtmtFullName;

                    // /* If the length of the saving path is not larger than 260 characters.*/
                    if (strAtmtPath.Length >= MAX_PATH)
                    {
                        strAtmtPath = strAtmtPath.Substring(0, MAX_PATH - 7);
                    }
                    #endregion

                    // True: This attachment can be saved.
                    if (save_images == true | strAtmtName[1].ToUpper() != "PNG" & strAtmtName[1].ToUpper() != "JPG" & strAtmtName[1].ToUpper() != "GIF")
                    {
                        // True: Not a picture
                        if (DELFILE == true)
                        {
                            if (File.Exists(strAtmtPath) == true)
                            {
                                File.Delete(strAtmtPath);
                            }
                            else if (File.Exists(strAtmtPath2) == true)
                            {
                                File.Delete(strAtmtPath2);
                            }
                            blnIsSave = false;
                        }

                        else
                        {
                            blnIsSave = true;

                            // /* Loop until getting the file name which does not exist in the folder. */
                            while (File.Exists(strAtmtPath))
                            {
                                AlreadyExists = true;

                                var strAtmtNameTemp = strAtmtName[0] + DateTime.Now.ToString("_MMddhhmmss");
                                strAtmtPath = fsLocation + DteString + strAtmtNameTemp;
                                if (FileExtExists)
                                    strAtmtPath = strAtmtPath + "." + strAtmtName[1];

                                // /* If the length of the saving path is over 260 characters.*/
                                if (strAtmtPath.Length > MAX_PATH)
                                {
                                    lCountEachItem = lCountEachItem - 1;
                                    // False: This attachment cannot be saved.
                                    blnIsSave = false;
                                    break;
                                }
                            }
                        }

                        // /* Save the current attachment if it is a valid file name. */
                        if (blnIsSave)
                        {
                            if (Verify_Action == true)
                            {

                                    

                                if ((int)_attachmentsOverwrite + (int)_responseSaveFile == 0)
                                {
                                    mailItem.Display();
                                }


                                if (AlreadyExists == true)
                                {
                                    // Response = MsgBox("File Already Exists. Save file: " & strAtmtPath, vbCritical + vbYesNo)
                                    if (_attachmentsOverwrite == YesNoToAllResponse.Empty)
                                    {
                                        response = YesNoToAll.ShowDialog("File Already Exists. Save file: " + strAtmtPath);
                                        if (response == YesNoToAllResponse.NoToAll | response == YesNoToAllResponse.YesToAll)
                                            _attachmentsOverwrite = response;
                                    }
                                    else
                                    {
                                        response = _attachmentsOverwrite;
                                    }
                                }
                                // Response = MsgBox("Save file: " & strAtmtPath, vbYesNo + vbExclamation)
                                else if (_responseSaveFile == YesNoToAllResponse.Empty)
                                {
                                    response = YesNoToAll.ShowDialog("Save file: " + strAtmtPath);
                                    if (response == YesNoToAllResponse.NoToAll | response == YesNoToAllResponse.YesToAll)
                                        _responseSaveFile = response;
                                }
                                else
                                {
                                    response = _responseSaveFile;

                                }

                                if (response == YesNoToAllResponse.Yes | response == YesNoToAllResponse.YesToAll)
                                {
                                    strAtmtName[0] = InputBox.ShowDialog($"Email Subject: {mailItem.Subject} \n Rename file: {strAtmtPath}",
                                                                        "Input Dialog", DefaultResponse: strAtmtName[0]);
                                    if (string.IsNullOrEmpty(strAtmtName[0]))
                                    {
                                        if (MessageBox.Show($"Revert to file name: {strAtmtPath}", "", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                                            response = YesNoToAllResponse.No;
                                    }
                                    else
                                    {
                                        strAtmtPath = fsLocation + DteString + " " + strAtmtName[0];
                                        if (FileExtExists)
                                            strAtmtPath = strAtmtPath + "." + strAtmtName[1];
                                    }
                                }

                                mailItem.Close(OlInspectorClose.olDiscard);
                            }
                            else
                            {
                                response = YesNoToAllResponse.Yes;
                            }
                            if (response == YesNoToAllResponse.Yes | response == YesNoToAllResponse.YesToAll)
                                attachment.SaveAsFile(strAtmtPath);
                        }
                    }
                    
                    
                }
            }

        }   

        #endregion

       
        

        private static void PushToUndoStack(
            MailItem beforeMove,
            MailItem afterMove,
            IApplicationGlobals _globals)
        {
            //TODO: Delete _globals.Ol.MovedMails_Stack because it is obsolete
            var info = new MovedMailInfo(beforeMove, afterMove, _globals.Ol.Root.FolderPath);
            _globals.AF.MovedMails.Push(info);
        }
        
        private static void CaptureMoveDetails(
            MailItem mailItem,
            MailItem oMailTmp,
            IApplicationGlobals _globals)
        {
            TraceUtility.LogMethodCall(mailItem, oMailTmp, _globals);

            string[] strAry = oMailTmp.Details(_globals.Ol.ArchiveRootPath).Skip(1).ToArray();
            var output = SanitizeArrayLineTSV(ref strAry);

            _globals.Ol.EmailMoveWriter.Enqueue(output);
        }

        private static string SanitizeArrayLineTSV(ref string[] strOutput)
        {
            //if (strOutput.IsInitialized())
            //{
            var line = string.Join("\t",strOutput
                         //.Where(s => !string.IsNullOrEmpty(s))
                         .Select(s => s ?? "")
                         .Select(s => StripTabsCrLf(s))
                         .ToArray());
            return line;
            //}
            //else { return ""; }
        }

        internal static string StripTabsCrLf(string str)
        {
            var _regex = new Regex(@"[\t\n\r]+");
            string result = _regex.Replace(str, " ");

            // ensure max of one space per word
            _regex = new Regex(@"  +");
            result = _regex.Replace(result, " ");
            result = result.Trim();
            return result;
        }

        public static void WriteCSV_StartNewFileIfDoesNotExist(string strFileName, string strFileLocation)
        {
            string[] strOutput = null;
            string[,] strAryOutput;
            if (File.Exists(Path.Combine(strFileName, strFileLocation)))
            {
                strAryOutput = new string[14, 2];

                strAryOutput[1, 1] = "Triage";
                strAryOutput[2, 1] = "FolderName";
                strAryOutput[3, 1] = "Sent_On";
                strAryOutput[4, 1] = "From";
                strAryOutput[5, 1] = "To";
                strAryOutput[6, 1] = "CC";
                strAryOutput[7, 1] = "Subject";
                strAryOutput[8, 1] = "Body";
                strAryOutput[9, 1] = "fromDomain";
                strAryOutput[10, 1] = "Conversation_ID";
                strAryOutput[11, 1] = "EntryID";
                strAryOutput[12, 1] = "Attachments";
                strAryOutput[13, 1] = "FlaggedAsTask";

                SanitizeArray(strAryOutput, ref strOutput);
                FileIO2.WriteTextFile(strFileName, strOutput, folderpath: strFileLocation);

            }
            strOutput = null;
            strAryOutput = null;
        }

        private static void SanitizeArray(string[,] strAryOutput, ref string[] strOutput)
        {
            if (strAryOutput == null) 
            {
                Debug.WriteLine($"The array {nameof(strAryOutput)} is empty.");
            }
            else
            {
                for (int j = 0; j < strAryOutput.GetLength(0); j++)
                {
                    strOutput[j] = string.Join("\t", strAryOutput
                                         .SliceRow(j)
                                         .Where(s => !string.IsNullOrEmpty(s))
                                         .Select(s => StripTabsCrLf(s))
                                         .ToArray());
                }
            }
        }
        
        
    }

}