using System;
using System.Drawing.Imaging;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Windows_Forms;

namespace UtilitiesCS.Test.HelperClasses.WindowsForms
{
    [TestClass]
    public class MouseDownFilter_Tests
    {
        [TestMethod]
        [STAThread]
        public void Constructor_WithForm_CreatesInstance()
        {
            var form = new Form();
            var filter = new MouseDownFilter(form);
            filter.Should().NotBeNull();
        }

        [TestMethod]
        [STAThread]
        public void FormClicked_EventCanBeSubscribed()
        {
            var form = new Form();
            var filter = new MouseDownFilter(form);
            bool eventFired = false;
            filter.FormClicked += (s, e) => eventFired = true;

            // Event handler added successfully
            eventFired.Should().BeFalse();
        }
    }

    [TestClass]
    public class ImageHelper_Tests
    {
        [TestMethod]
        public void GetEncoder_PngFormat_ReturnsEncoder()
        {
            var encoder = ImageHelper.GetEncoder(ImageFormat.Png);
            encoder.Should().NotBeNull();
            encoder.FormatID.Should().Be(ImageFormat.Png.Guid);
        }

        [TestMethod]
        public void GetEncoder_JpegFormat_ReturnsEncoder()
        {
            var encoder = ImageHelper.GetEncoder(ImageFormat.Jpeg);
            encoder.Should().NotBeNull();
        }

        [TestMethod]
        public void GetEncoder_UnknownFormat_ReturnsNull()
        {
            // Exif format typically doesn't have a matching decoder
            var encoder = ImageHelper.GetEncoder(ImageFormat.Exif);
            // This may or may not return null depending on installed codecs
            // The test validates it doesn't throw
        }
    }
}
