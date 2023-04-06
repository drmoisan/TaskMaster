using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using System.Windows.Forms;

namespace SVGControl
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ImagePickerClass
    {
        public ImagePickerClass() { }

        [Editor(typeof(SpecializedFileNameEditor), typeof(UITypeEditor))]
        public String ImagePath { get; set; }

        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        public String ImageFolder { get; set; }

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
    }
}
