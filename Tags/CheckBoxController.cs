using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Tags
{

    public class CheckBoxController
    {


        // By declaring Public WithEvents we can handle
        // events "collectively". In this case it is
        // the click event on a date label, and by
        // doing it this way we avoid writing click
        // events for each and every data label.
        private CheckBox _ctrlCB;

        public virtual CheckBox ctrlCB
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
                    _ctrlCB.Click -= (_, __) => ctrlCB_Click();
                    _ctrlCB.KeyDown -= ctrlCB_KeyDown;
                    _ctrlCB.GotFocus -= ctrlCB_GotFocus;
                    _ctrlCB.LostFocus -= ctrlCB_LostFocus;
                    _ctrlCB.PreviewKeyDown -= ctrlCB_PreviewKeyDown;
                }

                _ctrlCB = value;
                if (_ctrlCB != null)
                {
                    _ctrlCB.Click += (_, __) => ctrlCB_Click();
                    _ctrlCB.KeyDown += ctrlCB_KeyDown;
                    _ctrlCB.GotFocus += ctrlCB_GotFocus;
                    _ctrlCB.LostFocus += ctrlCB_LostFocus;
                    _ctrlCB.PreviewKeyDown += ctrlCB_PreviewKeyDown;
                }
            }
        }
        public bool TrigByKeyChg;
        private bool TrigByValChg;
        private TagController _parent;
        private string strTagPrefix;
        private string strTemp;

        //BUGFIX: CheckBoxController adds event to an object that is null
        public CheckBoxController()
        {
            ctrlCB.Click += (_, __) => ctrlCB_Click();
            ctrlCB.KeyDown += ctrlCB_KeyDown;
            ctrlCB.GotFocus += ctrlCB_GotFocus;
            ctrlCB.LostFocus += ctrlCB_LostFocus;
            ctrlCB.PreviewKeyDown += ctrlCB_PreviewKeyDown;
        }


        internal object Init(TagController parent, string strPrefix)
        {
            _parent = parent;
            strTagPrefix = strPrefix;
            return true;
        }

        private void ctrlCB_Click()
        {
            if (!TrigByKeyChg)
            {
                strTemp = strTagPrefix + ctrlCB.Text;
                _parent.ToggleChoice(strTemp);
                _parent.FocusCheckbox(ctrlCB);
            }
            else if (TrigByValChg)
            {
                TrigByKeyChg = false;
                TrigByValChg = false;
            }
            else
            {
                TrigByValChg = true;
                ctrlCB.Checked = !ctrlCB.Checked;
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
                        _parent.OK_Action();
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