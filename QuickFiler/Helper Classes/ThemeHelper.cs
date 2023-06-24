using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace QuickFiler
{
    internal static class ThemeHelper
    {
        public static void SetTheme(this TableLayoutPanel tlp, 
                                    Color backColor)
        {
            tlp.BackColor = backColor;
        }
        
        public static void SetTheme(this Label lbl, Color backColor, Color forecolor)
        {
            lbl.BackColor = backColor;
            lbl.ForeColor = forecolor;
        }

        public static void SetTheme(this Button btn, Color backColor)
        {
            btn.BackColor = backColor;
            //btn.ForeColor = forecolor;
        }
    }
}
