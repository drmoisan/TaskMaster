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

        public static Dictionary<string,Theme> SetupThemes(IQfcItemController controller, 
                                                           QfcItemViewer viewer,
                                                           Action<Enums.ToggleState> htmlConverter)
        {
            var themes = new Dictionary<string, Theme>
            {
                {
                    "LightNormal",
                    new Theme(name: "LightNormal",
                          itemViewer: viewer,
                          parent: controller,
                          web2ViewScheme: Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Light,
                          htmlConverter: htmlConverter,
                          htmlDark: Enums.ToggleState.Off,
                          navBackgColor: SystemColors.HotTrack,
                          navForeColor: SystemColors.Control,
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
                    "LightActive",
                    new Theme(name: "LightActive",
                          itemViewer: viewer,
                          parent: controller,
                          web2ViewScheme: Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Light,
                          htmlConverter: htmlConverter,
                          htmlDark: Enums.ToggleState.Off,
                          navBackgColor: Color.Green,
                          navForeColor: SystemColors.Control,
                          tlpBackColor: Color.LightCyan,
                          tipsForeColor: Color.Black,
                          tipsBackColor: Color.White,
                          mailReadBackColor: Color.LightCyan,
                          mailReadForeColor: SystemColors.ControlText,
                          mailUnreadBackColor: Color.LightCyan,
                          mailUnreadForeColor: Color.MediumBlue,
                          tipsDetailsBackColor: Color.Black,
                          tipsDetailsForeColor: Color.White,
                          buttonBackColor: Color.LightCyan,
                          txtboxSearchBackColor: Color.LightCyan,
                          txtboxSearchForeColor: SystemColors.ControlText,
                          txtboxBodyBackColor: Color.LightCyan,
                          txtboxBodyForeColor: SystemColors.ControlText,
                          cboFoldersBackColor: Color.LightCyan,
                          cboFoldersForeColor: SystemColors.ControlText,
                          defaultBackColor: Color.LightCyan,
                          defaultForeColor: SystemColors.ControlText)
                    },
                {
                    "DarkNormal",
                    new Theme(name: "DarkNormal",
                          itemViewer: viewer,
                          parent: controller,
                          web2ViewScheme: Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Dark,
                          htmlConverter: htmlConverter,
                          htmlDark: Enums.ToggleState.On,
                          navBackgColor: Color.FromArgb(64,64,64),
                          navForeColor: SystemColors.Control,
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
                },
                {
                    "DarkActive",
                    new Theme(name: "DarkActive",
                          itemViewer: viewer,
                          parent: controller,
                          web2ViewScheme: Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Dark,
                          htmlConverter: htmlConverter,
                          htmlDark: Enums.ToggleState.On,
                          navBackgColor: SystemColors.HotTrack,
                          navForeColor: SystemColors.Control,
                          tlpBackColor: Color.FromArgb(64,64,64),
                          tipsForeColor: Color.LightSkyBlue,
                          tipsBackColor: SystemColors.ActiveCaptionText,
                          mailReadForeColor: Color.WhiteSmoke,
                          mailReadBackColor: Color.FromArgb(64,64,64),
                          mailUnreadForeColor: Color.Goldenrod,
                          mailUnreadBackColor: Color.FromArgb(64,64,64),
                          tipsDetailsBackColor: Color.LightSkyBlue,
                          tipsDetailsForeColor: SystemColors.ActiveCaptionText,
                          buttonBackColor: Color.DimGray,
                          txtboxSearchBackColor: Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30))))),
                          txtboxSearchForeColor: Color.WhiteSmoke,
                          txtboxBodyBackColor: Color.FromArgb(64,64,64),
                          txtboxBodyForeColor: Color.WhiteSmoke,
                          cboFoldersBackColor: Color.DimGray,
                          cboFoldersForeColor: Color.WhiteSmoke,
                          defaultBackColor: Color.FromArgb(64,64,64),
                          defaultForeColor: Color.WhiteSmoke)
                }
            };
            return themes;
        }
    }
}
