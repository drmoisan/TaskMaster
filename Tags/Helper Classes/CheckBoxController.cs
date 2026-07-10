using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Tags
{
    /// <summary>
    /// Wires an option <see cref="CheckBox"/> to its parent <see cref="TagController"/>. The click
    /// state-machine decision is extracted into the host-neutral <see cref="DecideClick"/> helper
    /// (register E6, narrowed): the <see cref="CtrlCB"/> subscribe/unsubscribe wiring, the
    /// <see cref="ctrlCB_Click"/> wrapper, and <see cref="DecideClick"/> are NOT exempt from coverage.
    /// Only the four members that require a shown window, a real focus transition, or protected-method
    /// access (<see cref="ctrlCB_GotFocus"/>, <see cref="ctrlCB_LostFocus"/>, <see cref="ctrlCB_KeyDown"/>,
    /// <see cref="ctrlCB_PreviewKeyDown"/>) retain <see cref="ExcludeFromCodeCoverageAttribute"/>.
    /// </summary>
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
            get { return _ctrlCB; }
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

        /// <summary>The action a checkbox click resolves to under the trigger state machine.</summary>
        internal enum CheckBoxClickAction
        {
            Toggle,
            ResetFlags,
            FlipCheck,
        }

        /// <summary>
        /// Host-neutral outcome of <see cref="DecideClick"/>: the resolved action, the toggled choice
        /// key (for <see cref="CheckBoxClickAction.Toggle"/>), and the next trigger-flag values.
        /// </summary>
        internal readonly struct CheckBoxClickDecision
        {
            public CheckBoxClickDecision(
                CheckBoxClickAction action,
                string resolvedChoice,
                bool nextTrigByKeyChg,
                bool nextTrigByValChg
            )
            {
                Action = action;
                ResolvedChoice = resolvedChoice;
                NextTrigByKeyChg = nextTrigByKeyChg;
                NextTrigByValChg = nextTrigByValChg;
            }

            public CheckBoxClickAction Action { get; }
            public string ResolvedChoice { get; }
            public bool NextTrigByKeyChg { get; }
            public bool NextTrigByValChg { get; }
        }

        /// <summary>
        /// Pure decision for a checkbox click: given the current trigger flags and the checkbox's Tag,
        /// Text and prefix, returns the action to take and the resolved choice key. No WinForms state
        /// is read or written here, which makes the state machine unit-testable.
        /// </summary>
        internal static CheckBoxClickDecision DecideClick(
            bool trigByKeyChg,
            bool trigByValChg,
            string tag,
            string text,
            string prefix
        )
        {
            if (!trigByKeyChg)
            {
                string resolvedChoice = string.IsNullOrEmpty(tag) ? prefix + text : tag;
                return new CheckBoxClickDecision(
                    CheckBoxClickAction.Toggle,
                    resolvedChoice,
                    trigByKeyChg,
                    trigByValChg
                );
            }
            else if (trigByValChg)
            {
                return new CheckBoxClickDecision(
                    CheckBoxClickAction.ResetFlags,
                    null,
                    false,
                    false
                );
            }
            else
            {
                return new CheckBoxClickDecision(
                    CheckBoxClickAction.FlipCheck,
                    null,
                    trigByKeyChg,
                    true
                );
            }
        }

        private void ctrlCB_Click(object sender, EventArgs e)
        {
            var decision = DecideClick(
                TrigByKeyChg,
                TrigByValChg,
                CtrlCB.Tag as string,
                CtrlCB.Text,
                strTagPrefix
            );

            switch (decision.Action)
            {
                case CheckBoxClickAction.Toggle:
                    strTemp = decision.ResolvedChoice;
                    _parent.ToggleChoice(strTemp);
                    _parent.FocusCheckbox(CtrlCB);
                    break;
                case CheckBoxClickAction.ResetFlags:
                    TrigByKeyChg = decision.NextTrigByKeyChg;
                    TrigByValChg = decision.NextTrigByValChg;
                    break;
                case CheckBoxClickAction.FlipCheck:
                    TrigByValChg = decision.NextTrigByValChg;
                    CtrlCB.Checked = !CtrlCB.Checked;
                    break;
            }
        }

        [ExcludeFromCodeCoverage]
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

        [ExcludeFromCodeCoverage]
        private void ctrlCB_GotFocus(object sender, EventArgs e)
        {
            Control ctrl = sender as Control;
            var tmp_color = ctrl.BackColor;
            ctrl.BackColor = ctrl.ForeColor;
            ctrl.ForeColor = tmp_color;
        }

        [ExcludeFromCodeCoverage]
        private void ctrlCB_LostFocus(object sender, EventArgs e)
        {
            Control ctrl = sender as Control;
            var tmp_color = ctrl.BackColor;
            ctrl.BackColor = ctrl.ForeColor;
            ctrl.ForeColor = tmp_color;
        }

        [ExcludeFromCodeCoverage]
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
