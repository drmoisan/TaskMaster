using System;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.HelperClasses.WindowsForms
{
    [TestClass]
    public class ControlPosition_Tests
    {
        #region Constructors

        [TestMethod]
        public void DefaultConstructor_CreatesInstance()
        {
            var cp = new ControlPosition();
            cp.Should().NotBeNull();
        }

        [TestMethod]
        public void ParameterizedConstructor_SetsAllProperties()
        {
            var margin = new Padding(1, 2, 3, 4);
            var padding = new Padding(5, 6, 7, 8);
            var cp = new ControlPosition(10, 20, 100, 50, margin, padding);

            cp.Left.Should().Be(10);
            cp.Top.Should().Be(20);
            cp.Width.Should().Be(100);
            cp.Height.Should().Be(50);
            cp.Margin.Should().Be(margin);
            cp.Padding.Should().Be(padding);
        }

        #endregion

        #region Properties

        [TestMethod]
        public void Left_SetAndGet()
        {
            var cp = new ControlPosition();
            cp.Left = 42;
            cp.Left.Should().Be(42);
        }

        [TestMethod]
        public void Top_SetAndGet()
        {
            var cp = new ControlPosition();
            cp.Top = 99;
            cp.Top.Should().Be(99);
        }

        [TestMethod]
        public void Width_SetAndGet()
        {
            var cp = new ControlPosition();
            cp.Width = 200;
            cp.Width.Should().Be(200);
        }

        [TestMethod]
        public void Height_SetAndGet()
        {
            var cp = new ControlPosition();
            cp.Height = 150;
            cp.Height.Should().Be(150);
        }

        [TestMethod]
        public void Margin_SetAndGet()
        {
            var cp = new ControlPosition();
            var margin = new Padding(5);
            cp.Margin = margin;
            cp.Margin.Should().Be(margin);
        }

        [TestMethod]
        public void Padding_SetAndGet()
        {
            var cp = new ControlPosition();
            var padding = new Padding(10);
            cp.Padding = padding;
            cp.Padding.Should().Be(padding);
        }

        #endregion

        #region FromTemplate

        [TestMethod]
        public void FromTemplate_CellZeroZero_ReturnsTemplatePosition()
        {
            var margin = new Padding(2, 3, 2, 3);
            var padding = new Padding(0);
            var template = new ControlPosition(10, 20, 100, 50, margin, padding);
            template.FixedLeft = template.Left - margin.Left;
            template.FixedTop = template.Top - margin.Top;

            var result = ControlPosition.FromTemplate(template, 0, 0);

            result.Left.Should().Be(template.FixedLeft);
            result.Top.Should().Be(template.FixedTop);
            result.Width.Should().Be(template.Width);
            result.Height.Should().Be(template.Height);
        }

        [TestMethod]
        public void FromTemplate_CellOneZero_OffsetsVertically()
        {
            var margin = new Padding(2, 3, 2, 3);
            var padding = new Padding(0);
            var template = new ControlPosition(10, 20, 100, 50, margin, padding);
            template.FixedLeft = template.Left - margin.Left;
            template.FixedTop = template.Top - margin.Top;

            var result = ControlPosition.FromTemplate(template, 1, 0);

            var expectedTop = template.FixedTop + (template.Height + margin.Vertical) * 1;
            result.Top.Should().Be(expectedTop);
            result.Left.Should().Be(template.FixedLeft);
        }

        [TestMethod]
        public void FromTemplate_CellZeroOne_OffsetsHorizontally()
        {
            var margin = new Padding(2, 3, 2, 3);
            var padding = new Padding(0);
            var template = new ControlPosition(10, 20, 100, 50, margin, padding);
            template.FixedLeft = template.Left - margin.Left;
            template.FixedTop = template.Top - margin.Top;

            var result = ControlPosition.FromTemplate(template, 0, 1);

            var expectedLeft = template.FixedLeft + (template.Width + margin.Horizontal) * 1;
            result.Left.Should().Be(expectedLeft);
            result.Top.Should().Be(template.FixedTop);
        }

        #endregion

        #region Set (static)

        [TestMethod]
        [STAThread]
        public void Set_WithControlPosition_SetsControlProperties()
        {
            var control = new Label();
            var cp = new ControlPosition(5, 10, 50, 25, new Padding(1), new Padding(2));

            ControlPosition.Set(control, cp);

            control.Left.Should().Be(5);
            control.Top.Should().Be(10);
            control.Width.Should().Be(50);
            control.Height.Should().Be(25);
        }

        #endregion

        #region CreateTemplate

        [TestMethod]
        [STAThread]
        public void CreateTemplate_FromControl_CapturesProperties()
        {
            var control = new Label();
            control.Left = 15;
            control.Top = 25;
            control.Width = 120;
            control.Height = 40;
            control.Margin = new Padding(3, 4, 3, 4);

            var template = ControlPosition.CreateTemplate(control);

            template.Left.Should().Be(15);
            template.Top.Should().Be(25);
            template.Width.Should().Be(120);
            template.Height.Should().Be(40);
            template.FixedLeft.Should().Be(15 - 3);
            template.FixedTop.Should().Be(25 - 4);
        }

        #endregion
    }

    [TestClass]
    public class ControlResizer_Tests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_CreatesInstance()
        {
            var resizer = new ControlResizer();
            resizer.Should().NotBeNull();
        }

        #endregion

        #region FindAllControls

        [TestMethod]
        [STAThread]
        public void FindAllControls_WithNestedControls_PopulatesDict()
        {
            var resizer = new ControlResizer();
            var form = new Form { Width = 400, Height = 300 };
            var panel = new Panel
            {
                Name = "panel1",
                Width = 200,
                Height = 100,
            };
            var button = new Button
            {
                Name = "button1",
                Width = 80,
                Height = 30,
            };
            panel.Controls.Add(button);
            form.Controls.Add(panel);

            // Should not throw
            resizer.FindAllControls(form);
        }

        #endregion

        #region ResizeAllControls

        [TestMethod]
        [STAThread]
        public void ResizeAllControls_AfterFind_DoesNotThrow()
        {
            var resizer = new ControlResizer();
            var form = new Form { Width = 400, Height = 300 };
            var panel = new Panel
            {
                Name = "panel1",
                Width = 200,
                Height = 100,
            };
            form.Controls.Add(panel);

            resizer.FindAllControls(form);
            // Should not throw
            resizer.ResizeAllControls(form);
        }

        #endregion

        #region SetResizeDimensions

        [TestMethod]
        [STAThread]
        public void SetResizeDimensions_KnownControl_ReturnsTrue()
        {
            var resizer = new ControlResizer();
            var form = new Form { Width = 400, Height = 300 };
            var label = new Label
            {
                Name = "label1",
                Width = 100,
                Height = 30,
            };
            form.Controls.Add(label);
            resizer.FindAllControls(form);

            var result = resizer.SetResizeDimensions(
                label,
                ControlResizer.ResizeDimensions.Size,
                false
            );

            result.Should().BeTrue();
        }

        [TestMethod]
        [STAThread]
        public void SetResizeDimensions_UnknownControl_ReturnsFalse()
        {
            var resizer = new ControlResizer();
            var form = new Form { Width = 400, Height = 300 };
            resizer.FindAllControls(form);

            var unknownLabel = new Label
            {
                Name = "unknown",
                Width = 100,
                Height = 30,
            };
            var result = resizer.SetResizeDimensions(
                unknownLabel,
                ControlResizer.ResizeDimensions.All,
                false
            );

            result.Should().BeFalse();
        }

        #endregion

        #region ResizeDimensions Enum

        [TestMethod]
        public void ResizeDimensions_EnumValues()
        {
            ((int)ControlResizer.ResizeDimensions.None).Should().Be(0);
            ((int)ControlResizer.ResizeDimensions.Position).Should().Be(3);
            ((int)ControlResizer.ResizeDimensions.Size).Should().Be(12);
            ((int)ControlResizer.ResizeDimensions.Font).Should().Be(16);
            ((int)ControlResizer.ResizeDimensions.All).Should().Be(31);
        }

        #endregion
    }
}
