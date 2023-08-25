using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;
using UtilitiesCS;

namespace TaskVisualization
{

    public partial class TaskViewer
    {

        private MouseDownFilter __mouseFilter;

        private MouseDownFilter _mouseFilter
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return __mouseFilter;
            }

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

        public TaskViewer()
        {

            // This call is required by the designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.
            KeyPreview = true;

            // Attach Handler to capture mouseclick anywhere on form
            _mouseFilter = new MouseDownFilter(this);
            Application.AddMessageFilter(_mouseFilter);

        }


        public void SetController(TaskController controller)
        {
            _controller = controller;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData.HasFlag(Keys.Alt))
            {
                // If keyData = Keys.Up OrElse keyData = Keys.Down OrElse keyData = Keys.Left OrElse keyData = Keys.Right OrElse keyData = Keys.Alt Then
                object sender = FromHandle(msg.HWnd);
                var e = new KeyEventArgs(keyData);
                _controller.KeyboardHandler_KeyDown(sender, e);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

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

        private void OKButton_Click(object sender, EventArgs e)
        {
            _controller.OK_Action();
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

        private void TaskName_KeyDown(object sender, KeyEventArgs e)
        {
            // Debug.WriteLine("task_name_keydown fired with " & e.KeyCode.ToChar)
        }

        private void TaskName_KeyUp(object sender, KeyEventArgs e)
        {
            // Debug.WriteLine("task_name_keyup fired with " & e.KeyCode.ToChar)
        }

        private void TaskName_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Debug.WriteLine("task_name_keypress fired with " & e.KeyChar)
            if (_controller.SuppressKeystrokes)
            {
                e.Handled = true;
                // Debug.WriteLine("task_name_keypress suppressed keystrokes")
            }
        }
    }
}