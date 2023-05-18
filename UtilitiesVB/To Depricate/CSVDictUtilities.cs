using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.VisualBasic.FileIO;

namespace UtilitiesVB
{

    public static class CSVDictUtilities
    {

        public static Dictionary<string, string> LoadDictCSV(object stagingPath, object filename)
        {
            string filepath = Path.Combine(Conversions.ToString(stagingPath), Conversions.ToString(filename));
            var dictString = new Dictionary<string, string>();

            try
            {
                using (var MyReader = new TextFieldParser(filepath))
                {
                    MyReader.TextFieldType = FieldType.Delimited;
                    MyReader.SetDelimiters(",");

                    string[] currentRow;
                    while (!MyReader.EndOfData)
                    {
                        try
                        {
                            currentRow = MyReader.ReadFields();
                            object key = currentRow[0];
                            object value = currentRow[1];
                            dictString.Add(Conversions.ToString(key), Conversions.ToString(value));
                        }
                        catch (MalformedLineException ex)
                        {
                            var unused2 = Interaction.MsgBox("Line " + ex.Message + "is not valid and will be skipped.");
                        }
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                var unused1 = Interaction.MsgBox("File not found error -> " + filepath);
            }
            catch (FieldAccessException e)
            {
                var unused = Interaction.MsgBox("File is in use -> " + filepath);
            }

            return dictString;

        }

        public static void WriteDictCSV(Dictionary<string, string> dict_str, string staging_path, string filename)
        {
            string filepath = Path.Combine(staging_path, filename);
            string csv = string.Join(Environment.NewLine, dict_str.Select(d => $"{d.Key};{d.Value};"));
            File.WriteAllText(filepath, csv);
        }

    }
}