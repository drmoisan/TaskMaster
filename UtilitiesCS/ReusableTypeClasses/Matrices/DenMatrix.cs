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
    public class DenMatrix<T> : IDisposable where T : struct, IComparable<T>
    {
        private T[] __array1d;
        public int Width { get; set; }
        public int Height { get; set; }
        public int Length { get { return Width * Height; } }
        public bool IsEmpty
        {
            get
            {
                if (__array1d == null) return true;
                else return false;
            }
        }

        public DenMatrix() { }
        public DenMatrix(T[,] data)
        {
            this.Set(data);
        }

        public DenMatrix(int rows, int cols)
        {
            Width = rows;
            Height = cols;

            __array1d = new T[Length];
        }

        public T Get(int x, int y)
        {
            if (__array1d == null)
            {
                throw new Exception("array is empty");
            }
            if (x < Width && y < Height)
            {
                if (__array1d != null)
                {
                    return __array1d[x + y * Width];
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
            int length = Length;

            if (__array1d == null)
            {
                __array1d = new T[length];
            }
            else
            {
                if (length != __array1d.Length)
                {
                    __array1d = null;
                    __array1d = new T[length];
                }
            }

            if (x < Width && y < Height)
            {
                __array1d[x + y * Width] = val;
            }
            else
            {

                throw new Exception(x + ", " + Width + "," + y + "," + Height);
            }
        }

        public T[] To1d(T[,] array2d)
        {
            T[] array1d = new T[Length];

            for (int x = 0; x < Height; x++)
            {
                for (int y = 0; y < Width; y++)
                {
                    T val = array2d[x, y];

                    int index = x * Width + y;

                    array1d[index] = val;
                }
            }

            return array1d;
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

        public void Set(T[,] arr)
        {
            if (arr != null)
            {
                int rows = arr.GetLength(0);
                int cols = arr.GetLength(1);

                Width = cols;
                Height = rows;

                __array1d = To1d(arr);
            }
            else
            {
                throw new Exception("array is null");
            }
        }

        #region IDisposable implementation
        ~DenMatrix()
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

                    __array1d = null;
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
