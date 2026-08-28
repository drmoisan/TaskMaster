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
        public void JumpToFolderDropDown()
        {
            _kbdHandler.ToggleKeyboardDialog();
            _itemViewer.Invoke(
                new System.Action(() =>
                {
                    _itemViewer.FocusFolderDropDown();
                    _itemViewer.SetFolderDroppedDown(true);
                    _intEnterCounter = 0;
                })
            );
        }

        public async Task JumpToFolderDropDownAsync()
        {
            await _kbdHandler.ToggleKeyboardDialogAsync();
            await _uiDispatcher.InvokeAsync(() =>
            {
                _itemViewer.FocusFolderDropDown();
                _itemViewer.SetFolderDroppedDown(true);
                _intEnterCounter = 0;
            });
        }

        public void JumpToSearchTextbox()
        {
            _kbdHandler.ToggleKeyboardDialog();
            _itemViewer.FocusSearch();
        }

        internal async Task JumpToAsync(Control control)
        {
            await _uiDispatcher.InvokeAsync(() => control.Focus());
            await _kbdHandler.ToggleKeyboardDialogAsync();
        }

        public async Task KbdExecuteAsync(Func<Task> action, bool deactivateKbd)
        {
            if (deactivateKbd)
            {
                _homeController.KeyboardHandler.ToggleKeyboardDialog();
            }
            await action();
        }

        public async Task KbdExecuteAsync<T>(Func<T, Task> action, T parameter, bool deactivateKbd)
        {
            if (deactivateKbd)
            {
                _homeController.KeyboardHandler.ToggleKeyboardDialog();
            }
            await action(parameter);
        }

        public async Task MenuDropDown()
        {
            await _uiDispatcher.InvokeAsync(() => _itemViewer.ShowMoveOptionsMenu());
        }

        public async Task Reply()
        {
            // reply.Display() stays OUTSIDE the dispatched delegate to preserve the original
            // thread-affinity behavior (research §3.4.3 / plan P6-T2).
            var reply = await _uiDispatcher.InvokeAsync<MailItem>(() => _mailActions.Reply());
            reply.Display();
        }

        public async Task ReplyAll()
        {
            var reply = await _uiDispatcher.InvokeAsync<MailItem>(() => _mailActions.ReplyAll());
            reply.Display();
        }

        public async Task Forward()
        {
            var forward = await _uiDispatcher.InvokeAsync<MailItem>(() => _mailActions.Forward());
            forward.Display();
        }

        // ToggleCbMenuItemAsync(ToolStripMenuItemCb)[,ToggleState] and
        // ToggleCheckboxAsync(CheckBox)[,ToggleState] were removed in Phase 7 (P7-T1): they took raw
        // WinForms parameter types replaced by the cycle-1 Seam B intent members and had zero live call
        // sites across the solution (all references were commented out), i.e. dead after the narrowing.

        /// <summary>
        /// Function programmatically clicks the "Conversation" checkbox
        /// </summary>
        public void ToggleConversationCheckbox()
        {
            _uiDispatcher.Invoke(() =>
                _itemViewer.ConversationModeChecked = !_itemViewer.ConversationModeChecked
            );
        }

        /// <summary>
        /// Function programmatically sets the "Conversation" checkbox to the desired state
        /// if it is not already in that state
        /// </summary>
        /// <param name="desiredState">State of checkbox desired</param>
        public void ToggleConversationCheckbox(Enums.ToggleState desiredState)
        {
            _uiDispatcher.Invoke(() =>
            {
                switch (desiredState)
                {
                    case Enums.ToggleState.On:
                        if (_itemViewer.ConversationModeChecked == false)
                            _itemViewer.ConversationModeChecked = true;
                        break;
                    case Enums.ToggleState.Off:
                        if (_itemViewer.ConversationModeChecked == true)
                            _itemViewer.ConversationModeChecked = false;
                        break;
                    default:
                        _itemViewer.ConversationModeChecked = !_itemViewer.ConversationModeChecked;
                        break;
                }
            });
        }

        public void ToggleExpansion()
        {
            if (_expanded)
            {
                ToggleExpansion(Enums.ToggleState.Off);
            }
            else
            {
                ToggleExpansion(Enums.ToggleState.On);
            }
        }

        public async Task ToggleExpansionAsync()
        {
            if (_expanded)
            {
                await ToggleExpansionAsync(Enums.ToggleState.Off);
            }
            else
            {
                await ToggleExpansionAsync(Enums.ToggleState.On);
            }
        }

        /// <summary>
        /// Issue #482: the single owner of expansion keyboard registration, keyed on the actual
        /// current state rather than on which code path performed the toggle.
        /// </summary>
        /// <param name="expanded">
        /// The value of <c>_expanded</c> after the toggle. When true both registries are populated;
        /// when false both are left empty.
        /// </param>
        /// <remarks>
        /// The two unregister calls are unconditional and are safe when the entries are absent,
        /// because <c>KbdActions.Remove</c> returns <c>false</c> rather than throwing. That is what
        /// makes repeated and interleaved toggles idempotent without changing <c>Add</c>'s contract.
        /// Both registries are maintained together so a later focus-path cleanup removes from a
        /// registry that genuinely holds the entries.
        /// </remarks>
        private void SyncExpandedRegistrations(bool expanded)
        {
            UnregisterExpandedActions();
            UnregisterExpandedAsyncActions();
            if (expanded)
            {
                RegisterExpandedActions();
                RegisterExpandedAsyncActions();
            }
        }

        // Made virtual so tests can override the (TlpCellSnapShot-bound, out-of-scope) state-taking
        // body and verify the parameterless-overload routing without the control-tree collaborator.
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public virtual void ToggleExpansion(Enums.ToggleState desiredState)
        {
            _parent.ToggleExpansionStyle(ItemIndex, desiredState);
            if (desiredState == Enums.ToggleState.On)
            {
                ToggleExpansionOn();
            }
            else
            {
                ToggleExpansionOff();
            }
            SyncExpandedRegistrations(_expanded);
        }

        // Made virtual so tests can override the (TlpCellSnapShot-bound, out-of-scope) state-taking
        // body and verify the parameterless-overload routing without the control-tree collaborator.
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public virtual async Task ToggleExpansionAsync(Enums.ToggleState desiredState)
        {
            await _parent.ToggleExpansionStyleAsync(ItemIndex, desiredState);
            if (desiredState == Enums.ToggleState.On)
            {
                await _uiDispatcher.InvokeAsync(() => ToggleExpansionOn());
            }
            else
            {
                await _uiDispatcher.InvokeAsync(() => ToggleExpansionOff());
            }
            SyncExpandedRegistrations(_expanded);
        }

        private void ToggleExpansionOff()
        {
            _tlpStates["Compressed"].ApplyState(_itemViewer);
            _expanded = false;
            if (_emailIsReadTimer is not null)
            {
                _emailIsReadTimer.Dispose();
            }
        }

        private void ToggleExpansionOn()
        {
            _tlpStates["Expanded"].ApplyState(_itemViewer);
            _expanded = true;
            if ((ItemHelper is not null) && ItemHelper.UnRead == true)
            {
                _emailIsReadTimer = new System.Threading.Timer(ApplyReadEmailFormat);
                _emailIsReadTimer.Change(4000, System.Threading.Timeout.Infinite);
            }
        }
    }
}
