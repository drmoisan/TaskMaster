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
        //public MyBoxTemplate()
        //{
        //    InitializeComponent();
        //    string message = "This is a test to see if this is working properly";
        //    this.TextMessage.Text = message;

        //}
        private bool ableToRemoveStandard = true;
        private readonly Dictionary<string, Delegate> _map;

        //public MyBoxTemplate(string title, string message, Dictionary<string, Delegate> map)
        //{
        //    InitializeComponent();
        //    this.Text = title;
        //    this.TextMessage.Text = message;
        //    List<string> keys = map.Keys.ToList();
        //    Button1.Text = keys[0];
        //    Button2.Text = keys[1];
        //    this._map = map;
        //}
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
                this.L2Bottom.ColumnStyles.RemoveAt(2);
                this.L2Bottom.ColumnStyles.RemoveAt(1);
                this.L2Bottom.ColumnCount -= 2;
                ableToRemoveStandard = false;
            }
        }
    }
}
