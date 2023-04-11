using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilitiesCS
{
    public enum BoxIcon
    {
        None = 0,
        Critical = 1, 
        Warning = 2,
        Question = 4
    }

    public class DelegateButton
    {
        private string _name;
        private Button _button;
        private Delegate _delegate;

        public string Name { get => _name; set => _name = value; }
        public Button Button 
        { 
            get => _button;
            set 
            {
                _button.Click -= (object sender, EventArgs e)=>_delegate;
                _button = value;
               
            }
        }
        public Delegate Delegate { get => _delegate; set => _delegate = value; }
    }

    public class MyBoxController
    {
        private delegate DialogResult ResponseDelegate();

        public void CustomDialog(string Message, string Title, BoxIcon icon, IList<DelegateButton> delegateButtons) 
        {
            MyBoxViewer _viewer = new MyBoxViewer();
            _viewer.RemoveStandardButtons();

            foreach (var delegateButton in delegateButtons)
            {

            }
        }

        private void AppendButton(TableLayoutPanel tlp, DelegateButton dlb)
        {
            tlp.ColumnCount++;
            tlp.ColumnStyles.Insert(tlp.ColumnCount-2, new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 115F));
            tlp.Controls.Add(dlb.Button, tlp.ColumnCount -2,0);
        }

        

    }
}