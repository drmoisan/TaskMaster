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

        public QfcExplorerController(QfEnums.InitTypeEnum initType, IApplicationGlobals appGlobals, IFilerHomeController parent)
        {
            _initType = initType;
            _globals = appGlobals;
            _activeExplorer = _globals.Ol.App.ActiveExplorer();
            _parent = parent;
        }
        
        private QfEnums.InitTypeEnum _initType;
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
            _parent.FormController.MinimizeFormViewer();
            NavigateToOutlookFolder(mailItem);
            if (_initType.HasFlag(QfEnums.InitTypeEnum.Sort) & AutoFile.AreConversationsGrouped(_activeExplorer))
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
            if (_initType.HasFlag(QfEnums.InitTypeEnum.Sort) & BlShowInConversations)
                await Task.Run(() => ExplConvView_ToggleOn());
        }

        #region Email Sorting To Rewrite

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

        private static void UpdateForMove(MailItem mailItem, string fldr, CtfMap ctfMap, ISubjectMapSco subMap)
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
