using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Svg;

namespace SVGControl
{
    public class SvgResourceController
    {
        public SvgResourceController(SvgResourceViewer viewer) 
        {
            // Grab handle on viewer and hook controller
            _viewer = viewer;
            _viewer.SetController(this);

            // Initialize list of svgResources and send the model to the Olv
            _svgResources = SvgDialog.GetSvgResources(viewer.imageListSmall.ImageSize, viewer.imageListLarge.ImageSize);
            _viewer.olv.SetObjects(_svgResources);

            // Initialize viewer
            InitializeViewer();
        }

        public SvgResourceController(SvgResourceViewer viewer, IList<SvgResource> svgResources)
        {
            // Grab handle on viewer and hook controller
            _viewer = viewer;
            _viewer.SetController(this);

            // Grab handle on Svg resources and send the model to the Olv
            _svgResources = svgResources;
            _viewer.olv.SetObjects(_svgResources);

            // Initialize viewer
            InitializeViewer();
        }
                
        private SvgResourceViewer _viewer;
        private IList<SvgResource> _svgResources;
        private TypedColumn<SvgResource> _tResourceName;
        private TypedColumn<SvgResource> _tResourceImage;
        private TypedObjectListView<SvgResource> _tlv;
        private SvgResource _selection = null;

        public SvgResource Selection { get => _selection; }

        public IList<SvgResource> GetSvgResources(Size small, Size large) 
        {
            var svgResources = Properties.Resources.ResourceManager
                    .GetResourceSet(CultureInfo.CurrentCulture, true, true)
                    .Cast<DictionaryEntry>()
                    .Where(x => x.Value.GetType() == typeof(byte[]))
                    .Select(x => new SvgResource(x.Key.ToString(), (byte[])x.Value))
                    .ToList();
            foreach (SvgResource svgResource in svgResources)
            {
                svgResource.SmallSize = small;
                svgResource.LargeSize = large;
            }
            return svgResources;
        }

        public IList<SvgResource> GetSvgResources()
        {
            var svgResources = Properties.Resources.ResourceManager
                    .GetResourceSet(CultureInfo.CurrentCulture, true, true)
                    .Cast<DictionaryEntry>()
                    .Where(x => x.Value.GetType() == typeof(byte[]))
                    .Select(x => new SvgResource(x.Key.ToString(), (byte[])x.Value))
                    .ToList();
            return svgResources;
        }

        public void InitializeViewer()
        {
            InitializeTypedVars();
            InitializeImageGetter();
            InitializeEventHandlers();
            _viewer.olv.AutoResizeColumns();
        }

        internal void InitializeEventHandlers()
        {
            _viewer.Ok.Click += Ok_Click;
            _viewer.Cancel.Click += Cancel_Click;
        }

        public void Ok_Click(object sender, EventArgs e)
        {
            _selection = (SvgResource)_viewer.olv.SelectedObject;
            _viewer.Close();
        }

        public void Cancel_Click(object sender, EventArgs e)
        {
            _viewer.Close();
        }

        internal void InitializeImageGetter()
        {
            _viewer.resourceImage.AspectGetter = delegate (object row)
            {
                return ((SvgResource)row).Name;
            };
            //_tResourceImage.AspectGetter = delegate (SvgResource x)
            //{
            //    return x.Name;
            //};

            _viewer.resourceImage.AspectToStringConverter = delegate (object x) { return String.Empty; };

            _viewer.resourceImage.ImageGetter = delegate (object row)
            {
                SvgResource x = ((SvgResource)row);
                if (!_viewer.olv.LargeImageList.Images.ContainsKey(x.Name))
                {
                    _viewer.olv.LargeImageList.Images.Add(x.Name, x.LargeImage);
                    _viewer.olv.SmallImageList.Images.Add(x.Name, x.SmallImage);
                }
                return x.Name;
            };

            
            //_tResourceImage.ImageGetter = delegate (SvgResource x) 
            //{
            //    string key = x.Name;
            //    if (!_viewer.olv.LargeImageList.Images.ContainsKey(x.Name))
            //    {
            //        _viewer.olv.LargeImageList.Images.Add(x.Name, x.LargeImage);
            //        _viewer.olv.SmallImageList.Images.Add(x.Name, x.SmallImage);
            //    }
            //    return x.Name; 
            //};
        }

        private void InitializeTypedVars()
        {
            _tResourceName = new BrightIdeasSoftware.TypedColumn<SvgResource>(_viewer.resourceName);
            _tResourceImage = new BrightIdeasSoftware.TypedColumn<SvgResource>(_viewer.resourceImage);
            _tlv = new BrightIdeasSoftware.TypedObjectListView<SvgResource>(_viewer.olv);
        }

    }
}
