using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deedle.Internal;
using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public static class OlTableFunctions
    {
        /// <summary>
        /// Extension method that removes all columns in the supplied array 
        /// from an Outlook Table object
        /// </summary>
        /// <param name="table">Outlook table object</param>
        /// <param name="columnNames">Array of column names to remove</param>
        public static void RemoveColumns(this Outlook.Table table, string[] columnNames)
        {
            if (table is not null)
            {
                foreach (var column in columnNames) { table.Columns.Remove(column); }
            }
        }

        /// <summary>
        /// Extension method that iterates through an array of column names or
        /// schemas and adds the corresponding columns to an Outlook Table object
        /// </summary>
        /// <param name="table">Source Table</param>
        /// <param name="columnNames">Array of column names or schemas</param>
        public static void AddColumns(this Outlook.Table table, string[] columnNames)
        {
            if (table is not null)
            {
                foreach (var column in columnNames) { table.Columns.Add(column); }
            }
        }

        /// <summary>
        /// Extension that extracts a dictionary of column names and zero-based 
        /// positional indices. If columns are supplied as schemas and the schemas
        /// are known, columnn headers are converted from schema to semantic name
        /// </summary>
        /// <param name="table">Source Outlook Table object</param>
        /// <returns>Resulting Outlook Table object</returns>
        public static Dictionary<string, int> GetColumnDictionary(this Outlook.Table table)
        {
            var kvps = Enumerable.Range(1, table.Columns.Count)
                                 .Select(i =>
                                 {
                                     var name = table.Columns[i].Name;
                                     if (ConvHelper.SchemaToField.TryGetValue(name, out var adjustedName))
                                     { return new KeyValuePair<string, int>(adjustedName, i -1); }
                                     else { return new KeyValuePair<string, int>(name, i - 1); }
                                 });
                                 
            Dictionary<string, int> dict = new();
            try 
            {
                dict = kvps.ToDictionary();
            }
            catch (System.InvalidOperationException ex)
            {
                foreach(var kvp in kvps)
                {
                    if (!dict.ContainsKey(kvp.Key)) { dict.Add(kvp.Key, kvp.Value); }
                    else { dict[$"{kvp.Key}{kvp.Value}"] = kvp.Value; }                        
                }
            } 
            return dict;
        }

        /// <summary>
        /// Extension method extracts a 2D object array of data and a dictionary of 
        /// column names and indices. See <see cref="GetColumnDictionary(Table)"/>
        /// </summary>
        /// <param name="table"></param>
        /// <returns>ValueTuple of a 2D object array and a column dictionary</returns>
        public static (object[,] data, Dictionary<string, int> columnInfo) ExtractData(this Outlook.Table table)
        {
            var columnDictionary = table.GetColumnDictionary();
            var rowCount = table.GetRowCount();
            table.MoveToStart();
            object[,] data = table.GetArray(rowCount);
            return (data, columnDictionary);
        }
    }
}
