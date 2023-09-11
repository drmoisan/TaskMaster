using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilitiesCS
{
    public class ControlPosition
    {
        public ControlPosition() { }

        public ControlPosition(int left, int top, int width, int height, Padding margin, Padding padding)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
            Margin = margin;
            Padding = padding;
        }

        public static ControlPosition CreateTemplate(Control control)
        {            
            var cp = new ControlPosition(control.Left,
                                         control.Top,
                                         control.Width,
                                         control.Height,
                                         control.Margin,
                                         control.Padding);
            cp.FixedLeft = cp.Left - cp.Margin.Left;
            cp.FixedTop = cp.Top - cp.Margin.Top;
            return cp;
        }

        /// <summary>
        /// Method sets the position of a control based on the virtual grid defined
        /// by this <see cref="ControlPosition"/>. cellVertical and cellHorizontal 
        /// are zero based integer values that represent the grid position.
        /// </summary>
        /// <param name="control">Targeted <seealso cref="Control"/> to receive position</param>
        /// <param name="cellVertical">Zero based vertical position</param>
        /// <param name="cellHorizontal">Zero based horizontal position</param>
        public void Set(Control control, int cellVertical, int cellHorizontal)
        {
            var cp = FromTemplate(this, cellVertical, cellHorizontal);
            Set(control, cp);
        }
        
        public static void Set(Control control, ControlPosition template, int cellVertical, int cellHorizontal)
        {
            var cp = FromTemplate(template, cellVertical, cellHorizontal);
            Set(control, cp);
        }

        public static void Set(Control control, ControlPosition cp)
        {
            control.Left = cp.Left;
            control.Top = cp.Top;
            control.Width = cp.Width;
            control.Height = cp.Height;
            control.Margin = cp.Margin;
            control.Padding = cp.Padding;
        }
        
        public static ControlPosition FromTemplate(ControlPosition template, int cellVertical, int cellHorizontal)
        {
            var top = template.FixedTop + (template.Height + template.Margin.Vertical) * cellVertical;
            var left = template.FixedLeft + (template.Width + template.Margin.Horizontal) * cellHorizontal;

            var cp = new ControlPosition(left,
                                         top,
                                         template.Width,
                                         template.Height,
                                         template.Margin,
                                         template.Padding);

            cp.FixedLeft = cp.Left - cp.Margin.Left;
            cp.FixedTop = cp.Top - cp.Margin.Top;
            
            return cp;
        }

        private int _left;
        public int Left { get => _left; set => _left = value; }

        private int _top;
        public int Top { get => _top; set => _top = value; }

        private int _width;
        public int Width { get => _width; set => _width = value; }

        private int _height;
        public int Height { get => _height; set => _height = value; }

        private Padding _margin;
        public Padding Margin { get => _margin; set => _margin = value; }

        private Padding _padding;
        public Padding Padding { get => _padding; set => _padding = value; }

        private int _variableVertical;
        public int VariableVertical { get => _variableVertical; set => _variableVertical = value; }

        private int _variableHorizontal;
        public int VariableHorizontal { get => _variableHorizontal; set => _variableHorizontal = value; }

        private int _fixedLeft;
        public int FixedLeft { get => _fixedLeft; set => _fixedLeft = value; }

        private int _fixedTop;
        public int FixedTop { get => _fixedTop; set => _fixedTop = value; }
        
    }
}
