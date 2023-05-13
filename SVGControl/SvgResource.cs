using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Svg;
using System.Windows.Forms;
using System.Collections;
using System.Globalization;

namespace SVGControl
{
    public class SvgResource
    {
        // Constructor overloads
        public SvgResource() { }
        
        public SvgResource(string name, byte[] svgImage)
        {
            Name = name;
            SvgImage = SvgRenderer.GetSvgDocument(svgImage);
        }

        public SvgResource(string name, SvgDocument svgImage)
        {
            Name = name;
            SvgImage = svgImage;
        }

        // Private variables
        private Image _smallImage;
        private Size _smallSize;
        private SvgRenderer _smallRenderer;
        private Image _largeImage;
        private Size _largeSize;
        private SvgRenderer _largeRenderer;

        // Public methods and properties
        public string Name { get; set; }

        public Image SmallImage { get => _smallImage ?? throw new ArgumentNullException($"{nameof(SmallImage)} not rendered"); }
        
        public Image LargeImage { get => _largeImage ?? throw new ArgumentNullException($"{nameof(LargeImage)} not rendered"); }
        
        public SvgDocument SvgImage { get; set; }
        
        public Size SmallSize 
        { 
            get => _smallSize;
            set 
            { 
                _smallSize = value; 
                _smallRenderer = new SvgRenderer(doc: SvgImage, size: value, autoSize: AutoSize.MaintainAspectRatio);
                _smallImage = _smallRenderer.Render();
            }
        }
        
        public Size LargeSize 
        { 
            get => _largeSize;
            set
            { 
                _largeSize = value;
                _largeRenderer = new SvgRenderer(doc: SvgImage, size: value, autoSize: AutoSize.MaintainAspectRatio);
                _largeImage = _largeRenderer.Render();
            } 
        }

        public void RenderImages()
        {
            if (_smallRenderer != null) { _smallRenderer.Render(); }
            if (_largeRenderer != null) { _largeRenderer.Render(); }
        }

    }

    public static class SvgDialog
    {
        public static SvgResource SelectSvgResource() 
        {
            var viewer = new SvgResourceViewer();
            var controller = new SvgResourceController(viewer);
            DialogResult result = viewer.ShowDialog();
            if (result == DialogResult.OK)
            {
                return controller.Selection;
            }
            else { return null; }
        }

        public static SvgResource SelectSvgResource(IList<SvgResource> svgResources)
        {
            var viewer = new SvgResourceViewer();
            var controller = new SvgResourceController(viewer, svgResources);
            DialogResult result = viewer.ShowDialog();
            if (result == DialogResult.OK)
            {
                return controller.Selection;
            }
            else { return null; }
        }

        public static IList<SvgResource> GetSvgResources()
        {
            var svgResources = Properties.Resources.ResourceManager
                    .GetResourceSet(CultureInfo.CurrentCulture, true, true)
                    .Cast<DictionaryEntry>()
                    .Where(x => x.Value.GetType() == typeof(byte[]))
                    .Select(x => new SvgResource(x.Key.ToString(), (byte[])x.Value))
                    .ToList();
            return svgResources;
        }

        public static IList<SvgResource> GetSvgResources(Size small, Size large)
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
    }
}
