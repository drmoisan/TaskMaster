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
    public partial class MyBoxViewer : Form
    {
        
        private bool ableToRemoveStandard = true;
        private readonly Dictionary<string, Delegate> _map;
                
        public MyBoxViewer()
        {
            InitializeComponent();
        }

        public MyBoxViewer(string title, string message, Dictionary<string, Delegate> map)
        {
            InitializeComponent();
            this.Text = title;
            this.TextMessage.Text = message;
            List<string> keys = map.Keys.ToList();
            Button1.Text = keys[0];
            Button2.Text = keys[1];
            this._map = map;
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            string key = _map.Keys.ToList()[0];
            var result = _map[key].DynamicInvoke();
            this.DialogResult = (DialogResult)result;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            string key = _map.Keys.ToList()[1];
            var result = _map[key].DynamicInvoke();
            this.DialogResult = (DialogResult)result;
        }

        public void RemoveStandardButtons()
        {
            if (ableToRemoveStandard)
            {
                this.Button1.Click -= new System.EventHandler(this.Button1_Click);
                this.Button2.Click -= new System.EventHandler(this.Button2_Click);
                this.L2Bottom.Controls.Remove(this.Button1);
                this.L2Bottom.Controls.Remove(this.Button2);
                this.Button1.Dispose();
                this.Button2.Dispose();
                
                Size tmp = this.MinimumSize;
                float width = this.L2Bottom.ColumnStyles[2].Width;
                width += this.L2Bottom.ColumnStyles[1].Width;
                int width2 = (int)Math.Round(width, 0);
                if (tmp.Width > width2)
                {
                    tmp.Width -= width2;
                }
                else
                {
                    tmp.Width = 0;
                }
                
                this.L2Bottom.ColumnStyles.RemoveAt(2);
                this.L2Bottom.ColumnStyles.RemoveAt(1);
                this.L2Bottom.ColumnCount -= 2;
                
                this.MinimumSize = tmp;
                ableToRemoveStandard = false;
            }
        }
    }
}
