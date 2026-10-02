#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Deedle;
using Microsoft.Office.Interop.Outlook;
using SDILReader;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public static partial class SortEmail
    {
        // Duplicative with QuickFiler but it is still mapped to main menu so I need to take it out
        [ExcludeFromCodeCoverage]
        public static async Task UndoAsync(
            SloStack<IMovedMailInfo> movedStack,
            IApplicationGlobals globals
        )
        {
            DialogResult repeatResponse = DialogResult.Yes;
            var i = 0;

            while (i < movedStack.Count && repeatResponse == DialogResult.Yes)
            {
                var message = movedStack[i].UndoMoveMessage(globals.Ol.App);
                if (message is null)
                {
                    i++;
                }
                else
                {
                    var undoResponse = MessageBox.Show(
                        message,
                        "Undo Dialog",
                        MessageBoxButtons.YesNo
                    );
                    if (undoResponse == DialogResult.Yes)
                    {
                        var helper = await MailItemHelper.FromMailItemAsync(
                            movedStack[i].MailItem,
                            globals,
                            default,
                            true
                        );
                        (
                            await new OlFolderClassifierGroup(globals).GetFolderPredictorAsync()
                        ).UnTrain(helper.FolderInfo!.RelativePath, helper.Tokens!, 1);
                        movedStack[i].UndoMove();
                        movedStack.Pop(i);
                    }
                    else
                    {
                        i++;
                    }
                    repeatResponse = MessageBox.Show(
                        "Continue Undoing Moves?",
                        "Undo Dialog",
                        MessageBoxButtons.YesNo
                    );
                }
            }

            if (repeatResponse == DialogResult.Yes)
            {
                MessageBox.Show("Nothing to undo");
            }
            movedStack.Serialize();
        }

        [ExcludeFromCodeCoverage]
        private static void PushToUndoStack(
            MailItem beforeMove,
            MailItem afterMove,
            IApplicationGlobals _globals
        )
        {
            //TODO: Delete _globals.Ol.MovedMails_Stack because it is obsolete
            var info = new MovedMailInfo(beforeMove, afterMove, _globals.Ol.Root.FolderPath);
            _globals.AF.MovedMails.Push(info);
        }

        [ExcludeFromCodeCoverage]
        private static void CaptureMoveDetails(
            MailItem mailItem,
            MailItem oMailTmp,
            IApplicationGlobals _globals
        )
        {
            //TraceUtility.LogMethodCall(mailItem, oMailTmp, _globals);

            string[] strAry = oMailTmp.Details(_globals.Ol.ArchiveRootPath).Skip(1).ToArray();
            var output = SanitizeArrayLineTSV(ref strAry);

            _globals.Ol.EmailMoveWriter.Enqueue(output);
        }

        [ExcludeFromCodeCoverage]
        private static string SanitizeArrayLineTSV(ref string[] strOutput)
        {
            //if (strOutput.IsInitialized())
            //{
            var line = string.Join(
                "\t",
                strOutput
                    //.Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => s ?? "")
                    .Select(s => StripTabsCrLf(s))
                    .ToArray()
            );
            return line;
            //}
            //else { return ""; }
        }

        internal static string StripTabsCrLf(string str)
        {
            var _regex = new Regex(@"[\t\n\r]+");
            string result = _regex.Replace(str, " ");

            // ensure max of one space per word
            _regex = new Regex(@"  +");
            result = _regex.Replace(result, " ");
            result = result.Trim();
            return result;
        }

        [ExcludeFromCodeCoverage]
        public static void WriteCSV_StartNewFileIfDoesNotExist(
            string strFileName,
            string strFileLocation
        )
        {
            string[]? strOutput = null;
            string[,]? strAryOutput;
            if (File.Exists(Path.Combine(strFileName, strFileLocation)))
            {
                strAryOutput = new string[14, 2];

                strAryOutput[1, 1] = "Triage";
                strAryOutput[2, 1] = "FolderName";
                strAryOutput[3, 1] = "Sent_On";
                strAryOutput[4, 1] = "From";
                strAryOutput[5, 1] = "To";
                strAryOutput[6, 1] = "CC";
                strAryOutput[7, 1] = "Subject";
                strAryOutput[8, 1] = "Body";
                strAryOutput[9, 1] = "fromDomain";
                strAryOutput[10, 1] = "Conversation_ID";
                strAryOutput[11, 1] = "EntryID";
                strAryOutput[12, 1] = "Attachments";
                strAryOutput[13, 1] = "FlaggedAsTask";

                SanitizeArray(strAryOutput, ref strOutput);
                FileIO2.WriteTextFile(strFileName, strOutput!, folderpath: strFileLocation);
            }
            strOutput = null;
            strAryOutput = null;
        }

        [ExcludeFromCodeCoverage]
        private static void SanitizeArray(string[,]? strAryOutput, ref string[]? strOutput)
        {
            if (strAryOutput == null)
            {
                Debug.WriteLine($"The array {nameof(strAryOutput)} is empty.");
            }
            else
            {
                for (int j = 0; j < strAryOutput.GetLength(0); j++)
                {
                    strOutput![j] = string.Join(
                        "\t",
                        strAryOutput
                            .SliceRow(j)
                            .Where(s => !string.IsNullOrEmpty(s))
                            .Select(s => StripTabsCrLf(s))
                            .ToArray()
                    );
                }
            }
        }
    }
}
