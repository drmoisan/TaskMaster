using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using UtilitiesCS.OutlookExtensions;
using System.Collections.Generic;

namespace ToDoModel
{

    public static class SortItemsToExistingFolder
    {
        public static void InitializeSortToExisting(string InitType = "Sort", bool QuickLoad = false, bool WholeConversation = true, string strSeed = "", object objItem = null)
        {
            throw new NotImplementedException();
        }

        public static void Run(IList<MailItem> selItems, bool picturesCheckbox, string sortFolderpath, bool saveMsg, bool attchments, bool removeFlowFile, IApplicationGlobals appGlobals, string strRoot = "")
        {
            string loc;
            string FileSystem_LOC;
            string FileSystem_LOC2;
            string FileSystem_DelLOC;
            // Dim selItems            As Selection    ' A collection of Outlook item objects in a folder.
            // Dim selItems            As Collection
            object objItem;
            MailItem MSG;
            object objFSO;       // Computer's file system object.
            object objShell;       // Windows Shell application object.
            object objFolder;       // The selected folder object from Browse for Folder dialog box.
            object objSubFolders;
            object objNewFolder;
            Folder sortFolder;
            Folder folderCurrent;
            string strFolderPath = "";
            int i;
            MailItem oMailTmp;
            string strTemp;
            string[] strAry;
            var strOutput = new string[2];

            // ******************
            // ***INITIALIZE*****
            // ******************
            var _globals = appGlobals;
            if (string.IsNullOrEmpty(strRoot))
            {
                strRoot = _globals.Ol.ArchiveRootPath;
            }
            // TODO: Eliminate following line once Path.Combine used below
            loc = strRoot + @"\";

            var _olApp = _globals.Ol.App;
            var OlNS = _globals.Ol.NamespaceMAPI;


            if (selItems.Count > 0)
            {
                folderCurrent = GetCurrentExplorerFolder(_globals.Ol.App.ActiveExplorer(), selItems[0]);
            }
            else
            {
                folderCurrent = GetCurrentExplorerFolder(_globals.Ol.App.ActiveExplorer());
            }
            if (folderCurrent.FolderPath.Contains(_globals.Ol.Inbox.FolderPath))
            {
                strFolderPath = _globals.FS.FldrFlow;
            }
            else if (folderCurrent.FolderPath.Contains(strRoot) & (folderCurrent.FolderPath != strRoot ))
            {
                strFolderPath = folderCurrent.ToFsFolder(OlFolderRoot: _globals.Ol.ArchiveRootPath, FsFolderRoot: _globals.FS.FldrRoot);
            }
            // strFolderPath = _globals.FS.FldrRoot & Right(folderCurrent.FolderPath, Len(folderCurrent.FolderPath) - Len(_globals.Ol.ArchiveRootPath) - 1)
            else
            {

            }



            // *************************************************************************
            // ************** SAVE ATTACHMENTS IF ENABLED*******************************
            // *************************************************************************
            string strTemp2 = "";
            // QUESTION: Original code allowed path to be an optional variable and then did something if a value was supplied that didn't match the archive root. Need to determine why and if new treatment loses functionality
            if ((strRoot ?? "") != (_globals.Ol.ArchiveRootPath ?? ""))
            {
                strTemp2 = _globals.Ol.ArchiveRootPath.Substring(_globals.Ol.EmailRootPath.Length);
                FileSystem_LOC = _globals.FS.FldrRoot + strTemp2 + @"\" + sortFolderpath;  // Parent Directory
            }
            else
            {
                FileSystem_LOC = Path.Combine(_globals.FS.FldrRoot, sortFolderpath);
            }

            FileSystem_DelLOC = _globals.FS.FldrRoot;

            // If Save_PDF = True Then
            // Call SaveAsPDF.SaveMessageAsPDF(FileSystem_LOC, selItems)
            // End If

            if (saveMsg == true)
            {
                SaveMessageAsMSG(FileSystem_LOC, selItems);
            }
            // 



            // ****Save Attachment to OneDrive directory****

            if (attchments == true)
            {
                // Email_SortSaveAttachment.SaveAttachmentsFromSelection(SavePath:=FileSystem_LOC, Verify_Action:=Pictures_Checkbox, selItems:=selItems, save_images:=Pictures_Checkbox, SaveMSG:=Save_MSG)
                SaveAttachmentsModule.SaveAttachmentsFromSelection(AppGlobals: appGlobals, SavePath: FileSystem_LOC, Verify_Action: picturesCheckbox, selItems: selItems, save_images: picturesCheckbox, SaveMSG: saveMsg);
            }



            if (removeFlowFile == true)
            {
                SaveAttachmentsModule.SaveAttachmentsFromSelection(AppGlobals: appGlobals, SavePath: strFolderPath, DELFILE: true, selItems: selItems);
            }



            // *************************************************************************
            // *********** LABEL EMAIL AS AUTOSORTED AND MOVE TO EMAIL FOLDER***********
            // *************************************************************************

            // If strTemp2 = "" Then Add_Recent(SortFolderpath)
            if (string.IsNullOrEmpty(strTemp2))
                _globals.AF.RecentsList.AddRecent(sortFolderpath);
            loc = Path.Combine(strRoot, sortFolderpath);
            sortFolder = new FolderHandler(_globals).GetFolder(loc); // Call Function to turn text to Folder

            // Call Flag_Fields_Categories.SetCategory("Autosort")
            // Call Flag_Fields_Categories.SetUdf("Autosort", "True")
            if (sortFolder is null)
            {
                MessageBox.Show(loc + " does not exist, skipping email move.");
            }
            else
            {

                for (i = selItems.Count - 1; i >= 0; i -= 1)
                {
                    if (selItems[i] is MailItem)
                    {
                        if (!(selItems[i] is MeetingItem))
                        {
                            MSG = (MailItem)selItems[i];
                            if (string.IsNullOrEmpty(strTemp2))
                            {
                                // Email_AutoCategorize.UpdateForMove(MSG, SortFolderpath)
                                UpdateForMove(MSG, sortFolderpath, appGlobals.AF.CtfMap, appGlobals.AF.SubjectMap);
                            };
                            try
                            {
                                MSG.SetUdf("Autosort", "True");
                                MSG.UnRead = false;
                                MSG.Save();

                                oMailTmp = (MailItem)MSG.Move(sortFolder);
                                CaptureMoveDetails(MSG, oMailTmp, strOutput, _globals);
                            }
                            catch (System.Exception e)
                            {
                                Debug.WriteLine(e.Message);
                                Debug.WriteLine(e.StackTrace);
                            }
                        }
                    }
                }
            }
        }

        private static void CaptureMoveDetails(MailItem MSG, MailItem oMailTmp, string[] strOutput, IApplicationGlobals _globals)
        {
            if (_globals.Ol.MovedMails_Stack is null)
                _globals.Ol.MovedMails_Stack = new StackObjectCS<object>();
            _globals.Ol.MovedMails_Stack.Push(MSG);
            _globals.Ol.MovedMails_Stack.Push(oMailTmp);

            // TODO: Change this into a JSON file
            WriteCSV_StartNewFileIfDoesNotExist(_globals.FS.Filenames.EmailMoves, _globals.FS.FldrMyD);
            //string[] strAry = CaptureEmailDetailsModule.CaptureEmailDetails(oMailTmp, _globals.Ol.ArchiveRootPath);
            string[] strAry = oMailTmp.Details(_globals.Ol.ArchiveRootPath);
            strOutput[1] = SanitizeArrayLineTSV(ref strAry);
            FileIO2.WriteTextFile(_globals.FS.Filenames.EmailMoves, strOutput, _globals.FS.FldrMyD);
        }

        //private static string SanitizeArrayLineTSV(ref string[] strOutput)
        //{
        //    string strBuild = "";
        //    if (strOutput.IsInitialized())
        //    {
        //        int max = strOutput.Length;
        //        for (int i = 1, loopTo = max; i <= loopTo; i++)
        //        {
        //            string strTemp = StripTabsCrLf(strOutput[i]);
        //            strBuild = strBuild + "\t" + strTemp;

        //        }
        //        if (strBuild.Length > 0)
        //            strBuild = strBuild.Substring(1);
        //        return strBuild;
        //    }
        //    else
        //    {
        //        return "";
        //    }
        //}

        private static string SanitizeArrayLineTSV(ref string[] strOutput)
        {
            if (strOutput.IsInitialized())
            {
                return string.Join("\t",strOutput
                             .Where(s => !string.IsNullOrEmpty(s))
                             .Select(s => StripTabsCrLf(s))
                             .ToArray());
            }
            else { return ""; }
        }

        internal static string StripTabsCrLf(string str)
        {
            var _regex = new Regex(@"[\t\n\r]*");
            string result = _regex.Replace(str, " ");

            // ensure max of one space per word
            _regex = new Regex(@"  +");
            result = _regex.Replace(result, " ");
            result = result.Trim();
            return result;
        }

        private static void WriteCSV_StartNewFileIfDoesNotExist(string strFileName, string strFileLocation)
        {
            string[] strOutput = null;
            string[,] strAryOutput;
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
                FileIO2.WriteTextFile(strFileName, strOutput, folderpath: strFileLocation);

            }
            strOutput = null;
            strAryOutput = null;
        }

        private static void SanitizeArray(string[,] strAryOutput, ref string[] strOutput)
        {
            if (strAryOutput == null) 
            {
                Debug.WriteLine($"The array {nameof(strAryOutput)} is empty.");
            }
            else
            {
                for (int j = 0; j < strAryOutput.GetLength(0); j++)
                {
                    strOutput[j] = string.Join("\t", strAryOutput
                                         .SliceRow(j)
                                         .Where(s => !string.IsNullOrEmpty(s))
                                         .Select(s => StripTabsCrLf(s))
                                         .ToArray());
                }
            }
        }

        private static void UpdateForMove(MailItem mailItem, string fldr, CtfMap ctfMap, ISubjectMapSL subMap)
        {
            ctfMap.Add(mailItem.ConversationID, fldr, 1);
            subMap.Add(mailItem.Subject, fldr);
        }
                
        private static void SaveMessageAsMSG(string fileSystem_LOC, IList<MailItem> selItems)
        {
            throw new NotImplementedException();
        }

        private static Folder GetCurrentExplorerFolder(Explorer ActiveExplorer, object objItem = null)
        {
            if (objItem is null)
            {
                objItem = ActiveExplorer.Selection[0];
            }

            if (objItem is MailItem)
            {
                MailItem OlMail = (MailItem)objItem;
                return (Folder)OlMail.Parent;
            }

            else if (objItem is AppointmentItem)
            {
                AppointmentItem OlAppointment = (AppointmentItem)objItem;
                return (Folder)OlAppointment.Parent;
            }

            else if (objItem is MeetingItem)
            {
                MeetingItem OlMeeting = (MeetingItem)objItem;
                return (Folder)OlMeeting.Parent;
            }

            else if (objItem is TaskItem)
            {
                TaskItem OlTask = (TaskItem)objItem;
                return (Folder)OlTask.Parent;
            }

            else
            {
                return null;
            }

        }

        public static void Cleanup_Files()
        {
            // Call WRITE_Text_File     - Writes to the recents list
            // Call Email_AutoCategorize.CTF_Incidence_Text_File_WRITE - Writes to the CTF_Incidence file   
            // Call Email_AutoCategorize.Subject_MAP_Text_File_WRITE - Writes to the Subject_MAP file
        }

        // Public Function DialogueThrowNotImplemented() As Boolean
        // Return MsgBox("")
        // End Function

    }
}