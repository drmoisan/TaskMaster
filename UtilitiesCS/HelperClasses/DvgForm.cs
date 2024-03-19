using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilitiesCS
{
    public partial class DgvForm : Form
    {
        public DgvForm()
        {
            InitializeComponent();
        }

        private void DgvForm_ResizeEnd(object sender, EventArgs e)
        {
            Debug.WriteLine($"Size is {this.Size.ToString()}");
        }
    }
}
