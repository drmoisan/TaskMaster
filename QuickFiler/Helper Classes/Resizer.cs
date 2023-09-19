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
            _shiftRatio = shiftRatio;
            _stretchRatio = stretchRatio;
            _navShift = navShift;
            _navStretch = navStretch;
            _originalLocationNavOff = _originalLocation + _navShift;
            _originalSizeNavOff = _originalSize + _navStretch;
        }

        private Control _control;

        public void Transform(Point transformation)
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

        internal void TransformNavOn(Point transformation)
        {
            var shift = transformation.MultiplyRound(_shiftRatio);
            _control.Location = _originalLocation - shift;
            var remainingTransform = transformation - shift;
            _control.Size = _originalSize + remainingTransform.MultiplyRound(_stretchRatio);
        }

        internal void TransformNavOff(Point transformation)
        {
            var shift = transformation.MultiplyRound(_shiftRatio);
            _control.Location = _originalLocationNavOff - shift;
            var remainingTransform = transformation - shift;
            _control.Size = _originalSizeNavOff + remainingTransform.MultiplyRound(_stretchRatio);
        }

        private Enums.ToggleState _navState = Enums.ToggleState.On;
        private Point _originalLocation;
        private Size _originalSize;
        private Point _originalLocationNavOff;
        private Size _originalSizeNavOff;
        private Size _navShift;
        private Size _navStretch;
        
        private PointF _shiftRatio;
        private PointF _stretchRatio;
        

    }


}
