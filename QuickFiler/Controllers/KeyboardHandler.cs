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
        public KeyboardHandler(QfcFormViewer viewer, IFilerHomeController parent)
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

        private KbdActions<Keys, KaKeyAsync, Func<Keys, Task>> _keyActionsAsync = [];
        public KbdActions<Keys, KaKeyAsync, Func<Keys, Task>> KeyActionsAsync { get => _keyActionsAsync; set => _keyActionsAsync = value; }

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
            await KeyDownTaskAsync(sender, e);
        }

        public async Task KeyDownTaskAsync(object sender, KeyEventArgs e)
        {
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
            await UIThreadExtensions.UiDispatcher.InvokeAsync(() =>
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
                        UIThreadExtensions.UiDispatcher.Invoke(() => cbo.DroppedDown = false);
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
            switch (e.KeyCode)
            {
                case Keys.Right:
                    {
                        await UIThreadExtensions.UiDispatcher.InvokeAsync(() => cbo.DroppedDown = true);
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

    public class KbdActions<T, U, V> : IEnumerable<U> where U : IKbdAction<T,V>, new()
    {
        public KbdActions() 
        {
            _list = new ConcurrentObservableCollection<U>();
        }
        
        public KbdActions(IEnumerable<U> list)
        {
            _list = new ConcurrentObservableCollection<U>(list);
        }

        private ConcurrentObservableCollection<U> _list = new();

        public V this[T key]
        {
            get => this.Find(key).Delegate;

            set
            {
                var element = this.Find(key);
                if (element is not null)
                {
                    element.Delegate = value;
                }
            }
        }

        public bool ContainsKey(T key) => _list.Any(x => x.KeyEquals(key));

        public U Find(T key)
        {
            var matches = _list.Where(x => x.KeyEquals(key));
            var count = matches.Count();
            switch (count)
            {
                case 0:
                    return default(U);
                case 1:
                    return matches.First();
                default:
                    var message = $"Multiple sources have registered actions for Key {key}. SourceId list ";
                    message += $"[{matches.Select(x => x.SourceId).SentenceJoin()}]";
                    throw new InvalidOperationException(message);
            }
        }

        public int FindIndex(T key)
        {
            var matches = _list.Where(x => x.KeyEquals(key));
            var count = matches.Count();
            switch (count)
            {
                case 0:
                    return -1;
                case 1:
                    return _list.FindIndex(x => x.KeyEquals(key));
                default:
                    var message = $"Multiple sources have registered actions for Key {key}. SourceId list ";
                    message += $"[{matches.Select(x => x.SourceId).SentenceJoin()}]";
                    throw new InvalidOperationException(message);
            }
        }

        public void Add(string sourceId, T key, V @delegate)
        {
            if (_list.Any(x => x.SourceId == sourceId && x.KeyEquals(key)))
            {
                string message = $"Cannot add key because it already exists. Key {key} SourceId {sourceId}";
                throw new ArgumentException(message);
            }
            U instance = new();
            instance.SourceId = sourceId;
            instance.Key = key;
            instance.Delegate = @delegate;
            _list.Add(instance);
        }

        public void Add(U instance)
        {
            if (_list.Any(x => x.SourceId == instance.SourceId && x.KeyEquals(instance.Key)))
            {
                string message = $"Cannot add key because it already exists. Key {instance.Key} SourceId {instance.SourceId}";
                throw new ArgumentException(message);
            }
            _list.Add(instance);
        }

        public bool Remove(string sourceId, T key)
        {
            var index = _list.FindIndex(x => x.SourceId == sourceId && x.KeyEquals(key));
            if (index == -1) { return false; }
            else
            {
                _list.RemoveAt(index);
                return true;
            }
        }

        public IEnumerator<U> GetEnumerator() => _list.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
        
        public ICollection<T> Keys { get => _list.Select(x => x.Key).ToList(); }
    }

    public interface IKbdAction<T, U>
    {
        string SourceId { get; set; }
        T Key { get; set; }
        U Delegate { get; set; }
        bool KeyEquals(T other);
        //Type DelegateType { get; }
    }

    public class KaKey: IKbdAction<Keys, Action<Keys>>
    {
        public KaKey() { }

        public KaKey(string sourceId, Keys key, Action<Keys> action)
        {
            SourceId = sourceId;
            Key = key;
            Delegate = action;
        }
        
        private string _sourceId;
        public string SourceId { get => _sourceId; set => _sourceId = value; }
        
        private Keys _key;
        public Keys Key { get => _key; set => _key = value; }
        
        private Action<Keys> _action;
        public Action<Keys> Delegate { get => _action; set => _action = value; }

        public Type DelegateType { get => typeof(Action<Keys>); }

        public bool KeyEquals(Keys other) => Key == other;
        
    }

    public class KaKeyAsync: IKbdAction<Keys, Func<Keys, Task>>
    {
        public KaKeyAsync() { }
        
        public KaKeyAsync(string sourceId, Keys key, Func<Keys, Task> function)
        {
            SourceId = sourceId;
            Key = key;
            Delegate = function;
        }

        private string _sourceId;
        public string SourceId { get => _sourceId; set => _sourceId = value; }

        private Keys _key;
        public Keys Key { get => _key; set => _key = value; }

        private Func<Keys, Task> _function;
        public Func<Keys, Task> Delegate { get => _function; set => _function = value; }

        public bool KeyEquals(Keys other) => Key == other;
    }

    public class KaChar: IKbdAction<char, Action<char>>
    {
        public KaChar() { }

        public KaChar(string sourceId, char key, Action<char> action)
        {
            SourceId = sourceId;
            Key = key;
            Delegate = action;
        }

        private string _sourceId;
        public string SourceId { get => _sourceId; set => _sourceId = value; }

        private char _key;
        public char Key { get => _key; set => _key = value; }

        private Action<char> _action;
        public Action<char> Delegate { get => _action; set => _action = value; }

        public Type DelegateType { get => typeof(Action<Keys>); }

        public bool KeyEquals(char other) => Key == other;
    }

    public class KaCharAsync: IKbdAction<char, Func<char, Task>>
    {
        public KaCharAsync() { }

        public KaCharAsync(string sourceId, char key, Func<char, Task> function)
        {
            SourceId = sourceId;
            Key = key;
            Delegate = function;
        }

        private string _sourceId;
        public string SourceId { get => _sourceId; set => _sourceId = value; }

        private char _key;
        public char Key { get => _key; set => _key = value; }

        private Func<char, Task> _function;
        public Func<char, Task> Delegate { get => _function; set => _function = value; }

        public bool KeyEquals(char other) => Key == other;
    }
}
