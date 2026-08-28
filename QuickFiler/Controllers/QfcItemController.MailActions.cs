using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Web.WebView2.Core;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using QuickFiler.Viewers;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;
using UtilitiesCS.Extensions;

namespace QuickFiler.Controllers
{
    internal partial class QfcItemController
    {
        // #483: injectable seam for the user-facing move-failure notification. The default forwards
        // to the modal WinForms dialog, which cannot run in a headless unit test. Mirrors
        // EfcHomeController.MoveFailureMessageAction and QfcExplorerController.NotInViewDialogInvoker.
        internal System.Action<string> MoveFailureNotifier { get; set; } =
            text => MessageBox.Show(text);

        // #483: composes the two seams. _uiDispatcher is null in the existing SeamFactoryTests
        // MoveMailAsync tests, so the notifier is invoked directly when there is no dispatcher.
        private void NotifyMoveFailure(string message)
        {
            var notifier = MoveFailureNotifier;
            var dispatcher = _uiDispatcher;
            if (dispatcher is null)
            {
                notifier(message);
                return;
            }

            dispatcher.Invoke(() => notifier(message));
        }

        internal void CollapseConversation()
        {
            //TraceUtility.LogMethodCall();

            var folderList = _itemViewer.GetFolderItems();
            var entryID = _convOriginID != "" ? _convOriginID : _mailActions.EntryID;
            _parent.ToggleGroupConv(entryID);
        }

        internal void EnumerateConversation()
        {
            //TraceUtility.LogMethodCall();

            var folderList = _itemViewer.GetFolderItems();
            _parent.ToggleUnGroupConv(
                ConversationResolver,
                _mailActions.EntryID,
                ConversationResolver.Count.SameFolder,
                folderList
            );
        }

        internal async Task EnumerateConversationAsync()
        {
            Token.ThrowIfCancellationRequested();
            await _uiDispatcher.InvokeAsync(EnumerateConversation);
        }

        public Dictionary<string, System.Action> RightKeyActions
        {
            get =>
                new()
                {
                    { "&Pop Out", () => this._parent.PopOutControlGroup(ItemNumber) },
                    {
                        "&Expand",
                        () =>
                        {
                            _ = _itemViewer.FocusSubject();
                            this.EnumerateConversation();
                        }
                    },
                    { "&Cancel", () => { } },
                };
        }

        public Dictionary<string, Func<Task>> RightKeyActionsAsync
        {
            get =>
                new()
                {
                    { "&Pop Out", () => this._parent.PopOutControlGroupAsync(ItemNumber) },
                    { "&Expand", () => this.EnumerateConversationAsync() },
                    { "&Cancel", () => Task.CompletedTask },
                };
        }

        public async Task MoveMailAsync()
        {
            //TraceUtility.LogMethodCall();

            // #483: outside the try, so the catch below cannot swallow or re-wrap the cancellation.
            Token.ThrowIfCancellationRequested();

            if (ItemHelper is not null)
            {
                IList<MailItemHelper> helpers = PackageItems();
                bool attachments = SelectedFolder != "Trash to Delete" && _optionAttachments;
                try
                {
                    if (!_globals.FS.SpecialFolders.TryGetValue("OneDrive", out var oneDrive))
                    {
                        logger.Debug(
                            $"{nameof(MoveMailAsync)} aborted due to lack of OneDrive location"
                        );
                        return;
                    }
                    var config = new EmailFilerConfig()
                    {
                        SavePictures = _optionsPictures,
                        DestinationOlStem = SelectedFolder,
                        SaveMsg = _optionEmailCopy,
                        SaveAttachments = attachments,
                        Globals = _globals,
                        OlAncestor = _globals.Ol.ArchiveRootPath,
                        FsAncestorEquivalent = oneDrive,
                    };
                    var filer = _emailFilerFactory(config);
                    _homeController.FilerQueue.Enqueue(filer, helpers);
                    await Task.CompletedTask;
                    //await filer.SortAsync(helpers);
                }
                catch (System.Exception e)
                {
                    // #483: a broad catch is permitted only when it propagates with added context.
                    // The caller (QfcCollectionController.TryMoveEmailByGroupAsync) already catches,
                    // logs with subject context and continues, so the bulk loop is unaffected; what
                    // changes is that a failed file is no longer reported to it as a success.
                    logger.Error($"{e}");
                    NotifyMoveFailure(
                        $"Error moving mail {ItemHelper.Subject} from {ItemHelper.Sender} on {ItemHelper.SentDate}. Skipping"
                    );
                    throw new System.InvalidOperationException(
                        $"Failed to file mail '{ItemHelper.Subject}' to '{SelectedFolder}'.",
                        e
                    );
                }

                //SortEmail.Cleanup_Files();
            }
        }

        //async public Task MoveMailAsync()
        //{
        //    //TraceUtility.LogMethodCall();

        //    if (Mail is not null)
        //    {
        //        IList<MailItem> selItems = PackageItems();
        //        bool attachments = SelectedFolder != "Trash to Delete" && _optionAttachments;
        //        try
        //        {
        //            await SortEmail.SortAsync(
        //                mailItems: selItems,
        //                savePictures: _optionsPictures,
        //                destinationOlStem: SelectedFolder,
        //                saveMsg: _optionEmailCopy,
        //                saveAttachments: attachments,
        //                removePreviousFsFiles: false,
        //                appGlobals: _globals,
        //                olAncestor: _globals.Ol.ArchiveRootPath,
        //                fsAncestorEquivalent: _globals.FS.FldrOneDrive);
        //        }
        //        catch (System.Exception e)
        //        {
        //            //logger.Debug($"Error moving mail {Subject} from {Sender} on {SentDate}. Skipping");
        //            logger.Error($"{e}");
        //            MessageBox.Show($"Error moving mail {Subject} from {Sender} on {SentDate}. Skipping");
        //        }

        //        SortEmail.Cleanup_Files();
        //    }
        //}

        internal IList<MailItemHelper> PackageItems()
        {
            return _optionConversationChecked
                ? ConversationResolver.ConversationInfo.SameFolder
                : new List<MailItemHelper> { ItemHelper };
        }

        public void FlagAsTask()
        {
            List<MailItem> itemList = [Mail];
            var flagTask = _flagTasksFactory(
                _globals,
                itemList,
                false,
                _homeController.FormController.FormHandle
            );
            DialogResult flagTaskResult = flagTask.Run(modal: true);
            _itemViewer.FlagTaskDialogResult = flagTaskResult;
            if (flagTaskResult == DialogResult.OK)
            {
                _itemViewer.FlagTaskBackColor = _themes[_activeTheme].ButtonClickedColor;
            }
        }

        public async Task FlagAsTaskAsync()
        {
            Token.ThrowIfCancellationRequested();
            List<MailItem> itemList = [Mail];
            await _uiDispatcher.InvokeAsync(() =>
            {
                var flagTask = _flagTasksFactory(
                    _globals,
                    itemList,
                    false,
                    _homeController.FormController.FormHandle
                );
                DialogResult flagTaskResult = flagTask.Run(modal: true);
                _itemViewer.FlagTaskDialogResult = flagTaskResult;
                if (flagTaskResult == DialogResult.OK)
                {
                    _itemViewer.FlagTaskBackColor = _themes[_activeTheme].ButtonClickedColor;
                }
            });
        }

        public void MarkItemForDeletion()
        {
            if (!_itemViewer.FolderContains("Trash to Delete"))
            {
                _itemViewer.AddFolderItems(new[] { "Trash to Delete" });
            }
            _itemViewer.SetFolderSelectedItem("Trash to Delete");
        }

        public async Task MarkItemForDeletionAsync()
        {
            Token.ThrowIfCancellationRequested();
            await _uiDispatcher.InvokeAsync(() =>
            {
                if (!_itemViewer.FolderContains("Trash to Delete"))
                {
                    _itemViewer.AddFolderItems(new[] { "Trash to Delete" });
                }
                _itemViewer.SetFolderSelectedItem("Trash to Delete");
            });
        }
    }
}
