using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using Tags;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.OutlookExtensions;

namespace TaskVisualization
{
    public partial class TaskController
    {
        // Accelerator-init half of Initialize: deactivates accelerator controls/nav tips,
        // applies option-driven activation, and wires KeyPress handlers. Measured via the STA
        // accelerator tests; the only Form-bound residue is isolated in WireKeyPressHandlers.
        private void InitializeAccelerators()
        {
            // Deactivate accelerator controls
            NavTips.ForEach(x => x.ToggleColumnOnly(Enums.ToggleState.Off));
            ToggleXl(
                (from x in _xlCtrlLookup select x).ToDictionary(x => x.Key, x => 'A'),
                Enums.ToggleState.Off
            );

            // Deactivate controls that are not set in _options
            if (_options != Enums.FlagsToSet.All)
                ActivateOptions();

            // Wire keypress event handler
            WireKeyPressHandlers();
        }

        // Attaches KeyboardHandler_KeyPress to every KeyPress-capable control. Form-bound
        // residue (walks the live control tree): exempt and guarded on the concrete type so
        // pump-less STA tests (non-Form ITaskViewer) skip it; production _viewer is a TaskViewer.
        [ExcludeFromCodeCoverage]
        private void WireKeyPressHandlers()
        {
            if (_viewer is TaskViewer form)
            {
                form.ForAllControls(control =>
                {
                    if (control.GetType().GetEvent("KeyPress") is not null)
                    {
                        control.KeyPress += KeyboardHandler_KeyPress;
                    }
                });
            }
        }

        [ExcludeFromCodeCoverage]
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool PostMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        public const int WM_LBUTTONDOWN = 0x201;

        public void MouseFilter_FormClicked(object sender, EventArgs e)
        {
            if (_altActive)
            {
                _altActive = false;
                ToggleXl(_xlCtrlsActive, Enums.ToggleState.Off);
            }
        }

        public bool KeyboardHandler_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt)
            {
                if (_altActive)
                {
                    ToggleXlGroupNav(Enums.ToggleState.Off);
                    if (_xlCtrlsActive is not null)
                    {
                        (_xlCtrlsActive, _altActive, _altLevel) = RecurseXl(
                            _xlCtrlsActive,
                            _altActive,
                            '\0',
                            _altLevel
                        );
                    }
                    _altActive = false;
                    //_activeNavGroup = -1;
                    return true;
                }
                else
                {
                    ToggleXlGroupNav(Enums.ToggleState.On);
                    if (_activeNavGroup != -1)
                    {
                        var groupNumber = _activeNavGroup;
                        _activeNavGroup = -1;
                        (_xlCtrlsActive, _altActive, _altLevel) = ActivateXlGroup(
                            groupNumber.ToString()[0]
                        );
                    }
                    _altActive = true;
                    return true;
                }
            }
            else if (_altActive)
            {
                if (e.KeyCode >= Keys.A & e.KeyCode <= Keys.Z)
                {
                    e.SuppressKeyPress = true;
                    if (_xlCtrlsActive is not null)
                        (_xlCtrlsActive, _altActive, _altLevel) = RecurseXl(
                            _xlCtrlsActive,
                            _altActive,
                            e.KeyCode.ToString().ToUpper()[0],
                            _altLevel
                        );
                    return true;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    if (_activeNavGroup == -1)
                    {
                        (_xlCtrlsActive, _altActive, _altLevel) = ActivateXlGroup('1', 1);
                        return true;
                    }
                    else if (_activeNavGroup < (_xlCtrlsNav.Count))
                    {
                        (_xlCtrlsActive, _altActive, _altLevel) = ActivateXlGroup(
                            _activeNavGroup + 1
                        );
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (e.KeyCode == Keys.Up)
                {
                    if (_activeNavGroup == -1)
                    {
                        (_xlCtrlsActive, _altActive, _altLevel) = ActivateXlGroup(
                            _xlCtrlsNav.Count
                        );
                        return true;
                    }
                    else if (_activeNavGroup > 1)
                    {
                        (_xlCtrlsActive, _altActive, _altLevel) = ActivateXlGroup(
                            _activeNavGroup - 1
                        );
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void KeyboardHandler_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (_altActive)
            {
                string key = e.KeyChar.ToString();
                int.TryParse(key, out int digit);
                if (digit > 0 && digit <= 9)
                {
                    (_xlCtrlsActive, _altActive, _altLevel) = ActivateXlGroup(key[0], digit);
                    e.Handled = true;
                }
            }
        }

        public bool SuppressKeystrokes
        {
            get { return _altActive; }
        }

        private void ToggleXl(Dictionary<Label, char> dictLabels, Enums.ToggleState desiredState)
        {
            switch (desiredState)
            {
                case Enums.ToggleState.On:
                {
                    foreach (var row in dictLabels)
                        row.Key.Visible = true;
                    break;
                }

                case Enums.ToggleState.Off:
                {
                    foreach (var row in dictLabels)
                        row.Key.Visible = false;
                    break;
                }
                default:
                {
                    foreach (var row in dictLabels)
                        row.Key.Visible = !row.Key.Visible;
                    break;
                }
            }
        }

        private void UpdateCaptions(Dictionary<Label, char> dictLabels)
        {
            foreach (var row in dictLabels)
                row.Key.Text = row.Value.ToString();
        }

        private void ExecuteXlAction(Label lbl)
        {
            var ctrl = _xlCtrlLookup[lbl];
            if (ctrl is Button)
            {
                Button btn = ctrl as Button;
                btn.PerformClick();
            }
            else if (ctrl is CheckBox)
            {
                CheckBox checkBox = ctrl as CheckBox;
                checkBox.Checked = !checkBox.Checked;
            }
            else if (ctrl is TextBox)
            {
                FocusTextBox((TextBox)ctrl);
            }
            else if (ctrl is ComboBox)
            {
                FocusComboBox((ComboBox)ctrl);
            }
            else if (ctrl is DateTimePicker)
            {
                DispatchDateTimePickerClick((DateTimePicker)ctrl);
            }
            else if (ctrl is Label)
            {
                if (lbl.Equals(ViewerControls.XlProject))
                {
                    AssignProject();
                }
                else if (lbl.Equals(ViewerControls.XlPeople))
                {
                    AssignPeople();
                }
                else if (lbl.Equals(ViewerControls.XlTopic))
                {
                    AssignTopic();
                }
                else if (lbl.Equals(ViewerControls.XlContext))
                {
                    AssignContext();
                }
                else
                {
                    throw new ArgumentException(
                        "lbl not assigned properly to control",
                        nameof(lbl)
                    );
                }
            }
            else
            {
                throw new ArgumentException("lbl not assigned properly to control", nameof(lbl));
            }
        }

        // Exempt: TextBox.Select / caret positioning require input focus on a live handled control.
        [ExcludeFromCodeCoverage]
        private static void FocusTextBox(TextBox txt)
        {
            txt.Select();
            txt.SelectionStart = txt.Text.Length;
        }

        // Exempt: ComboBox.Select / DroppedDown require input focus and a live window handle.
        [ExcludeFromCodeCoverage]
        private static void FocusComboBox(ComboBox combo)
        {
            combo.Select();
            combo.DroppedDown = true;
        }

        // Exempt: requires the picker's live window Handle and the Windows message pump
        // (PostMessage posts to a live hWnd), neither available for a never-shown STA control.
        [ExcludeFromCodeCoverage]
        private void DispatchDateTimePickerClick(DateTimePicker dt)
        {
            int x = dt.Width - 10;
            int y = (int)Math.Round(dt.Height / 2d);
            int lParam = x + y * 0x10000;
            bool unused = PostMessage(dt.Handle, WM_LBUTTONDOWN, 1, lParam);
        }

        internal void ToggleXlGroupNav(Enums.ToggleState desiredState)
        {
            _navTips
                .Where(tip => tip.GroupNumber == 0)
                .ForEach(tip => tip.Toggle(desiredState, true));
        }

        internal (
            Dictionary<Label, char> dictActive,
            bool altActive,
            int level
        ) DeactivateActiveXlGroup()
        {
            if (_xlCtrlsActive is not null)
            {
                ToggleXl(_xlCtrlsActive, Enums.ToggleState.Off);
            }
            if (_activeNavGroup != -1)
            {
                var tips = NavTips.Where(x => x.GroupNumber == _activeNavGroup);
                tips.ForEach(x => x.ToggleColumnOnly(Enums.ToggleState.Off));
                tips.ElementAt(0).TLP.BackColor = SystemColors.Control;
                //_activeNavGroup = -1;
            }
            return (null, true, 0);
        }

        internal (Dictionary<Label, char> dictActive, bool altActive, int level) ActivateXlGroup(
            char selectedChar,
            int groupNumber
        )
        {
            if (
                (groupNumber != _activeNavGroup)
                && (groupNumber >= 1)
                && (groupNumber <= _xlCtrlsNav.Count)
            )
            {
                DeactivateActiveXlGroup();

                var captionLookup = GetCaptionLookup(groupNumber);
                var dictActivate = (
                    from controlCaption in captionLookup
                    where _xlCtrlOptions[controlCaption.Key]
                    select controlCaption
                ).ToDictionary(
                    controlCaption => controlCaption.Key,
                    controlCaption => controlCaption.Value[0]
                );
                if (dictActivate.Count == 0)
                {
                    return (null, true, 0);
                }
                else
                {
                    var tips = NavTips.Where(x => x.GroupNumber == groupNumber);
                    tips.ForEach(x => x.ToggleColumnOnly(Enums.ToggleState.On));
                    tips.ElementAt(0).TLP.BackColor = Color.LightCyan;
                    ToggleXl(dictActivate, Enums.ToggleState.On);
                    UpdateCaptions(dictActivate);
                    _activeNavGroup = groupNumber;
                    return (dictActivate, true, 1);
                }
            }
            else
            {
                return (_xlCtrlsActive, _altActive, _altLevel);
            }
        }

        internal (Dictionary<Label, char> dictActive, bool altActive, int level) ActivateXlGroup(
            char selectedChar
        )
        {
            int.TryParse(selectedChar.ToString(), out int groupNumber);
            if (groupNumber != 0)
            {
                return ActivateXlGroup(selectedChar, groupNumber);
            }
            else
            {
                return (null, true, 0);
            }
        }

        internal (Dictionary<Label, char> dictActive, bool altActive, int level) ActivateXlGroup(
            int groupNumber
        )
        {
            if (groupNumber != 0)
            {
                return ActivateXlGroup(groupNumber.ToString()[0], groupNumber);
            }
            else
            {
                return (null, true, 0);
            }
        }

        internal (Dictionary<Label, char> dictActive, bool altActive, int level) RecurseXl(
            Dictionary<Label, char> dictSeed,
            bool altActive,
            char selectedChar,
            int level
        )
        {
            Dictionary<Label, char> dictDeactivate;
            Dictionary<Label, char> dictActivate;

            if (!altActive)
            {
                dictActivate = (
                    from x in _xlCtrlCaptions
                    where _xlCtrlOptions[x.Key]
                    select x
                ).ToDictionary(x => x.Key, x => char.ToUpper(x.Value[0]));

                ToggleXl(dictActivate, Enums.ToggleState.On);
                UpdateCaptions(dictActivate);

                return (dictActivate, true, 1);
            }
            else if (dictSeed is null)
            {
                // Ensure that dictSeed is assigned. Alt key should not be
                // active if there is no seed value
                throw new ArgumentNullException(nameof(dictSeed));
            }
            else if (selectedChar == '\0')
            {
                // Empty character is only passed if Alt key is pressed again.
                // In this case, we should deactivate the accelerator dialogue

                DeactivateActiveXlGroup();
                ToggleXlGroupNav(Enums.ToggleState.Off);
                return (null, false, 0);
            }
            else
            {
                // Get accelerator labels that match the key stroke
                dictActivate = (
                    from x in dictSeed
                    where x.Value == selectedChar
                    select x
                ).ToDictionary(x => x.Key, x => char.ToUpper(_xlCtrlCaptions[x.Key][level]));

                switch (dictActivate.Count)
                {
                    case 0:
                    {
                        // If character doesn't match, ignore it
                        return (dictSeed, true, 0);
                    }

                    case 1:
                    {
                        // If only 1 element, we have found a match.

                        // Turn off all remaining accelerator labels, including the match
                        DeactivateActiveXlGroup();
                        ToggleXlGroupNav(Enums.ToggleState.Off);
                        //ToggleXl(dictSeed, Enums.ToggleState.Off);

                        // Execute the designated action for the control
                        ExecuteXlAction(dictActivate.First().Key);

                        // Return values to reset the seed values
                        return (null, false, 0);
                    }

                    default:
                    {
                        // If more than 1 element, we need to keep searching letters

                        // Get controls to deactivate
                        dictDeactivate = (
                            from x in dictSeed
                            where x.Value != selectedChar
                            select x
                        ).ToDictionary(x => x.Key, x => x.Value);
                        ToggleXl(dictDeactivate, Enums.ToggleState.Off);
                        UpdateCaptions(dictActivate);

                        // Return values to seed the next recursion
                        return (dictActivate, true, level + 1);
                    }
                }
            }
        }
    }
}
