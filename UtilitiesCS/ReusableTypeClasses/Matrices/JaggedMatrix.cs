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
    /// <typeparam name="T"></typeparam>
    public class JagMatrix<T> : IDisposable where T : struct, IComparable<T>
    {
        private T[][] __array2d;
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsEmpty
        {
            get
            {
                if (__array2d == null) return true;
                else return false;
            }
        }

        public JagMatrix() { }
        public JagMatrix(T[][] data)
        {
            this.Set(data);
        }

        public JagMatrix(int rows, int cols)
        {
            Width = rows;
            Height = cols;

            __array2d = new T[Width][];
            for (int i = 0; i < Width; i++)
            {
                __array2d[i] = new T[Height];
            }
        }
        public T Get(int x, int y)
        {
            if (__array2d == null)
            {
                throw new Exception("array is empty");
            }
            if (x < Width && y < Height)
            {
                if (__array2d != null)
                {
                    return __array2d[x][y];
                }
                else
                {
                    throw new Exception("array is null");
                }
            }
            else
            {
                string message = string.Empty;

                if (x >= Width) message = "x-value exceeds Width ";
                if (y >= Height) message += "y-value exceeds Height ";
                message += "in Array2d.Get(x,y).";

                throw new Exception(message);
            }
        }

        public void Set(int x, int y, T val)
        {
            if (__array2d == null)
            {
                __array2d = new T[Width][];
                for (int i = 0; i < Width; i++)
                {
                    __array2d[i] = new T[Height];
                }
            }
            else
            {
                if (Width != __array2d.GetLength(0))
                {
                    __array2d = null;

                    __array2d = new T[Width][];
                    for (int i = 0; i < Width; i++)
                    {
                        __array2d[i] = new T[Height];
                    }
                }
            }

            if (x < Width && y < Height)
            {
                __array2d[x][y] = val;
            }
            else
            {

                throw new Exception(x + ", " + Width + "," + y + "," + Height);
            }
        }

        public static T[,] To2d(T[][] source)
        {
            T[,] dest = new T[source.Length, source[0].Length];

            for (int i = 0; i < source.Length; i++)
            {
                for (int j = 0; j < source[0].Length; j++)
                {
                    dest[i, j] = source[i][j];
                }
            }

            return dest;
        }

        public T this[int x, int y]
        {
            get
            {
                return Get(x, y);
            }
            set
            {
                Set(x, y, value);
            }
        }

        public void Set(T[][] arr)
        {
            if (arr != null)
            {
                int rows = arr.Length;
                int cols = arr[0].Length;

                __array2d = arr;

                Width = rows;
                Height = cols;
            }
            else
            {
                throw new Exception("array is null");
            }
        }

        #region IDisposable implementation
        ~JagMatrix()
        {
            this.Dispose(false);
        }

        protected bool Disposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.Disposed)
            {
                if (disposing)
                {
                    // Perform managed cleanup here.
                    //IDisposable disp = (IDisposable)_2dArray;

                    __array2d = null;
                }

                // Perform unmanaged cleanup here.
                Width = 0;
                Height = 0;

                this.Disposed = true;
            }
        }
        #endregion
    }
}
