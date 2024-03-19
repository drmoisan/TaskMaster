using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.ReusableTypeClasses.Matrices
{
    /// <summary>
    /// Taken from https://codereview.stackexchange.com/questions/204889/2d-matrix-with-jagged-array-isnt-faster-than-one-with-a-multidimensional-array
    /// </summary>
    public class DataConverter2d
    {
        public static double[][] ToDouble(int[][] image)
        {
            int Width = image.Length;
            int Height = image[0].Length;

            double[][] array2d = new double[Width][];

            for (int x = 0; x < Width; x++)
            {
                array2d[x] = new double[Height];

                for (int y = 0; y < Height; y++)
                {
                    double d = image[x][y] / 255.0;

                    array2d[x][y] = d;
                }
            }

            return array2d;
        }

        public static Matrix<double> ToDouble(Matrix<int> image)
        {
            int Width = image.Width;
            int Height = image.Height;

            Matrix<double> array2d = new Matrix<double>(Width, Height);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    double d = image[x, y] / 255.0;

                    array2d[x, y] = d;
                }
            }

            return array2d;
        }
    }
}
