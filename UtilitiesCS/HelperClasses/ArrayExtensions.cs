using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public static class ArrayExtensions
    {
        public static string[,] ToStringArray<T>(this T[,] array)
        {
            int rowCount = array.GetLength(0);
            int columnCount = array.GetLength(1);
            string[,] stringArray = new string[rowCount, columnCount];
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    stringArray[i, j] = array[i, j].ToString();
                }
            }
            return stringArray;
        }

        public static string[] ToStringArray<T>(this T[] array)
        {
            int rowCount = array.Length;
            string[] stringArray = new string[rowCount];
            for (int i = 0; i < rowCount; i++)
            {
                stringArray[i] = array[i].ToString();
            }
            return stringArray;
        }

        public static IEnumerable<T> SliceRow<T>(this T[,] array, int row)
        {
            for (var i = 0; i < array.GetLength(1); i++)
            {
                yield return array[row, i];
            }
        }

        public static IEnumerable<T> SliceColumn<T>(this T[,] array, int column)
        {
            for (var i = 0; i < array.GetLength(0); i++)
            {
                yield return array[i, column];
            }
        }
    }
}
