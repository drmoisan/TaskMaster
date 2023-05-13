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
using System.Globalization;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.CompilerServices;

namespace SVGControl
{
    public enum AutoSize
    {
        Disabled = 0,
        MaintainAspectRatio = 1,
        AllowStretching = 2
    }

    [TypeConverter(typeof(SvgOptionsConverterFilepath))]
    public class SvgImageSelector : INotifyPropertyChanged
    {
        public SvgImageSelector() { }

        public SvgImageSelector(Size outer, Padding margin, AutoSize autoSize)
        {
            _outer = outer;
            Margin = margin;
            AutoSize = autoSize;
            Size = CalcInnerSize(outer, margin);
            Debug.WriteLine("SVGImage Initialized");
        }

        private SvgDocument _doc;
        private string _relativeImagePath;
        private string _absoluteImagePath;
        private Size _outer;
        private Size _original { get; set; }
        private Padding _margin;
        private bool _saveRendering = false;

        internal String AboluteImagePath
        {
            get { return _absoluteImagePath; }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [NotifyParentProperty(true)]
        [Editor(typeof(SVGFileNameEditor), typeof(UITypeEditor))]
        public String ImagePath
        {
            get
            {
                if (_absoluteImagePath == null)
                {
                    return "(none)";
                }
                else
                {
                    return _relativeImagePath;
                    //string workingDirectory = Environment.CurrentDirectory;
                    //string relativePath = _relativeImagePath.MakeRelativePath(workingDirectory);
                    //return relativePath;
                }

            }
            set
            {
                //string valueAbs = value.AbsoluteFromURI(anchorPath:);
                if (_relativeImagePath != value)
                {
                    if ((value == "")|(value == "(none)"))
                    {
                        _relativeImagePath = value;
                        _doc = null;
                    }
                    else
                    {
                        string valueAbs = value.AbsoluteFromURI(GetAnchorPath());
                        _doc = SvgDocument.Open(valueAbs);
                        _original = _doc.Draw().Size;
                        _absoluteImagePath = valueAbs;
                        _relativeImagePath = valueAbs.GetRelativeURI(GetAnchorPath());
                    }
                    NotifyPropertyChanged("ImagePath");
                }
            }
        }

        private string GetAnchorPath()
        {
            string workingDirectory = Environment.CurrentDirectory;
            List<string> directories = new List<string>(workingDirectory.Split(Path.DirectorySeparatorChar));
            if ((directories.Count > 2) && (directories[directories.Count - 2] == "bin"))
            {
                // Backwards traverse 2 levels
                workingDirectory = Directory.GetParent(workingDirectory).Parent.FullName;
            }
            if (workingDirectory[workingDirectory.Length - 1] != Path.DirectorySeparatorChar)
            {
                workingDirectory += Path.DirectorySeparatorChar;
            }
            return workingDirectory; 
        }

        [NotifyParentProperty(true)]
        internal Size Outer
        {
            get { return _outer; }
            set
            {
                _outer = value;
                Size = CalcInnerSize(Outer, _margin);
                NotifyPropertyChanged("Outer");
            }
        }

        [NotifyParentProperty(true)]
        public Size Size { get; set; }

        [NotifyParentProperty(true)]
        public Padding Margin
        {
            get { return _margin; }
            set
            {
                _margin = value;
                Size = CalcInnerSize(Outer, _margin);
                NotifyPropertyChanged("Margin");
            }
        }

        [NotifyParentProperty(true)]
        [DefaultValue(AutoSize.MaintainAspectRatio)]
        public AutoSize AutoSize { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

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
                //AddMargins(targetAdjusted.Width, targetAdjusted.Height);
                return _doc.Draw();
            }
            else
            { return null; }
        }

        public bool SaveRendering
        {
            get
            {
                return _saveRendering;
            }
            set
            {
                if ((value == true) && (_relativeImagePath != "") && (_doc != null))
                {
                    // Launch file save dialog with appropriate filters
                    SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                    saveFileDialog1.Filter = "Png Image|*.png|JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
                    saveFileDialog1.Title = "Save rendered Image File";
                    saveFileDialog1.InitialDirectory = Path.GetFullPath(_relativeImagePath);
                    saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_relativeImagePath);

                    saveFileDialog1.ShowDialog();

                    // If the file name is not an empty string open it for saving.
                    if (saveFileDialog1.FileName != "")
                    {
                        // Saves the Image via a FileStream created by the OpenFile method.
                        using FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                        {
                            Image image = Render();
                            // Saves the Image in the appropriate ImageFormat based upon the
                            // File type selected in the dialog box.
                            // NOTE that the FilterIndex property is one-based.
                            switch (saveFileDialog1.FilterIndex)
                            {
                                case 1:
                                    image.Save(fs, System.Drawing.Imaging.ImageFormat.Png); 
                                    break;
                                case 2:
                                    image.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
                                    break;
                                case 3:
                                    image.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
                                    break;
                                case 4:
                                    image.Save(fs, System.Drawing.Imaging.ImageFormat.Gif);
                                    break;
                            }
                        } // end using FileStream fs
                    }
                    
                }
                else if ((value == true) && (_relativeImagePath == ""))
                {
                    MessageBox.Show("Image path must have a value to save the rendering");
                    
                }
                else if(_doc == null)
                {
                    // MessageBox.Show("Image path does not refer to a valid SVG document");
                    
                }
                _saveRendering = false; 
                
            }
        }

        private void AddMargins(int widthCurrent, int heightCurrent)
        {
            var group = new SvgGroup();
            _doc.Children.Add(group);
            group.Children.Add(new SvgRectangle
            {
                X = - _margin.Left,
                Y = - _margin.Top,
                Width = widthCurrent + Margin.Left + Margin.Right,
                Height = heightCurrent + Margin.Top + Margin.Bottom,
                Stroke = new SvgColourServer(Color.Transparent),
                Fill = new SvgColourServer(Color.Transparent)
            });
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

namespace SVGControl.Defaults
{
    public static class GetDefault
    {
        public static byte[] SvgImage 
        { 
            get 
            {
                string svgXML = 
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16"">
  <defs>
    <style>.canvas{fill: none; opacity: 0;}.light-defaultgrey-10{fill: #212121; opacity: 0.1;}.light-defaultgrey{fill: #212121; opacity: 1;}.light-yellow{fill: #996f00; opacity: 1;}.light-blue{fill: #005dba; opacity: 1;}</style>
  </defs>
  <title>IconLightImage</title>
  <g id=""canvas"" class=""canvas"">
    <path class=""canvas"" d=""M16,16H0V0H16Z"" />
  </g>
  <g id=""level-1"">
    <path class=""light-defaultgrey-10"" d=""M14.5,2.5v12H1.5V2.5Z"" />
    <path class=""light-defaultgrey"" d=""M14.5,2H1.5L1,2.5v12l.5.5h13l.5-.5V2.5ZM14,14H2V3H14Z"" />
    <path class=""light-yellow"" d=""M12,5.5A1.5,1.5,0,1,1,10.5,4,1.5,1.5,0,0,1,12,5.5Z"" />
    <path class=""light-blue"" d=""M14,11.09V12.5l-2.819-2.82L8.988,11.877H8.281L4.814,8.41,2,11.225V9.811L4.461,7.35h.707l3.466,3.466,2.193-2.193h.707Z"" />
  </g>
</svg>";
                return Encoding.ASCII.GetBytes(svgXML);
            } 
        }
        
    }
}
