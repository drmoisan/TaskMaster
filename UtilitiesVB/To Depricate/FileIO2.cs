using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace UtilitiesVB
{

    public static class FileIO2
    {
        public static void DELETE_TextFile(string filename, string stagingPath)
        {
            string filepath = Path.Combine(stagingPath, filename);

            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }

        }

        [Flags]
        private enum WriteOptions
        {
            None = 0,
            AppendNewLine = 1,
            OpenAsAppend = 2
        }

        public static void Write_TextFile(string strFileName, string[] strOutput, string strFileLocation)
        {
            string filepath = Path.Combine(strFileLocation, strFileName);
            var listOutput = new List<string>(strOutput);
            foreach (var output in listOutput)
                WriteUTF8(filepath, output, (WriteOptions.AppendNewLine | WriteOptions.OpenAsAppend));
            
            
        }

        private static void WriteUTF8(string filepath, string textString, WriteOptions options)
        {

            bool asAppend = options.HasFlag(WriteOptions.OpenAsAppend);

            using (var sw = new StreamWriter(filepath, asAppend, System.Text.Encoding.UTF8))
            {
                if (options.HasFlag(WriteOptions.AppendNewLine))
                {
                    sw.WriteLine(textString);
                }
                else
                {
                    sw.Write(textString);
                }
                sw.Close();
            }

        }

        public static string[] CSV_ReadTxtF(string filename, string fileaddress, bool SkipHeaders = true)
        {

            string filepath = Path.Combine(fileaddress, filename);

            if (File.Exists(filepath))
            {
                if (SkipHeaders)
                {
                    string[] lines = File.ReadAllLines(filepath);
                    return lines.Skip(1).ToArray();
                }
                else
                {
                    return File.ReadAllLines(filepath);
                }
            }

            else
            {
                return null;
            }

        }

        public static string[] CSV_Read(string filename, string fileaddress, bool SkipHeaders = false)
        {

            string filepath = Path.Combine(fileaddress, filename);

            if (File.Exists(filepath))
            {
                string[] lines = File.ReadAllLines(filepath, System.Text.Encoding.UTF8);
                if (SkipHeaders)
                {
                    return lines.Skip(1).ToArray();
                }
                else
                {
                    return lines;
                }
            }

            else
            {
                return null;
            }

        }

        public static string[,] CSV_SPLIT_TO_2D(string[] str1D, string Delimeter = ",", bool zerobased = false)
        {
            int i, j;
            int Count;
            var maxj = default(int);

            string[,] strD2_tmp;
            string[] strTmp;
            string strLine;
            int intBase;

            if (zerobased)
            {
                intBase = 0;
            }
            else
            {
                intBase = 1;
            }

            var loopTo = Information.UBound(str1D);
            for (i = Information.LBound(str1D); i <= loopTo; i++)
            {
                strLine = str1D[i];
                Count = Strings.Len(strLine) - Strings.Len(Strings.Replace(strLine, Delimeter, ""));
                if (Count > maxj)
                    maxj = Count;
            }

            strD2_tmp = new string[Information.UBound(str1D) + intBase + 1, maxj + intBase + 1];

            var loopTo1 = Information.UBound(str1D);
            for (i = 0; i <= loopTo1; i++)
            {
                strTmp = Strings.Split(str1D[i], Delimeter);
                var loopTo2 = Information.UBound(strTmp);
                for (j = 0; j <= loopTo2; j++)
                    strD2_tmp[i + intBase, j + intBase] = strTmp[j];
            }

            return strD2_tmp;

        }

    }
}