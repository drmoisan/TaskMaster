using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS;

namespace TaskVisualization
{
    [ExcludeFromCodeCoverage]
    public partial class TaskViewer : Form, ITaskViewer, ITaskViewerControls
    {
        public TaskViewer()
        {
            InitializeComponent();

            KeyPreview = true;

            //_mouseFilter = new MouseDownFilter(this);
            //Application.AddMessageFilter(_mouseFilter);
        }

        private MouseDownFilter __mouseFilter;

        private MouseDownFilter _mouseFilter
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return __mouseFilter; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (__mouseFilter != null)
                {
                    __mouseFilter.FormClicked -= _mouseFilter_FormClicked;
                }

                __mouseFilter = value;
                if (__mouseFilter != null)
                {
                    __mouseFilter.FormClicked += _mouseFilter_FormClicked;
                }
            }
        }
        private TaskController _controller;

        public void SetController(TaskController controller)
        {
            _controller = controller;

            // Pure UI wiring relocated from the TaskController constructors so the
            // controller no longer needs the concrete OKButton/Cancel_Button on its
            // viewer interface (behavior preserved: both ctors already call SetController).
            AcceptButton = OKButton;
            CancelButton = Cancel_Button;
        }

        #region ITaskViewer primitive facade

        public string TaskNameText
        {
            get => TaskName.Text;
            set => TaskName.Text = value;
        }

        public string ContextText
        {
            get => CategorySelection.Text;
            set => CategorySelection.Text = value;
        }

        public string PeopleText
        {
            get => PeopleSelection.Text;
            set => PeopleSelection.Text = value;
        }

        public string ProjectText
        {
            get => ProjectSelection.Text;
            set => ProjectSelection.Text = value;
        }

        public string TopicText
        {
            get => TopicSelection.Text;
            set => TopicSelection.Text = value;
        }

        public string DurationText
        {
            get => Duration.Text;
            set => Duration.Text = value;
        }

        public object PrioritySelectedItem
        {
            get => PriorityBox.SelectedItem;
            set => PriorityBox.SelectedItem = value;
        }

        public object KbSelectedItem
        {
            get => KbSelector.SelectedItem;
            set => KbSelector.SelectedItem = value;
        }

        public bool TodayChecked
        {
            get => CbxToday.Checked;
            set => CbxToday.Checked = value;
        }

        public bool BullpinChecked
        {
            get => CbxBullpin.Checked;
            set => CbxBullpin.Checked = value;
        }

        public bool FlagAsTaskChecked
        {
            get => CbxFlagAsTask.Checked;
            set => CbxFlagAsTask.Checked = value;
        }

        public DateTime ReminderValue
        {
            get => DtReminder.Value;
            set => DtReminder.Value = value;
        }

        public bool ReminderChecked
        {
            get => DtReminder.Checked;
            set => DtReminder.Checked = value;
        }

        public DateTime DueDateValue
        {
            get => DtDuedate.Value;
            set => DtDuedate.Value = value;
        }

        public bool DueDateChecked
        {
            get => DtDuedate.Checked;
            set => DtDuedate.Checked = value;
        }

        public void FocusDuration() => Duration.Focus();

        #endregion

        #region ITaskViewerControls control-identity surface (explicit implementation)

        // Explicit implementation returns the Designer fields (whose names match the
        // interface members); the control-identity regions of TaskController read these
        // through the ITaskViewerControls accessor.

        Label ITaskViewerControls.XlSector1 => XlSector1;
        Label ITaskViewerControls.XlSector2 => XlSector2;
        Label ITaskViewerControls.XlSector3 => XlSector3;
        Label ITaskViewerControls.XlSector4 => XlSector4;

        Label ITaskViewerControls.C1S1 => C1S1;
        Label ITaskViewerControls.C3S1 => C3S1;
        Label ITaskViewerControls.C4S1 => C4S1;
        Label ITaskViewerControls.C2S2 => C2S2;
        Label ITaskViewerControls.C3S2 => C3S2;
        Label ITaskViewerControls.C4S2 => C4S2;
        Label ITaskViewerControls.C2S3 => C2S3;
        Label ITaskViewerControls.C3S3 => C3S3;
        Label ITaskViewerControls.C4S3 => C4S3;
        Label ITaskViewerControls.C2S4 => C2S4;
        Label ITaskViewerControls.C3S4 => C3S4;

        Label ITaskViewerControls.XlTopic => XlTopic;
        Label ITaskViewerControls.XlProject => XlProject;
        Label ITaskViewerControls.XlPeople => XlPeople;
        Label ITaskViewerControls.XlContext => XlContext;
        Label ITaskViewerControls.XlTaskname => XlTaskname;
        Label ITaskViewerControls.XlImportance => XlImportance;
        Label ITaskViewerControls.XlKanban => XlKanban;
        Label ITaskViewerControls.XlWorktime => XlWorktime;
        Label ITaskViewerControls.XlReminder => XlReminder;
        Label ITaskViewerControls.XlDuedate => XlDuedate;
        Label ITaskViewerControls.XlOk => XlOk;
        Label ITaskViewerControls.XlCancel => XlCancel;
        Label ITaskViewerControls.XlAutotag => XlAutotag;

        Label ITaskViewerControls.XlScWaiting => XlScWaiting;
        Label ITaskViewerControls.XlScUnprocessed => XlScUnprocessed;
        Label ITaskViewerControls.XlScNews => XlScNews;
        Label ITaskViewerControls.XlScEmail => XlScEmail;
        Label ITaskViewerControls.XlScReadingbusiness => XlScReadingbusiness;
        Label ITaskViewerControls.XlScCalls => XlScCalls;
        Label ITaskViewerControls.XlScInternet => XlScInternet;
        Label ITaskViewerControls.XlScPreread => XlScPreread;
        Label ITaskViewerControls.XlScMeeting => XlScMeeting;
        Label ITaskViewerControls.XlScPersonal => XlScPersonal;
        Label ITaskViewerControls.XlScBullpin => XlScBullpin;
        Label ITaskViewerControls.XlScToday => XlScToday;

        Label ITaskViewerControls.LblTopic => LblTopic;
        Label ITaskViewerControls.LblProject => LblProject;
        Label ITaskViewerControls.LblPeople => LblPeople;
        Label ITaskViewerControls.LblContext => LblContext;
        Label ITaskViewerControls.LblTaskname => LblTaskname;
        Label ITaskViewerControls.LblPriority => LblPriority;
        Label ITaskViewerControls.LblKbf => LblKbf;
        Label ITaskViewerControls.LblDuration => LblDuration;
        Label ITaskViewerControls.LblReminder => LblReminder;
        Label ITaskViewerControls.LblDuedate => LblDuedate;

        Label ITaskViewerControls.CategorySelection => CategorySelection;
        Label ITaskViewerControls.PeopleSelection => PeopleSelection;
        Label ITaskViewerControls.ProjectSelection => ProjectSelection;
        Label ITaskViewerControls.TopicSelection => TopicSelection;

        TextBox ITaskViewerControls.TaskName => TaskName;
        ComboBox ITaskViewerControls.PriorityBox => PriorityBox;
        ComboBox ITaskViewerControls.KbSelector => KbSelector;
        TextBox ITaskViewerControls.Duration => Duration;
        DateTimePicker ITaskViewerControls.DtReminder => DtReminder;
        DateTimePicker ITaskViewerControls.DtDuedate => DtDuedate;

        Button ITaskViewerControls.OKButton => OKButton;
        Button ITaskViewerControls.Cancel_Button => Cancel_Button;
        Button ITaskViewerControls.AutoTagButton => AutoTagButton;
        Button ITaskViewerControls.ShortcutWaitingFor => ShortcutWaitingFor;
        Button ITaskViewerControls.ShortcutUnprocessed => ShortcutUnprocessed;
        Button ITaskViewerControls.ShortcutNews => ShortcutNews;
        Button ITaskViewerControls.ShortcutEmail => ShortcutEmail;
        Button ITaskViewerControls.ShortcutReadingBusiness => ShortcutReadingBusiness;
        Button ITaskViewerControls.ShortcutCalls => ShortcutCalls;
        Button ITaskViewerControls.ShortcutInternet => ShortcutInternet;
        Button ITaskViewerControls.ShortcutPreRead => ShortcutPreRead;
        Button ITaskViewerControls.ShortcutMeeting => ShortcutMeeting;
        Button ITaskViewerControls.ShortcutPersonal => ShortcutPersonal;

        CheckBox ITaskViewerControls.CbxBullpin => CbxBullpin;
        CheckBox ITaskViewerControls.CbxToday => CbxToday;

        #endregion

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData.HasFlag(Keys.Alt))
            {
                // Issue #726 finding 7: the return value of KeyboardHandler_KeyDown was previously
                // discarded and this method unconditionally returned true, claiming the entire Alt
                // chord class regardless of whether the handler actually consumed the key -- the
                // same over-claim shape already fixed for the QuickFiler EFC/QFC surfaces (#467,
                // #663). Falling through to base.ProcessCmdKey when the handler does not consume the
                // key restores standard WinForms menu-mnemonic routing for Alt chords it doesn't own.
                object sender = FromHandle(msg.HWnd);
                var e = new KeyEventArgs(keyData);
                bool consumed = _controller.KeyboardHandler_KeyDown(sender, e);
                if (consumed)
                {
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private List<Label> _navColumns;
        internal List<Label> NavColumns
        {
            get
            {
                if (_navColumns is null)
                    _navColumns = new() { C1S1, C2S2, C2S4, C3S1, C3S2, C3S4, C4S1, C4S2 };
                return _navColumns;
            }
        }

        #region Event Handlers

        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            _controller.Cancel_Action();
        }

        private void PeopleSelection_Click(object sender, EventArgs e)
        {
            _controller.AssignPeople();
        }

        private void CategorySelection_Click(object sender, EventArgs e)
        {
            _controller.AssignContext();
        }

        private void ProjectSelection_Click(object sender, EventArgs e)
        {
            _controller.AssignProject();
        }

        private void TopicSelection_Click(object sender, EventArgs e)
        {
            _controller.AssignTopic();
        }

        private void ShortcutPersonal_Click(object sender, EventArgs e)
        {
            _controller.Shortcut_Personal();
        }

        private void ShortcutMeeting_Click(object sender, EventArgs e)
        {
            _controller.Shortcut_Meeting();
        }

        private void ShortcutPreRead_Click(object sender, EventArgs e)
        {
            _controller.Shortcut_PreRead();
        }

        private void ShortcutInternet_Click(object sender, EventArgs e)
        {
            // TODO: ShortcutInternet_Click -> hook function to controller
        }

        private void ShortcutCalls_Click(object sender, EventArgs e)
        {
            _controller.Shortcut_Calls();
        }

        private void ShortcutReadingBusiness_Click(object sender, EventArgs e)
        {
            _controller.Shortcut_ReadingBusiness();
        }

        private void ShortcutEmail_Click(object sender, EventArgs e)
        {
            _controller.Shortcut_Email();
        }

        private void ShortcutReadingNews_Click(object sender, EventArgs e)
        {
            _controller.Shortcut_ReadingNews();
        }

        private void ShortcutUnprocessed_Click(object sender, EventArgs e)
        {
            _controller.Shortcut_Unprocessed();
        }

        private void ShortcutWaitingFor_Click(object sender, EventArgs e)
        {
            _controller.Shortcut_WaitingFor();
        }

        private async void OKButton_Click(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            await _controller.OK_Action();
        }

        private void CbxToday_CheckedChanged(object sender, EventArgs e)
        {
            _controller.Today_Change();
        }

        private void CbxBullpin_CheckedChanged(object sender, EventArgs e)
        {
            _controller.Bullpin_Change();
        }

        private void KbSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            _controller.Assign_KB();
        }

        private void PriorityBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _controller.Assign_Priority();
        }

        private void CbxFlag_CheckedChanged(object sender, EventArgs e)
        {
            if (_controller is not null)
                _controller.FlagAsTask_Change();
        }

        private void TaskViewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (_controller is not null)
            {
                Debug.WriteLine(e.KeyCode.ToString());
                bool consumed = _controller.KeyboardHandler_KeyDown(sender, e);
                if (consumed)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                else
                {
                    e.Handled = false;
                }
            }
        }

        private void _mouseFilter_FormClicked(object sender, EventArgs e)
        {
            if (_controller is not null)
                _controller.MouseFilter_FormClicked(sender, e);
        }

        //private void TaskName_KeyDown(object sender, KeyEventArgs e)
        //{
        //    // Debug.WriteLine("task_name_keydown fired with " & e.KeyCode.ToChar)
        //}

        //private void TaskName_KeyUp(object sender, KeyEventArgs e)
        //{
        //    // Debug.WriteLine("task_name_keyup fired with " & e.KeyCode.ToChar)
        //}

        //private void TaskName_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    // Debug.WriteLine("task_name_keypress fired with " & e.KeyChar)
        //    if (_controller.SuppressKeystrokes)
        //    {
        //        e.Handled = true;
        //        // Debug.WriteLine("task_name_keypress suppressed keystrokes")
        //    }
        //}

        #endregion

        private void TaskViewer_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up)
            {
                e.IsInputKey = true;
            }
        }

        private async void AutoTagButton_Click(object sender, EventArgs e)
        {
            try
            {
                await _controller.AutoAssignAllAsync().ConfigureAwait(true);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
