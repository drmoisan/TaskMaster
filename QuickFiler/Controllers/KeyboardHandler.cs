using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses;

namespace QuickFiler.Controllers
{
    [ExcludeFromCodeCoverage]
    internal class KeyboardHandler : IQfcKeyboardHandler
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        public KeyboardHandler(IQfcFormViewer viewer, IFilerHomeController parent)
        {
            viewer.SetKeyboardHandler(this);
            _parent = parent;
        }

        public KeyboardHandler(EfcViewer viewer, IFilerHomeController parent)
        {
            viewer.SetKeyboardHandler(this);
            _parent = parent;
        }

        private IFilerHomeController _parent;
        private bool _kbdActive = false;

        private KbdActions<char, KaChar, Action<char>> _charActions = [];
        public KbdActions<char, KaChar, Action<char>> CharActions
        {
            get => _charActions;
            set => _charActions = value;
        }

        private KbdActions<char, KaCharAsync, Func<char, Task>> _charActionsAsync = [];
        public KbdActions<char, KaCharAsync, Func<char, Task>> CharActionsAsync
        {
            get => _charActionsAsync;
            set => _charActionsAsync = value;
        }

        private KbdActions<Keys, KaKey, Action<Keys>> _keyActions = [];
        public KbdActions<Keys, KaKey, Action<Keys>> KeyActions
        {
            get => _keyActions;
            set => _keyActions = value;
        }

        private KbdActions<Keys, KaKeyAsync, Func<Keys, Task>> _alwaysOnKeyActionsAsync = [];
        public KbdActions<Keys, KaKeyAsync, Func<Keys, Task>> AlwaysOnKeyActionsAsync
        {
            get => _alwaysOnKeyActionsAsync;
            set => _alwaysOnKeyActionsAsync = value;
        }

        private KbdActions<Keys, KaKeyAsync, Func<Keys, Task>> _keyActionsAsync = [];
        public KbdActions<Keys, KaKeyAsync, Func<Keys, Task>> KeyActionsAsync
        {
            get => _keyActionsAsync;
            set => _keyActionsAsync = value;
        }

        private StringBuilder _filterBuilder = new StringBuilder();

        public void ClearFilter() => _filterBuilder = new StringBuilder();

        private KbdActions<string, KaStringAsync, Func<string, Task>> _stringActionsAsync = [];
        public KbdActions<string, KaStringAsync, Func<string, Task>> StringActionsAsync
        {
            get => _stringActionsAsync;
            set => _stringActionsAsync = value;
        }

        public bool KbdActive
        {
            get => _kbdActive;
            set { _kbdActive = value; }
        }

        public void KeyboardHandler_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (KbdActive && (KeyActions != null) && KeyActions.ContainsKey(e.KeyCode))
            {
                e.IsInputKey = true;
            }
        }

        public void KeyboardHandler_PreviewKeyDownAsync(object sender, PreviewKeyDownEventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(_parent.UiSyncContext);
            if (KbdActive && (KeyActionsAsync != null) && KeyActionsAsync.ContainsKey(e.KeyCode))
            {
                e.IsInputKey = true;
            }
        }

        public void KeyboardHandler_KeyDown(object sender, KeyEventArgs e)
        {
            if (KbdActive)
            {
                if ((KeyActions != null) && KeyActions.ContainsKey(e.KeyCode))
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    KeyActions[e.KeyCode].DynamicInvoke(e.KeyCode);
                }
                else if ((CharActions != null) && CharActions.ContainsKey((char)e.KeyValue))
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    CharActions[(char)e.KeyValue].DynamicInvoke((char)e.KeyValue);
                }
            }
        }

        public async void KeyboardHandler_KeyDownAsync(object sender, KeyEventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(_parent.UiSyncContext);
            try
            {
                await KeyDownTaskAsync(sender, e);
            }
            catch (System.Exception ex)
            {
                logger.Error(
                    $"Error in {nameof(KeyboardHandler_KeyDownAsync)} for key {e.KeyValue}. {ex.Message}",
                    ex
                );
            }
        }

        public async Task KeyDownTaskAsync(object sender, KeyEventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(_parent.UiSyncContext);

            if ((AlwaysOnKeyActionsAsync != null) && AlwaysOnKeyActionsAsync.ContainsKey(e.KeyCode))
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                await AlwaysOnKeyActionsAsync[e.KeyCode](e.KeyCode);
            }

            if (KbdActive)
            {
                if ((KeyActionsAsync != null) && KeyActionsAsync.ContainsKey(e.KeyCode))
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    await KeyActionsAsync[e.KeyCode](e.KeyCode);
                }
                else if (
                    (CharActionsAsync != null) && CharActionsAsync.ContainsKey((char)e.KeyValue)
                )
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    await CharActionsAsync[(char)e.KeyValue]((char)e.KeyValue);
                }
                else if (StringActionsAsync != null)
                {
                    _filterBuilder.Append(char.ToLower((char)e.KeyValue));
                    if (StringActionsAsync.ContainsKey(_filterBuilder.ToString()))
                    {
                        e.SuppressKeyPress = true;
                        e.Handled = true;

                        if (_filterBuilder.Length == 1)
                            StringActionsAsync.ForEach(x => x.Activated = true);
                        var actions = StringActionsAsync.FilterKeys(_filterBuilder.ToString());
                        if (actions.Length == 0)
                            _filterBuilder.Length = 0;
                        else if (actions.Length == 1)
                        {
                            var keyName = actions[0].Key;
                            await StringActionsAsync[keyName](keyName);
                            _filterBuilder.Length = 0;
                        }
                    }
                    else
                    {
                        _filterBuilder.Length--;
                    }
                }
            }
        }

        public void ToggleKeyboardDialog()
        {
            if (_kbdActive)
            {
                _parent.FormController.ToggleOffNavigation(async: false);
            }
            else
            {
                _parent.FormController.ToggleOnNavigation(async: false);
            }
            _kbdActive = !_kbdActive;
        }

        public void ToggleKeyboardDialog(object sender, KeyEventArgs e)
        {
            ToggleKeyboardDialog();
            e.Handled = true;
        }

        public async Task ToggleKeyboardDialogAsync()
        {
            if (_kbdActive)
            {
                await _parent.FormController.ToggleOffNavigationAsync();
            }
            else
            {
                await _parent.FormController.ToggleOnNavigationAsync();
            }
            _kbdActive = !_kbdActive;
        }

        public async void ToggleKeyboardDialogAsync(object sender, KeyEventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(_parent.UiSyncContext);

            await ToggleKeyboardDialogAsync();
            e.Handled = true;
        }

        internal ItemViewer GetItemViewer(Control control)
        {
            if (control as ItemViewer != null)
            {
                return (control as ItemViewer);
            }
            else if (control.Parent != null)
            {
                return GetItemViewer(control.Parent);
            }
            else
            {
                return null;
            }
        }

        // #351: the breadcrumb surface raises its arrows through the JS bridge, so this handler
        // now serves only genuine ComboBox senders (the old sender-type ArgumentException guard is
        // bypassed for the breadcrumb's synthetic FolderKeyDown events instead of throwing).
        public async void CboFolders_KeyDownAsync(object sender, KeyEventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            if (!(sender is ComboBox cb))
            {
                // Breadcrumb-originated synthetic key events are fully handled by the bridge
                // (BreadcrumbBridgeCoordinator.UnhandledArrow drives the legacy fall-throughs).
                return;
            }
            if (cb.DroppedDown)
            {
                await DdOpen_KeyDownAsync(cb, e);
            }
            else
            {
                await DdClosed_KeyDownAsync(cb, e);
            }
        }

        // #351: legacy fall-through target for BreadcrumbBridgeCoordinator.UnhandledArrow —
        // Right (nothing to expand) opens the Pop Out / Enumerate Conversation dialog exactly as
        // the old dropdown-open Right did; Left (nothing to collapse) closes the folder control
        // via the SetFolderDroppedDown(false) intent, matching the old close-the-dropdown branch.
        public void BreadcrumbArrowFallThrough(
            ItemViewer viewer,
            UtilitiesCS.OutlookObjects.Folder.BreadcrumbArrowDirection direction
        )
        {
            if (viewer is null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }

            if (direction == UtilitiesCS.OutlookObjects.Folder.BreadcrumbArrowDirection.Right)
            {
                MyBox.ShowDialog(
                    "Pop Out Item or Enumerate Conversation?",
                    "Dialog",
                    BoxIcon.Question,
                    viewer.Controller.RightKeyActions
                );
            }
            else
            {
                viewer.SetFolderDroppedDown(false);
            }
        }

        public async Task DdOpen_KeyDownAsync(ComboBox cbo, KeyEventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            switch (e.KeyCode)
            {
                //case Keys.Escape:
                //    {
                //        // Close the drop down box
                //        UIThreadExtensions.UiDispatcher.Invoke(() => cbo.DroppedDown = false);
                //        e.SuppressKeyPress = true;
                //        e.Handled = true;
                //        break;
                //    }
                case Keys k when (k == Keys.Up || k == Keys.Down):
                {
                    // Don't handle the instruction so that it moves the selection up
                    break;
                }
                //case Keys.Down:
                //    {
                //        // Don't handle the instruction so that it moves the selection down
                //        break;
                //    }
                case Keys.Right:
                {
                    // #351: the breadcrumb owns Right-arrow expansion via the JS bridge; for a
                    // remaining ComboBox sender only the legacy Pop Out / Enumerate dialog applies.
                    e.SuppressKeyPress = true;
                    e.Handled = true;

                    MyBox.ShowDialog(
                        "Pop Out Item or Enumerate Conversation?",
                        "Dialog",
                        BoxIcon.Question,
                        cbo.GetAncestor<ItemViewer>().Controller.RightKeyActions
                    );
                    break;
                }
                case Keys.Left:
                {
                    // #351: the breadcrumb owns Left-arrow collapse via the JS bridge; for a
                    // remaining ComboBox sender only the legacy close-the-dropdown branch applies.
                    UiThread.Dispatcher.Invoke(() => cbo.DroppedDown = false);
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    break;
                }
                case Keys k when (k == Keys.Return || k == Keys.Escape):
                {
                    // Close the drop down box
                    UiThread.Dispatcher.Invoke(() => cbo.DroppedDown = false);
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    break;
                }
                //case Keys.Return:
                //    {
                //        UIThreadExtensions.UiDispatcher.Invoke(() => cbo.DroppedDown = false);
                //        e.SuppressKeyPress = true;
                //        e.Handled = true;
                //        break;
                //    }
                default:
                {
                    // Pass on the instruction to the normal handler
                    await KeyDownTaskAsync((object)cbo, e);
                    break;
                }
            }
        }

        public async Task DdClosed_KeyDownAsync(ComboBox cbo, KeyEventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            switch (e.KeyCode)
            {
                case Keys.Right:
                {
                    await UiThread.Dispatcher.InvokeAsync(() => cbo.DroppedDown = true);
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    break;
                }
                default:
                {
                    await KeyDownTaskAsync((object)cbo, e);
                    break;
                }
            }
        }
    }
}
