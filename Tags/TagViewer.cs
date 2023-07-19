using System;
using System.Windows.Forms;

namespace Tags
{

    public partial class TagViewer
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

        private void button_ok_Click(object sender, EventArgs e)
        {
            _controller.OK_Action();
        }

        private void button_new_Click(object sender, EventArgs e)
        {
            _controller.AddColorCategory();
        }

        private void button_autoassign_Click(object sender, EventArgs e)
        {
            _controller.AutoAssign();
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            _controller.SearchAndReload();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            _controller.Cancel_Action();
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            _controller.TextBox1_KeyDown(sender, e);
        }

        private void TextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            _controller.TextBox1_KeyUp(sender, e);
        }

        private void OptionsPanel_KeyDown(object sender, KeyEventArgs e)
        {
            _controller.OptionsPanel_KeyDown(sender, e);
        }

        private void TagViewer_KeyDown(object sender, KeyEventArgs e)
        {
            _controller.TagViewer_KeyDown(sender, e);
        }

        private void OptionsPanel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            _controller.OptionsPanel_PreviewKeyDown(sender, e);
        }

        private void Hide_Archive_CheckedChanged(object sender, EventArgs e)
        {
            if (_controller is not null)
                _controller.ToggleArchive();
        }
    }
}