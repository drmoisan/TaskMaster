#nullable enable
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
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

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
            OpenAsAppend = 2,
        }

        public static void WriteTextFile(string filename, string[] strOutput, string folderpath)
        {
            //TraceUtility.LogMethodCall(filename, strOutput, folderpath);

            string filepath = Path.Combine(folderpath, filename);
            var listOutput = new List<string>(strOutput);
            foreach (var output in listOutput)
                WriteUTF8(
                    filepath,
                    output,
                    (WriteOptions.AppendNewLine | WriteOptions.OpenAsAppend)
                );
        }

        /// <summary>
        /// Appends each entry of <paramref name="strOutput"/> as a line to the file named
        /// <paramref name="filename"/> under <paramref name="folderpath"/>, retrying a bounded
        /// number of times while the file cannot be opened.
        /// </summary>
        /// <param name="filename">Name of the target file.</param>
        /// <param name="strOutput">Lines to append, in order.</param>
        /// <param name="folderpath">Folder containing the target file.</param>
        /// <param name="token">Observed before each attempt and by the retry delay.</param>
        /// <returns>
        /// <see langword="true"/> when the write completed, meaning every line was written and the
        /// writer was disposed without error; <see langword="false"/> when it did not, either
        /// because the retry budget was exhausted without the file ever opening or because a
        /// failure was raised after the writer opened. The method does not throw on a failed write:
        /// a caller that ignores the result cannot distinguish the two outcomes. An
        /// <see cref="OperationCanceledException"/> is still raised when
        /// <paramref name="token"/> is cancelled, and a non-<see cref="IOException"/> failure still
        /// propagates.
        /// </returns>
        public static Task<bool> WriteTextFileAsync(
            string filename,
            string[] strOutput,
            string folderpath,
            CancellationToken token
        ) => WriteTextFileAsync(filename, strOutput, folderpath, token, null, null);

        /// <summary>
        /// Test seam for <see cref="WriteTextFileAsync(string, string[], string, CancellationToken)"/>.
        /// The writer factory and the retry delay are supplied as parameters rather than as static
        /// state because UtilitiesCS.Test runs class-level parallel, so a shared mutable seam would
        /// be a genuine cross-class race. Passing null for either delegate selects the production
        /// default, which is what the public overload does.
        /// </summary>
        internal static async Task<bool> WriteTextFileAsync(
            string filename,
            string[] strOutput,
            string folderpath,
            CancellationToken token,
            Func<string, TextWriter>? writerFactory,
            Func<int, CancellationToken, Task>? delay
        )
        {
            //TraceUtility.LogMethodCall(filename, strOutput, folderpath, token);

            string filepath = Path.Combine(folderpath, filename);

            // Both delegates are coalesced once, before the loop, into explicitly typed non-nullable
            // locals. An explicit type is required because a coalescing expression whose right
            // operand is a lambda has no natural type, and coalescing here rather than inside the
            // loop avoids a conditional dereference that the type-check gate promotes to an error.
            Func<string, TextWriter> createWriter =
                writerFactory ?? (p => new StreamWriter(p, true, System.Text.Encoding.UTF8));
            Func<int, CancellationToken, Task> delayAsync = delay ?? ((ms, t) => Task.Delay(ms, t));

            int attempts = 0;

            while (true)
            {
                // Tracks whether this attempt got past the writer's construction. A failure raised
                // after that point is terminal: the file is opened in append mode, so retrying
                // after a partial flush would duplicate the lines already written.
                bool opened = false;
                try
                {
                    token.ThrowIfCancellationRequested();
                    using (var sw = createWriter(filepath))
                    {
                        opened = true;
                        foreach (var output in strOutput)
                            await sw.WriteLineAsync(output);
                    }

                    // Reached only when every line was written and the writer was disposed without
                    // error, so this is the single point at which success is established.
                    return true;
                }
                catch (DirectoryNotFoundException ex)
                {
                    logger.Error(
                        $"Failed to write to {filepath}: the target directory does not exist.",
                        ex
                    );
                    return false;
                }
                catch (IOException ex)
                {
                    if (opened)
                    {
                        logger.Error(
                            $"Write to {filepath} failed after the writer opened. The file may hold a partial record.",
                            ex
                        );
                        return false;
                    }

                    Interlocked.Increment(ref attempts);
                    if (attempts >= 100)
                    {
                        logger.Error(
                            $"Failed to write to {filepath} after {attempts} attempts.",
                            ex
                        );
                        return false;
                    }

                    await delayAsync(100, token);
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

        public static string[]? CSV_ReadTxtF(
            string filename,
            string folderpath,
            bool skipHeaders = true
        )
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

        public static string[]? CsvRead(
            string filename,
            string folderpath,
            bool skipHeaders = false
        )
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

        public static string[,] SplitArrayTo2D(
            string[] str1D,
            string delimeter = ",",
            bool zerobased = false
        )
        {
            int i,
                j;
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

        public static string[,] CsvReadTo2D(
            string filename,
            string folderpath,
            bool skipHeaders = false,
            string delimiter = ","
        )
        {
            string[]? array1D = CsvRead(filename, folderpath, skipHeaders);
            return SplitArrayTo2D(array1D!, delimiter);
        }

        public static string[][] CsvReadToJagged(
            string filename,
            string folderpath,
            bool skipHeaders = false,
            string delimiter = ","
        )
        {
            string[]? array1D = CsvRead(filename, folderpath, skipHeaders);
            var jagged = array1D!.Select(x => x.Split(delimiter, trim: true)).ToArray();
            return jagged;
        }
    }
}
