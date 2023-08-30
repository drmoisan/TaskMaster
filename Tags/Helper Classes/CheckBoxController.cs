using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Tags
{

    public class CheckBoxController
    {       
        
        public bool TrigByKeyChg;
        private bool TrigByValChg;
        private TagController _parent;
        private string strTagPrefix;
        private string strTemp;

        public CheckBoxController() { }

        public CheckBoxController(CheckBox checkBox)
        {
            CtrlCB = checkBox;
        }

        internal object Init(TagController parent, string strPrefix)
        {
            _parent = parent;
            strTagPrefix = strPrefix;
            return true;
        }

        private CheckBox _ctrlCB;
        public virtual CheckBox CtrlCB
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _ctrlCB;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_ctrlCB != null)
                {
                    _ctrlCB.Click -= ctrlCB_Click;
                    _ctrlCB.KeyDown -= ctrlCB_KeyDown;
                    _ctrlCB.GotFocus -= ctrlCB_GotFocus;
                    _ctrlCB.LostFocus -= ctrlCB_LostFocus;
                    _ctrlCB.PreviewKeyDown -= ctrlCB_PreviewKeyDown;
                }

                _ctrlCB = value;
                if (_ctrlCB != null)
                {
                    _ctrlCB.Click += ctrlCB_Click;
                    _ctrlCB.KeyDown += ctrlCB_KeyDown;
                    _ctrlCB.GotFocus += ctrlCB_GotFocus;
                    _ctrlCB.LostFocus += ctrlCB_LostFocus;
                    _ctrlCB.PreviewKeyDown += ctrlCB_PreviewKeyDown;
                }
            }
        }

        private void ctrlCB_Click(object sender, EventArgs e)
        {
            if (!TrigByKeyChg)
            {
                strTemp = strTagPrefix + CtrlCB.Text;
                _parent.ToggleChoice(strTemp);
                _parent.FocusCheckbox(CtrlCB);
            }
            else if (TrigByValChg)
            {
                TrigByKeyChg = false;
                TrigByValChg = false;
            }
            else
            {
                TrigByValChg = true;
                CtrlCB.Checked = !CtrlCB.Checked;
            }
            // Me.ctrlCB.Value = Not Me.ctrlCB.Value
        }

        private void ctrlCB_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    {
                        _parent.Select_Ctrl_By_Offset(1);
                        break;
                    }

                case Keys.Up:
                    {
                        _parent.Select_Ctrl_By_Offset(-1);
                        break;
                    }

                case Keys.End:
                    {
                        _parent.Select_Last_Control();
                        break;
                    }

                case Keys.Home:
                    {
                        _parent.Select_First_Control();
                        break;
                    }

                case Keys.PageDown:
                    {
                        _parent.Select_PageDown();
                        break;
                    }

                case Keys.PageUp:
                    {
                        _parent.Select_PageUp();
                        break;
                    }

                case Keys.Enter:
                    {
                        _parent.ButtonOk_Action();
                        break;
                    }
            }
        }

        private void ctrlCB_GotFocus(object sender, EventArgs e)
        {
            Control ctrl = sender as Control;
            var tmp_color = ctrl.BackColor;
            ctrl.BackColor = ctrl.ForeColor;
            ctrl.ForeColor = tmp_color;
        }

        private void ctrlCB_LostFocus(object sender, EventArgs e)
        {
            Control ctrl = sender as Control;
            var tmp_color = ctrl.BackColor;
            ctrl.BackColor = ctrl.ForeColor;
            ctrl.ForeColor = tmp_color;
        }

        private void ctrlCB_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    {
                        e.IsInputKey = true;
                        break;
                    }
                case Keys.Up:
                    {
                        e.IsInputKey = true;
                        break;
                    }
            }
        }
    }
}