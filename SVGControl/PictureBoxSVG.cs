using Fizzler;
using Svg;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace SVGControl
{
    public partial class PictureBoxSVG : PictureBox
    {
        public PictureBoxSVG()
        {
            InitializeComponent();
            _imageSvg = new SvgResourceSelector(base.Size, 
                                                new Padding(0), 
                                                SVGControl.AutoSize.MaintainAspectRatio, 
                                                useDefaultImage: true);
            this.Image = _imageSvg.Render();
            _imageSvg.PropertyChanged += ImageSVG_PropertyChanged;
            this.Resize += Control_Resize;
        }

        private SvgResourceSelector _imageSvg;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public SvgResourceSelector ImageSvg 
        { 
            get => _imageSvg; 
            set => _imageSvg = value; 
        }

        private void Control_Resize(object sender, EventArgs e)
        {
            _imageSvg.Outer = this.Size;
        }

        private void ImageSVG_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.Image = ImageSvg.Render();
            this.InvokePaint(this, new PaintEventArgs(this.CreateGraphics(), this.DisplayRectangle));
        }
    }
}
