using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilitiesCS
{
    public class DelegateButton
    {
        private string _name;
        private Button _button;
        
        private Delegate _delegate;

        public DelegateButton() { }
        public DelegateButton(string name, 
                              Image buttonImage, 
                              string buttonText, 
                              DialogResult dialogResult, 
                              Delegate @delegate)
        {
            _name = name;
            _delegate = @delegate;
            Button = MakeButton(buttonText, buttonImage, dialogResult);
        }

        public string Name { get => _name; set => _name = value; }
        public Button Button
        {
            get => _button;
            set
            {
                if (_button != null)
                    _button.Click -= new System.EventHandler(Button_Click);
                _button = value;
                _button.Click += new System.EventHandler(Button_Click);

            }
        }
        public Delegate Delegate { get => _delegate; set => _delegate = value; }

        public Button MakeButton(string Text)
        {
            Button button = new Button();
            button.Text = Text;
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.Size = new Size(126, 56);
            button.Dock = DockStyle.Fill;
            return button;
        }

        public Button MakeButton(string Text, Image Image)
        {
            Button button = MakeButton(Text);
            AddImageToButton(Image, ref button);
            return button;
        }

        public Button MakeButton(string Text, Image Image, DialogResult dialogResult)
        {
            Button button = MakeButton(Text, Image);
            button.DialogResult = dialogResult;
            return button;
        }

        public Button MakeButton(string Text, DialogResult dialogResult)
        {
            Button button = MakeButton(Text);
            button.DialogResult = dialogResult;
            return button;
        }

        private static void AddImageToButton(Image Image, ref Button button)
        {
            if (button.Image != null)
                button.Image.Dispose();
            button.Image = Image;
            button.TextImageRelation = TextImageRelation.ImageBeforeText;
        }

        internal void Button_Click(object sender, EventArgs e)
        {
            _delegate.DynamicInvoke();
        }
        
    }
}
