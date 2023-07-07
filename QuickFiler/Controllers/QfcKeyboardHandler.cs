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

        public bool KbdActive { get => _kbdActive; set => _kbdActive = value; }

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

        public void KeyboardHandler_KeyDown(object sender, KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void KeyboardHandler_KeyPress(object sender, KeyPressEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void KeyboardHandler_KeyUp(object sender, KeyEventArgs e)
        {
            throw new NotImplementedException();
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
            if (_kbdActive) { _parent.FrmCtrlr.Groups.ToggleOffTips(); }
            else { _parent.FrmCtrlr.Groups.ToggleOnTips(); }
            _kbdActive = !_kbdActive;
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
