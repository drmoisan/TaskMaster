using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.Services.Common;
using UtilitiesCS;


namespace ToDoModel
{

    public static class SmithWaterman
    {
        // Global Const Match_Score = 1
        // Global Const Mismatch_Score = 0
        // Global Const Gap_penalty = -1
        // Global Const Word_Match_Optimal = 5
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

        public static int SW_Calc(string Str_X, string Str_Y, ref object[,] Matrix, IAppAutoFileObjects AFSettings, SW_Options SWOptions = SW_Options.ByWords)
        {
            //TODO: Migrate Current SW Matrix which is not efficient because it is of object and mixes string and int.
            //TODO: Store AFSettings in local variables to avoid multiple calls
            int SW_CalcRet = default;
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
            SW_CalcRet = maxSmith_Watterman;

            // StopWatch_SW.Pause
            return SW_CalcRet;
            
        }

        public static int SW_CalcInt(int[] words_X,
                                     int[] wordLength_X,
                                     int[] words_Y,
                                     int[] wordLength_Y,
                                     int matchScore,
                                     int mismatchScore,
                                     int gapPenalty)
        {
            // Check if any of the parameters are null and throw an exception showing which one and the call stack            // Check if any of the parameters are null and throw an exception showing which one and the call stack
            if (words_X == null || wordLength_X == null || words_Y == null || wordLength_Y == null)
            {
                var stackTrace = new StackTrace();
                var callingMethod = stackTrace.GetFrame(1).GetMethod();
                Debug.WriteLine(stackTrace.ToString());
                throw new ArgumentNullException($"One of the parameters in {callingMethod} is null");
            }
            
            int SW_CalcRet = default;
            int LenX, LenY, x, y, calcA, calcB, calcC, tempa;
            
            var maxSmith_Watterman = default(int);    
                
            LenX = words_X.Length;
            LenY = words_Y.Length;
            int[,] Matrix = new int[LenX + 3 + 1, LenY + 3 + 1];
            //flatcsv = new string[LenY + 3 + 1];

            // *********************************
            // **********Initialize*************
            var loopTo = LenX + 3;
            for (x = 3; x < loopTo; x++)
                Matrix[x, 1] = words_X[x - 3];

            var loopTo1 = LenY + 3;
            for (y = 3; y < loopTo1; y++)
                Matrix[1, y] = words_Y[y - 3];

            var loopTo2 = LenX + 3;
            for (x = 2; x < loopTo2; x++)
                Matrix[x, 2] = 0;

            var loopTo3 = LenY + 3;
            for (y = 2; y < loopTo3; y++)
                Matrix[2, y] = 0;
            // *********************************

            // *********************************

            var loopTo4 = LenX + 3;
            for (x = 3; x < loopTo4; x++)
            {
                var loopTo5 = LenY + 3;
                for (y = 3; y < loopTo5; y++)
                {
                    calcA = (int)Matrix[x - 1, y - 1];
                    if (Matrix[x, 1] == Matrix[1, y])
                    {
                        calcA = calcA + matchScore * wordLength_X[x-3];
                    }
                    else
                    {
                        calcA = calcA + mismatchScore;
                    }

                    calcB = (int)((int)Matrix[x, y - 1] + gapPenalty * wordLength_Y[y-3]);
                    calcC = (int)((int)Matrix[x - 1, y] + gapPenalty * wordLength_X[x-3]);
                    tempa = max(0, calcA, calcB, calcC);
                    Matrix[x, y] = tempa;
                    if (tempa > maxSmith_Watterman)
                        maxSmith_Watterman = tempa;
                }
            }

            //var loopTo6 = LenY + 3;
            //for (y = 1; y <= loopTo6; y++)
            //{
            //    flatcsv[y] = "";
            //    var loopTo7 = LenX + 2;
            //    for (x = 1; x <= loopTo7; x++)
            //        flatcsv[y] = string.Concat(flatcsv[y], Matrix[x, y].ToString(), ", ");
            //    flatcsv[y] = string.Concat(flatcsv[y], Matrix[LenX + 3, y].ToString());
            //}

            // Call Printout(flatcsv)
            // MsgBox (maxSmith_Watterman & " of " & Max(LenX + 1, LenY + 1))
            SW_CalcRet = maxSmith_Watterman;

            // StopWatch_SW.Pause
            return SW_CalcRet;

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