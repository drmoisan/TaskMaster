using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses;
using Swordfish.NET.Collections;
using Microsoft.Office.Interop.Outlook;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Threading;


namespace QuickFiler.Controllers
{
    internal class KeyboardHandler : IQfcKeyboardHandler
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
        public KbdActions<char, KaChar, Action<char>> CharActions { get => _charActions; set => _charActions = value; }

        private KbdActions<char, KaCharAsync, Func<char, Task>> _charActionsAsync = [];
        public KbdActions<char, KaCharAsync, Func<char, Task>> CharActionsAsync { get => _charActionsAsync; set => _charActionsAsync = value; }

        private KbdActions<Keys, KaKey, Action<Keys>> _keyActions = [];
        public KbdActions<Keys, KaKey, Action<Keys>> KeyActions
        {
            get => _keyActions;
            set => _keyActions = value;
        }

        private KbdActions<Keys, KaKeyAsync, Func<Keys, Task>> _alwaysOnKeyActionsAsync = [];
        public KbdActions<Keys, KaKeyAsync, Func<Keys, Task>> AlwaysOnKeyActionsAsync { get => _alwaysOnKeyActionsAsync; set => _alwaysOnKeyActionsAsync = value; }

        private KbdActions<Keys, KaKeyAsync, Func<Keys, Task>> _keyActionsAsync = [];
        public KbdActions<Keys, KaKeyAsync, Func<Keys, Task>> KeyActionsAsync { get => _keyActionsAsync; set => _keyActionsAsync = value; }

        private StringBuilder _filterBuilder = new StringBuilder();
        public void ClearFilter() => _filterBuilder = new StringBuilder();

        private KbdActions<string, KaStringAsync, Func<string, Task>> _stringActionsAsync = [];
        public KbdActions<string, KaStringAsync, Func<string, Task>> StringActionsAsync { get => _stringActionsAsync; set => _stringActionsAsync = value; }

        public bool KbdActive
        {
            get => _kbdActive;
            set
            {
                _kbdActive = value;
            }
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
                logger.Error($"Error in {nameof(KeyboardHandler_KeyDownAsync)} for key {e.KeyValue}. {ex.Message}", ex);                
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
                else if ((CharActionsAsync != null) && CharActionsAsync.ContainsKey((char)e.KeyValue))
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

                        if (_filterBuilder.Length ==1)
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
            if (_kbdActive) { _parent.FormController.ToggleOffNavigation(async: false); }
            else { _parent.FormController.ToggleOnNavigation(async: false); }
            _kbdActive = !_kbdActive;
        }

        public void ToggleKeyboardDialog(object sender, KeyEventArgs e)
        {
            ToggleKeyboardDialog();
            e.Handled = true;
        }

        public async Task ToggleKeyboardDialogAsync()
        {
            if (_kbdActive) { await _parent.FormController.ToggleOffNavigationAsync(); }
            else { await _parent.FormController.ToggleOnNavigationAsync(); }
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
            if (control as ItemViewer != null) { return (control as ItemViewer); }
            else if (control.Parent != null) { return GetItemViewer(control.Parent); }
            else { return null; }
        }

        private List<Keys> _cboKeys = new List<Keys> { Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.Escape, Keys.Return };

        public void CboFolders_KeyDown(object sender, KeyEventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

            ItemViewer viewer = null;
            if (_cboKeys.Contains(e.KeyCode)) { viewer = GetItemViewer(sender as Control); }

            switch (e.KeyCode)
            {
                case Keys.Escape:
                    {
                        viewer.Controller.CounterEnter = 1;
                        viewer.Controller.CounterComboRight = 0;
                        viewer.CboFolders.DroppedDown = false;
                        e.SuppressKeyPress = true;
                        e.Handled = true;
                        break;
                    }
                case Keys.Up:
                    {
                        viewer.Controller.CounterEnter = 0;
                        break;
                    }
                case Keys.Down:
                    {
                        viewer.Controller.CounterEnter = 0;
                        break;
                    }
                case Keys.Right:
                    {
                        viewer.Controller.CounterEnter = 0;
                        switch (viewer.Controller.CounterComboRight)
                        {
                            case 0:
                                {
                                    viewer.CboFolders.DroppedDown = true;
                                    viewer.Controller.CounterComboRight++;
                                    break;
                                }
                            case 1:
                                {
                                    viewer.CboFolders.DroppedDown = false;
                                    viewer.Controller.CounterComboRight = 0;
                                    MyBox.ShowDialog("Pop Out Item or Enumerate Conversation?",
                                        "Dialog", BoxIcon.Question, viewer.Controller.RightKeyActions);
                                    break;
                                }
                            default:
                                {
                                    MessageBox.Show(
                                        "Error in intComboRightCtr ... setting to 0 and continuing",
                                        "Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                                    viewer.Controller.CounterComboRight = 0;
                                    break;
                                }
                        }
                        e.SuppressKeyPress = true;
                        e.Handled = true;
                        break;
                    }
                case Keys.Left:
                    {
                        viewer.Controller.CounterEnter = 1;
                        viewer.Controller.CounterComboRight = 0;
                        if (viewer.CboFolders.DroppedDown)
                        {
                            viewer.CboFolders.DroppedDown = false;
                            e.SuppressKeyPress = true;
                            e.Handled = true;
                        }
                        else { this.KeyboardHandler_KeyDown(sender, e); }

                        break;
                    }
                case Keys.Return:
                    {
                        if (viewer.Controller.CounterEnter == 1)
                        {
                            viewer.Controller.CounterEnter = 0;
                            viewer.Controller.CounterComboRight = 0;
                            KeyboardHandler_KeyDown(sender, e);
                        }
                        else
                        {
                            viewer.Controller.CounterEnter = 1;
                            viewer.Controller.CounterComboRight = 0;
                            viewer.CboFolders.DroppedDown = false;
                            e.SuppressKeyPress = true;
                            e.Handled = true;
                        }
                        break;
                    }
            }
        }

        public async void CboFolders_KeyDownAsyncOld(object sender, KeyEventArgs e)
        {
            await UiThread.Dispatcher.InvokeAsync(() =>
            {
                ItemViewer viewer = null;
                if (_cboKeys.Contains(e.KeyCode)) { viewer = GetItemViewer(sender as Control); }

                switch (e.KeyCode)
                {
                    case Keys.Escape:
                        {
                            viewer.Controller.CounterEnter = 1;
                            viewer.Controller.CounterComboRight = 0;
                            viewer.CboFolders.DroppedDown = false;
                            e.SuppressKeyPress = true;
                            e.Handled = true;
                            break;
                        }
                    case Keys.Up:
                        {
                            viewer.Controller.CounterEnter = 0;
                            break;
                        }
                    case Keys.Down:
                        {
                            viewer.Controller.CounterEnter = 0;
                            break;
                        }
                    case Keys.Right:
                        {
                            viewer.Controller.CounterEnter = 0;
                            switch (viewer.Controller.CounterComboRight)
                            {
                                case 0:
                                    {
                                        viewer.CboFolders.DroppedDown = true;
                                        viewer.Controller.CounterComboRight++;
                                        break;
                                    }
                                case 1:
                                    {
                                        viewer.CboFolders.DroppedDown = false;
                                        viewer.Controller.CounterComboRight = 0;
                                        MyBox.ShowDialog("Pop Out Item or Enumerate Conversation?",
                                            "Dialog", BoxIcon.Question, viewer.Controller.RightKeyActions);
                                        break;
                                    }
                                default:
                                    {
                                        MessageBox.Show(
                                            "Error in intComboRightCtr ... setting to 0 and continuing",
                                            "Error",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Error);
                                        viewer.Controller.CounterComboRight = 0;
                                        break;
                                    }
                            }
                            e.SuppressKeyPress = true;
                            e.Handled = true;
                            break;
                        }
                    case Keys.Left:
                        {
                            viewer.Controller.CounterEnter = 1;
                            viewer.Controller.CounterComboRight = 0;
                            if (viewer.CboFolders.DroppedDown)
                            {
                                viewer.CboFolders.DroppedDown = false;
                                e.SuppressKeyPress = true;
                                e.Handled = true;
                            }
                            else { this.KeyboardHandler_KeyDownAsync(sender, e); }

                            break;
                        }
                    case Keys.Return:
                        {
                            if (viewer.Controller.CounterEnter == 1)
                            {
                                viewer.Controller.CounterEnter = 0;
                                viewer.Controller.CounterComboRight = 0;
                                KeyboardHandler_KeyDownAsync(sender, e);
                            }
                            else
                            {
                                viewer.Controller.CounterEnter = 1;
                                viewer.Controller.CounterComboRight = 0;
                                viewer.CboFolders.DroppedDown = false;
                                e.SuppressKeyPress = true;
                                e.Handled = true;
                            }
                            break;
                        }
                }
            });
        }

        public async void CboFolders_KeyDownAsync(object sender, KeyEventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
            if (sender is not ComboBox)
            {
                throw new ArgumentException(
                $"{nameof(CboFolders_KeyDownAsync)} event handler can " +
                $"only be assigned to a ComboBox.  must be a ComboBox");
            }
            var cb = (ComboBox)sender;
            if (cb.DroppedDown) { await DdOpen_KeyDownAsync(cb, e); }
            else { await DdClosed_KeyDownAsync(cb, e); }
        }

        public async Task DdOpen_KeyDownAsync(ComboBox cbo, KeyEventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
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
                        var controller = cbo.GetAncestor<ItemViewer>();
                        e.SuppressKeyPress = true;
                        e.Handled = true;

                        MyBox.ShowDialog("Pop Out Item or Enumerate Conversation?",
                                         "Dialog", BoxIcon.Question,
                                         cbo.GetAncestor<ItemViewer>().Controller.RightKeyActions);
                        break;
                    }
                case Keys k when (k == Keys.Left || k == Keys.Return || k == Keys.Escape):
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
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
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
