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

namespace SVGControl
{
    [Designer(typeof(SVGControlDesigner))]
    public partial class ButtonSVG : Button
    {
        private Image _image = null;
        private string _imagePath = "";
        private SVGParser _parser;

        public ButtonSVG()
        {
            InitializeComponent();
            _parser = new SVGParser();
        }
        
        //public new Image Image 
        public object SVGImage
        {
            get { return _image; }
            set 
            {
                if (value != null)
                {
                var val = ObjectToByteArray(value);
                _image = _parser.GetBitmapFromSVG(val,this.Size);
                }
                else
                {
                    _image = null;
                }
            } 
        }

        //[Browsable(false)]
        public new Image Image
        {
            get { return _image; }
        }

        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

    }
}
