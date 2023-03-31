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
        private bool blShowInConversations;
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
        private readonly QfcFormViewer _viewer;
        private StackObjectVB _movedMails;
        private Enums.InitTypeEnum _initType;

        // Collections
        private QfcGroupOperationsLegacy _legacy;
        private Queue<MailItem> _queueEmailsInFolder;
        internal Panel Frm;

        // Window Handles
        private IntPtr _olAppHWnd;
        private IntPtr _lFormHandle;

        // Cleanup
        public delegate void ParentCleanupMethod();
        private ParentCleanupMethod _parentCleanup;
        #endregion

        public QuickFileController(
            IApplicationGlobals AppGlobals,
            QfcFormViewer Viewer,
            Queue<MailItem> ListEmailsInFolder,
            ParentCleanupMethod ParentCleanup)
        {
            // Link viewer to controller
            _viewer = Viewer;
            _viewer.SetController(this);

            // Link model to controller
            _queueEmailsInFolder = ListEmailsInFolder;
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
            var colEmails = DequeueNextEmailGroup(ref _queueEmailsInFolder, _intEmailsPerIteration);
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
                _objViewMem = _activeExplorer.CurrentView.Name;

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

        public bool BlShowInConversations { get => blShowInConversations; set => blShowInConversations = value; }

        #endregion

        #region Form UI Control

        private void RemoveControlsTabstops()
        {
            // Set defaults for controls on main form
            _viewer.L1v2L2h3_ButtonOK.TabStop = false;
            _viewer.L1v2L2h4_ButtonCancel.TabStop = false;
            _viewer.L1v2L2h4_ButtonUndo.TabStop = false;
            _viewer.L1v1L2_PanelMain.TabStop = false;
            _viewer.KeyboardDialog.TabStop = true;
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
            _lngTopAcceleratorDialogueMin = _viewer.KeyboardDialog.Top;
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
            _lngAcceleratorDialogueTop = _viewer.KeyboardDialog.Top + lngHeightDifference;
            _viewer.KeyboardDialog.Top = (int)_lngAcceleratorDialogueTop;
            _viewer.L1v2L2h5_SpnEmailPerLoad.Top = (int)(_viewer.L1v2L2h5_SpnEmailPerLoad.Top + lngHeightDifference);
            _lngTopSpnMin = _viewer.L1v2L2h5_SpnEmailPerLoad.Top;
            _lngAcceleratorDialogueLeft = _viewer.KeyboardDialog.Left;

            // Resize form
            lngPreviousHeight = _viewer.Height;
            _viewer.Height = (int)_heightFormMax;
            lngHeightDifference = _viewer.Height - lngPreviousHeight;
            _viewer.L1v2L2h3_ButtonOK.Top = (int)(_viewer.L1v2L2h3_ButtonOK.Top + lngHeightDifference);
            _viewer.L1v2L2h4_ButtonCancel.Top = (int)(_viewer.L1v2L2h4_ButtonCancel.Top + lngHeightDifference);
            _viewer.L1v2L2h4_ButtonUndo.Top = (int)(_viewer.L1v2L2h4_ButtonUndo.Top + lngHeightDifference);
            _lngAcceleratorDialogueTop = _viewer.KeyboardDialog.Top + lngHeightDifference;
            _viewer.KeyboardDialog.Top = (int)_lngAcceleratorDialogueTop;
            _lngAcceleratorDialogueLeft = _viewer.KeyboardDialog.Left;
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
                _viewer.KeyboardDialog.Top = (int)(_lngTopAcceleratorDialogueMin + intDiffy);
                _viewer.L1v2L2h5_SpnEmailPerLoad.Top = (int)(_lngTopSpnMin + intDiffy);
                _viewer.L1v2L2h5_SpnEmailPerLoad.Left = (int)(QuickFileControllerConstants.spn_left + intDiffx);

                _legacy.ResizeChildren(intDiffx);

            }

        }

        #endregion

        #region Data Model Manipulation

        private void EliminateDuplicateConversationIDs(ref List<MailItem> listEmails)
        {
            //TODO: Convert listObjItems logic to List<T>
            var dictID = new Dictionary<string, int>();
            int i;
            int max;

            foreach (MailItem olMail in listEmails)
            {
                if (dictID.ContainsKey(olMail.ConversationID))
                {
                    dictID[olMail.ConversationID] += 1;
                }
                else
                {
                    //QUESTION: I believe this should be 1 so I updated the count
                    dictID.Add(olMail.ConversationID, 1);
                }
            }

            max = listEmails.Count - 1;

            for (i = max; i >= 0; i += -1)
            {
                MailItem objItem = (MailItem)listEmails[i];
                // Debug.Print dictID(olMail.ConversationID)
                if (dictID[objItem.ConversationID] > 1)
                {
                    listEmails.RemoveAt(i);
                    dictID[objItem.ConversationID] = dictID[objItem.ConversationID] - 1;
                }
            }
        }

        private List<object> ItemsToCollection(Items OlItems)
        {
            List<object> listObjItems = new List<object>();
            foreach (var objItem in OlItems)
                listObjItems.Add(objItem);
            return listObjItems;

        }

        private void DebugOutPutEmailCollection(List<object> listObjItems)
        {
            MailItem OlMail;
            MeetingItem OlAppt;
            string strLine;
            int i;

            i = 0;
            foreach (var objItem in listObjItems)
            {
                i += 1;
                strLine = "";
                if (objItem is MailItem)
                {
                    OlMail = (MailItem)objItem;
                    strLine = i + " " + GetFields.CustomFieldID_GetValue(objItem, "Triage") + " " + OlMail.SentOn.ToString("General Date") + " " + OlMail.Subject;
                }
                else if (objItem is AppointmentItem)
                {
                    OlAppt = (MeetingItem)objItem;
                    strLine = i + " " + GetFields.CustomFieldID_GetValue(objItem, "Triage") + " " + OlAppt.SentOn.ToString("General Date") + " " + OlAppt.Subject;
                }
                Debug.WriteLine(strLine);
            }
        }

        private List<MailItem> DequeueNextEmailGroup(ref Queue<MailItem> MasterQueue, int Quantity)
        {
            int i;
            double max;

            List<MailItem> listEmails = new();

            max = Quantity < MasterQueue.Count ? Quantity : MasterQueue.Count;

            var loopTo = (int)Math.Round(max);
            for (i = 1; i <= loopTo; i++)
                listEmails.Add(MasterQueue.Dequeue());
            return listEmails;
        }

        #endregion


        #region Keyboard event handlers
        internal void KeyboardDialog_Change()
        {
            if (!_blSuppressEvents)
                _legacy.ParseKeyboardText();
        }

        internal void KeyboardDialog_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Alt:
                    {
                        e.Handled = true;
                        _legacy.ToggleKeyboardDialog();
                        break;
                    }
                case Keys.Down:
                    {
                        e.Handled = true;
                        _legacy.SelectNextItem();
                        break;
                    }
                case Keys.Up:
                    {
                        e.Handled = true;
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

        internal void KeyboardDialog_KeyUp(object sender, KeyEventArgs e)
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
            KeyboardHandler_KeyUp(sender, e);
        }

        internal void PanelMain_KeyDown(object sender, KeyEventArgs e)
        {
            KeyboardHandler_KeyDown(sender, e);
        }

        internal void PanelMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            KeyboardHandler_KeyPress(sender, e);
        }

        internal void PanelMain_KeyUp(object sender, KeyEventArgs e)
        {
            KeyboardHandler_KeyUp(sender, e);
        }

        private void SpnEmailPerLoad_KeyDown(object sender, KeyEventArgs e)
        {
            KeyboardHandler_KeyDown(sender, e);
        }

        private void UserForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!_blSuppressEvents)
                KeyboardHandler_KeyPress(sender, e);
        }

        private void UserForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (!_blSuppressEvents)
                KeyboardHandler_KeyUp(sender, e);
        }

        private void UserForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_blSuppressEvents)
                KeyboardHandler_KeyDown(sender, e);
        }

        public void KeyboardHandler_KeyPress(object sender, KeyPressEventArgs e)
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

        public void KeyboardHandler_KeyUp(object sender, KeyEventArgs e)
        {
            if (!_blSuppressEvents)
            {
                switch (e.KeyCode)
                {
                    case Keys.Alt:
                        {
                            if (_viewer.KeyboardDialog.Visible)
                            {
                                _viewer.KeyboardDialog.Focus();
                                _viewer.KeyboardDialog.SelectionStart = _viewer.KeyboardDialog.TextLength;
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
                            if (_viewer.KeyboardDialog.Visible)
                                _viewer.KeyboardDialog.Focus();
                            break;
                        }
                    case Keys.Down:
                        {
                            if (_viewer.KeyboardDialog.Visible)
                                _viewer.KeyboardDialog.Focus();
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
                    _legacy.ToggleKeyboardDialog();
                    if (_viewer.KeyboardDialog.Visible)
                    {
                        _viewer.KeyboardDialog.Focus();
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
                                _legacy.ToggleKeyboardDialog();
                                // Case vbKeyEscape
                                // vbMsgResponse = MsgBox("Stop all filing actions and close quick-filer?", vbOKCancel)
                                // If vbMsgResponse = vbOK Then ButtonCancel_Click
                                if (_viewer.KeyboardDialog.Visible)
                                    _viewer.KeyboardDialog.Focus();
                                break;
                            }

                        default:
                            {
                                if (_viewer.KeyboardDialog.Visible)
                                {
                                    KeyboardDialog_KeyDown(sender, e);
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
                    MessageBox.Show("Error", "Can't Execute While Running Modal Code");
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
            List<object> listItems;
            DialogResult undoResponse;
            DialogResult repeatResponse = DialogResult.Yes;

            if (_movedMails is null)
                _movedMails = new StackObjectVB();

            i = _movedMails.Count() - 1;
            listItems = _movedMails.ToList();

            while (i > 0 & repeatResponse == DialogResult.Yes)
            {
                objTemp = listItems[i];
                // objTemp = _movedMails.Pop
                if (objTemp is MailItem)
                    oMail_Current = (MailItem)objTemp;
                // objTemp = _movedMails.Pop
                objTemp = listItems[i - 1];
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
            try
            {
                ObjView.Apply();
                if (ObjViewTemp is not null)
                    ObjViewTemp.Delete();
                BlShowInConversations = false;
            }
            catch (System.Exception)
            {
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

                        ObjView.XML = ObjView.XML.Replace("<upgradetoconv>1</upgradetoconv>", "");
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
                    ObjViewTemp.XML = ObjView.XML.Replace("<upgradetoconv>1</upgradetoconv>", "");
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
            if (_activeExplorer.IsItemSelectableInView(OlMail))
            {
                _activeExplorer.ClearSelection();
                _activeExplorer.AddToSelection(OlMail);

                MAPIFolder tmp = _activeExplorer.CurrentFolder;
                MAPIFolder drafts = _globals.Ol.NamespaceMAPI.GetDefaultFolder(OlDefaultFolders.olFolderDrafts);
                _activeExplorer.CurrentFolder = drafts;
                _activeExplorer.CurrentFolder.Display();
                _activeExplorer.CurrentFolder = tmp;
                _activeExplorer.CurrentFolder.Display();


            }
            else
            {
                DialogResult result = MessageBox.Show("Error",
                    "Selected message is not in view. Would you like to open it?", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes) { OlMail.Display(); }
            }
            if (_initType.HasFlag(Enums.InitTypeEnum.InitSort) & BlShowInConversations)
                ExplConvView_ToggleOn();
        }

        private void NavigateToOutlookFolder(MailItem olMail)
        {
            if (_globals.Ol.App.ActiveExplorer().CurrentFolder.FolderPath != olMail.Parent.FolderPath)
            {
                ExplConvView_ReturnState();
                _globals.Ol.App.ActiveExplorer().CurrentFolder = (MAPIFolder)olMail.Parent;
                BlShowInConversations = AutoFile.AreConversationsGrouped(_activeExplorer);
            }
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
            curDateText = DateTime.Now.ToString("mm/dd/yyyy");
            // If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable curDateText = " & curDateText

            curTimeText = DateTime.Now.ToString("hh:mm");
            // If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable curTimeText = " & curTimeText

            dataLineBeg = curDateText + "," + curTimeText + ",";

            LOC_TXT_FILE = Path.Combine(_globals.FS.FldrMyD, filename);

            Duration = _stopWatch.timeElapsed;
            OlEndTime = DateTime.Now;
            OlStartTime = OlEndTime.Subtract(new TimeSpan(0, 0, 0, (int)Duration));

            if (_legacy.EmailsLoaded > 0)
            {
                Duration /= _legacy.EmailsLoaded;
            }

            durationText = Duration.ToString("##0");
            // If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable durationText = " & durationText

            durationMinutesText = (Duration / 60d).ToString("##0.00");

            OlEmailCalendar = Calendar.GetCalendar("Email Time", _olApp.Session);
            OlAppointment = (AppointmentItem)OlEmailCalendar.Items.Add(new AppointmentItem());
            {
                OlAppointment.Subject = "Quick Filed " + _legacy.EmailsLoaded as string + " emails";
                OlAppointment.Start = OlStartTime;
                OlAppointment.End = OlEndTime;
                OlAppointment.Categories = "@ Email";
                OlAppointment.ReminderSet = false;
                OlAppointment.Sensitivity = OlSensitivity.olPrivate;
                OlAppointment.Save();
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