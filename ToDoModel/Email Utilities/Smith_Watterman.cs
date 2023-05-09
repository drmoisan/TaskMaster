using System;
using System.Diagnostics;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using UtilitiesVB;

namespace ToDoModel
{

    public static class Smith_Watterman
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


        public static object StrToChars(string strTmp)
        {
            object StrToCharsRet = default;
            string[] buff;
            int i;

            buff = new string[(Strings.Len(strTmp))];
            var loopTo = Strings.Len(strTmp);
            for (i = 1; i <= loopTo; i++)
                buff[i - 1] = Strings.Mid(strTmp, i, 1);

            StrToCharsRet = buff;
            return StrToCharsRet;

        }

        public static int SW_Calc(string Str_X, string Str_Y, ref object[,] Matrix, IAppAutoFileObjects AFSettings, SW_Options SWOptions = SW_Options.ByWords)
        {
            int SW_CalcRet = default;
            int LenX, LenY, x, y, calcA, calcB, calcC, tempa, tempB, tempC;
            object words_X, words_Y;
            var maxSmith_Watterman = default(int);

            // StopWatch_SW.reStart
            string[] flatcsv;
            try
            {
                if (SWOptions == SW_Options.ByWords)
                {
                    words_X = Strings.Split(Str_X, " ");
                    words_Y = Strings.Split(Str_Y, " ");
                }
                else if (SWOptions == SW_Options.ByLetters)
                {
                    words_X = StrToChars(Str_X);
                    words_Y = StrToChars(Str_Y);
                }
                else
                {
                    // Default is bywords
                    words_X = Strings.Split(Str_X, " ");
                    words_Y = Strings.Split(Str_Y, " ");
                }

                LenX = Information.UBound((Array)words_X);
                LenY = Information.UBound((Array)words_Y);
                Matrix = new object[LenX + 3 + 1, LenY + 3 + 1];
                flatcsv = new string[LenY + 3 + 1];

                // *********************************
                // **********Initialize*************
                var loopTo = LenX + 3;
                for (x = 3; x <= loopTo; x++)
                    Matrix[x, 1] = words_X((object)(x - 3));

                var loopTo1 = LenY + 3;
                for (y = 3; y <= loopTo1; y++)
                    Matrix[1, y] = words_Y((object)(y - 3));

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
                        calcA = Conversions.ToInteger(Matrix[x - 1, y - 1]);
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(Matrix[x, 1], Matrix[1, y], false)))
                        {
                            calcA = calcA + AFSettings.SmithWatterman_MatchScore * Strings.Len(Matrix[x, 1]);
                        }
                        else
                        {
                            calcA = calcA + AFSettings.SmithWatterman_MismatchScore;
                        }

                        calcB = Conversions.ToInteger(Operators.AddObject(Matrix[x, y - 1], AFSettings.SmithWatterman_GapPenalty * Strings.Len(Matrix[1, y])));
                        calcC = Conversions.ToInteger(Operators.AddObject(Matrix[x - 1, y], AFSettings.SmithWatterman_GapPenalty * Strings.Len(Matrix[x, 1])));
                        tempa = Conversions.ToInteger(max(0, calcA, calcB, calcC));
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
                        flatcsv[y] = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(flatcsv[y], Matrix[x, y]), ", "));
                    flatcsv[y] = Conversions.ToString(Operators.ConcatenateObject(flatcsv[y], Matrix[LenX + 3, y]));
                }

                // Call Printout(flatcsv)
                // MsgBox (maxSmith_Watterman & " of " & Max(LenX + 1, LenY + 1))
                SW_CalcRet = maxSmith_Watterman;

                // StopWatch_SW.Pause
                return SW_CalcRet;
            }
            catch
            {

                Interaction.MsgBox(Information.Err().Description);
                Debugger.Break();
                Information.Err().Clear();
                ;
#error Cannot convert ResumeStatementSyntax - see comment for details
                /* Cannot convert ResumeStatementSyntax, CONVERSION ERROR: Conversion for ResumeStatement not implemented, please report this issue in 'Resume' at character 4307


                Input:
                        Resume

                 */
            }

        }



        public static object max(params object[] values)
        {
            object maxRet = default;
            object maxValue;
            maxValue = values[0];
            foreach (var Value in values)
            {
                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectGreater(Value, maxValue, false)))
                    maxValue = Value;
            }
            maxRet = maxValue;
            return maxRet;
        }





    }
}