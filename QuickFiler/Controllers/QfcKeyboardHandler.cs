using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS;

namespace QuickFiler.Controllers
{
    internal class QfcKeyboardHandler : IQfcKeyboardHandler
    {
        public QfcKeyboardHandler(QfcFormViewer viewer, IFilerHomeController parent) 
        { 
            viewer.SetKeyboardHandler(this);
            _parent = parent;
        }

        public QfcKeyboardHandler(EfcViewer viewer, IFilerHomeController parent)
        {
            viewer.SetKeyboardHandler(this);
            _parent = parent;
        }

        private IFilerHomeController _parent;
        private bool _kbdActive = false;
        private Dictionary<char, Action<char>> _kdCharActions = null;
        private Dictionary<Keys, Action<Keys>> _kdSpecActions = null;

        public Dictionary<char, Action<char>> KdCharActions { get => _kdCharActions; set => _kdCharActions = value; }
        public Dictionary<Keys, Action<Keys>> KdKeyActions { get => _kdSpecActions; set => _kdSpecActions = value;}

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
            if (KbdActive && (KdKeyActions != null) && KdKeyActions.ContainsKey(e.KeyCode))
            {
                e.IsInputKey = true;
            }
        }

        public void KeyboardHandler_KeyDown(object sender, KeyEventArgs e)
        {
            if (KbdActive)
            {
                if ((KdKeyActions != null) && KdKeyActions.ContainsKey(e.KeyCode))
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    KdKeyActions[e.KeyCode].DynamicInvoke(e.KeyCode);
                }
                else if ((KdCharActions != null) && KdCharActions.ContainsKey((char)e.KeyValue))
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    KdCharActions[(char)e.KeyValue].DynamicInvoke((char)e.KeyValue);
                }   
            }
        }

        public void ToggleKeyboardDialog()
        {
            if (_kbdActive) { _parent.FormCtrlr.ToggleOffNavigation(async: false); }
            else { _parent.FormCtrlr.ToggleOnNavigation(async: false); }
            _kbdActive = !_kbdActive;
        }
        
        public void ToggleKeyboardDialog(object sender, KeyEventArgs e)
        {
            ToggleKeyboardDialog();
            e.Handled = true;
        }

        internal QfcItemViewer GetItemViewer(Control control)
        {
            if (control as QfcItemViewer != null) { return (control as QfcItemViewer); }
            else if (control.Parent != null) { return GetItemViewer(control.Parent); }
            else { return null; }
        }

        private List<Keys> _cboKeys = new List<Keys> { Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.Escape, Keys.Return };
        
        public void CboFolders_KeyDown(object sender, KeyEventArgs e)
        {
            QfcItemViewer viewer = null;
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
    }
}
