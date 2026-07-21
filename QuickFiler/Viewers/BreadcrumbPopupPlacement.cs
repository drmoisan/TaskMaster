#nullable enable
using System;
using System.Drawing;

namespace QuickFiler.Viewers
{
    /// <summary>The clamped popup bounds and selected vertical direction.</summary>
    public readonly struct BreadcrumbPopupPlacementResult
    {
        public BreadcrumbPopupPlacementResult(Rectangle bounds, bool opensBelow)
        {
            Bounds = bounds;
            OpensBelow = opensBelow;
        }

        /// <summary>Popup bounds in screen coordinates.</summary>
        public Rectangle Bounds { get; }

        /// <summary>True when the popup starts below the anchor; false when it ends above it.</summary>
        public bool OpensBelow { get; }
    }

    /// <summary>Pure Windows-combo-style popup placement over supplied screen geometry.</summary>
    public static class BreadcrumbPopupPlacement
    {
        /// <summary>
        /// Prefers a full-height popup below, then a full-height popup above, then the side with
        /// greater available space (below on ties), and clamps both axes to the working area.
        /// </summary>
        public static BreadcrumbPopupPlacementResult Calculate(
            Rectangle anchorScreenBounds,
            Rectangle workingArea,
            Size desiredSize
        )
        {
            int workingWidth = Math.Max(0, workingArea.Width);
            int workingHeight = Math.Max(0, workingArea.Height);
            int workingRight = workingArea.Left + workingWidth;
            int workingBottom = workingArea.Top + workingHeight;
            int desiredWidth = Math.Max(0, desiredSize.Width);
            int desiredHeight = Math.Max(0, desiredSize.Height);

            int belowSpace = Math.Min(
                workingHeight,
                Math.Max(0, workingBottom - anchorScreenBounds.Bottom)
            );
            int aboveSpace = Math.Min(
                workingHeight,
                Math.Max(0, anchorScreenBounds.Top - workingArea.Top)
            );
            bool opensBelow;
            if (desiredHeight <= belowSpace)
            {
                opensBelow = true;
            }
            else if (desiredHeight <= aboveSpace)
            {
                opensBelow = false;
            }
            else
            {
                opensBelow = belowSpace >= aboveSpace;
            }

            int width = Math.Min(desiredWidth, workingWidth);
            int height = Math.Min(desiredHeight, opensBelow ? belowSpace : aboveSpace);
            int x = Clamp(anchorScreenBounds.Left, workingArea.Left, workingRight - width);
            int proposedY = opensBelow
                ? anchorScreenBounds.Bottom
                : anchorScreenBounds.Top - height;
            int y = Clamp(proposedY, workingArea.Top, workingBottom - height);
            return new BreadcrumbPopupPlacementResult(
                new Rectangle(x, y, width, height),
                opensBelow
            );
        }

        private static int Clamp(int value, int minimum, int maximum)
        {
            if (value < minimum)
            {
                return minimum;
            }
            return value > maximum ? maximum : value;
        }
    }
}
