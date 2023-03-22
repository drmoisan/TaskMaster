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
    public partial class MyBoxTemplate : Form
    {
        //public MyBoxTemplate()
        //{
        //    InitializeComponent();
        //    string message = "This is a test to see if this is working properly";
        //    this.TextMessage.Text = message;

        //}
        private Dictionary<string, Delegate> _map;

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
        public MyBoxTemplate(string title, string message, Dictionary<string, Delegate> map)
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
    }
}
