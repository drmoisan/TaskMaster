using System;

namespace QuickFiler.Test
{
    public partial class Form1
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            ControlGroup1.ToggleAccelerator();
        }

        private void LoadControlGroup()
        {
            var _controlGroup = new QfcItemViewerForm();
            TableLayoutPanel1.SuspendLayout();
            TableLayoutPanel1.RowCount += 1;
            TableLayoutPanel1.RowStyles.Insert(TableLayoutPanel1.RowCount - 2, new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110.0f));
            TableLayoutPanel1.Controls.Add(_controlGroup, 0, TableLayoutPanel1.RowCount - 2);
            SetControlGroupOptions(_controlGroup);
            TableLayoutPanel1.ResumeLayout(true);

        }

        private void SetControlGroupOptions(QfcItemViewerForm group)
        {
            group.AutoSize = true;
            group.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            group.Dock = System.Windows.Forms.DockStyle.Fill;
            group.Padding = new System.Windows.Forms.Padding(3);
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            LoadControlGroup();
        }
    }
}