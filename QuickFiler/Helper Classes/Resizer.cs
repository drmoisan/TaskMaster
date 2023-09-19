using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using UtilitiesCS;

namespace QuickFiler
{
    internal class Resizer
    {
        public Resizer(Control control, PointF shiftRatio, PointF stretchRatio, Size navShift, Size navStretch)
        {
            _control = control;
            _originalLocation = control.Location;
            _originalSize = control.Size;
            _originalLowerRight = _originalLocation + _originalSize;
            _shiftRatio = shiftRatio;
            _stretchRatio = stretchRatio;
            _navShift = navShift;
            _navStretch = navStretch;
            _originalLocationNavOff = _originalLocation + _navShift;
            _originalSizeNavOff = _originalSize + _navStretch;
            _originalLowerRightNavOff = _originalLocationNavOff + _originalSizeNavOff;
        }

        private Control _control;
        private Enums.ToggleState _navState = Enums.ToggleState.On;
        private Point _originalLocation;
        private Point _originalLowerRight;
        private Size _originalSize;
        private Point _originalLocationNavOff;
        private Point _originalLowerRightNavOff;
        private Size _originalSizeNavOff;
        private Size _navShift;
        private Size _navStretch;
        
        private PointF _shiftRatio;
        private PointF _stretchRatio;

        public void Transform(Size transformation)
        {
            if (_navState.HasFlag(Enums.ToggleState.On)) { TransformNavOn(transformation); }
            else { TransformNavOff(transformation); }
        }

        public void ToggleNav()
        {
            _navState ^= Enums.ToggleState.On;
        }

        public void ToggleNav(Enums.ToggleState desiredState)
        {
            _navState = desiredState;
        }

        internal void TransformNavOn(Size transformation)
        {
            var shift = transformation.MultiplyRound(_shiftRatio);
            _control.Location = _originalLocation + shift;
            var stretch = transformation.MultiplyRound(_stretchRatio);
            _control.Size = _originalSize + stretch - shift;
        }

        internal void TransformNavOff(Size transformation)
        {
            var shift = transformation.MultiplyRound(_shiftRatio);
            _control.Location = _originalLocationNavOff + shift;
            var stretch = transformation.MultiplyRound(_stretchRatio);
            _control.Size = _originalSizeNavOff + stretch - shift;
        }

        

    }


}
