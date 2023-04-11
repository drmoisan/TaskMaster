using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

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
                _button.Click -= new System.EventHandler((object sender, EventArgs e)=>_delegate.DynamicInvoke());
                _button = value;
                _button.Click += new System.EventHandler((object sender, EventArgs e) => _delegate.DynamicInvoke());

            }
        }
        public Delegate Delegate { get => _delegate; set => _delegate = value; }
        
        public Button MakeButton(string Text, Image Image) 
        {
            Button button = new Button();
            button.Text = Text;
            if (button.Image != null)
                button.Image.Dispose();
            button.Image = Image;
            button.TextImageRelation = TextImageRelation.ImageBeforeText;
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.Size = new Size(252, 108);
            return button;
        }

        public Button MakeButton(string Text)
        {
            Button button = new Button();
            button.Text = Text;
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.Size = new Size(252, 108);
            return button;
        }
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
                AppendButton(_viewer.L2Bottom, delegateButton);
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