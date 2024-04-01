using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;


namespace SVGControl
{
    public partial class ButtonSVG : Button
    { 
        private SvgImageSelector _imageSVG;

        public ButtonSVG()
        {
            var caller = System.Reflection.Assembly.GetCallingAssembly();
            InitializeComponent();
            _imageSVG = new SvgImageSelector(base.Size,
                                             new Padding(3),
                                             SVGControl.AutoSize.MaintainAspectRatio,
                                             caller);
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
