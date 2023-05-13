using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SVGControl
{
    public partial class SvgResourceViewer : Form
    {
        public SvgResourceViewer()
        {
            InitializeComponent();
            this.Ok.DialogResult = DialogResult.OK;
            this.Cancel.DialogResult = DialogResult.Cancel;
        }

        private SvgResourceController _controller;

        public void SetController(SvgResourceController controller)
        {
            _controller = controller;
        }
    }
}
