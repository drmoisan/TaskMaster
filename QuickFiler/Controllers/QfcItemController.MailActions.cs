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
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal void CollapseConversation()
        {
            //TraceUtility.LogMethodCall();

            var folderList = _itemViewer.GetFolderItems();
            var entryID = _convOriginID != "" ? _convOriginID : Mail.EntryID;
            _parent.ToggleGroupConv(entryID);
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal void EnumerateConversation()
        {
            //TraceUtility.LogMethodCall();

            var folderList = _itemViewer.GetFolderItems();
            _parent.ToggleUnGroupConv(
                ConversationResolver,
                Mail.EntryID,
                ConversationResolver.Count.SameFolder,
                folderList
            );
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal async Task EnumerateConversationAsync()
        {
            await UiThread.Dispatcher.InvokeAsync(EnumerateConversation);
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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
                            _itemViewer.FocusSubject();
                            this.EnumerateConversation();
                        }
                    },
                    { "&Cancel", () => { } },
                };
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task MoveMailAsync()
        {
            //TraceUtility.LogMethodCall();

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
                    var filer = new EmailFiler(config);
                    _homeController.FilerQueue.Enqueue(filer, helpers);
                    await Task.CompletedTask;
                    //await filer.SortAsync(helpers);
                }
                catch (System.Exception e)
                {
                    //logger.Debug($"Error moving mail {Subject} from {Sender} on {SentDate}. Skipping");
                    logger.Error($"{e}");
                    MessageBox.Show(
                        $"Error moving mail {ItemHelper.Subject} from {ItemHelper.Sender} on {ItemHelper.SentDate}. Skipping"
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

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void FlagAsTask()
        {
            List<MailItem> itemList = [Mail];
            var flagTask = new FlagTasks(
                globals: _globals,
                itemList: itemList,
                blFile: false,
                hWndCaller: _homeController.FormController.FormHandle
            );
            _itemViewer.FlagTaskDialogResult = flagTask.Run(modal: true);
            if (_itemViewer.FlagTaskDialogResult == DialogResult.OK)
            {
                _itemViewer.FlagTaskBackColor = _themes[_activeTheme].ButtonClickedColor;
            }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task FlagAsTaskAsync()
        {
            List<MailItem> itemList = [Mail];
            await UiThread.Dispatcher.InvokeAsync(() =>
            {
                var flagTask = new FlagTasks(
                    globals: _globals,
                    itemList: itemList,
                    blFile: false,
                    hWndCaller: _homeController.FormController.FormHandle
                );
                _itemViewer.FlagTaskDialogResult = flagTask.Run(modal: true);
                if (_itemViewer.FlagTaskDialogResult == DialogResult.OK)
                {
                    _itemViewer.FlagTaskBackColor = _themes[_activeTheme].ButtonClickedColor;
                }
            });
        }

        public void MarkItemForDeletion()
        {
            if (!_itemViewer.FolderContains("Trash to Delete"))
            {
                _itemViewer.SetFolderItems(new[] { "Trash to Delete" });
            }
            _itemViewer.SetFolderSelectedItem("Trash to Delete");
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task MarkItemForDeletionAsync()
        {
            Token.ThrowIfCancellationRequested();
            await UiThread.Dispatcher.InvokeAsync(() =>
            {
                if (!_itemViewer.FolderContains("Trash to Delete"))
                {
                    _itemViewer.SetFolderItems(new[] { "Trash to Delete" });
                }
                _itemViewer.SetFolderSelectedItem("Trash to Delete");
            });
        }
    }
}
