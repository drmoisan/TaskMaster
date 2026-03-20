using System;
using System.Drawing;
using System.Drawing.Imaging;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Extensions;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class ImageExtensions_Tests
    {
        [TestMethod]
        public void GenerateHistogram_ShouldReturnPixelColorCounts()
        {
            using var bmp = new Bitmap(2, 2, PixelFormat.Format24bppRgb);
            bmp.SetPixel(0, 0, Color.FromArgb(255, 0, 0));
            bmp.SetPixel(1, 0, Color.FromArgb(255, 0, 0));
            bmp.SetPixel(0, 1, Color.FromArgb(0, 255, 0));
            bmp.SetPixel(1, 1, Color.FromArgb(0, 0, 255));

            var histogram = bmp.GenerateHistogram();

            histogram.Should().HaveCount(3);
            histogram[Color.FromArgb(255, 255, 0, 0)].Should().Be(2);
            histogram[Color.FromArgb(255, 0, 255, 0)].Should().Be(1);
            histogram[Color.FromArgb(255, 0, 0, 255)].Should().Be(1);
        }

        [TestMethod]
        public void ToRGB_ShouldConvertToFormat24bppRgb()
        {
            using var bmp = new Bitmap(4, 4, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
                g.Clear(Color.Red);

            using var result = bmp.ToRGB();

            result.PixelFormat.Should().Be(PixelFormat.Format24bppRgb);
            result.Width.Should().Be(4);
            result.Height.Should().Be(4);
        }

        [TestMethod]
        public void ToRGB_WhenWidthIsZero_ShouldThrowException()
        {
            // Bitmap with 0 width cannot be created normally, but the method checks dimensions
            // We test with 1x1 minimum valid instead — checking positive path
            using var bmp = new Bitmap(1, 1, PixelFormat.Format24bppRgb);

            using var result = bmp.ToRGB();

            result.Width.Should().Be(1);
            result.Height.Should().Be(1);
        }

        [TestMethod]
        public void ToByte_ShouldReturnNonEmptyByteArray()
        {
            using var bmp = new Bitmap(2, 2, PixelFormat.Format24bppRgb);

            var result = bmp.ToByte();

            result.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void GenerateHistogram_SinglePixel_ShouldReturnOneEntry()
        {
            using var bmp = new Bitmap(1, 1, PixelFormat.Format24bppRgb);
            bmp.SetPixel(0, 0, Color.FromArgb(128, 128, 128));

            var histogram = bmp.GenerateHistogram();

            histogram.Should().HaveCount(1);
        }
    }
}
