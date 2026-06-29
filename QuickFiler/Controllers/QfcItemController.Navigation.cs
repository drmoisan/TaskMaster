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

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task JumpToFolderDropDownAsync()
        {
            await _kbdHandler.ToggleKeyboardDialogAsync();
            await UiThread.Dispatcher.InvokeAsync(() =>
            {
                _itemViewer.FocusFolderDropDown();
                _itemViewer.SetFolderDroppedDown(true);
                _intEnterCounter = 0;
            });
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void JumpToSearchTextbox()
        {
            _kbdHandler.ToggleKeyboardDialog();
            _itemViewer.FocusSearch();
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal async Task JumpToAsync(Control control)
        {
            await UiThread.Dispatcher.InvokeAsync(() => control.Focus());
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

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task MenuDropDown()
        {
            await UiThread.Dispatcher.InvokeAsync(() => _itemViewer.ShowMoveOptionsMenu());
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task Reply()
        {
            var reply = await UiThread.Dispatcher.InvokeAsync(() => this.Mail.Reply());
            reply.Display();
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task ReplyAll()
        {
            var reply = await UiThread.Dispatcher.InvokeAsync(() => this.Mail.ReplyAll());
            reply.Display();
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task Forward()
        {
            var forward = await UiThread.Dispatcher.InvokeAsync(() => this.Mail.Forward());
            forward.Display();
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task ToggleCbMenuItemAsync(ToolStripMenuItemCb menuItem)
        {
            await UiThread.Dispatcher.InvokeAsync(() => menuItem.Checked = !menuItem.Checked);
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task ToggleCbMenuItemAsync(
            ToolStripMenuItemCb menuItem,
            Enums.ToggleState desiredState
        )
        {
            var booleanState = desiredState.HasFlag(Enums.ToggleState.On);

            await UiThread.Dispatcher.InvokeAsync(() =>
            {
                if (menuItem.Checked != booleanState)
                {
                    menuItem.Checked = booleanState;
                }
            });
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task ToggleCheckboxAsync(CheckBox checkBox)
        {
            await UiThread.Dispatcher.InvokeAsync(() => checkBox.Checked = !checkBox.Checked);
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task ToggleCheckboxAsync(CheckBox checkBox, Enums.ToggleState desiredState)
        {
            var booleanState = desiredState.HasFlag(Enums.ToggleState.On);

            await UiThread.Dispatcher.InvokeAsync(() =>
            {
                if (checkBox.Checked != booleanState)
                {
                    checkBox.Checked = booleanState;
                }
            });
            //await _homeController.KeyboardHandler.ToggleKeyboardDialogAsync();
        }

        /// <summary>
        /// Function programmatically clicks the "Conversation" checkbox
        /// </summary>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void ToggleConversationCheckbox()
        {
            UiThread.Dispatcher.Invoke(() =>
                _itemViewer.ConversationModeChecked = !_itemViewer.ConversationModeChecked
            );
        }

        /// <summary>
        /// Function programmatically sets the "Conversation" checkbox to the desired state
        /// if it is not already in that state
        /// </summary>
        /// <param name="desiredState">State of checkbox desired</param>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void ToggleConversationCheckbox(Enums.ToggleState desiredState)
        {
            UiThread.Dispatcher.Invoke(() =>
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

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void ToggleExpansion(Enums.ToggleState desiredState)
        {
            _parent.ToggleExpansionStyle(ItemIndex, desiredState);
            if (desiredState == Enums.ToggleState.On)
            {
                ToggleExpansionOn();
                RegisterExpandedActions();
            }
            else
            {
                ToggleExpansionOff();
                UnregisterExpandedActions();
            }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task ToggleExpansionAsync(Enums.ToggleState desiredState)
        {
            await _parent.ToggleExpansionStyleAsync(ItemIndex, desiredState);
            if (desiredState == Enums.ToggleState.On)
            {
                await UiThread.Dispatcher.InvokeAsync(() => ToggleExpansionOn());
                RegisterExpandedAsyncActions();
            }
            else
            {
                await UiThread.Dispatcher.InvokeAsync(() => ToggleExpansionOff());
                UnregisterExpandedAsyncActions();
            }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private void ToggleExpansionOff()
        {
            _tlpStates["Compressed"].ApplyState((ItemViewer)_itemViewer); // concrete-bound seam (P2-T4): control-host path, runs on real ItemViewer during init
            _expanded = false;
            if (_emailIsReadTimer is not null)
            {
                _emailIsReadTimer.Dispose();
            }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private void ToggleExpansionOn()
        {
            _tlpStates["Expanded"].ApplyState((ItemViewer)_itemViewer); // concrete-bound seam (P2-T4): control-host path, runs on real ItemViewer during init
            _expanded = true;
            if ((ItemHelper is not null) && ItemHelper.UnRead == true)
            {
                _emailIsReadTimer = new System.Threading.Timer(ApplyReadEmailFormat);
                _emailIsReadTimer.Change(4000, System.Threading.Timeout.Infinite);
            }
        }
    }
}
