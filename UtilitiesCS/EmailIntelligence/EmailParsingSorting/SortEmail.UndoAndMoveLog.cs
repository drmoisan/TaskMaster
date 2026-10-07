#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;

namespace UtilitiesCS
{
    public static partial class SortEmail
    {
        // Column names of the moved-mails log, in the order of the record fields that
        // EmailDetails.Details produces after its unused index zero.
        private static readonly string[] MovedMailsHeader =
        {
            "Triage",
            "FolderName",
            "Sent_On",
            "From",
            "To",
            "CC",
            "Subject",
            "Body",
            "fromDomain",
            "Conversation_ID",
            "EntryID",
            "Attachments",
            "FlaggedAsTask",
        };

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

        // Excluded from coverage: wiring of the real file-system defaults only; a test call would
        // read and write the disk (UT4). The four-parameter overload is the test seam.
        [ExcludeFromCodeCoverage]
        public static void WriteCSV_StartNewFileIfDoesNotExist(
            string strFileName,
            string strFileLocation
        )
        {
            WriteCSV_StartNewFileIfDoesNotExist(
                strFileName,
                strFileLocation,
                File.Exists,
                FileIO2.WriteTextFile
            );
        }

        /// <summary>
        /// Seeds the moved-mails log with its single tab-separated header line when the file does
        /// not exist. <paramref name="fileExists"/> answers whether the combination of the folder
        /// and the file name exists; <paramref name="writeTextFile"/> receives the file name, the
        /// lines and the folder, in that order.
        /// </summary>
        internal static void WriteCSV_StartNewFileIfDoesNotExist(
            string strFileName,
            string strFileLocation,
            Func<string, bool> fileExists,
            Action<string, string[], string> writeTextFile
        )
        {
            if (fileExists(Path.Combine(strFileLocation, strFileName)))
            {
                return;
            }

            writeTextFile(
                strFileName,
                new[] { string.Join("\t", MovedMailsHeader) },
                strFileLocation
            );
        }
    }
}
