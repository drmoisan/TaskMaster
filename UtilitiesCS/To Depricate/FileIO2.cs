using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Deedle.Vectors.VectorConstruction;


namespace UtilitiesCS
{
    public static class FileIO2
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

        public static void WriteTextFile(string filename, string[] strOutput, string folderpath)
        {
            TraceUtility.LogMethodCall(filename, strOutput, folderpath);

            string filepath = Path.Combine(folderpath, filename);
            var listOutput = new List<string>(strOutput);
            foreach (var output in listOutput)
                WriteUTF8(filepath, output, (WriteOptions.AppendNewLine | WriteOptions.OpenAsAppend));


        }

        public static async Task WriteTextFileAsync(string filename, string[] strOutput, string folderpath, CancellationToken token)
        {
            TraceUtility.LogMethodCall(filename, strOutput, folderpath);

            string filepath = Path.Combine(folderpath, filename);
            bool success = false;
            int attempts = 0;

            while (!success) 
            { 
                try
                {
                    token.ThrowIfCancellationRequested();
                    using (var sw = new StreamWriter(filepath, true, System.Text.Encoding.UTF8))
                    {
                        success = true;
                        foreach (var output in strOutput)
                            await sw.WriteLineAsync(output);
                    }
                }
                catch (IOException)
                {
                    Interlocked.Increment(ref attempts);
                    if (attempts < 100)
                    {
                        await Task.Delay(100);
                    }
                    else 
                    { 
                        logger.Error($"Failed to write to {filepath} after {attempts} attempts.");
                        success = true; 
                    }
                }            
            }
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

        public static string[] CSV_ReadTxtF(string filename, string folderpath, bool skipHeaders = true)
        {

            string filepath = Path.Combine(folderpath, filename);

            if (File.Exists(filepath))
            {
                if (skipHeaders)
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

        public static string[] CsvRead(string filename, string folderpath, bool skipHeaders = false)
        {

            string filepath = Path.Combine(folderpath, filename);

            if (File.Exists(filepath))
            {
                string[] lines = File.ReadAllLines(filepath, System.Text.Encoding.UTF8);
                if (skipHeaders)
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

        public static string[,] SplitArrayTo2D(string[] str1D, string delimeter = ",", bool zerobased = false)
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

            var loopTo = str1D.GetUpperBound(0);
            for (i = str1D.GetLowerBound(0); i <= loopTo; i++)
            {
                strLine = str1D[i];
                Count = strLine.Length - strLine.Replace(delimeter, "").Length;
                if (Count > maxj)
                    maxj = Count;
            }

            strD2_tmp = new string[str1D.GetUpperBound(0) + intBase + 1, maxj + intBase + 1];

            var loopTo1 = str1D.GetUpperBound(0);
            for (i = 0; i <= loopTo1; i++)
            {
                strTmp = str1D[i].Split(delimeter.ToCharArray());
                var loopTo2 = strTmp.GetUpperBound(0);
                for (j = 0; j <= loopTo2; j++)
                    strD2_tmp[i + intBase, j + intBase] = strTmp[j];
            }

            return strD2_tmp;
        }

        public static string[,] CsvReadTo2D(string filename, string folderpath, bool skipHeaders = false, string delimiter = ",")
        {
            string[] array1D = CsvRead(filename, folderpath, skipHeaders);
            return SplitArrayTo2D(array1D, delimiter);
        }

        public static string[][] CsvReadToJagged(string filename, string folderpath, bool skipHeaders = false, string delimiter = ",")
        {
            string[] array1D = CsvRead(filename, folderpath, skipHeaders);
            var jagged = array1D.Select(x => x.Split(delimiter, trim:true)).ToArray();
            return jagged;
        }
    }
}