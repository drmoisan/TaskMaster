using System;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS;


namespace UtilitiesCS
{
    public class ThemeControlGroup
    {
        private ThemeControlGroup() { }

        public ThemeControlGroup(IList<Control> controls, Color back)
        {
            if (controls is null) { throw new ArgumentNullException(nameof(controls)); }
            if (controls.Count == 0) 
            { 
                throw new ArgumentOutOfRangeException(nameof(controls), $"To create a " + 
                    $"{nameof(ThemeControlGroup)}, the parameter "+
                    $"{nameof(controls)} must contain at least one {nameof(Control)}"); 
            }
            _controls = controls;
            _backColor = back;
            _groupType = GroupTypeEnum.OneField;
        }

        public ThemeControlGroup(IList<Control> controls, Color fore, Color back)
        {
            if (controls is null) { throw new ArgumentNullException(nameof(controls)); }
            if (controls.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(controls), $"To create a " +
                    $"{nameof(ThemeControlGroup)}, the parameter " +
                    $"{nameof(controls)} must contain at least one {nameof(Control)}");
            }

            _controls = controls;
            _foreColor = fore;
            _backColor = back;
            _groupType = GroupTypeEnum.TwoField;
        }

        public ThemeControlGroup(IList<Control> controls,
                                 Color foreMain,
                                 Color backMain,
                                 Color foreAlt,
                                 Color backAlt,
                                 Func<bool> isAlt)
        {
            _controls = controls;
            _foreColorMain = foreMain;
            _backColorMain = backMain;
            _foreColorAlt = foreAlt;
            _backColorAlt = backAlt;
            IsAlt = isAlt;
            _groupType = GroupTypeEnum.TwoFieldAlt;
        }

        public ThemeControlGroup(IList<object> objects,
                                 Color fore,
                                 Color back,
                                 Action<IList<object>, Color, Color> objectSetter)
        { 
            _objects = objects;
            _foreColor = fore;
            _backColor = back;
            ObjectSetter = objectSetter;
            _groupType = GroupTypeEnum.TwoFieldObjWithSetter;
        }

        public ThemeControlGroup(Microsoft.Web.WebView2.WinForms.WebView2 webView2,
                                 Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme web2ViewScheme,
                                 Action<Enums.ToggleState> htmlConverter,
                                 Enums.ToggleState htmlDark)
        {
            _webView2 = webView2;
            _controls = new List<Control> { webView2 };
            _web2ViewScheme = web2ViewScheme;
            _htmlConverter = htmlConverter;
            _htmlDark = htmlDark;
            _groupType = GroupTypeEnum.WebView2;
        }
                
        private enum GroupTypeEnum 
        { 
            Unsupported = 0,
            OneField = 1,
            TwoField = 2,
            TwoFieldAlt = 4,
            TwoFieldObjWithSetter = 8,
            WebView2 = 16
        }
        private GroupTypeEnum _groupType;
        
        private IList<Control> _controls;
        private IList<Button> _buttons;
        private IList<object> _objects;
        private Action<IList<object>, Color, Color> ObjectSetter;
        private Color _foreColor;
        private Color _backColor;
        private Color _hoverColor;
        private Color _foreColorMain;
        private Color _backColorMain;
        private Color _foreColorAlt;
        private Color _backColorAlt;
        private Microsoft.Web.WebView2.WinForms.WebView2 _webView2;
        Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme _web2ViewScheme;
        Action<Enums.ToggleState> _htmlConverter;
        Enums.ToggleState _htmlDark;
        private Func<bool> IsAlt;
        

        private string _groupName;
        public string GroupName { get => _groupName; set => _groupName = value; }
        
        public void ApplyTheme()
        {
            switch (_groupType)
            {
                case GroupTypeEnum.OneField:
                    ApplyThemeOneField();
                    break;
                case GroupTypeEnum.TwoField:
                    ApplyThemeTwoField();
                    break;
                case GroupTypeEnum.TwoFieldAlt:
                    ApplyThemeTwoFieldAlt();
                    break;
                case GroupTypeEnum.TwoFieldObjWithSetter:
                    ApplyThemeTwoFieldWithSetter();
                    break;
                case GroupTypeEnum.WebView2:
                    ApplyThemeWebView2();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_groupType),$"Unsupported group type");
            }
        }

        public void ApplyTheme(bool async)
        {
            if (_controls is not null) 
            { 
                if (async) { _controls[0].BeginInvoke(new Action(() => ApplyTheme())); }
                else { _controls[0].Invoke(new Action(() => ApplyTheme())); }
            }
            else { ApplyTheme(); }
        }

        private void ApplyThemeOneField()
        {
            _controls.ForEach(c => c.BackColor = _backColor);
        }
        
        private void ApplyThemeTwoField()
        {
            _controls.ForEach(c => 
            { 
                c.ForeColor = _foreColor;
                c.BackColor = _backColor; 
            });
        }

        private void ApplyThemeTwoFieldAlt()
        {
            if (IsAlt())
            {
                _controls.ForEach(c =>
                {
                    c.ForeColor = _foreColorAlt;
                    c.BackColor = _backColorAlt;
                });
            }
            else
            {
                _controls.ForEach(c =>
                {
                    c.ForeColor = _foreColorMain;
                    c.BackColor = _backColorMain;
                });
            }
        }

        private void ApplyThemeTwoFieldWithSetter() => ObjectSetter(_objects, _foreColor, _backColor);

        private void ApplyThemeWebView2() 
        {
            if (_webView2.CoreWebView2 is not null)
            {
                _webView2.CoreWebView2.Profile.PreferredColorScheme = _web2ViewScheme;
                _htmlConverter(_htmlDark);
            }
        }


    }
}
