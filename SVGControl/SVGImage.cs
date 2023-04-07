using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.VisualStyles;
using System.Drawing.Printing;
using System.Diagnostics;
using Svg;
using Fizzler;

namespace SVGControl
{
    public enum AutoSize
    {
        Disabled = 0,
        MaintainAspectRatio = 1,
        AllowStretching = 2
    }
        
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class SVGImage : TypeConverter, INotifyPropertyChanged
    {
        public SVGImage() { }

        public SVGImage(Size outer, Padding margin, AutoSize autoSize)
        {
            _outer = outer;
            Margin = margin;
            AutoSize = autoSize;
            Size = CalcInnerSize(outer, margin);
            Debug.WriteLine("SVGImage Initialized");
        }

        private SvgDocument _doc;
        private String _imagePath;
        private Size _outer;
        private Size _original { get; set; }
        private Padding _margin;

        [NotifyParentProperty(true)]
        [Editor(typeof(SVGFileNameEditor), typeof(UITypeEditor))]
        public String ImagePath 
        {
            get { return _imagePath; }
            set 
            {
                if (_imagePath != value)
                {
                    _imagePath = value;
                    if (_imagePath == "")
                    {
                        _doc = null;
                    }
                    else
                    {
                        _doc = SvgDocument.Open(_imagePath);
                        _original = _doc.Draw().Size;
                    }
                }
            } 
        }

        internal Size Outer 
        {
            get { return _outer; }
            set { _outer = value; }
        }

        [NotifyParentProperty(true)]
        public Size Size { get; set; }

        [NotifyParentProperty(true)]
        public Padding Margin 
        {
            get {return _margin; }
            set 
            {
                _margin = value;
                Size = CalcInnerSize(Outer, _margin);
            } 
        }

        [NotifyParentProperty(true)]
        [DefaultValue(AutoSize.MaintainAspectRatio)]
        public AutoSize AutoSize { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context,
                                         System.Globalization.CultureInfo culture,
                                         object value,
                                         Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return "";
            }
            return base.ConvertTo(context, culture, value, destinationType); 
        }

        private Size CalcInnerSize(Size outer, Padding margin) 
        {
            var innerWidth = outer.Width - margin.Left - margin.Right;
            var innerHeight = outer.Height - margin.Top - margin.Bottom;             
            return new Size(innerWidth, innerHeight);
        }
                        
        public Bitmap Render()
        {
            if (_doc == null)
            {
                return null;
            }
            else if ((AutoSize == AutoSize.Disabled) || (Size == null) || (Size.Height == 0) || (Size.Width == 0))
            { 
                return _doc.Draw(); 
            }
            else if (AutoSize == AutoSize.AllowStretching) 
            {
                _doc.Width = Size.Width;
                _doc.Height = Size.Height;
                return _doc.Draw();
            }
            else if (AutoSize == AutoSize.MaintainAspectRatio)
            {
                var targetAdjusted = AdjustSizeProportionately(_original, Size);
                _doc.Width = targetAdjusted.Width;
                _doc.Height = targetAdjusted.Height;
                return _doc.Draw();
            }
            else
            { return null; }
        }
        
        private Size AdjustSizeProportionately(Size proportions, Size targetSize)
        {
            if ((targetSize.Height > 0) && (targetSize.Width > 0) && ((proportions.Height != targetSize.Height) || (proportions.Width != targetSize.Width)))
            {
                int widthAspect = (int)(targetSize.Height * proportions.Width / (double)proportions.Height);
                if (widthAspect < targetSize.Width)
                {
                    return new Size(widthAspect, targetSize.Height);
                    
                }
                else
                {
                    int heightAspect = (int)(targetSize.Width * proportions.Height / (double)proportions.Width);
                    return new Size(targetSize.Width, heightAspect);
                }
            }
            return proportions;
        }
    }
}
