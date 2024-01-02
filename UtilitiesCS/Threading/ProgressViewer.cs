using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilitiesCS
{
    public partial class ProgressViewer : Form
    {
        public ProgressViewer()
        {
            InitializeComponent();
            this.ButtonCancel.Enabled = false;
        }

        private CancellationTokenSource _tokenSource;

        public void SetCancellationTokenSource(CancellationTokenSource tokenSource)
        {
            _tokenSource = tokenSource;
            this.ButtonCancel.Enabled = true;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            _tokenSource.Cancel();
            this.Close();
        }
    }
}
