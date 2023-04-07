using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace SVGControl
{
    internal class SVGFileNameEditor : FileNameEditor
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
