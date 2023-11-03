using System;
using System.ComponentModel;
using System.Windows.Forms;


namespace QuickFiler.Viewers
{
    [RefreshProperties(RefreshProperties.Repaint)]
    [Browsable(true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public partial class ToolStripMenuItemCb : ToolStripMenuItem
    {
        public ToolStripMenuItemCb()
        {
            InitializeComponent();
            if (Checked)
            {
                base.Image = Properties.Resources.CheckBoxChecked;
            }
            else
            {
                base.Image = null;
            }
            base.Invalidate();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public new bool Checked 
        { 
            get => _checked;
            set {
                _checked = value;
                if (value)
                {
                    base.Image = Properties.Resources.CheckBoxChecked;
                }
                else
                {
                    base.Image = null;
                }
                base.Invalidate();                
            }
            
        }
        private bool _checked;

        private void ToolStripMenuItemCb_Click(object sender, EventArgs e)
        {
                Checked = !Checked;
        }

        public new event EventHandler Click;

        private bool _checkOnClick;
        public new bool CheckOnClick 
        { 
            get => _checkOnClick;
            set
            {
                _checkOnClick = value;
                if (_checkOnClick)
                {
                    base.Click += ToolStripMenuItemCb_Click;
                }
                else
                {
                    base.Click -= ToolStripMenuItemCb_Click;
                }
            }

        }
    }
}
