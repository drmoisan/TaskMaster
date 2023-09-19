using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace UtilitiesCS
{
    public static class DrawingExtensions
    {
        public static PointF Multiply(this PointF pt, Size sz)
        {
            return new PointF(pt.X * (float)sz.Width, pt.Y * (float)sz.Height);
        }

        public static Size MultiplyRound(this Point pt, PointF scaleRatio)
        {
            return Size.Round(new SizeF((float)pt.X * scaleRatio.X, (float)pt.Y * scaleRatio.Y));
        }
        
    }
}
