using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.Office.Interop.Outlook;
//using Microsoft.VisualBasic;
//using Microsoft.VisualBasic.CompilerServices;
using ToDoModel;
using UtilitiesVB;
using Windows.Win32;



//[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace QuickFiler
{


    public class QuickFileController
    {

        private bool _useOld = true;
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region State Variables
        // Private state variables
        private Folder _folderCurrent;
        private int _intUniqueItemCounter;
        private int _intEmailPosition;
        private int _intEmailsPerIteration;
        private long _lngAcceleratorDialogueTop;
        private long _lngAcceleratorDialogueLeft;
        private bool _blSuppressEvents;
        private bool _blRunningModalCode = false;
        private bool _boolRemoteMouseApp;
        private cStopWatch _stopWatch;

        // Public state variables
        public bool BlFrmKll;
        public bool BlShowInConversations;
        #endregion
        #region Outlook View Variables
        public Microsoft.Office.Interop.Outlook.View ObjView;
        private string _objViewMem;
        public Microsoft.Office.Interop.Outlook.View ObjViewTemp;
        #endregion
        #region Resizing Variables
        // Left and Width Constants
        private long _heightFormMax;
        private long _heightFormMin;
        private long _heightPanelMainMax;
        private long _heightPanelMainMin;
        private long _lngPanelMainSCTop;
        private long _lngTopButtonOkMin;
        private long _lngTopButtonCancelMin;
        private long _lngTopButtonUndoMin;
        private long _lngTopAcceleratorDialogueMin;
        private long _lngTopSpnMin;
        #endregion
        #region Global Variables, Window Handles and Collections
        // Globals
        private IApplicationGlobals _globals;
        private readonly Explorer _activeExplorer;
        private readonly IOlObjects _olObjects;
        private readonly Microsoft.Office.Interop.Outlook.Application _olApp;
        private readonly QuickFileViewer _viewer;
        private cStackObject _movedMails;
        private Enums.InitTypeEnum _initType;

        // Collections
        private QfcGroupOperationsLegacy _legacy;
        // Public _colQFClass As Collection
        // Public ColFrames As Collection
        // Public ColMailJustMoved As Collection
        private Collection _colEmailsInFolder;
        internal Panel Frm;

        // Window Handles
        private IntPtr _olAppHWnd;
        private IntPtr _lFormHandle;

        // Cleanup
        public delegate void ParentCleanupMethod();
        private ParentCleanupMethod _parentCleanup;
        #endregion

        public QuickFileController(IApplicationGlobals AppGlobals, QuickFileViewer Viewer, Collection ColEmailsInFolder, ParentCleanupMethod ParentCleanup)
        {

            // Link viewer to controller
            _viewer = Viewer;
            _viewer.SetController(this);

            // Link model to controller
            _colEmailsInFolder = ColEmailsInFolder;
            InitializeModelProcessingMetrics();

            _parentCleanup = ParentCleanup;

            // Link controller to global variables 
            _globals = AppGlobals;
            _olObjects = AppGlobals.Ol;
            _olApp = AppGlobals.Ol.App;
            _activeExplorer = AppGlobals.Ol.App.ActiveExplorer();
            _folderCurrent = (Folder)_activeExplorer.CurrentFolder;
            _movedMails = AppGlobals.Ol.MovedMails_Stack;

            // Set readonly window handles
            _lFormHandle = _viewer.Handle;
            _olAppHWnd = PInvoke.GetAncestor((Windows.Win32.Foundation.HWND)_lFormHandle, Windows.Win32.UI.WindowsAndMessaging.GET_ANCESTOR_FLAGS.GA_PARENT);

            InitializeFormConfigurations();
            _legacy = new QfcGroupOperationsLegacy(_viewer, _initType, _globals, this);
            _viewer.Show();

            Iterate();
        }

        #region Master Control Functions

        public void Iterate()
        {
            _stopWatch = new cStopWatch();
            _stopWatch.Start();
            var colEmails = DequeNextEmailGroup(ref _colEmailsInFolder, _intEmailsPerIteration);
            _legacy.LoadControlsAndHandlers(colEmails);
        }

        private void InitializeModelProcessingMetrics()
        {
            _intEmailPosition = 0;    // Reverse sort is 0   'Regular sort is 1
        }

        private void InitializeFormConfigurations()
        {
            // Set conversation state variable with initial state
            BlShowInConversations = CurrentConversationState;
            if (BlShowInConversations)
                _objViewMem = Conversions.ToString(_activeExplorer.CurrentView.Name);

            // Suppress events while initializing form
            _blSuppressEvents = true;

            // Configure viewer for SORTING rather than FINDING items
            _initType = Enums.InitTypeEnum.InitSort;

            RemoveControlsTabstops();
            InitializeToleranceMinimums();
            _heightPanelMainMax = ResizeForToleranceMax();

            // Calculate the emails per page based on screen settings
            _intEmailsPerIteration = (int)Math.Round(Math.Round(_heightPanelMainMax / (double)(QuickFileControllerConstants.frmHt + QuickFileControllerConstants.frmSp), 0));
            _viewer.L1v2L2h5_SpnEmailPerLoad.Value = _intEmailsPerIteration;

            _blSuppressEvents = false;
        }

        private bool CurrentConversationState
        {
            get
            {
                if (_activeExplorer.CommandBars.GetPressedMso("ShowInConversations"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        #region Form UI Control

        private void RemoveControlsTabstops()
        {
            // Set defaults for controls on main form
            _viewer.L1v2L2h3_ButtonOK.TabStop = false;
            _viewer.L1v2L2h4_ButtonCancel.TabStop = false;
            _viewer.L1v2L2h4_ButtonUndo.TabStop = false;
            _viewer.L1v1L2_PanelMain.TabStop = false;
            _viewer.AcceleratorDialogue.TabStop = true;
            _viewer.L1v2L2h5_SpnEmailPerLoad.TabStop = false;
        }

        private void InitializeToleranceMinimums()
        {
            _lngPanelMainSCTop = 0L;
            _heightFormMin = _viewer.Height + QuickFileControllerConstants.frmHt + QuickFileControllerConstants.frmSp;
            _heightPanelMainMin = QuickFileControllerConstants.frmHt + QuickFileControllerConstants.frmSp;
            _lngTopButtonOkMin = _viewer.L1v2L2h3_ButtonOK.Top;
            _lngTopButtonCancelMin = _viewer.L1v2L2h4_ButtonCancel.Top;
            _lngTopButtonUndoMin = _viewer.L1v2L2h4_ButtonUndo.Top;
            _lngTopAcceleratorDialogueMin = _viewer.AcceleratorDialogue.Top;
            var _screen = Screen.FromControl(_viewer);
            _heightFormMax = _screen.WorkingArea.Height;
        }

        private long ResizeForToleranceMax()
        {
            // Resize form
            long lngPreviousHeight;
            long lngHeightDifference;
            lngHeightDifference = _heightFormMin - _viewer.Height;
            _viewer.L1v2L2h3_ButtonOK.Top = (int)(_viewer.L1v2L2h3_ButtonOK.Top + lngHeightDifference);
            _viewer.L1v2L2h4_ButtonCancel.Top = (int)(_viewer.L1v2L2h4_ButtonCancel.Top + lngHeightDifference);
            _viewer.L1v2L2h4_ButtonUndo.Top = (int)(_viewer.L1v2L2h4_ButtonUndo.Top + lngHeightDifference);
            _lngAcceleratorDialogueTop = _viewer.AcceleratorDialogue.Top + lngHeightDifference;
            _viewer.AcceleratorDialogue.Top = (int)_lngAcceleratorDialogueTop;
            _viewer.L1v2L2h5_SpnEmailPerLoad.Top = (int)(_viewer.L1v2L2h5_SpnEmailPerLoad.Top + lngHeightDifference);
            _lngTopSpnMin = _viewer.L1v2L2h5_SpnEmailPerLoad.Top;
            _lngAcceleratorDialogueLeft = _viewer.AcceleratorDialogue.Left;

            // Resize form
            lngPreviousHeight = _viewer.Height;
            _viewer.Height = (int)_heightFormMax;
            lngHeightDifference = _viewer.Height - lngPreviousHeight;
            _viewer.L1v2L2h3_ButtonOK.Top = (int)(_viewer.L1v2L2h3_ButtonOK.Top + lngHeightDifference);
            _viewer.L1v2L2h4_ButtonCancel.Top = (int)(_viewer.L1v2L2h4_ButtonCancel.Top + lngHeightDifference);
            _viewer.L1v2L2h4_ButtonUndo.Top = (int)(_viewer.L1v2L2h4_ButtonUndo.Top + lngHeightDifference);
            _lngAcceleratorDialogueTop = _viewer.AcceleratorDialogue.Top + lngHeightDifference;
            _viewer.AcceleratorDialogue.Top = (int)_lngAcceleratorDialogueTop;
            _lngAcceleratorDialogueLeft = _viewer.AcceleratorDialogue.Left;
            _viewer.L1v2L2h5_SpnEmailPerLoad.Top = (int)(_viewer.L1v2L2h5_SpnEmailPerLoad.Top + lngHeightDifference);

            // Set Max Size of the main panel based on resizing
            _viewer.L1v1L2_PanelMain.Height = (int)(_viewer.L1v1L2_PanelMain.Height + lngHeightDifference);

            return _viewer.L1v1L2_PanelMain.Height;
        }

        internal void FormResize(bool Force = false)
        {
            int intDiffy;
            int intDiffx;

            // MsgBox "App Width " & Me.Width & vbCrLf & "Screen Width " & ScreenWidth * PointsPerPixel
            if (!_blSuppressEvents | Force)
            {

                intDiffx = (int)(_viewer.Width >= QuickFileControllerConstants.Width_UserForm - 100L ? _viewer.Width - QuickFileControllerConstants.Width_UserForm : 0L);

                intDiffy = (int)(_viewer.Height >= _heightFormMin ? _viewer.Height - _heightFormMin : 0L);

                _viewer.L1v1L2_PanelMain.Width = (int)(QuickFileControllerConstants.Width_PanelMain + intDiffx);
                _viewer.L1v1L2_PanelMain.Height = (int)(_heightPanelMainMin + intDiffy);

                _viewer.L1v2L2h3_ButtonOK.Top = (int)(_lngTopButtonOkMin + intDiffy);
                _viewer.L1v2L2h3_ButtonOK.Left = (int)Math.Round(QuickFileControllerConstants.OK_left + intDiffx / 2d);
                _viewer.L1v2L2h4_ButtonCancel.Top = (int)(_lngTopButtonCancelMin + intDiffy);
                _viewer.L1v2L2h4_ButtonCancel.Left = (int)(_viewer.L1v2L2h3_ButtonOK.Left + QuickFileControllerConstants.CANCEL_left - QuickFileControllerConstants.OK_left);
                _viewer.L1v2L2h4_ButtonUndo.Top = (int)(_lngTopButtonUndoMin + intDiffy);
                _viewer.L1v2L2h4_ButtonUndo.Left = (int)(_viewer.L1v2L2h3_ButtonOK.Left + QuickFileControllerConstants.UNDO_left - QuickFileControllerConstants.OK_left);
                // Button1.top = lngTop_Button1_Min + intDiffy
                _viewer.AcceleratorDialogue.Top = (int)(_lngTopAcceleratorDialogueMin + intDiffy);
                _viewer.L1v2L2h5_SpnEmailPerLoad.Top = (int)(_lngTopSpnMin + intDiffy);
                _viewer.L1v2L2h5_SpnEmailPerLoad.Left = (int)(QuickFileControllerConstants.spn_left + intDiffx);

                _legacy.ResizeChildren(intDiffx);

            }

        }

        #endregion

        #region Data Model Manipulation

        private void EliminateDuplicateConversationIDs(ref Collection colTemp)
        {
            var dictID = new Dictionary<string, int>();
            int i;
            int max;

            foreach (MailItem olMail in colTemp)
            {
                if (dictID.ContainsKey(Conversions.ToString(olMail.ConversationID)))
                {
                    dictID[Conversions.ToString(olMail.ConversationID)] = dictID[Conversions.ToString(olMail.ConversationID)] + 1;
                }
                else
                {
                    dictID.Add(Conversions.ToString(olMail.ConversationID), 0);
                }
            }

            max = colTemp.Count;

            for (i = max; i >= 1L; i += -1)
            {
                MailItem objItem = (MailItem)colTemp[i];
                // Debug.Print dictID(olMail.ConversationID)
                if (dictID[Conversions.ToString(objItem.ConversationID)] > 0)
                {
                    colTemp.Remove(i);
                    dictID[Conversions.ToString(objItem.ConversationID)] = dictID[Conversions.ToString(objItem.ConversationID)] - 1;
                }
            }
        }

        private Collection ItemsToCollection(Items OlItems)
        {
            Collection ItemsToCollectionRet = default;
            Collection colTemp;
            colTemp = new Collection();
            foreach (var objItem in OlItems)
                colTemp.Add(objItem);
            ItemsToCollectionRet = colTemp;
            return ItemsToCollectionRet;

        }

        private void DebugOutPutEmailCollection(Collection colTemp)
        {
            MailItem OlMail;
            MeetingItem OlAppt;
            string strLine;
            int i;

            i = 0;
            foreach (var objItem in colTemp)
            {
                i += 1;
                strLine = "";
                if (objItem is MailItem)
                {
                    OlMail = (MailItem)objItem;
                    strLine = i + " " + GetFields.CustomFieldID_GetValue(objItem, "Triage") + " " + Strings.Format(OlMail.SentOn, "General Date") + " " + OlMail.Subject;
                }
                else if (objItem is AppointmentItem)
                {
                    OlAppt = (MeetingItem)objItem;
                    strLine = i + " " + GetFields.CustomFieldID_GetValue(objItem, "Triage") + " " + Strings.Format(OlAppt.SentOn, "General Date") + " " + OlAppt.Subject;
                }
                Debug.WriteLine(strLine);
            }
        }

        private Collection DequeNextEmailGroup(ref Collection MasterQueue, int Quantity)
        {
            int i;
            double max;

            Collection colEmails;

            colEmails = new Collection();
            max = Quantity < MasterQueue.Count ? Quantity : MasterQueue.Count;

            var loopTo = (int)Math.Round(max);
            for (i = 1; i <= loopTo; i++)
                colEmails.Add(MasterQueue[i]);
            for (i = (int)Math.Round(max); i >= 1; i -= 1)
                MasterQueue.Remove(i);

            return colEmails;
        }

        #endregion


        #region Keyboard event handlers
        internal void AcceleratorDialogue_Change()
        {
            if (!_blSuppressEvents)
                _legacy.ParseAcceleratorText();
        }

        internal void AcceleratorDialogue_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Alt:
                    {
                        _legacy.toggleAcceleratorDialogue();
                        break;
                    }
                case Keys.Down:
                    {
                        _legacy.SelectNextItem();
                        break;
                    }
                case Keys.Up:
                    {
                        _legacy.SelectPreviousItem();
                        break;
                    }
                case Keys.A:
                    {
                        if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift & (Control.ModifierKeys & Keys.Control) == Keys.Control)
                        {
                            _legacy.ToggleRemoteMouseLabels();
                        }

                        break;
                    }
            }
        }

        internal void AcceleratorDialogue_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox accelerator = sender as TextBox;
            if (e.Alt)
            {
                if (accelerator.Visible)
                {
                    accelerator.Focus();
                    accelerator.SelectionStart = accelerator.TextLength;
                }
                else
                {
                    _viewer.L1v1L2_PanelMain.Focus();
                }
                SendKeys.Send("{ESC}");
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Right:
                        {
                            if (accelerator.Visible)
                            {
                                _legacy.MakeSpaceToEnumerateConversation();
                            }

                            break;
                        }
                    case Keys.Left:
                        {
                            if (accelerator.Visible)
                            {
                                _legacy.RemoveSpaceToCollapseConversation();
                            }

                            break;
                        }

                    default:
                        {
                            break;
                        }
                }
            }

        }

        internal void ButtonCancel_KeyDown(object sender, KeyEventArgs e)
        {
            KeyboardHandler_KeyDown(sender, e);
        }

        internal void Button_OK_KeyDown(object sender, KeyEventArgs e)
        {
            // If DebugLVL And vbProcedure Then Debug.Print "Fired Button_OK_KeyDown"
            KeyboardHandler_KeyDown(sender, e);
        }

        internal void Button_OK_KeyUp(object sender, KeyEventArgs e)
        {
            KeyUpHandler(sender, e);
        }

        internal void PanelMain_KeyDown(object sender, KeyEventArgs e)
        {
            KeyboardHandler_KeyDown(sender, e);
        }

        internal void PanelMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            KeyPressHandler(sender, e);
        }

        internal void PanelMain_KeyUp(object sender, KeyEventArgs e)
        {
            KeyUpHandler(sender, e);
        }

        private void SpnEmailPerLoad_KeyDown(object sender, KeyEventArgs e)
        {
            KeyboardHandler_KeyDown(sender, e);
        }

        private void UserForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!_blSuppressEvents)
                KeyPressHandler(sender, e);
        }

        private void UserForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (!_blSuppressEvents)
                KeyUpHandler(sender, e);
        }

        private void UserForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_blSuppressEvents)
                KeyboardHandler_KeyDown(sender, e);
        }

        public void KeyPressHandler(object sender, KeyPressEventArgs e)
        {
            if (!_blSuppressEvents)
            {
                switch (e.KeyChar)
                {
                    default:
                        {
                            break;
                        }
                }
            }
        }

        public void KeyUpHandler(object sender, KeyEventArgs e)
        {
            if (!_blSuppressEvents)
            {
                switch (e.KeyCode)
                {
                    case Keys.Alt:
                        {
                            if (_viewer.AcceleratorDialogue.Visible)
                            {
                                _viewer.AcceleratorDialogue.Focus();
                                _viewer.AcceleratorDialogue.SelectionStart = _viewer.AcceleratorDialogue.TextLength;
                            }
                            else
                            {
                                bool unused = _viewer.L1v1L2_PanelMain.Focus();
                            }
                            SendKeys.Send("{ESC}");
                            break;
                        }
                    case Keys.Up:
                        {
                            if (_viewer.AcceleratorDialogue.Visible)
                                _viewer.AcceleratorDialogue.Focus();
                            break;
                        }
                    case Keys.Down:
                        {
                            if (_viewer.AcceleratorDialogue.Visible)
                                _viewer.AcceleratorDialogue.Focus();
                            break;
                        }

                    default:
                        {
                            break;
                        }
                }
            }
        }

        public void KeyboardHandler_KeyDown(object sender, KeyEventArgs e)
        {

            if (!_blSuppressEvents)
            {

                if (e.Alt)
                {
                    _legacy.toggleAcceleratorDialogue();
                    if (_viewer.AcceleratorDialogue.Visible)
                    {
                        _viewer.AcceleratorDialogue.Focus();
                    }
                    else
                    {
                        _viewer.L1v1L2_PanelMain.Focus();
                    }
                }

                else
                {
                    switch (e.KeyCode)
                    {
                        case Keys.Enter:
                            {
                                ButtonOK_Click();
                                break;
                            }
                        case Keys.Tab:
                            {
                                _legacy.toggleAcceleratorDialogue();
                                // Case vbKeyEscape
                                // vbMsgResponse = MsgBox("Stop all filing actions and close quick-filer?", vbOKCancel)
                                // If vbMsgResponse = vbOK Then ButtonCancel_Click
                                if (_viewer.AcceleratorDialogue.Visible)
                                    _viewer.AcceleratorDialogue.Focus();
                                break;
                            }

                        default:
                            {
                                if (_viewer.AcceleratorDialogue.Visible)
                                {
                                    AcceleratorDialogue_KeyDown(sender, e);
                                }
                                else
                                {
                                }

                                break;
                            }
                    }
                }
            }
        }

        #endregion

        #region Other Event Handlers

        internal void Cleanup()
        {
            ExplConvView_ReturnState();
            _olAppHWnd = default;
            _lFormHandle = default;
            _parentCleanup.Invoke();
        }

        internal void ButtonCancel_Click()
        {
            // ExplConvView_ToggleOn
            if (BlShowInConversations)
            {
                // ExplConvView_ToggleOn
                ExplConvView_Cleanup();
            }
            // ToggleShowAsConversation 1
            _legacy.RemoveControls();
            BlFrmKll = true;

            _viewer.Close();
        }

        internal void ButtonOK_Click()
        {

            if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
            {
                if (_blRunningModalCode == false)
                {
                    _blRunningModalCode = true;

                    if (_legacy.ReadyForMove)
                    {
                        _blSuppressEvents = true;
                        _legacy.MoveEmails(ref _movedMails);
                        QuickFileMetrics_WRITE("9999TimeWritingEmail.csv");
                        _legacy.RemoveControls();
                        Iterate();
                        _blSuppressEvents = false;
                    }
                    _blRunningModalCode = false;
                }
                else
                {
                    Interaction.MsgBox("Can't Execute While Running Modal Code");
                }
            }
            else
            {
                _viewer.Close();
            }
        }

        internal void ButtonUndo_Click()
        {
            int i;
            MailItem oMail_Old = null;
            MailItem oMail_Current = null;
            object objTemp;
            Folder oFolder_Current;
            Folder oFolder_Old;
            Collection colItems;
            DialogResult undoResponse;
            DialogResult repeatResponse;

            if (_movedMails is null)
                _movedMails = new cStackObject();
            repeatResponse = Constants.vbYes;

            i = _movedMails.Count();
            colItems = _movedMails.ToCollection();

            while (i > 1 & repeatResponse == Constants.vbYes)
            {
                objTemp = colItems[i];
                // objTemp = _movedMails.Pop
                if (objTemp is MailItem)
                    oMail_Current = (MailItem)objTemp;
                // objTemp = _movedMails.Pop
                objTemp = colItems[i - 1];
                if (objTemp is MailItem)
                    oMail_Old = (MailItem)objTemp;

                // oMail_Old = _movedMails.Pop
                if (MailEncrypted.Mail_IsItEncrypted(oMail_Current) == false & MailEncrypted.Mail_IsItEncrypted(oMail_Old) == false)
                {
                    oFolder_Current = (Folder)oMail_Current.Parent;
                    oFolder_Old = (Folder)oMail_Old.Parent;
                    undoResponse = MessageBox.Show("Undo Dialog", "Undo Move of email?" + Environment.NewLine + 
                        "Sent On: " + oMail_Current.SentOn.ToString("mm\\dd\\yyyy") + System.Environment.NewLine + 
                        oMail_Current.Subject, MessageBoxButtons.YesNo);

                    if (undoResponse == DialogResult.Yes & (oFolder_Current.FolderPath != oFolder_Old.FolderPath))
                    {
                        var unused = oMail_Current.Move(oFolder_Old);
                        _movedMails.Pop(i);
                        _movedMails.Pop(i - 1);
                    }
                }
                i -= 2;
                repeatResponse = MessageBox.Show("Undo Dialog", "Continue Undoing Moves?", MessageBoxButtons.YesNo);
            }
        }

        internal void SpnEmailPerLoad_Change()
        {
            if (_viewer.L1v2L2h5_SpnEmailPerLoad.Value >= 0m)
            {
                _intEmailsPerIteration = (int)Math.Round(_viewer.L1v2L2h5_SpnEmailPerLoad.Value);
            }
        }

        internal void Viewer_Activate()
        {
            if (_stopWatch is not null)
            {
                if (_stopWatch.isPaused == true)
                {
                    _stopWatch.reStart();
                }
            }
        }

        private void focusListener_ChangeFocus(bool gotFocus)
        {
            if (gotFocus)
            {
            }
            // Debug.Print "Gained Focus"
            // tn = TypeName(selection)
            // CopyButton.Enabled = IIf(tn = "Series", True, False)
            // On Error Resume Next
            // AC = ActiveChart
            // On Error GoTo 0
            // If AC Is Nothing Then
            // PasteButton.Enabled = False
            // Else
            // PasteButton.Enabled = readyToPaste 'TRUE if curve has been copied
            // End If
            else
            {
                Debug.Print("Lost Focus");
                // 'GoingAway

            }
        }

        // Friend Sub Form_Dispose()
        // Cleanup()
        // End Sub

        #endregion

        #region Outlook View UI Actions

        public void QFD_Minimize()
        {
            if (_stopWatch is not null)
            {
                if (_stopWatch.isPaused == false)
                {
                    _stopWatch.Pause();
                }
            }
            _viewer.WindowState = FormWindowState.Minimized;
        }

        public void QFD_Maximize()
        {
            _viewer.WindowState = FormWindowState.Maximized;
        }

        public void ExplConvView_Cleanup()
        {

            ObjView = _activeExplorer.CurrentFolder.Views[_objViewMem];
            if (Information.Err().Number == 0)
            {
                // ObjView.Reset
                ObjView.Apply();
                if (ObjViewTemp is not null)
                    ObjViewTemp.Delete();
                BlShowInConversations = false;
            }
            else
            {
                Information.Err().Clear();
                ObjViewTemp = (Microsoft.Office.Interop.Outlook.View)_activeExplorer.CurrentView.Parent("tmpNoConversation");
                if (ObjViewTemp is not null)
                    ObjViewTemp.Delete();
            }
        }

        public void ExplConvView_ToggleOff()
        {
            if (_olApp.ActiveExplorer().CommandBars.GetPressedMso("ShowInConversations"))
            {
                BlShowInConversations = true;
                ObjView = (Microsoft.Office.Interop.Outlook.View)_activeExplorer.CurrentView;

                if (ObjView.Name == "tmpNoConversation")
                {
                    if (_activeExplorer.CommandBars.GetPressedMso("ShowInConversations"))
                    {

                        ObjView.XML = Strings.Replace(ObjView.XML, "<upgradetoconv>1</upgradetoconv>", "", 1, Compare: Constants.vbTextCompare);
                        ObjView.Save();
                        ObjView.Apply();
                    }

                }

                _objViewMem = ObjView.Name;
                if (_objViewMem == "tmpNoConversation")
                    _objViewMem = _globals.Ol.View_Wide;

                ObjViewTemp = (Microsoft.Office.Interop.Outlook.View)ObjView.Parent("tmpNoConversation");

                if (ObjViewTemp is null)
                {
                    ObjViewTemp = ObjView.Copy("tmpNoConversation", OlViewSaveOption.olViewSaveOptionThisFolderOnlyMe);
                    ObjViewTemp.XML = Strings.Replace(ObjView.XML, "<upgradetoconv>1</upgradetoconv>", "", 1, Compare: Constants.vbTextCompare);
                    ObjViewTemp.Save();

                }
                ObjViewTemp.Apply();
            }

        }

        public void ExplConvView_ToggleOn()
        {
            if (BlShowInConversations)
            {
                ObjView = _activeExplorer.CurrentFolder.Views[_objViewMem];
                ObjView.Apply();
                BlShowInConversations = false;
            }

        }

        internal void ExplConvView_ReturnState()
        {
            if (BlShowInConversations)
                ExplConvView_ToggleOn();
        }

        internal void OpenQFMail(MailItem OlMail)
        {
            NavigateToOutlookFolder(OlMail);
            if (_initType.HasFlag(Enums.InitTypeEnum.InitSort) & AutoFile.AreConversationsGrouped(_activeExplorer))
                ExplConvView_ToggleOff();
            QFD_Minimize();
            _activeExplorer.ClearSelection();
            if (_activeExplorer.IsItemSelectableInView(OlMail))
                _activeExplorer.AddToSelection(OlMail);
            if (_initType.HasFlag(Enums.InitTypeEnum.InitSort) & BlShowInConversations)
                ExplConvView_ToggleOn();
        }

        private void NavigateToOutlookFolder(MailItem olMail)
        {
            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(_globals.Ol.App.ActiveExplorer().CurrentFolder.FolderPath, olMail.Parent.FolderPath, false)))
            {
                ExplConvView_ReturnState();
                _globals.Ol.App.ActiveExplorer().CurrentFolder = (MAPIFolder)olMail.Parent;
                BlShowInConversations = AutoFile.AreConversationsGrouped(_activeExplorer);
            }
            // If _globals.Ol.App.ActiveExplorer.CurrentFolder.DefaultItemType <> OlItemType.olMailItem Then
            // _globals.Ol.App.ActiveExplorer.NavigationPane.CurrentModule =
            // _globals.Ol.App.ActiveExplorer.NavigationPane.Modules _
            // .GetNavigationModule(OlNavigationModuleType.olModuleMail)
            // End If
        }

        #endregion

        #region Action Tracking

        private void QuickFileMetrics_WRITE(string filename)
        {

            string LOC_TXT_FILE;
            string curDateText, curTimeText, durationText, durationMinutesText;
            double Duration;
            string dataLineBeg;
            DateTime OlEndTime;
            DateTime OlStartTime;
            AppointmentItem OlAppointment;
            Folder OlEmailCalendar;


            // Create a line of comma seperated valued to store data
            curDateText = Strings.Format(DateTime.Now, "mm/dd/yyyy");
            // If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable curDateText = " & curDateText

            curTimeText = Strings.Format(DateTime.Now, "hh:mm");
            // If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable curTimeText = " & curTimeText

            dataLineBeg = curDateText + "," + curTimeText + ",";

            LOC_TXT_FILE = Path.Combine(_globals.FS.FldrMyD, filename);

            Duration = _stopWatch.timeElapsed;
            OlEndTime = DateTime.Now;
            OlStartTime = DateAndTime.DateAdd("S", -Duration, OlEndTime);

            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectGreater(_legacy.EmailsLoaded, 0, false)))
            {
                Duration = Conversions.ToDouble(Duration / _legacy.EmailsLoaded);
            }

            durationText = Strings.Format(Duration, "##0");
            // If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable durationText = " & durationText

            durationMinutesText = Strings.Format(Duration / 60d, "##0.00");

            OlEmailCalendar = Calendar.GetCalendar("Email Time", _olApp.Session);
            OlAppointment = (AppointmentItem)OlEmailCalendar.Items.Add(new AppointmentItem());
            {
                ref var withBlock = ref OlAppointment;
                withBlock.Subject = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Quick Filed ", _legacy.EmailsLoaded), " emails"));
                withBlock.Start = OlStartTime;
                withBlock.End = OlEndTime;
                withBlock.Categories = "@ Email";
                withBlock.ReminderSet = false;
                withBlock.Sensitivity = OlSensitivity.olPrivate;
                withBlock.Save();
            }

            string[] strOutput = _legacy.GetMoveDiagnostics(durationText, durationMinutesText, Duration, dataLineBeg, OlEndTime, ref OlAppointment);

            FileIO2.Write_TextFile(filename, strOutput, _globals.FS.FldrMyD);

        }

        private void GetDetails(string durationText, string durationMinutesText, double Duration, ref string dataLine, string dataLineBeg, ref QfcController QF, DateTime OlEndTime, cInfoMail infoMail, ref AppointmentItem OlAppointment, string[] strOutput)
        {

        }


        #endregion

    }
}