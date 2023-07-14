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
        public QfcKeyboardHandler(QfcFormViewer viewer, IQfcHomeController parent) 
        { 
            _viewer = viewer;
            _viewer.SetKeyboardHandler(this);
            _parent = parent;
        }
        
        private QfcFormViewer _viewer;
        private IQfcHomeController _parent;
        private bool _kbdActive = false;
        private Dictionary<char, Action<char>> _kdCharActions = null;
        private Dictionary<Keys, Action<Keys>> _kdSpecActions = null;
        private Dictionary<char, System.Action> _kuCharActions = null;
        private Dictionary<Keys, System.Action> _kuSpecActions = null;
        private Dictionary<char, System.Action> _kprsCharActions = null;
        private Dictionary<Keys, System.Action> _kprsSpecActions = null;

        public Dictionary<char, Action<char>> KdCharActions { get => _kdCharActions; set => _kdCharActions = value; }
        public Dictionary<Keys, Action<Keys>> KdKeyActions { get => _kdSpecActions; set => _kdSpecActions = value;}
        public Dictionary<char, System.Action> KuCharActions { get => _kuCharActions; set => _kuCharActions = value; }
        public Dictionary<Keys, System.Action> KuKeyActions { get => _kuSpecActions; set => _kuSpecActions = value; }
        public Dictionary<char, System.Action> KprsCharActions { get => _kprsCharActions; set => _kprsCharActions = value; }
        public Dictionary<Keys, System.Action> KprsKeyActions { get => _kprsSpecActions; set => _kprsSpecActions = value; }

        public bool KbdActive 
        { 
            get => _kbdActive;
            set 
            { 
                _kbdActive = value;
                //if (value is false) 
                //{ 
                //    ActionsChar = null;
                //    ActionsKey = null;
                //}
            }
        }

        public void KeyboardDialog_Change()
        {
            throw new NotImplementedException();
        }

        public void KeyboardDialog_KeyDown(object sender, KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void KeyboardDialog_KeyUp(object sender, KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void KeyboardHandler_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (KbdActive && (KdKeyActions != null) && KdKeyActions.ContainsKey(e.KeyCode))
            {
                e.IsInputKey = true;
            }
            //switch (e.KeyCode)
            //{
            //    case Keys.Down:
            //        e.IsInputKey = true;
            //        break;
            //    case Keys.Up:
            //        e.IsInputKey = true;
            //        break;
            //    case Keys.Left:
            //        e.IsInputKey = true;
            //        break;
            //    case Keys.Right:
            //        e.IsInputKey = true;
            //        break;
            //}
        }

        public void KeyboardHandler_KeyDown(object sender, KeyEventArgs e)
        {
            if (KbdActive)
            {
                if ((KdKeyActions != null) && KdKeyActions.ContainsKey(e.KeyCode))
                {
                    e.SuppressKeyPress = true;
                    KdKeyActions[e.KeyCode].DynamicInvoke(e.KeyCode);
                    e.Handled = true;
                }
                else if ((KdCharActions != null) && KdCharActions.ContainsKey((char)e.KeyValue))
                {
                    e.SuppressKeyPress = true;
                    KdCharActions[(char)e.KeyValue].DynamicInvoke((char)e.KeyValue);
                    e.Handled = true;
                }   
            }
        }

        public void KeyboardHandler_KeyPress(object sender, KeyPressEventArgs e)
        {
            //throw new NotImplementedException();
        }

        public void KeyboardHandler_KeyUp(object sender, KeyEventArgs e)
        {
            //throw new NotImplementedException();
        }

        public void PanelMain_KeyDown(object sender, KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void PanelMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void PanelMain_KeyUp(object sender, KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void ResetAcceleratorSilently()
        {
            throw new NotImplementedException();
        }

        public void ToggleKeyboardDialog()
        {
            if (_kbdActive) { _parent.FrmCtrlr.Groups.ToggleOffNavigation(); }
            else { _parent.FrmCtrlr.Groups.ToggleOnNavigation(); }
            _kbdActive = !_kbdActive;
        }
        
        public void ToggleKeyboardDialog(object sender, KeyEventArgs e)
        {
            ToggleKeyboardDialog();
            e.Handled = true;
        }

        public bool ToggleOffActiveItem(bool parentBlExpanded)
        {
            throw new NotImplementedException();
        }

        public void ToggleRemoteMouseLabels()
        {
            throw new NotImplementedException();
        }
    }
}
