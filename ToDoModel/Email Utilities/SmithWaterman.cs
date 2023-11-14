using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

//using Microsoft.VisualStudio.Services.Common;
using UtilitiesCS;


namespace ToDoModel
{

    public static class SmithWaterman
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public enum SW_Options
        {
            ByWords = 0,
            ByLetters = 1
        }

        internal static string[] GetWords(this string sentence, SW_Options SWOptions)
        {
            if (SWOptions == SW_Options.ByWords)
            {
                return sentence.Tokenize();
            }
            else
            {
                return sentence.ToStringArrayOfCharsAsString();
            }
        }

        private static string[] ToStringArrayOfCharsAsString(this string sentence)
        {
            return sentence.ToCharArray().Select(c => c.ToString()).ToArray();
        }

        public static int CalculateScore(string Str_X, string Str_Y, ref object[,] Matrix, IAppAutoFileObjects AFSettings, SW_Options SWOptions = SW_Options.ByWords)
        {
            //TODO: Migrate Current SW Matrix which is not efficient because it is of object and mixes string and int.
            int result = default;
            int LenX, LenY, x, y, calcA, calcB, calcC, tempa;
            var maxSmith_Watterman = default(int);

            int matchScore = AFSettings.SmithWatterman_MatchScore;
            int mismatchScore = AFSettings.SmithWatterman_MismatchScore;
            int gapPenalty = AFSettings.SmithWatterman_GapPenalty;

            // StopWatch_SW.reStart
            string[] flatcsv;
            string[] words_X = Str_X.GetWords(SWOptions);
            string[] words_Y = Str_Y.GetWords(SWOptions);
                        
            LenX = words_X.Length;
            LenY = words_Y.Length;
            Matrix = new object[LenX + 3 + 1, LenY + 3 + 1];
            flatcsv = new string[LenY + 3 + 1];

            // *********************************
            // **********Initialize*************
            var loopTo = LenX + 3;
            for (x = 3; x <= loopTo; x++)
                Matrix[x, 1] = words_X[x - 3];

            var loopTo1 = LenY + 3;
            for (y = 3; y <= loopTo1; y++)
                Matrix[1, y] = words_Y[y - 3];

            var loopTo2 = LenX + 3;
            for (x = 2; x <= loopTo2; x++)
                Matrix[x, 2] = 0;

            var loopTo3 = LenY + 3;
            for (y = 2; y <= loopTo3; y++)
                Matrix[2, y] = 0;
            // *********************************

            // *********************************

            var loopTo4 = LenX + 3;
            for (x = 3; x <= loopTo4; x++)
            {
                var loopTo5 = LenY + 3;
                for (y = 3; y <= loopTo5; y++)
                {
                    calcA = (int)Matrix[x - 1, y - 1];
                    if (Matrix[x, 1] == Matrix[1, y])
                    {
                        calcA = calcA + matchScore * ((string)Matrix[x, 1]).Length;
                    }
                    else
                    {
                        calcA = calcA + mismatchScore;
                    }

                    calcB = (int)((int)Matrix[x, y - 1] + gapPenalty * ((string)Matrix[1, y]).Length);
                    calcC = (int)((int)Matrix[x - 1, y] + gapPenalty * ((string)Matrix[x, 1]).Length);
                    tempa = max(0, calcA, calcB, calcC);
                    Matrix[x, y] = tempa;
                    if (tempa > maxSmith_Watterman)
                        maxSmith_Watterman = tempa;
                }
            }

            var loopTo6 = LenY + 3;
            for (y = 1; y <= loopTo6; y++)
            {
                flatcsv[y] = "";
                var loopTo7 = LenX + 2;
                for (x = 1; x <= loopTo7; x++)
                    flatcsv[y] = string.Concat(flatcsv[y], Matrix[x, y].ToString(), ", ");
                flatcsv[y] = string.Concat(flatcsv[y], Matrix[LenX + 3, y].ToString());
            }

            // Call Printout(flatcsv)
            // MsgBox (maxSmith_Watterman & " of " & Max(LenX + 1, LenY + 1))
            result = maxSmith_Watterman;

            // StopWatch_SW.Pause
            return result;
            
        }

        public static int CalculateScore(int[] wordsX,
                                         int[] wordLengthX,
                                         int[] wordsY,
                                         int[] wordLengthY,
                                         int matchScore,
                                         int mismatchScore,
                                         int gapPenalty,
                                         string xString,
                                         string yString,
                                         int logThreshhold)
        {
            
            var tup = CalculateMatrixTuple(wordsX, wordLengthX, wordsY, wordLengthY, matchScore, mismatchScore, gapPenalty);
            if (logThreshhold > -1 && tup.Score > logThreshhold)
                LogMatrixState(tup.Matrix, xString, yString);
            return tup.Score;
        }

        public static int CalculateScore(int[] wordsX,
                                         int[] wordLengthX,
                                         int[] wordsY,
                                         int[] wordLengthY,
                                         int matchScore,
                                         int mismatchScore,
                                         int gapPenalty)
        {

            var tup = CalculateMatrixTuple(wordsX, wordLengthX, wordsY, wordLengthY, matchScore, mismatchScore, gapPenalty);
            return tup.Score;
        }

        public static (int[,] Matrix, int Score) CalculateMatrixTuple(
            int[] wordsX,
            int[] wordLengthX,
            int[] wordsY,
            int[] wordLengthY,
            int matchScore,
            int mismatchScore,
            int gapPenalty)
        {
            ValidateInputs(wordsX, wordLengthX, wordsY, wordLengthY);
            DeclareMatrix(wordsX, wordsY, out int lengthX, out int lengthY, out int maxValue, out int[,] matrix);
            
            //LogMatrixState(matrix);

                        
            for (int x = 3; x < lengthX + 3; x++)
                matrix[x, 1] = wordsX[x - 3];
            
            for (int y = 3; y < lengthY + 3; y++)
                matrix[1, y] = wordsY[y - 3];
            

            //LogMatrixState(matrix);

            int result = default;
            int calcA, calcB, calcC, tempA;
            // *********************************

            // *********************************

            var loopTo4 = lengthX + 3;
            for (int x = 3; x < loopTo4; x++)
            {
                var loopTo5 = lengthY + 3;
                for (int y = 3; y < loopTo5; y++)
                {
                    calcA = (int)matrix[x - 1, y - 1];
                    if (matrix[x, 1] == matrix[1, y])
                    {
                        calcA = calcA + matchScore * wordLengthX[x - 3];
                    }
                    else
                    {
                        calcA = calcA + mismatchScore;
                    }

                    calcB = (int)((int)matrix[x, y - 1] + gapPenalty * wordLengthY[y - 3]);
                    try
                    {
                        calcC = (int)((int)matrix[x - 1, y] + gapPenalty * wordLengthX[x - 3]);
                    }
                    catch (Exception e)
                    {
                        LogMatrixState(matrix);
                        logger.Error(e);
                        calcC = 0;
                    }
                    tempA = max(0, calcA, calcB, calcC);
                    matrix[x, y] = tempA;
                    if (tempA > maxValue)
                        maxValue = tempA;
                }
            }

            result = maxValue;

            
            return (matrix, result);

        }

        internal static void LogMatrixState(int[,] matrix)
        {
            logger.Debug($"\n{GetFormattedMatrixText(matrix)}");
        }

        internal static void LogMatrixState(int[,] matrix, string xString, string yString)
        {
            logger.Debug($"Smith-Watterman Matrix for \n{xString} and \n{yString}\n{GetFormattedMatrixText(matrix)}");
        }

        internal static string GetFormattedMatrixText(int[,] matrix)
        {
            string[,] matrixString = new string[matrix.GetLength(0), matrix.GetLength(1)];
            for (int x = 0; x < matrix.GetLength(0); x++)
                for (int y = 0; y < matrix.GetLength(1); y++)
                    matrixString[x, y] = matrix[x, y].ToString();
            var matrixText = matrixString.ToFormattedText();
            return matrixText;
        }

        private static void DeclareMatrix(int[] wordsX, int[] wordsY, out int lengthX, out int lengthY, out int maxSmith_Watterman, out int[,] Matrix)
        {
            lengthX = wordsX.Length;
            lengthY = wordsY.Length;

            maxSmith_Watterman = 0;
            Matrix = new int[lengthX + 3 + 1, lengthY + 3 + 1];
        }

        private static void ValidateInputs(int[] words_X, int[] wordLength_X, int[] words_Y, int[] wordLength_Y)
        {
            ThrowIfNull(words_X, wordLength_X, words_Y, wordLength_Y);
            ThrowIfLengthsDiffer(words_X, wordLength_X);
            ThrowIfLengthsDiffer(words_Y, wordLength_Y);
        }

        private static void ThrowIfNull(params object[] parameters)
        {
            int i = 0;
            foreach (object parameter in parameters)
            {
                if (parameter is null)
                {
                    var stackTrace = new StackTrace();
                    var frame = stackTrace.GetFrame(1);
                    var callingMethod = frame.GetMethod();
                    var parameterName = callingMethod.GetParameters()[i].Name;
                    var message = $"{callingMethod.Name} received a null parameter named {parameterName}";
                    logger.Warn(message);
                    throw new ArgumentNullException(message);
                }
                i++;
            }
        }

        private static void ThrowIfLengthsDiffer(params object[] parameters)
        {
            var lengths = parameters.Select(x => ((Array)x).Length).ToArray();
            var val = lengths.First();
            for (int i = 0; i < lengths.Count(); i++)
            {
                if(lengths[i] != val)
                {
                    var stackTrace = new StackTrace();
                    var frame = stackTrace.GetFrame(1);
                    var callingMethod = frame.GetMethod();
                    var parameterReferences = callingMethod.GetParameters();
                    var message = $"{callingMethod.Name} parameter {parameterReferences[0].Name} has " +
                        $"a length of {lengths[0]}, but parameter {parameterReferences[i].Name} has " +
                        $"a length of {lengths[i]}. The two arrays should have the same length.";
                    logger.Warn(message);
                    throw new ArgumentOutOfRangeException(message);
                }
            }
            
        }

        public static int max(params int[] values)
        {
            int max = values[0]; 
            for (int i = 1; i < values.Length; i++)
            {
                max = Math.Max(max, values[i]);
            }
            return max;
        }

    }
}