using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SVGControl
{
    public partial class ButtonSVG : Button
    {
        
        private SVGParser _parser;
        private String _imagePath = "";
        private SvgImageSelector _imageSVG;

        public ButtonSVG()
        {
            InitializeComponent();
            
            _imageSVG = new SvgImageSelector(base.Size,
                                    new Padding(3),
                                    SVGControl.AutoSize.MaintainAspectRatio);
            _imageSVG.PropertyChanged += ImageSVG_PropertyChanged;
            this.Resize += ButtonSVG_Resize;
        }

        private void ButtonSVG_Resize(object sender, EventArgs e)
        {
            _imageSVG.Outer = this.Size;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public SvgImageSelector ImageSVG 
        {
            get { return this._imageSVG; }
            set { this._imageSVG = value; }
        }


        //protected override void OnPaint(PaintEventArgs e)
        //{
        //    if (ImageSVG.ImagePath != null)
        //    {
        //        ImageSVG.Outer = e.ClipRectangle.Size;

        //        base.Image = ImageSVG.Render();
        //        //base.Invalidate();
        //    }
        //    base.OnPaint(e);
        //}



        private void ImageSVG_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.Image = ImageSVG.Render();
            //this.Refresh();
            this.InvokePaint(this, new PaintEventArgs(this.CreateGraphics(), this.DisplayRectangle));
            //this.Image = ImageSVG.Render();
        }

        

        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                if (obj!=null)
                    bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
                
    }
}
