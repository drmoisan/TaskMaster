using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;
using System.Windows.Forms;
using System.Drawing;
using UtilitiesCS.ReusableTypeClasses;
using BrightIdeasSoftware;

namespace QuickFiler.Helper_Classes
{
    internal static class EfcThemeHelper
    {
        public static Dictionary<string, Theme> SetupThemes(IList<Control> nav, 
                                                            IList<Control> tips,
                                                            IList<Control> dflt2,
                                                            IList<Control> selectors,
                                                            IList<Control> mail,
                                                            Func<bool> isAlt,
                                                            IList<object> olvColumns,
                                                            Action<IList<object>, Color, Color> olvSetter,
                                                            Microsoft.Web.WebView2.WinForms.WebView2 webView2,
                                                            Action<Enums.ToggleState> htmlConverter)
        {
            //dflt2 should have tlps, multiline textboxes
            //selectors should have combo, search
            var lightNormal = new Dictionary<string, ThemeControlGroup> 
            {
                //{ "Nav", new ThemeControlGroup(controls: nav, back: SystemColors.HotTrack, fore: SystemColors.Control) },
                { "Tips", new ThemeControlGroup(controls: tips, back: Color.Black, fore: Color.White) },
                { "Default2Color", new ThemeControlGroup(controls: dflt2, back: SystemColors.Control, fore: SystemColors.ControlText) },
                { "MailRelated", new ThemeControlGroup(controls: mail, backMain: SystemColors.Control, foreMain: SystemColors.ControlText, backAlt: SystemColors.Control, foreAlt: Color.MediumBlue, isAlt: isAlt) },
                { "OlvColumns", new ThemeControlGroup(objects: olvColumns, back: SystemColors.Control, fore: SystemColors.ControlText, objectSetter: olvSetter) },
                { "WebView2", new ThemeControlGroup(webView2: webView2, web2ViewScheme: Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Light, htmlConverter: htmlConverter, Enums.ToggleState.Off) }
            };
            var lightActive = new Dictionary<string, ThemeControlGroup>
            {
                //{ "Nav", new ThemeControlGroup(controls: nav, back: Color.Green, fore: SystemColors.Control) },
                { "Tips", new ThemeControlGroup(controls: tips, back: Color.Black, fore: Color.White) },
                { "Default2Color", new ThemeControlGroup(controls: dflt2, back: Color.LightCyan, fore: SystemColors.ControlText) },
                { "MailRelated", new ThemeControlGroup(controls: mail, backMain: Color.LightCyan, foreMain: SystemColors.ControlText, backAlt: Color.LightCyan, foreAlt: Color.MediumBlue, isAlt: isAlt) },
                { "OlvColumns", new ThemeControlGroup(objects: olvColumns, back: SystemColors.Control, fore: SystemColors.ControlText, objectSetter: olvSetter) },
                { "WebView2", new ThemeControlGroup(webView2: webView2, web2ViewScheme: Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Light, htmlConverter: htmlConverter, Enums.ToggleState.Off) }
            };
            var darkNormal = new Dictionary<string, ThemeControlGroup>
            {
                //{ "Nav", new ThemeControlGroup(controls: nav, back: Color.FromArgb(64,64,64), fore: SystemColors.Control) },
                { "Tips", new ThemeControlGroup(controls: tips, back: Color.LightSkyBlue, fore: SystemColors.ControlText) },
                { "Default2Color", new ThemeControlGroup(controls: dflt2, back: Color.Black, fore: Color.WhiteSmoke) },
                { "MailRelated", new ThemeControlGroup(controls: mail, backMain: Color.Black, foreMain: Color.WhiteSmoke, backAlt: Color.Black, foreAlt: Color.Goldenrod, isAlt: isAlt) },
                { "OlvColumns", new ThemeControlGroup(objects: olvColumns, back: Color.Black, fore: Color.WhiteSmoke, objectSetter: olvSetter) },
                { "WebView2", new ThemeControlGroup(webView2: webView2, web2ViewScheme: Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Dark, htmlConverter: htmlConverter, Enums.ToggleState.On) }
            };
            var darkActive = new Dictionary<string, ThemeControlGroup>
            {
                //{ "Nav", new ThemeControlGroup(controls: nav, back: SystemColors.HotTrack, fore: SystemColors.Control) },
                { "Tips", new ThemeControlGroup(controls: tips, back: Color.LightSkyBlue, fore: SystemColors.ControlText) },
                { "Default2Color", new ThemeControlGroup(controls: dflt2, back: Color.FromArgb(64,64,64), fore:Color.WhiteSmoke) },
                { "MailRelated", new ThemeControlGroup(controls: mail, backMain: Color.FromArgb(64,64,64), foreMain: Color.WhiteSmoke, backAlt: Color.FromArgb(64,64,64), foreAlt: Color.Goldenrod, isAlt: isAlt) },
                { "OlvColumns", new ThemeControlGroup(objects: olvColumns, back: Color.Black, fore: Color.WhiteSmoke, objectSetter: olvSetter) },
                { "WebView2", new ThemeControlGroup(webView2: webView2, web2ViewScheme: Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Dark, htmlConverter: htmlConverter, Enums.ToggleState.On) }
            };

            return new Dictionary<string, Theme>
            {
                { "LightNormal", new Theme("LightNormal", lightNormal) },
                { "LightActive", new Theme("LightActive", lightActive) },
                { "DarkNormal", new Theme("DarkNormal", darkNormal) },
                { "DarkActive", new Theme("DarkActive", darkActive) }
            };

        }

        public static Dictionary<string, Theme> SetupFormThemes(IList<Control> tips,
                                                                IList<Control> dflt2,
                                                                IList<Control> buttons)
        {
            //dflt2 should have tlps, multiline textboxes
            //selectors should have combo, search
            var lightNormal = new Dictionary<string, ThemeControlGroup>
            {
                //{ "Nav", new ThemeControlGroup(controls: nav, back: SystemColors.HotTrack, fore: SystemColors.Control) },
                { "Tips", new ThemeControlGroup(controls: tips, back: Color.Black, fore: Color.White) },
                { "Default2Color", new ThemeControlGroup(controls: dflt2, back: SystemColors.Control, fore: SystemColors.ControlText) },
                { "Buttons", new ThemeControlGroup(controls: buttons, back: SystemColors.Control, fore: SystemColors.ControlText) },
            };
            var lightActive = new Dictionary<string, ThemeControlGroup>
            {
                //{ "Nav", new ThemeControlGroup(controls: nav, back: Color.Green, fore: SystemColors.Control) },
                { "Tips", new ThemeControlGroup(controls: tips, back: Color.Black, fore: Color.White) },
                { "Default2Color", new ThemeControlGroup(controls: dflt2, back: Color.LightCyan, fore: SystemColors.ControlText) },
                { "Buttons", new ThemeControlGroup(controls: buttons, back: SystemColors.Control, fore: SystemColors.ControlText) },
            };
            var darkNormal = new Dictionary<string, ThemeControlGroup>
            {
                //{ "Nav", new ThemeControlGroup(controls: nav, back: Color.FromArgb(64,64,64), fore: SystemColors.Control) },
                { "Tips", new ThemeControlGroup(controls: tips, back: Color.LightSkyBlue, fore: SystemColors.ControlText) },
                { "Default2Color", new ThemeControlGroup(controls: dflt2, back: Color.Black, fore: Color.WhiteSmoke) },
                { "Buttons", new ThemeControlGroup(controls: buttons, back: Color.DimGray, fore: Color.WhiteSmoke) },
            };
            var darkActive = new Dictionary<string, ThemeControlGroup>
            {
                //{ "Nav", new ThemeControlGroup(controls: nav, back: SystemColors.HotTrack, fore: SystemColors.Control) },
                { "Tips", new ThemeControlGroup(controls: tips, back: Color.LightSkyBlue, fore: SystemColors.ControlText) },
                { "Default2Color", new ThemeControlGroup(controls: dflt2, back: Color.FromArgb(64,64,64), fore:Color.WhiteSmoke) },
                { "Buttons", new ThemeControlGroup(controls: buttons, back: Color.DimGray, fore: Color.WhiteSmoke) },
            };

            var themes = new Dictionary<string, Theme>
            {
                { "LightNormal", new Theme("LightNormal", lightNormal) },
                { "LightActive", new Theme("LightActive", lightActive) },
                { "DarkNormal", new Theme("DarkNormal", darkNormal) },
                { "DarkActive", new Theme("DarkActive", darkActive) }
            };

            themes["LightNormal"].ButtonMouseOverColor = Color.LightCyan;
            themes["LightActive"].ButtonMouseOverColor = Color.LightCyan;
            themes["DarkNormal"].ButtonMouseOverColor = Color.DarkGray;
            themes["DarkActive"].ButtonMouseOverColor = Color.DarkGray;

            themes["LightNormal"].ButtonBackColor = SystemColors.Control;
            themes["LightActive"].ButtonBackColor = SystemColors.Control;
            themes["DarkNormal"].ButtonBackColor = Color.DimGray;
            themes["DarkActive"].ButtonBackColor = Color.DimGray;

            themes["LightNormal"].ButtonClickedColor = SystemColors.Control;
            themes["LightActive"].ButtonClickedColor = SystemColors.Control;
            themes["DarkNormal"].ButtonClickedColor = Color.DimGray;
            themes["DarkActive"].ButtonClickedColor = Color.DimGray;

            return themes;

        }
    }
}
