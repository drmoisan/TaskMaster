using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace UtilitiesCS
{

    public static class CSVDictUtilities
    {

        public static Dictionary<string, string> LoadDictCSV(string stagingPath, string filename)
        {
            var jagged = FileIO2.CsvReadToJagged(filename: filename, folderpath: stagingPath);

            int i = 0;
            return jagged.Select(row =>
            {
                if (row.Length != 2)
                {
                    throw new InvalidOperationException("CSV cannot be loaded to dictionary because" +
                        $"line {i} has {row.Length} members: {row}");
                }
                else
                {
                    return new KeyValuePair<string, string>(row[0], row[1]);
                }
            }).ToDictionary();
        }

        public static void WriteDictCSV(Dictionary<string, string> dict_str, string staging_path, string filename)
        {
            string filepath = Path.Combine(staging_path, filename);
            string csv = string.Join(Environment.NewLine, dict_str.Select(d => $"{d.Key};{d.Value};"));
            File.WriteAllText(filepath, csv);
        }

    }
}