using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.OutlookExtensions;

namespace QuickFiler.Controllers
{
    internal class QfcExplorerController : IQfcExplorerController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public QfcExplorerController(Enums.InitTypeEnum initType, IApplicationGlobals appGlobals, IFilerHomeController parent)
        {
            _initType = initType;
            _globals = appGlobals;
            _activeExplorer = _globals.Ol.App.ActiveExplorer();
            _parent = parent;
        }
        
        private Enums.InitTypeEnum _initType;
        private IApplicationGlobals _globals;
        private IFilerHomeController _parent;
        private Explorer _activeExplorer;
        private Outlook.View _objView;
        private string _objViewMem;
        public Outlook.View ObjViewTemp;


        //PRIORITY: Implement BlShowInConversations
        private bool _blShowInConversations;
        public bool BlShowInConversations { get => _blShowInConversations; set => _blShowInConversations = value; }

        internal bool CurrentConversationState { get => _activeExplorer.CommandBars.GetPressedMso("ShowInConversations"); }

        //PRIORITY: Implement ExplConvView_Cleanup
        public void ExplConvView_Cleanup()
        {
            throw new NotImplementedException();
        }

        
        public void ExplConvView_ReturnState()
        {
            if (BlShowInConversations)
                ExplConvView_ToggleOn();
        }

        public void ExplConvView_ToggleOff()
        {
            if (_activeExplorer.CommandBars.GetPressedMso("ShowInConversations"))
            {
                BlShowInConversations = true;
                _objView = (Outlook.View)_activeExplorer.CurrentView;

                if (_objView.Name == "tmpNoConversation")
                {
                    if (_activeExplorer.CommandBars.GetPressedMso("ShowInConversations"))
                    {

                        _objView.XML = _objView.XML.Replace("<upgradetoconv>1</upgradetoconv>", "");
                        _objView.Save();
                        _objView.Apply();
                    }
                }
                _objViewMem = _objView.Name;
                if (_objViewMem == "tmpNoConversation")
                    _objViewMem = _globals.Ol.ViewWide;

                //ObjViewTemp = ObjView.Parent("tmpNoConversation");
                ObjViewTemp = GetSiblingView(_objView, "tmpNoConversation");

                if (ObjViewTemp is null)
                {
                    ObjViewTemp = _objView.Copy("tmpNoConversation", OlViewSaveOption.olViewSaveOptionThisFolderOnlyMe);
                    ObjViewTemp.XML = _objView.XML.Replace("<upgradetoconv>1</upgradetoconv>", "");
                    ObjViewTemp.Save();

                }
                ObjViewTemp.Apply();
            }
        }

        public Outlook.View GetSiblingView(Outlook.View currentView, string viewName)
        {
            Outlook.View view = null;
            var views = (Views)currentView.Parent;
            foreach (Outlook.View v in views)
            {
                if (v.Name == viewName)
                {
                    view = v;
                    break;
                }
            }
            return view;
        }

        public void ExplConvView_ToggleOn()
        {
            if (BlShowInConversations)
            {
                _objView = _activeExplorer.CurrentFolder.Views[_objViewMem];
                _objView.Apply();
                BlShowInConversations = false;
            }
        }

        private void NavigateToOutlookFolder(MailItem mailItem)
        {
            if (_activeExplorer.CurrentFolder.FolderPath !=
                ((MAPIFolder)mailItem.Parent).FolderPath)
            {
                ExplConvView_ReturnState();
                _globals.Ol.App.ActiveExplorer().CurrentFolder = (MAPIFolder)mailItem.Parent;
                BlShowInConversations = AutoFile.AreConversationsGrouped(_activeExplorer);
            }
        }

        //PRIORITY: Implement OpenQFItem
        async public Task OpenQFItem(MailItem mailItem)
        {
            _parent.FormCtrlr.MinimizeFormViewer();
            NavigateToOutlookFolder(mailItem);
            if (_initType.HasFlag(Enums.InitTypeEnum.Sort) & AutoFile.AreConversationsGrouped(_activeExplorer))
                await Task.Run(() => ExplConvView_ToggleOff());
            
            if (_activeExplorer.IsItemSelectableInView(mailItem))
            {
                await Task.Run(() => _activeExplorer.ClearSelection());
                await Task.Run(() => _activeExplorer.AddToSelection(mailItem));

                //MAPIFolder tmp = _activeExplorer.CurrentFolder;
                //MAPIFolder drafts = _globals.Ol.NamespaceMAPI.GetDefaultFolder(OlDefaultFolders.olFolderDrafts);
                //_activeExplorer.CurrentFolder = drafts;
                //_activeExplorer.CurrentFolder.Display();
            }
            else
            {
                DialogResult result = MessageBox.Show("Selected message is not in view. Would you like to open it?",
                    "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes) { mailItem.Display(); }
            }
            if (_initType.HasFlag(Enums.InitTypeEnum.Sort) & BlShowInConversations)
                await Task.Run(() => ExplConvView_ToggleOn());
        }

        #region Email Sorting To Rewrite

        //TODO: Rewrite this MASTER_SortEmailsToExistingFolder
        public static void MASTER_SortEmailsToExistingFolder2(IList<MailItem> selItems, bool Pictures_Checkbox, string SortFolderpath, bool Save_MSG, bool Attchments, bool Remove_Flow_File, IApplicationGlobals AppGlobals, string StrRoot = "")
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
            var _globals = AppGlobals;
            if (string.IsNullOrEmpty(StrRoot))
            {
                StrRoot = _globals.Ol.ArchiveRootPath;
            }
            // TODO: Eliminate following line once Path.Combine used below
            loc = StrRoot + @"\";

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
            else if (folderCurrent.FolderPath.Contains(StrRoot) & (folderCurrent.FolderPath != StrRoot))
            {
                strFolderPath = folderCurrent.ToFsFolderpath(olAncestor: _globals.Ol.ArchiveRootPath, fsAncestorEquivalent: _globals.FS.FldrRoot);
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
            if ((StrRoot ?? "") != (_globals.Ol.ArchiveRootPath ?? ""))
            {
                strTemp2 = _globals.Ol.ArchiveRootPath.Substring(_globals.Ol.EmailRootPath.Length);
                FileSystem_LOC = _globals.FS.FldrRoot + strTemp2 + @"\" + SortFolderpath;  // Parent Directory
            }
            else
            {
                FileSystem_LOC = Path.Combine(_globals.FS.FldrRoot, SortFolderpath);
            }

            FileSystem_DelLOC = _globals.FS.FldrRoot;

            // If Save_PDF = True Then
            // Call SaveAsPDF.SaveMessageAsPDF(FileSystem_LOC, selItems)
            // End If

            if (Save_MSG == true)
            {
                SaveMessageAsMSG(FileSystem_LOC, selItems);
            }
            // 



            // ****Save Attachment to OneDrive directory****

            if (Attchments == true)
            {
                // Email_SortSaveAttachment.SaveAttachmentsFromSelection(SavePath:=FileSystem_LOC, Verify_Action:=Pictures_Checkbox, selItems:=selItems, save_images:=Pictures_Checkbox, SaveMSG:=Save_MSG)
                SaveAttachmentsModule.SaveAttachmentsFromSelection(AppGlobals: AppGlobals, SavePath: FileSystem_LOC, Verify_Action: Pictures_Checkbox, selItems: selItems, save_images: Pictures_Checkbox, SaveMSG: Save_MSG);
            }



            if (Remove_Flow_File == true)
            {
                SaveAttachmentsModule.SaveAttachmentsFromSelection(AppGlobals: AppGlobals, SavePath: strFolderPath, DELFILE: true, selItems: selItems);
            }



            // *************************************************************************
            // *********** LABEL EMAIL AS AUTOSORTED AND MOVE TO EMAIL FOLDER***********
            // *************************************************************************

            // If strTemp2 = "" Then Add_Recent(SortFolderpath)
            if (string.IsNullOrEmpty(strTemp2))
                _globals.AF.RecentsList.Add(SortFolderpath);
            loc = Path.Combine(StrRoot, SortFolderpath);
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
                                UpdateForMove(MSG, SortFolderpath, AppGlobals.AF.CtfMap, AppGlobals.AF.SubjectMap);
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

        //TODO: Rewrite CaptureMoveDetails 
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
                return string.Join("\t", strOutput
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

        //TODO: Rewrite WriteCSV_StartNewFileIfDoesNotExist To Split it into one task per function
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

        //QUESTION: Does this exist in a utility class? Check FileIO2
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

        //TODO: Implement SaveMessageAsMSG
        private static void SaveMessageAsMSG(string fileSystem_LOC, IList<MailItem> selItems)
        {
            throw new NotImplementedException();
        }

        //TODO: Convert GetCurrentExplorerFolder to use the folder handler class
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

        //public static void Cleanup_Files()
        //{
        //    // Call WRITE_Text_File     - Writes to the recents list
        //    // Call Email_AutoCategorize.CTF_Incidence_Text_File_WRITE - Writes to the CTF_Incidence file   
        //    // Call Email_AutoCategorize.Subject_MAP_Text_File_WRITE - Writes to the Subject_MAP file
        //}

        #endregion

    }
}
