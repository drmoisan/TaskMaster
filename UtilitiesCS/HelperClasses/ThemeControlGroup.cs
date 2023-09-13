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

        public ThemeControlGroup(string groupName, IList<Control> controls, Color backColor)
        {
            if (controls is null) { throw new ArgumentNullException(nameof(controls)); }
            if (controls.Count == 0) 
            { 
                throw new ArgumentOutOfRangeException(nameof(controls), $"To create a " + 
                    $"{nameof(ThemeControlGroup)}, the parameter "+
                    $"{nameof(controls)} must contain at least one {nameof(Control)}"); 
            }
            _groupName = groupName;
            _controls = controls;
            _backColor = backColor;
            _groupType = GroupTypeEnum.OneField;
        }

        public ThemeControlGroup(IList<Control> controls, Color foreColor, Color backColor)
        {
            if (controls is null) { throw new ArgumentNullException(nameof(controls)); }
            if (controls.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(controls), $"To create a " +
                    $"{nameof(ThemeControlGroup)}, the parameter " +
                    $"{nameof(controls)} must contain at least one {nameof(Control)}");
            }

            _controls = controls;
            _foreColor = foreColor;
            _backColor = backColor;
            _groupType = GroupTypeEnum.TwoField;
        }

        public ThemeControlGroup(IList<Control> controls,
                                 Color foreColorMain,
                                 Color backColorMain,
                                 Color foreColorAlt,
                                 Color backColorAlt,
                                 Func<bool> isAlt)
        {
            _controls = controls;
            _foreColorMain = foreColorMain;
            _backColorMain = backColorMain;
            _foreColorAlt = foreColorAlt;
            _backColorAlt = backColorAlt;
            IsAlt = isAlt;
            _groupType = GroupTypeEnum.TwoFieldAlt;
        }

        public ThemeControlGroup(IList<object> objects,
                                 Color foreColor,
                                 Color backColor,
                                 Action<object, Color, Color> objectSetter)
        { 
            _objects = objects;
            _foreColor = foreColor;
            _backColor = backColor;
            _groupType = GroupTypeEnum.TwoFieldObjWithSetter;
        }

                
        private enum GroupTypeEnum 
        { 
            Unsupported = 0,
            OneField = 1,
            TwoField = 2,
            TwoFieldAlt = 4,
            TwoFieldObjWithSetter = 8
        }
        private GroupTypeEnum _groupType;
        private IList<Control> _controls;
        private IList<object> _objects;
        private Action<object, Color, Color> ObjectSetter;
        private Color _foreColor;
        private Color _backColor;
        private Color _foreColorMain;
        private Color _backColorMain;
        private Color _foreColorAlt;
        private Color _backColorAlt;
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(_groupType),$"Unsupported group type");
            }
        }

        public void ApplyTheme(bool beginInvoke)
        {
            if (beginInvoke) { _controls[0].BeginInvoke(new Action(() => ApplyTheme())); }
            else { _controls[0].Invoke(new Action(() => ApplyTheme())); }
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

        private void ApplyThemeTwoFieldWithSetter()
        {
            _objects.ForEach(o => ObjectSetter(o,_foreColor, _backColor));
        }
    }
}
