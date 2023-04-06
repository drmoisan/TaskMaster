using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.Design;
using System.Numerics;
using System.Drawing.Design;
using SVGControl.Properties;

namespace SVGControl
{
    public partial class SVG : UserControl
    {
        private string _imagePath = string.Empty;
        private SVGParser _parser;

        public SVG()
        {
            InitializeComponent();
            _parser = new SVGParser();
            pictureBox1.Image = _parser.GetBitmapFromSVG(Properties.Resources.Image, pictureBox1.Size);
        }


        [Editor(typeof(SpecializedFileNameEditor), typeof(UITypeEditor))]
        public string ImagePath 
        { 
            get 
            { 
                return _imagePath; 
            } 
            set 
            { 
                _imagePath = value; 
                if ((value != null) && (value != string.Empty))
                {
                    this.pictureBox1.Image = _parser.GetBitmapFromSVG(value, pictureBox1.Size);
                }
            } 
        }

        public class SpecializedFileNameEditor : FileNameEditor
        {
            private string currentValue = string.Empty;

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                if (value is string)
                    currentValue = (string)value;
                return base.EditValue(context, provider, value);
            }

            protected override void InitializeDialog(OpenFileDialog ofd)
            {
                base.InitializeDialog(ofd);
                if (!currentValue.Equals(string.Empty))
                    ofd.InitialDirectory = Path.GetDirectoryName(currentValue);
                ofd.Filter = "Vector Graphics(*.svg) | *.svg";
            }
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            
            if (_imagePath != string.Empty) 
            {
                this.pictureBox1.Image = _parser.GetBitmapFromSVG(_imagePath, pictureBox1.Size);
                
            }
            else 
            {
                this.pictureBox1.Image = _parser.GetBitmapFromSVG(Properties.Resources.Image, pictureBox1.Size);
            }
            
        }

        //private void pictureBox1_Paint(object sender, PaintEventArgs e)
        //{
        //    if (_imagePath != string.Empty)
        //    {
        //        this.pictureBox1.Image = SVGParser.GetBitmapFromSVG(_imagePath, pictureBox1.Size);
        //    }
        //    else
        //    {
        //        this.pictureBox1.Image = this.pictureBox1.InitialImage;
        //    }

        //}
    }
}
