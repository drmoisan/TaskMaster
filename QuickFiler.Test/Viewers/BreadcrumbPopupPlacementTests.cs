using System;
using System.Drawing;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Failure-first pure popup placement contracts for issue #400.</summary>
    [TestClass]
    public sealed class BreadcrumbPopupPlacementTests
    {
        [TestMethod]
        public void Calculate_FullHeightFitsBelow_PrefersBelow()
        {
            // Act
            Placement placement = Calculate(
                new Rectangle(100, 100, 200, 25),
                new Rectangle(0, 0, 800, 600),
                new Size(300, 200)
            );

            // Assert
            placement.OpensBelow.Should().BeTrue();
            placement.Bounds.Should().Be(new Rectangle(100, 125, 300, 200));
        }

        [TestMethod]
        public void Calculate_BelowInsufficientAndFullHeightFitsAbove_UsesAbove()
        {
            // Act
            Placement placement = Calculate(
                new Rectangle(100, 400, 200, 25),
                new Rectangle(0, 0, 800, 600),
                new Size(300, 300)
            );

            // Assert
            placement.OpensBelow.Should().BeFalse();
            placement.Bounds.Should().Be(new Rectangle(100, 100, 300, 300));
        }

        [TestMethod]
        public void Calculate_NeitherFits_UsesGreaterAvailableSideAndClampsHeight()
        {
            // Act
            Placement placement = Calculate(
                new Rectangle(100, 300, 200, 25),
                new Rectangle(0, 0, 800, 500),
                new Size(300, 400)
            );

            // Assert
            placement.OpensBelow.Should().BeFalse();
            placement.Bounds.Should().Be(new Rectangle(100, 0, 300, 300));
        }

        [TestMethod]
        public void Calculate_EqualSpaceTie_PrefersBelowAndClampsHeight()
        {
            // Act
            Placement placement = Calculate(
                new Rectangle(100, 225, 200, 50),
                new Rectangle(0, 0, 800, 500),
                new Size(300, 400)
            );

            // Assert
            placement.OpensBelow.Should().BeTrue();
            placement.Bounds.Should().Be(new Rectangle(100, 275, 300, 225));
        }

        [TestMethod]
        public void Calculate_RightEdgeAndOversizeWidth_ClampsLocationAndSize()
        {
            // Act
            Placement right = Calculate(
                new Rectangle(750, 100, 40, 25),
                new Rectangle(0, 0, 800, 600),
                new Size(300, 200)
            );
            Placement oversize = Calculate(
                new Rectangle(200, 100, 40, 25),
                new Rectangle(0, 0, 800, 600),
                new Size(1000, 200)
            );

            // Assert
            right.Bounds.Should().Be(new Rectangle(500, 125, 300, 200));
            oversize.Bounds.Should().Be(new Rectangle(0, 125, 800, 200));
        }

        [TestMethod]
        public void Calculate_NegativeCoordinateMonitor_StaysWithinItsWorkingArea()
        {
            // Act
            Placement placement = Calculate(
                new Rectangle(-100, 100, 80, 25),
                new Rectangle(-1920, 0, 1920, 1080),
                new Size(500, 300)
            );

            // Assert
            placement.Bounds.Should().Be(new Rectangle(-500, 125, 500, 300));
        }

        [TestMethod]
        public void Calculate_AnchorOutsideVerticalBounds_ClampsLocation()
        {
            // Act
            Placement placement = Calculate(
                new Rectangle(100, -50, 80, 25),
                new Rectangle(0, 0, 800, 600),
                new Size(300, 200)
            );

            // Assert
            placement.OpensBelow.Should().BeTrue();
            placement.Bounds.Should().Be(new Rectangle(100, 0, 300, 200));
        }

        [TestMethod]
        public void Calculate_ZeroWorkingArea_ProducesZeroSizeAtWorkingOrigin()
        {
            // Act
            Placement placement = Calculate(
                new Rectangle(50, 50, 100, 25),
                new Rectangle(-10, -20, 0, 0),
                new Size(300, 200)
            );

            // Assert
            placement.OpensBelow.Should().BeTrue("equal zero space must use the below tie rule");
            placement.Bounds.Should().Be(new Rectangle(-10, -20, 0, 0));
        }

        private static Placement Calculate(Rectangle anchor, Rectangle workingArea, Size desired)
        {
            Type type = typeof(BreadcrumbBridgeCoordinator).Assembly.GetType(
                "QuickFiler.Viewers.BreadcrumbPopupPlacement",
                false
            );
            type.Should().NotBeNull("issue #400 requires a pure popup placement calculator");
            MethodInfo method = type.GetMethod(
                "Calculate",
                BindingFlags.Public | BindingFlags.Static
            );
            method.Should().NotBeNull();
            object result = method.Invoke(null, new object[] { anchor, workingArea, desired });
            return new Placement(
                (Rectangle)result.GetType().GetProperty("Bounds").GetValue(result),
                (bool)result.GetType().GetProperty("OpensBelow").GetValue(result)
            );
        }

        private sealed class Placement
        {
            public Placement(Rectangle bounds, bool opensBelow)
            {
                Bounds = bounds;
                OpensBelow = opensBelow;
            }

            public Rectangle Bounds { get; }
            public bool OpensBelow { get; }
        }
    }
}
