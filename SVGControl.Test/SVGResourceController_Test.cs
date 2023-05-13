using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SVGControl.Test
{
    [TestClass]
    public class SVGResourceController_Test
    {
        [TestMethod]
        public void SvgResources_Test()
        {
            var viewer = new SvgResourceViewer();
            var controller = new SvgResourceController(viewer);
        }
    }
}
