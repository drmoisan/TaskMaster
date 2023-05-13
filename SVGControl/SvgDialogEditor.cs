using Svg;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace SVGControl
{
    public class SvgDialogEditor : UITypeEditor
    {
        private IWindowsFormsEditorService _service;

        private SvgResource currentValue = null;
        
        public override object EditValue(ITypeDescriptorContext context,
                                         IServiceProvider provider,
                                         object value)
        {
            if ((context != null) && (context.Instance != null) && (provider != null))
            {
                _service = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

                if (_service != null)
                {
                    var viewer = new SvgResourceViewer();
                    var controller = new SvgResourceController(viewer);
                    DialogResult result = viewer.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        return controller.Selection;
                    }
                    else { return value; }
                }

                //if (value is SvgResource)
                //    currentValue = (SvgResource)value;
                //return base.EditValue(context, provider, value);
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if ((context != null) && (context.Instance != null))
                return UITypeEditorEditStyle.Modal;

            return base.GetEditStyle(context);
        }
    }
}
