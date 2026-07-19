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
        // De-exempted cycle-5 (R3): both WireControlTreeEvents() and WireIntentEvents() are now non-exempt; covered by a headless real-ItemViewer test, QfcItemController.EventWiringTests.cs.
        internal void WireEvents()
        {
            WireControlTreeEvents();
            WireIntentEvents();
        }

        // De-exempted cycle-5 (R1): covered by a headless real-ItemViewer test, QfcItemController.EventWiringTests.cs.
        internal void WireControlTreeEvents()
        {
            ((ItemViewer)_itemViewer).ForAllControls( // concrete-bound seam (P2-T4): control-host path, runs on real ItemViewer during init
                x =>
                {
                    x.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(
                        _kbdHandler.KeyboardHandler_PreviewKeyDownAsync
                    );
                    //x.KeyDown += new System.Windows.Forms.KeyEventHandler(_kbdHandler.KeyboardHandler_KeyDown);
                    x.KeyDown += new System.Windows.Forms.KeyEventHandler(
                        _kbdHandler.KeyboardHandler_KeyDownAsync
                    );
                },
                // #351: the breadcrumb WebView2 replaced CboFolders; keep it excluded from the
                // blanket key wiring exactly as the ComboBox was (its keys route via the bridge).
                new List<Control> { ((ItemViewer)_itemViewer).L0vhBreadcrumb_WebView2 } // concrete-bound seam (P2-T4): control-host path, runs on real ItemViewer during init
            );

            foreach (var btn in Buttons)
            {
                btn.MouseEnter += this.Button_MouseEnter;
                btn.MouseLeave += this.Button_MouseLeave;
            }

            foreach (ToolStripMenuItem menuItem in _itemViewer.MenuItems)
            {
                menuItem.MouseEnter += this.MenuItem_MouseEnter;
                menuItem.MouseLeave += this.MenuItem_MouseLeave;
            }
        }

        internal void WireIntentEvents()
        {
            _itemViewer.ConversationModeChanged += this.CbxConversation_CheckedChanged;
            _itemViewer.FlagTaskClicked += this.BtnFlagTask_Click;
            _itemViewer.PopOutClicked += this.BtnPopOut_Click;
            _itemViewer.DeleteItemClicked += this.BtnDelItem_Click;
            _itemViewer.ReplyClicked += this.BtnReply_Click;
            _itemViewer.ReplyAllClicked += this.BtnReplyAll_Click;
            _itemViewer.ForwardClicked += this.BtnForward_Click;
            _itemViewer.BodyDoubleClick += this.TxtboxBody_DoubleClick;

            _itemViewer.SearchTextChanged += new System.EventHandler(
                this.TextBoxSearch_TextChanged
            );
            //_itemViewer.TxtboxSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxSearch_KeyDown);
            _itemViewer.FolderKeyDown += new System.Windows.Forms.KeyEventHandler(
                _kbdHandler.CboFolders_KeyDownAsync
            );
            // #351: FolderSelectionChanged is now raised by the breadcrumb bridge coordinator
            // (synthetic .NET event) instead of the removed ComboBox; the wiring is unchanged.
            _itemViewer.FolderSelectionChanged += this.CboFolders_SelectedIndexChanged;
            _itemViewer.WebViewInitializationCompleted +=
                WebView2Control_CoreWebView2InitializationCompleted;
            _itemViewer.ConversationItemSelectionChanged +=
                new ListViewItemSelectionChangedEventHandler(this.TopicThread_ItemSelectionChanged);
            _itemViewer.SearchKeyDown += this.TextBoxSearch_KeyDown;
            _itemViewer.EmailCopyChanged += this.CbxEmailCopy_CheckedChanged;
            _itemViewer.AttachmentsChanged += this.CbxAttachments_CheckedChanged;
        }

        // Thin async-void shell (research §3.5): WinForms-event-signature boilerplate forwarding to the
        // testable core with the event args destructured, so no CoreWebView2InitializationCompletedEventArgs
        // needs to be constructed in tests.
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal async void WebView2Control_CoreWebView2InitializationCompleted(
            object sender,
            CoreWebView2InitializationCompletedEventArgs e
        )
        {
            await HandleWebViewInitializedAsync(e.IsSuccess, e.InitializationException);
        }

        internal async Task HandleWebViewInitializedAsync(
            bool isSuccess,
            System.Exception initException
        )
        {
            try
            {
                if (!isSuccess)
                {
                    throw (initException);
                }
                _isWebViewerInitialized = true;

                var delayCount = 0;
                double totalDelay = 0;
                double maxDelay = 10000;
                while (ItemHelper is null)
                {
                    var newDelay = 100 * ++delayCount;

                    if (totalDelay > maxDelay)
                    {
                        throw new TimeoutException(
                            $"ItemHelper is null in {nameof(WebView2Control_CoreWebView2InitializationCompleted)}"
                                + $"and has exceeded the maximum wait time of {Math.Round(maxDelay / 1000, 1)} seconds"
                        );
                    }
                    await Task.Delay(newDelay);
                    totalDelay += newDelay;
                }

                if (_itemViewer.InvokeRequired)
                {
                    _itemViewer.Invoke(() => _itemViewer.NavigateToString(ItemHelper.Html));
                }
                else
                {
                    _itemViewer.NavigateToString(ItemHelper.Html);
                }
            }
            catch (System.Exception ex)
            {
                logger.Error(
                    $"Error in WebView2Control Initialization Completed Event: {ex.Message}",
                    ex
                );
            }
        }

        internal void RegisterFocusActions()
        {
            _kbdHandler.KeyActions.Add(
                ItemHelper.EntryId,
                Keys.Right,
                (x) => this.ToggleConversationCheckbox(Enums.ToggleState.Off)
            );
            _kbdHandler.KeyActions.Add(
                ItemHelper.EntryId,
                Keys.Left,
                (x) => this.ToggleConversationCheckbox(Enums.ToggleState.On)
            );
            _kbdHandler.CharActions.Add(
                ItemHelper.EntryId,
                'O',
                (x) => _ = _explorerController.OpenQFItem(Mail)
            );
            _kbdHandler.CharActions.Add(
                ItemHelper.EntryId,
                'C',
                (x) => this.ToggleConversationCheckbox()
            );
            _kbdHandler.CharActions.Add(
                ItemHelper.EntryId,
                'A',
                (x) => this.ToggleSaveAttachments()
            );
            _kbdHandler.CharActions.Add(
                ItemHelper.EntryId,
                'M',
                (x) => this.ToggleSaveCopyOfMail()
            );
            _kbdHandler.CharActions.Add(ItemHelper.EntryId, 'E', (x) => this.ToggleExpansion());
            _kbdHandler.CharActions.Add(ItemHelper.EntryId, 'S', (x) => this.JumpToSearchTextbox());
            _kbdHandler.CharActions.Add(ItemHelper.EntryId, 'T', (x) => this.FlagAsTask());
            _kbdHandler.CharActions.Add(
                ItemHelper.EntryId,
                'P',
                (x) => this._parent.PopOutControlGroup(ItemNumber)
            );
            _kbdHandler.CharActions.Add(
                ItemHelper.EntryId,
                'R',
                (x) => this._parent.RemoveSpecificControlGroup(ItemNumber)
            );
            _kbdHandler.CharActions.Add(ItemHelper.EntryId, 'X', (x) => this.MarkItemForDeletion());
            _kbdHandler.CharActions.Add(
                ItemHelper.EntryId,
                'F',
                (x) => this.JumpToFolderDropDown()
            );
            if (_expanded)
            {
                RegisterExpandedActions();
            }
        }

        internal void RegisterFocusAsyncActions()
        {
            // TODO: Reference controls from new menu
            //_kbdHandler.KeyActionsAsync.Add(_itemInfo.EntryId, Keys.Right, (x) => ToggleCheckboxAsync(_itemViewer.CbxConversation, Enums.ToggleState.Off));
            //_kbdHandler.KeyActionsAsync.Add(_itemInfo.EntryId, Keys.Left, (x) => ToggleCheckboxAsync(_itemViewer.CbxConversation, Enums.ToggleState.On));
            //_kbdHandler.CharActionsAsync.Add(_itemInfo.EntryId, 'A', (x) => this.ToggleCheckboxAsync(_itemViewer.CbxAttachments));
            // Right arrow expands the conversation thread for the focused item.
            _kbdHandler.KeyActionsAsync.Add(
                ItemHelper.EntryId,
                Keys.Right,
                (x) => this.ToggleExpansionAsync()
            );
            _kbdHandler.CharActionsAsync.Add(
                ItemHelper.EntryId,
                'C',
                (x) =>
                {
                    this.ToggleConversationCheckbox();
                    return Task.CompletedTask;
                }
            );
            _kbdHandler.CharActionsAsync.Add(
                ItemHelper.EntryId,
                'O',
                (x) => _ = _explorerController.OpenQFItem(Mail)
            );
            _kbdHandler.CharActionsAsync.Add(
                ItemHelper.EntryId,
                'M',
                (x) => this.KbdExecuteAsync(MenuDropDown, true)
            );
            _kbdHandler.CharActionsAsync.Add(
                ItemHelper.EntryId,
                'R',
                (x) => this.KbdExecuteAsync(Reply, true)
            );
            _kbdHandler.CharActionsAsync.Add(
                ItemHelper.EntryId,
                'L',
                (x) => this.KbdExecuteAsync(ReplyAll, true)
            );
            _kbdHandler.CharActionsAsync.Add(
                ItemHelper.EntryId,
                'W',
                (x) => this.KbdExecuteAsync(Forward, true)
            );
            _kbdHandler.CharActionsAsync.Add(
                ItemHelper.EntryId,
                'E',
                (x) => this.ToggleExpansionAsync()
            );
            _kbdHandler.CharActionsAsync.Add(
                ItemHelper.EntryId,
                'S',
                (x) =>
                {
                    this.JumpToSearchTextbox();
                    return Task.CompletedTask;
                }
            );
            _kbdHandler.CharActionsAsync.Add(
                ItemHelper.EntryId,
                'T',
                (x) => this.KbdExecuteAsync(FlagAsTaskAsync, true)
            );
            _kbdHandler.CharActionsAsync.Add(
                ItemHelper.EntryId,
                'P',
                (x) => this.KbdExecuteAsync(_parent.PopOutControlGroupAsync, ItemNumber, false)
            );
            _kbdHandler.CharActionsAsync.Add(
                ItemHelper.EntryId,
                'Z',
                (x) =>
                    this.KbdExecuteAsync(_parent.RemoveSpecificControlGroupAsync, ItemNumber, false)
            );
            _kbdHandler.CharActionsAsync.Add(
                ItemHelper.EntryId,
                'X',
                (x) => this.KbdExecuteAsync(this.MarkItemForDeletionAsync, false)
            );
            _kbdHandler.CharActionsAsync.Add(
                ItemHelper.EntryId,
                'F',
                (x) => this.JumpToFolderDropDownAsync()
            );
            if (_expanded)
            {
                RegisterExpandedAsyncActions();
            }
        }

        internal void RegisterExpandedActions()
        {
            _kbdHandler.CharActions.Add(
                ItemHelper.EntryId,
                'B',
                async (x) => await JumpToAsync(((ItemViewer)_itemViewer).L0v2h2_WebView2) // concrete-bound seam (P2-T4): control-host path, runs on real ItemViewer during init
            );
            _kbdHandler.CharActions.Add(
                ItemHelper.EntryId,
                'D',
                async (x) => await JumpToAsync(((ItemViewer)_itemViewer).TopicThread) // concrete-bound seam (P2-T4): control-host path, runs on real ItemViewer during init
            );
        }

        internal void RegisterExpandedAsyncActions()
        {
            _kbdHandler.CharActionsAsync.Add(
                ItemHelper.EntryId,
                'B',
                (x) => JumpToAsync(((ItemViewer)_itemViewer).L0v2h2_WebView2) // concrete-bound seam (P2-T4): control-host path, runs on real ItemViewer during init
            );
            _kbdHandler.CharActionsAsync.Add(
                ItemHelper.EntryId,
                'D',
                (x) => JumpToAsync(((ItemViewer)_itemViewer).TopicThread) // concrete-bound seam (P2-T4): control-host path, runs on real ItemViewer during init
            );
        }

        internal void UnregisterFocusActions()
        {
            _kbdHandler.KeyActions.Remove(ItemHelper.EntryId, Keys.Right);
            _kbdHandler.KeyActions.Remove(ItemHelper.EntryId, Keys.Left);
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'O');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'C');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'A');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'M');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'E');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'S');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'T');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'P');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'R');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'X');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'F');
            if (_expanded)
            {
                UnregisterExpandedActions();
            }
        }

        internal void UnregisterFocusAsyncActions()
        {
            //_kbdHandler.KeyActionsAsync.Remove(_itemInfo.EntryId, Keys.Left);
            //_kbdHandler.CharActionsAsync.Remove(_itemInfo.EntryId, 'A');
            _kbdHandler.KeyActionsAsync.Remove(ItemHelper.EntryId, Keys.Right);
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'C');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'O');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'M');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'R');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'L');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'W');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'E');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'S');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'T');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'P');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'Z');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'X');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'F');
            if (_expanded)
            {
                UnregisterExpandedAsyncActions();
            }
        }

        internal void UnregisterExpandedActions()
        {
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'B');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'D');
        }

        internal void UnregisterExpandedAsyncActions()
        {
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'B');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'D');
        }
    }
}
