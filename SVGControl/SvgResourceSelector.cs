using Fizzler;
using Svg;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SVGControl.PictureBoxSVG;

namespace SVGControl
{
    [TypeConverter(typeof(SvgOptionsConverterChooser))]
    public class SvgResourceSelector : INotifyPropertyChanged
    {
        #region Constructors

        public SvgResourceSelector(Size outer, Padding margin, AutoSize autoSize)
        {
            _renderer = new SvgRenderer(outer, margin, autoSize);
            _renderer.PropertyChanged += Renderer_PropertyChanged;
            Debug.WriteLine("SvgResourceSelector Initialized");
        }

        public SvgResourceSelector(Size outer, Padding margin, AutoSize autoSize, bool useDefaultImage)
        {
            _useDefaultImage = useDefaultImage;
            if (useDefaultImage)
            {
                _renderer = new SvgRenderer(Defaults.GetDefault.SvgImage,
                                            outer,
                                            margin,
                                            autoSize);
            }
            else { _renderer = new SvgRenderer(outer, margin, autoSize); }
            _renderer.PropertyChanged += Renderer_PropertyChanged;
            Debug.WriteLine("SvgResourceSelector Initialized");
        }

        #endregion

        private string _name;
        private SvgResource _svgResource;
        private SvgDocument _doc;
        private SvgRenderer _renderer;
        private bool _saveRendering = false;
        private bool _useDefaultImage = false;

        //[NotifyParentProperty(true)]
        //[Editor(typeof(SVGFileNameEditor), typeof(UITypeEditor))]
        //public String ImagePath
        //{
        //    get
        //    {
        //        if (_absoluteImagePath == null)
        //        {
        //            return "(none)";
        //        }
        //        else
        //        {
        //            return _relativeImagePath;
        //            //string workingDirectory = Environment.CurrentDirectory;
        //            //string relativePath = _relativeImagePath.MakeRelativePath(workingDirectory);
        //            //return relativePath;
        //        }

        //    }
        //    set
        //    {
        //        //string valueAbs = value.AbsoluteFromURI(anchorPath:);
        //        if (_relativeImagePath != value)
        //        {
        //            if ((value == "") | (value == "(none)"))
        //            {
        //                _relativeImagePath = value;
        //                _doc = null;
        //            }
        //            else
        //            {
        //                string valueAbs = value.AbsoluteFromURI(GetAnchorPath());
        //                _doc = SvgDocument.Open(valueAbs);
        //                _original = _doc.Draw().Size;
        //                _absoluteImagePath = valueAbs;
        //                _relativeImagePath = valueAbs.GetRelativeURI(GetAnchorPath());
        //            }
        //            NotifyPropertyChanged("ImagePath");
        //        }
        //    }
        //}

        [NotifyParentProperty(true)]
        [Editor(typeof(SvgDialogEditor), typeof(UITypeEditor))]
        public SvgResource SvgImage
        {
            get
            {
                return _svgResource;
            }
            set
            {
                _svgResource = value;
                if (value != null)
                {
                    if (_svgResource.SvgImage != null)
                    {
                        _renderer.Document = _svgResource.SvgImage;
                    }
                }
                else 
                { 
                    if (_useDefaultImage) { SetDefaultImage(); }
                    else { _renderer.Document = null; }
                }
            }
        }

        [DefaultValue(AutoSize.MaintainAspectRatio)]
        public AutoSize AutoSize { get => _renderer.AutoSize; set => _renderer.AutoSize = value; }
        public Size Size { get => _renderer.Size; set => _renderer.Size = value; }
        public Padding Margin { get => _renderer.Margin; set => _renderer.Margin = value; }
        public Bitmap Render() => _renderer.Render();
        internal Size Outer { get => _renderer.Outer; set => _renderer.Outer = value; }

        internal void SetDefaultImage()
        {
            _renderer.Document = SvgRenderer.GetSvgDocument(Defaults.GetDefault.SvgImage);
        }
                
        

        //public bool SaveRendering
        //{
        //    get
        //    {
        //        return _saveRendering;
        //    }
        //    set
        //    {
        //        if ((value == true) && (_relativeImagePath != "") && (_doc != null))
        //        {
        //            // Launch file save dialog with appropriate filters
        //            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
        //            saveFileDialog1.Filter = "Png Image|*.png|JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
        //            saveFileDialog1.Title = "Save rendered Image File";
        //            saveFileDialog1.InitialDirectory = Path.GetFullPath(_relativeImagePath);
        //            saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_relativeImagePath);

        //            saveFileDialog1.ShowDialog();

        //            // If the file name is not an empty string open it for saving.
        //            if (saveFileDialog1.FileName != "")
        //            {
        //                // Saves the Image via a FileStream created by the OpenFile method.
        //                using FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
        //                {
        //                    Image image = Render();
        //                    // Saves the Image in the appropriate ImageFormat based upon the
        //                    // File type selected in the dialog box.
        //                    // NOTE that the FilterIndex property is one-based.
        //                    switch (saveFileDialog1.FilterIndex)
        //                    {
        //                        case 1:
        //                            image.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
        //                            break;
        //                        case 2:
        //                            image.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
        //                            break;
        //                        case 3:
        //                            image.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
        //                            break;
        //                        case 4:
        //                            image.Save(fs, System.Drawing.Imaging.ImageFormat.Gif);
        //                            break;
        //                    }
        //                } // end using FileStream fs
        //            }

        //        }
        //        else if (_relativeImagePath == "")
        //        {
        //            MessageBox.Show("Image path must have a value to save the rendering");

        //        }
        //        else if (_doc == null)
        //        {
        //            // MessageBox.Show("Image path does not refer to a valid SVG document");

        //        }
        //        _saveRendering = false;

        //    }
        //}

        #region EventHandlers

        public event PropertyChangedEventHandler PropertyChanged;

        private void Renderer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

}
