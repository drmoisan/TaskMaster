using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace UtilitiesCS.Windows_Forms
{
    public static class ScreenHelper
    {
        public static int Area(this Rectangle rectangle)
        {
            return rectangle.Width * rectangle.Height;
        }

        public static float Area(this RectangleF rectangle)
        {
            return rectangle.Width * rectangle.Height;
        }
        
        public static void ToggleScreens(this ContainerControl container, bool withScaling)
        {
            if (Screen.AllScreens.Count() < 2) { throw new ArgumentOutOfRangeException(nameof(container), $"There is only one screen"); }
            if (container is null) { throw new ArgumentNullException(nameof(container)); }
            if (container.IsDisposed) { throw new ObjectDisposedException(nameof(container)); }

            var (currentScreen, screenNumber) = container.Location.GetScreen();

            // Get the next screen in the sequence
            if (++screenNumber == Screen.AllScreens.Count()) { screenNumber = 0; }
            var targetScreen = Screen.AllScreens[screenNumber];

            container.SwitchScreens(currentScreen, targetScreen, withScaling);
        }

        public static bool TryToggleScreens(this ContainerControl container, bool withScaling)
        {
            if (Screen.AllScreens.Count() < 2 || container is null || container.IsDisposed) { return false; }

            if (container.Location.TryGetScreen(out Screen sourceScreen, out int screenNumber)) { return false; }

            // Get the next screen in the sequence
            if (++screenNumber == Screen.AllScreens.Count()) { screenNumber = 0; }
            var targetScreen = Screen.AllScreens[screenNumber];
            
            return container.TrySwitchScreens(sourceScreen, targetScreen, withScaling);
        }

        internal static bool TryGetScreen(this Point point, out Screen screen, out int screenNumber)
        {
            screen = default;
            screenNumber = -1;
            
            screenNumber = Screen.AllScreens.FindIndex(screen => screen.Bounds.Contains(point));
            if (screenNumber == -1) { return false; }

            screen = Screen.AllScreens[screenNumber];
            
            return true;
        }
        
        internal static (Screen Screen, int ScreenNumber) GetScreen(this Point point)
        {
            var screenNumber = Screen.AllScreens.FindIndex(screen => screen.Bounds.Contains(point));
            if (screenNumber == -1) { throw new ArgumentOutOfRangeException($"point {point} not on any screen"); }

            var currentScreen = Screen.AllScreens[screenNumber];
            return (currentScreen, screenNumber);
        }

        /// <summary>
        /// Extension translates a point from one screen to another if the desired screen is not on 
        /// the current screen. 
        /// </summary>
        /// <param name="point">Original System.Drawing.<seealso cref="Point"/></param>
        /// <param name="targetScreen">Target System.Windows.Forms.<seealso cref="Screen"/></param>
        /// <param name="withScaling">If true, the point is scaled to the new screen dimensions</param>
        /// <returns>A System.Drawing.<seealso cref="Point"/> in the same relative location on the new screen</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Point SwitchTo(this Point point, Screen targetScreen, bool withScaling)
        {
            if (targetScreen is null) { throw new ArgumentNullException(nameof(targetScreen)); }

            (var currentScreen, _) = point.GetScreen();

            double scalingFactorX = 1;
            double scalingFactorY = 1;

            if (withScaling) 
            {
                scalingFactorX = currentScreen.Bounds.Width <= 0 ? 1 : targetScreen.Bounds.Width / (double)currentScreen.Bounds.Width;
                scalingFactorY = currentScreen.Bounds.Height <= 0 ? 1 : targetScreen.Bounds.Height / (double)currentScreen.Bounds.Height;
            }

            // Calculate the location on the new screen
            var newPoint = new Point(
                targetScreen.Bounds.X + (int)(scalingFactorX * (currentScreen.Bounds.X - point.X)),
                targetScreen.Bounds.Y + (int)(scalingFactorY * (currentScreen.Bounds.Y - point.Y)));

            return newPoint;
        }

        public static bool TrySwitchTo(this Point point, Screen targetScreen, bool withScaling, out Point newPoint)
        {
            newPoint = default;
            Screen sourceScreen; 
            int screenNumber;
            if (point == default || targetScreen is null || TryGetScreen(point, out sourceScreen, out screenNumber)) { return false; }
            return TrySwitchScreens(point, sourceScreen, targetScreen, withScaling, out newPoint);
        }

        public static void SwitchScreens(this ContainerControl container, Screen sourceScreen, Screen targetScreen, bool withScaling)
        {
            var point = container.Location;
            var newPoint = point.SwitchScreens(sourceScreen, targetScreen, withScaling);
            
            if (container.InvokeRequired)
            {
                container.Invoke(() => container.Location = newPoint);
            }
            else
            {
                container.Location = newPoint;
            }
        }

        /// <summary>
        /// Extension translates a point from one screen to another if the desired screen. 
        /// </summary>
        /// <param name="point">Original System.Drawing.<seealso cref="Point"/></param>
        /// <param name="sourceScreen">Source System.Windows.Forms.<seealso cref="Screen"/></param>
        /// <param name="targetScreen">Target System.Windows.Forms.<seealso cref="Screen"/></param>
        /// <param name="withScaling">If true, the point is scaled to the new screen dimensions</param>
        /// <returns>A System.Drawing.<seealso cref="Point"/> in the same relative location on the new screen</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Point SwitchScreens(this Point point, Screen sourceScreen,Screen targetScreen, bool withScaling)
        {
            if (sourceScreen is null) { throw new ArgumentNullException(nameof(sourceScreen)); }
            if (targetScreen is null) { throw new ArgumentNullException(nameof(targetScreen)); }

            double scalingFactorX = 1;
            double scalingFactorY = 1;

            if (withScaling)
            {
                scalingFactorX = sourceScreen.Bounds.Width <= 0 ? 1 : targetScreen.Bounds.Width / (double)sourceScreen.Bounds.Width;
                scalingFactorY = sourceScreen.Bounds.Height <= 0 ? 1 : targetScreen.Bounds.Height / (double)sourceScreen.Bounds.Height;
            }

            // Calculate the location on the new screen
            var newPoint = new Point(
                targetScreen.Bounds.X + (int)(scalingFactorX * (sourceScreen.Bounds.X - point.X)),
                targetScreen.Bounds.Y + (int)(scalingFactorY * (sourceScreen.Bounds.Y - point.Y)));

            return newPoint;
        }

        public static bool TrySwitchScreens(this ContainerControl container, Screen targetScreen, bool withScaling)
        {
            if (container is null || container.IsDisposed ) { return false; }
            if (!container.Location.TryGetScreen(out Screen sourceScreen, out _)) { return false; }
            return container.TrySwitchScreens(sourceScreen, targetScreen, withScaling);
        }

        public static bool TrySwitchScreens(this ContainerControl container, Screen sourceScreen, Screen targetScreen, bool withScaling)
        {
            Point newPoint;
            if (container is null || container.IsDisposed) { return false; }
            if (!container.Location.TrySwitchScreens(sourceScreen, targetScreen, withScaling, out newPoint)) { return false; }

            if (container.InvokeRequired)
            {
                container.Invoke(() => container.Location = newPoint);
            }
            else
            {
                container.Location = newPoint;
            }
            return true;
        }

        /// <summary>
        /// Extension tries to translate a point from one screen to another. Returns 
        /// true if successful and false if it fails. The new point is returned as an
        /// out variable
        /// </summary>
        /// <param name="point">Original System.Drawing.<seealso cref="Point"/></param>
        /// <param name="sourceScreen">Source System.Windows.Forms.<seealso cref="Screen"/></param>
        /// <param name="targetScreen">Target System.Windows.Forms.<seealso cref="Screen"/></param>
        /// <param name="withScaling">If true, the point is scaled to the new screen dimensions</param>
        /// <param name="newPoint">A System.Drawing.<seealso cref="Point"/> in the same relative location on the new screen</param>
        /// <returns>True if successful and False if it fails</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static bool TrySwitchScreens(this Point point, Screen sourceScreen, Screen targetScreen, bool withScaling, out Point newPoint)
        {
            newPoint = default;
            if (sourceScreen is null || targetScreen is null) { return false; }
            
            double scalingFactorX = 1;
            double scalingFactorY = 1;

            if (withScaling)
            {
                scalingFactorX = sourceScreen.Bounds.Width <= 0 ? 1 : targetScreen.Bounds.Width / (double)sourceScreen.Bounds.Width;
                scalingFactorY = sourceScreen.Bounds.Height <= 0 ? 1 : targetScreen.Bounds.Height / (double)sourceScreen.Bounds.Height;
            }

            // Calculate the location on the new screen
            newPoint = new Point(
                targetScreen.Bounds.X + (int)(scalingFactorX * (sourceScreen.Bounds.X - point.X)),
                targetScreen.Bounds.Y + (int)(scalingFactorY * (sourceScreen.Bounds.Y - point.Y)));

            return true;
        }

    }
}
