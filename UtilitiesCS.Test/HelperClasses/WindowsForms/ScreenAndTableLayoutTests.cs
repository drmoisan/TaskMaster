using System;
using System.Drawing;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Windows_Forms;

namespace UtilitiesCS.Test.HelperClasses.WindowsForms
{
    [TestClass]
    public class ScreenHelper_Tests
    {
        [TestMethod]
        public void Area_Rectangle_CalculatesCorrectly()
        {
            var rect = new Rectangle(0, 0, 100, 50);
            rect.Area().Should().Be(5000);
        }

        [TestMethod]
        public void Area_RectangleF_CalculatesCorrectly()
        {
            var rect = new RectangleF(0, 0, 10.5f, 20.0f);
            rect.Area().Should().Be(210.0f);
        }

        [TestMethod]
        public void Area_ZeroSizedRectangle_ReturnsZero()
        {
            var rect = new Rectangle(0, 0, 0, 0);
            rect.Area().Should().Be(0);
        }

        [TestMethod]
        public void Area_NegativeDimension_ReturnsNegative()
        {
            var rect = new Rectangle(0, 0, -10, 5);
            rect.Area().Should().Be(-50);
        }
    }

    [TestClass]
    public class TableLayoutHelper_Tests
    {
        [TestMethod]
        [STAThread]
        public void InsertSpecificRow_NegativeIndex_ThrowsArgumentOutOfRange()
        {
            var tlp = new System.Windows.Forms.TableLayoutPanel();
            tlp.RowCount = 2;
            var style = new System.Windows.Forms.RowStyle(
                System.Windows.Forms.SizeType.Absolute,
                30
            );

            Action act = () => tlp.InsertSpecificRow(-1, style);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        [STAThread]
        public void InsertSpecificRow_ZeroInsertCount_ThrowsArgumentOutOfRange()
        {
            var tlp = new System.Windows.Forms.TableLayoutPanel();
            tlp.RowCount = 2;
            var style = new System.Windows.Forms.RowStyle(
                System.Windows.Forms.SizeType.Absolute,
                30
            );

            Action act = () => tlp.InsertSpecificRow(0, style, insertCount: 0);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        [STAThread]
        public void InsertSpecificRow_ValidIndex_IncreasesRowCount()
        {
            var tlp = new System.Windows.Forms.TableLayoutPanel();
            tlp.RowCount = 2;
            tlp.ColumnCount = 1;
            var style = new System.Windows.Forms.RowStyle(
                System.Windows.Forms.SizeType.Absolute,
                30
            );

            tlp.InsertSpecificRow(0, style);
            tlp.RowCount.Should().Be(3);
        }
    }
}
