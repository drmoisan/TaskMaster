using System.Drawing;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class DrawingExtensions_Tests
    {
        [TestMethod]
        public void Multiply_ShouldScalePointBySizeDimensions()
        {
            var pt = new PointF(2.0f, 3.0f);
            var sz = new Size(10, 20);

            var result = pt.Multiply(sz);

            result.X.Should().Be(20.0f);
            result.Y.Should().Be(60.0f);
        }

        [TestMethod]
        public void MultiplyRound_Point_ShouldScaleAndRound()
        {
            var pt = new Point(3, 4);
            var scale = new PointF(2.5f, 1.5f);

            var result = pt.MultiplyRound(scale);

            result.Width.Should().Be(8);
            result.Height.Should().Be(6);
        }

        [TestMethod]
        public void MultiplyRound_Size_ShouldScaleAndRound()
        {
            var sz = new Size(10, 20);
            var scale = new PointF(0.5f, 0.3f);

            var result = sz.MultiplyRound(scale);

            result.Width.Should().Be(5);
            result.Height.Should().Be(6);
        }

        [TestMethod]
        public void Multiply_WithZeroSize_ShouldReturnOrigin()
        {
            var pt = new PointF(5.0f, 10.0f);
            var sz = new Size(0, 0);

            var result = pt.Multiply(sz);

            result.X.Should().Be(0);
            result.Y.Should().Be(0);
        }

        [TestMethod]
        public void MultiplyRound_WithUnitScale_ShouldReturnSameValues()
        {
            var pt = new Point(7, 11);
            var scale = new PointF(1.0f, 1.0f);

            var result = pt.MultiplyRound(scale);

            result.Width.Should().Be(7);
            result.Height.Should().Be(11);
        }
    }
}
