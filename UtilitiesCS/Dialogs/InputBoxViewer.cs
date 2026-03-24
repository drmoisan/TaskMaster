using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilitiesCS
{
    public partial class InputBoxViewer : Form
    {
        public InputBoxViewer()
        {
            InitializeComponent();
        }

        [STAThread]
        public static void DpiAware()
        {
            try
            {
                Application.EnableVisualStyles();
                // SetCompatibleTextRenderingDefault throws InvalidOperationException if any
                // IWin32Window has already been created (e.g., during unit-test runs).
                Application.SetCompatibleTextRenderingDefault(false);
            }
            catch (InvalidOperationException)
            {
                // Application is already initialized; visual-style configuration cannot be
                // changed. Record the call regardless so callers can detect it.
            }
            DpiCalled = true;
        }

        public static bool DpiCalled { get; set; } = false;

        private void Ok_Click(object sender, EventArgs e)
        {
            if (Input.Text == "")
            {
                MessageBox.Show("Please enter a value or cancel.");
            }
            else
            {
                DialogResult = DialogResult.OK;
                this.Hide();
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Hide();
        }
    }
}
