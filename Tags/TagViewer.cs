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

    }
}