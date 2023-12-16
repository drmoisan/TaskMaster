using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace UtilitiesCS.Extensions
{
    public static class ImageExtensions
    {
        public static Dictionary<Color, int> GenerateHistogram(this Bitmap image)
        {
            Dictionary<Color, int> histogram = new Dictionary<Color, int>();

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color pixelColor = image.GetPixel(i, j);

                    if (histogram.ContainsKey(pixelColor))
                    {
                        histogram[pixelColor]++;
                    }
                    else
                    {
                        histogram[pixelColor] = 1;
                    }
                }
            }

            return histogram;
        }

        public static Bitmap ToRGB(this Bitmap image)
        {
            var width = image.Width;
            var height = image.Height;
            
            Bitmap rgbImage = new(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (Graphics graphics = Graphics.FromImage(rgbImage))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.DrawImage(image, 0, 0, width, height);
            }
            return rgbImage;
        }

        public static byte[] ToByte(this Bitmap image)
        {
            ImageConverter converter = new();
            return (byte[])converter.ConvertTo(image, typeof(byte[]));
        }
    }
}
