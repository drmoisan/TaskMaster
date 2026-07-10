using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using UtilitiesCS;

namespace Tags
{
    /// <summary>
    /// Rendering and keyboard-navigation partial of <see cref="TagController"/>. Builds the option
    /// <see cref="CheckBox"/> rows against the <see cref="ITagViewer"/> option-panel abstraction and
    /// routes the focus rectangle through the injectable <c>_drawFocus</c> seam so the navigation
    /// arithmetic is testable without an HWND. The <see cref="DrawFocusDefault"/> body (register E2,
    /// removed) is intentionally NOT excluded from coverage; it is exercised by the dedicated STA
    /// test <c>Tags.Test/TagControllerRendering.StaTests.cs</c>.
    /// </summary>
    public partial class TagController
    {
        public bool LoadControls(SortedDictionary<string, bool> dictOptions, string prefix) //internal
        {
            CheckBox ctrlCB;
            string strChkName;
            CheckBoxController clsCheckBox;

            _model.FilteredOptions = dictOptions;
            intFocus = -1;
            _colCbxCtrl = new();
            _colCbxEvent = new();

            for (int i = 0, loopTo = _model.FilteredOptions.Count - 1; i <= loopTo; i++)
            {
                strChkName = i.ToString("00") + " ChkBx";
                ctrlCB = new CheckBox();
                var optionKey = _model.FilteredOptions.Keys.ElementAt(i);
                try
                {
                    _viewer.AddOptionControl(ctrlCB);
                }
                catch
                {
                    _prompt.ShowMessage(
                        $"Error adding {nameof(CheckBox)} in {nameof(Tags)}.{nameof(LoadControls)}"
                    );
                    return false;
                }

                ctrlCB.Name = strChkName;
                ctrlCB.Tag = optionKey;
                ctrlCB.Text =
                    prefix.Length > 0 && optionKey.StartsWith(prefix)
                        ? optionKey.Substring(prefix.Length)
                        : optionKey;
                ctrlCB.Checked = _model.FilteredOptions.Values.ElementAt(i);

                try
                {
                    clsCheckBox = new CheckBoxController();
                    clsCheckBox.Init(this, prefix);
                    clsCheckBox.CtrlCB = ctrlCB;
                }
                catch
                {
                    _prompt.ShowMessage("Error wiring checkbox event in Tags.LoadControls");
                    return false;
                }

                // ctrlCB.AutoSize = True
                ControlPosition.Set(ctrlCB, _gridTemplate, i, 0);

                try
                {
                    _colCbxCtrl.Add(ctrlCB);
                    _colCbxEvent.Add(clsCheckBox);
                }
                catch
                {
                    _prompt.ShowMessage("Error saving checkbox control and event to collection");
                    return false;
                }
            }
            return true;
        }

        public void RemoveControls() //internal
        {
            int max = _colCbxCtrl.Count - 1;
            for (int i = max; i >= 0; i -= 1)
            {
                _viewer.RemoveOptionControl(_colCbxCtrl[i]);
                _colCbxCtrl.RemoveAt(i);
                _colCbxEvent.RemoveAt(i);
            }

            max = _colColorbox.Count - 1;
            for (int i = max; i >= 0; i -= 1)
            {
                if (_colColorbox[i] is CheckBox colorBox)
                {
                    _viewer.RemoveOptionControl(colorBox);
                }
                bool unused = _colColorbox.Remove(i);
            }
        }

        public void FilterToSelected() //internal
        {
            RemoveControls();
            _model.FilterToSelectedSet();
            bool unused = LoadControls(_model.FilteredOptions, _model.Prefix.Value);
        }

        public void FocusCheckbox(CheckBox cbx) //internal
        {
            intFocus = _colCbxCtrl.IndexOf(cbx);
            Select_Ctrl_By_Offset(0);
        }

        /// <summary>
        /// Draws the focus rectangle on the supplied checkbox through the injectable seam. Tests
        /// inject a no-op; production uses <see cref="DrawFocusDefault"/>.
        /// </summary>
        private void DrawFocus(CheckBox cbx) => _drawFocus(cbx);

        /// <summary>
        /// Production default focus-draw body. Forces an HWND via <c>Graphics.FromHwnd</c> and draws
        /// the focus rectangle. Not excluded from coverage (register E2 removed); covered by the
        /// dedicated STA test against an unshown checkbox.
        /// </summary>
        private void DrawFocusDefault(CheckBox cbx)
        {
            ControlPaint.DrawFocusRectangle(
                System.Drawing.Graphics.FromHwnd(cbx.Handle),
                cbx.ClientRectangle
            );
        }

        public void Select_Ctrl_By_Offset(int increment) //internal
        {
            int newpos = intFocus + increment;
            if (newpos <= -1)
            {
                _viewer.FocusSearch();
                intFocus = -1;
            }
            else if (newpos <= _colCbxCtrl.Count - 1)
            {
                _colCbxCtrl[newpos].Focus();
                DrawFocus(_colCbxCtrl[newpos]);
                intFocus = newpos;
            }
        }

        public void Select_Last_Control() //internal
        {
            Select_Ctrl_By_Position(_colCbxCtrl.Count - 1);
        }

        public void Select_First_Control() //internal
        {
            Select_Ctrl_By_Position(0);
        }

        public void Select_PageDown() //internal
        {
            if (_viewer.OptionsScrollMaximum > _viewer.OptionsPanelHeight)
            {
                int y = _viewer.OptionsPanelHeight;
                var filteredIEnumerable = _colCbxCtrl
                    .Select((n, i) => new { Value = n, Index = i })
                    .Where(p => (p.Index > intFocus) & (p.Value.Bottom > y));

                if (filteredIEnumerable.Count() == 0)
                {
                    Select_Last_Control();
                }
                else
                {
                    int idx = filteredIEnumerable.First().Index;

                    Select_Ctrl_By_Position(idx);

                    int y_scroll = _colCbxCtrl[idx].Top - _viewer.OptionsAutoScrollPosition.Y;

                    _viewer.OptionsAutoScrollPosition = new System.Drawing.Point(
                        _viewer.OptionsAutoScrollPosition.X,
                        y_scroll
                    );
                }
            }
        }

        public void Select_PageUp() //internal
        {
            if (_viewer.OptionsScrollMaximum > _viewer.OptionsPanelHeight)
            {
                int idx_top;

                var filteredIEnumerable = _colCbxCtrl
                    .Select((n, i) => new { Value = n, Index = i })
                    .Where(p => p.Value.Top < 0);

                if (filteredIEnumerable.Count() == 0)
                {
                    Select_First_Control();
                }
                else
                {
                    idx_top = filteredIEnumerable.Last().Index;
                    Select_Ctrl_By_Position(idx_top);
                    int y_scroll =
                        (-1 * _viewer.OptionsAutoScrollPosition.Y)
                        - (_viewer.OptionsPanelHeight - _colCbxCtrl[idx_top].Height);

                    _viewer.OptionsAutoScrollPosition = new System.Drawing.Point(
                        _viewer.OptionsAutoScrollPosition.X,
                        y_scroll
                    );
                }
            }
        }

        public void Select_Ctrl_By_Position(int position) //internal
        {
            if (position < -1 | position > _colCbxCtrl.Count - 1)
            {
                throw new ArgumentOutOfRangeException(
                    "Cannot select control with postition " + position
                );
            }
            else if (position == -1)
            {
                _viewer.FocusSearch();
                intFocus = position;
            }
            else
            {
                _colCbxCtrl[position].Focus();
                DrawFocus(_colCbxCtrl[position]);
                intFocus = position;
            }
        }

        public void OptionsPanel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) //internal
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

        public void OptionsPanel_KeyDown(object sender, KeyEventArgs e) //internal
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                {
                    Select_Ctrl_By_Offset(1);
                    break;
                }
                case Keys.Up:
                {
                    Select_Ctrl_By_Offset(-1);
                    break;
                }
            }
        }

        public void TagViewer_KeyDown(object sender, KeyEventArgs e) //internal
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                {
                    ButtonOk_Action();
                    break;
                }
            }
        }

        public void SearchText_KeyDown(object sender, KeyEventArgs e) //internal
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                {
                    _cursorPosition = _viewer.SearchSelectionStart;
                    break;
                }
                case Keys.Down:
                {
                    Select_Ctrl_By_Offset(1);
                    break;
                }
            }
        }

        public void SearchText_KeyUp(object sender, KeyEventArgs e) //internal
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                {
                    if (_viewer.SearchSelectionStart == _cursorPosition)
                    {
                        FilterToSelected();
                    }

                    break;
                }
                case Keys.Enter:
                {
                    ButtonOk_Action();
                    break;
                }
            }
        }
    }
}
