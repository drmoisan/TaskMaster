using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UtilitiesCS;

namespace Tags
{
    /// <summary>
    /// WinForms realization of <see cref="ITagViewer"/>. Register E3 (exempt category b): a
    /// <see cref="Form"/>-derived class whose intent members are thin 1:1 mappings onto the
    /// designer controls, with no decision logic to unit-test. The exemption is applied to this
    /// single partial part so it also covers <c>TagViewer.Designer.cs</c> without a duplicate
    /// attribute.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class TagViewer : Form, ITagViewer
    {
        private TagController _controller;

        public TagViewer()
        {
            // This call is required by the designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.
            KeyPreview = false;
        }

        public void SetController(TagController controller)
        {
            _controller = controller;
        }

        // Command intent events forward to the corresponding designer-control events.
        public event EventHandler OkClicked
        {
            add => ButtonOk.Click += value;
            remove => ButtonOk.Click -= value;
        }

        public event EventHandler CancelClicked
        {
            add => ButtonCancel.Click += value;
            remove => ButtonCancel.Click -= value;
        }

        public event EventHandler NewClicked
        {
            add => ButtonNew.Click += value;
            remove => ButtonNew.Click -= value;
        }

        public event EventHandler AutoAssignClicked
        {
            add => ButtonAutoAssign.Click += value;
            remove => ButtonAutoAssign.Click -= value;
        }

        public event EventHandler SearchTextChanged
        {
            add => SearchText.TextChanged += value;
            remove => SearchText.TextChanged -= value;
        }

        public event KeyEventHandler SearchKeyDown
        {
            add => SearchText.KeyDown += value;
            remove => SearchText.KeyDown -= value;
        }

        public event KeyEventHandler SearchKeyUp
        {
            add => SearchText.KeyUp += value;
            remove => SearchText.KeyUp -= value;
        }

        public event EventHandler HideArchiveChanged
        {
            add => HideArchive.CheckedChanged += value;
            remove => HideArchive.CheckedChanged -= value;
        }

        public event KeyEventHandler ViewKeyDown
        {
            add => KeyDown += value;
            remove => KeyDown -= value;
        }

        public event PreviewKeyDownEventHandler OptionsPreviewKeyDown
        {
            add => L1v2L2_OptionsPanel.PreviewKeyDown += value;
            remove => L1v2L2_OptionsPanel.PreviewKeyDown -= value;
        }

        public event KeyEventHandler OptionsKeyDown
        {
            add => L1v2L2_OptionsPanel.KeyDown += value;
            remove => L1v2L2_OptionsPanel.KeyDown -= value;
        }

        // State intent properties map onto the designer controls.
        public bool HideArchiveChecked => HideArchive.Checked;

        public bool AutoAssignVisible
        {
            get => ButtonAutoAssign.Visible;
            set => ButtonAutoAssign.Visible = value;
        }

        public bool AutoAssignEnabled
        {
            get => ButtonAutoAssign.Enabled;
            set => ButtonAutoAssign.Enabled = value;
        }

        public bool ButtonNewVisible
        {
            get => ButtonNew.Visible;
            set => ButtonNew.Visible = value;
        }

        public string SearchTextValue
        {
            get => SearchText.Text;
            set => SearchText.Text = value;
        }

        public int SearchSelectionStart => SearchText.SelectionStart;

        public string Caption
        {
            get => Text;
            set => Text = value;
        }

        // Intent methods and option-panel abstraction.
        public ControlPosition CaptureAndRemoveTemplate()
        {
            var cp = ControlPosition.CreateTemplate(TemplateCheckBox);
            L1v2L2_OptionsPanel.Controls.Remove(TemplateCheckBox);
            return cp;
        }

        public void FocusSearch() => SearchText.Focus();

        public void AddOptionControl(CheckBox control) => L1v2L2_OptionsPanel.Controls.Add(control);

        public void RemoveOptionControl(CheckBox control) =>
            L1v2L2_OptionsPanel.Controls.Remove(control);

        public IReadOnlyList<CheckBox> OptionControls =>
            L1v2L2_OptionsPanel.Controls.OfType<CheckBox>().ToList();

        public int OptionsPanelHeight => L1v2L2_OptionsPanel.Height;

        public int OptionsScrollMaximum => L1v2L2_OptionsPanel.VerticalScroll.Maximum;

        public Point OptionsAutoScrollPosition
        {
            get => L1v2L2_OptionsPanel.AutoScrollPosition;
            set => L1v2L2_OptionsPanel.AutoScrollPosition = value;
        }
    }
}
