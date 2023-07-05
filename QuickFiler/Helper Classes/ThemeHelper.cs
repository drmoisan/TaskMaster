using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;

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

        public static Dictionary<string,Theme> SetupThemes(IQfcItemController controller, QfcItemViewer viewer)
        {
            var themes = new Dictionary<string, Theme>
            {
                {
                    "LightNormal",
                    new Theme(name: "LightNormal",
                          itemViewer: viewer,
                          parent: controller,
                          tlpBackColor: SystemColors.Control,
                          tipsForeColor: Color.Black,
                          tipsBackColor: Color.White,
                          mailReadBackColor: SystemColors.Control,
                          mailReadForeColor: SystemColors.ControlText,
                          mailUnreadBackColor: SystemColors.Control,
                          mailUnreadForeColor: Color.MediumBlue,
                          tipsDetailsBackColor: Color.Black,
                          tipsDetailsForeColor: Color.White,
                          buttonBackColor: SystemColors.Control,
                          txtboxSearchBackColor: SystemColors.Control,
                          txtboxSearchForeColor: SystemColors.ControlText,
                          txtboxBodyBackColor: SystemColors.Control,
                          txtboxBodyForeColor: SystemColors.ControlText,
                          cboFoldersBackColor: SystemColors.Control,
                          cboFoldersForeColor: SystemColors.ControlText,
                          defaultBackColor: SystemColors.Control,
                          defaultForeColor: SystemColors.ControlText)
                    },
                {
                    "DarkNormal",
                    new Theme(name: "DarkNormal",
                          itemViewer: viewer,
                          parent: controller,
                          tlpBackColor: Color.Black,
                          tipsForeColor: Color.LightSkyBlue,
                          tipsBackColor: SystemColors.ActiveCaptionText,
                          mailReadForeColor: Color.WhiteSmoke,
                          mailReadBackColor: Color.Black,
                          mailUnreadForeColor: Color.Goldenrod,
                          mailUnreadBackColor: Color.Black,
                          tipsDetailsBackColor: Color.LightSkyBlue,
                          tipsDetailsForeColor: SystemColors.ActiveCaptionText,
                          buttonBackColor: Color.DimGray,
                          txtboxSearchBackColor: Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30))))),
                          txtboxSearchForeColor: Color.WhiteSmoke,
                          txtboxBodyBackColor: Color.Black,
                          txtboxBodyForeColor: Color.WhiteSmoke,
                          cboFoldersBackColor: Color.DimGray,
                          cboFoldersForeColor: Color.WhiteSmoke,
                          defaultBackColor: Color.Black,
                          defaultForeColor: Color.WhiteSmoke)
                }
            };
            return themes;
        }
    }
}
