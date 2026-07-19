#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public static partial class OlTableExtensions
    {
        internal static void WriteValuesToData(
            ref object[,] data,
            Dictionary<string, int> columnDictionary,
            IOrderedEnumerable<int> binIndices,
            int rowNumber,
            Dictionary<int, string> binStrings,
            IEnumerable<int>? objIndices,
            Dictionary<int, string> objStrings,
            object[] rawValues
        )
        {
            for (int j = 0; j < columnDictionary.Count; j++)
            {
                if ((binIndices is not null) && binIndices.Contains(j))
                {
                    data[rowNumber, j] = binStrings[j];
                }
                else if (objIndices is not null && objIndices.Contains(j))
                {
                    data[rowNumber, j] = objStrings[j];
                }
                else
                {
                    data[rowNumber, j] = rawValues[j];
                }
            }
        }

        internal static object[] ToObjectRow(
            this object[] rawValues,
            IOrderedEnumerable<int> binIndices,
            Dictionary<int, string> binStrings,
            IEnumerable<int>? objIndices,
            Dictionary<int, string> objStrings
        )
        {
            if (binIndices is not null)
            {
                binIndices.ForEach(i => rawValues[i] = binStrings[i]);
            }
            if (objIndices is not null)
            {
                objIndices.ForEach(i => rawValues[i] = objStrings[i]);
            }
            return rawValues;
        }

        internal static Dictionary<int, string> ConvertBinColumnsToString(
            Outlook.Row row,
            IOrderedEnumerable<int> binIndices
        )
        {
            return binIndices
                .Select(binIndex => new KeyValuePair<int, string>(
                    binIndex,
                    row.BinaryToString(binIndex + 1)
                ))
                .ToDictionary();
        }

        internal static Dictionary<int, string> ConvertObjectColumnsToString(
            Outlook.Row row,
            IEnumerable<int>? objIndices,
            IEnumerable<string>? objFields,
            Dictionary<string, Func<object, string>>? objectConverters
        )
        {
            var objStrings = new Dictionary<int, string>();
            if (objectConverters is not null && objIndices is not null && objFields is not null)
            {
                foreach (var objIndex in objIndices)
                {
                    var element = row[objIndex + 1];
                    var objString = objectConverters[objFields.ElementAt(objIndex)](element);
                    objStrings[objIndex] = objString;
                }
            }
            return objStrings;
        }
    }
}
