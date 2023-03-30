using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using TaskVisualization;
using ToDoModel;
using UtilitiesVB;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

namespace QuickFiler
{


    internal class QfcController
    {

        #region Global Variables, Window Handles and Collections
        private IQfcControllerCallbacks _callbacks;
        private Enums.InitTypeEnum _initType;
        private cFolderHandler _fldrHandler;
        private IntPtr hWndCaller;

        private bool p_BoolRemoteMouseApp;
        private cConversation conv;

        private IApplicationGlobals _globals;
        private Explorer _activeExplorer;
        #endregion

        #region QFC Specific Variables
        private int _intMyPosition;
        private cSuggestions _suggestions = new cSuggestions();
        private string[] _strFolders;
        //TODO: Need to ensure references to _colCtrls are zero based
        private List<Control> _colCtrls;
        //TODO: Need to ensure references to _selItemsInClass are zero based
        private IList _selItemsInClass;
        private bool _blAccelFocusToggle;
        private int _intEnterCounter;
        private int _intComboRightCtr;
        public bool blExpanded;
        public bool blHasChild;
        #endregion
        
        #region UI Controls WithEvents
        private Control _mPassedControl;

        // Checkbox to Group Conversations
        private CheckBox _conversationCb;               // CONTROL WITH EVENTS
        public virtual CheckBox ConversationCb          // PROPERTY used to assign the control and wire events
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _conversationCb;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_conversationCb != null)
                {
                    _conversationCb.CheckedChanged -= chk_Click;
                    _conversationCb.KeyDown -= chk_KeyDown;
                    _conversationCb.KeyUp -= chk_KeyUp;
                }

                _conversationCb = value;
                if (_conversationCb != null)
                {
                    _conversationCb.CheckedChanged += chk_Click;
                    _conversationCb.KeyDown += chk_KeyDown;
                    _conversationCb.KeyUp += chk_KeyUp;
                }
            }
        }

        // Combo box containing Folder Suggestions
        private ComboBox _folderCbo;                    // CONTROL WITH EVENTS
        public virtual ComboBox FolderCbo               // PROPERTY used to assign the control and wire events
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _folderCbo;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_folderCbo != null)
                {
                    _folderCbo.KeyDown -= cbo_KeyDown;
                    _folderCbo.KeyUp -= cbo_KeyUp;
                }

                _folderCbo = value;
                if (_folderCbo != null)
                {
                    _folderCbo.KeyDown += cbo_KeyDown;
                    _folderCbo.KeyUp += cbo_KeyUp;
                }
            }
        }
          
        // Input for folder search
        private TextBox _searchTxt;                     // CONTROL WITH EVENTS
        private TextBox SearchTxt                       // PROPERTY used to assign the control and wire events
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _searchTxt;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_searchTxt != null)
                {
                    _searchTxt.TextChanged -= txt_Change;
                    _searchTxt.KeyDown -= txt_KeyDown;
                    _searchTxt.KeyPress -= txt_KeyPress;
                    _searchTxt.KeyUp -= txt_KeyUp;
                }

                _searchTxt = value;
                if (_searchTxt != null)
                {
                    _searchTxt.TextChanged += txt_Change;
                    _searchTxt.KeyDown += txt_KeyDown;
                    _searchTxt.KeyPress += txt_KeyPress;
                    _searchTxt.KeyUp += txt_KeyUp;
                }
            }
        }
        
        // Body of the Email
        private TextBox _bdy;                          // CONTROL  Private body control with events assigned
        private TextBox TxtBoxBody                     // PROPERTY Private body property to assign the control and wite events
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _bdy;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_bdy != null)
                {
                    _bdy.Click -= bdy_Click;
                }

                _bdy = value;
                if (_bdy != null)
                {
                    _bdy.Click += bdy_Click;
                }
            }
        }

        // Button to remove email from Processing
        private Button __cbKll;                         // CONTROL  Private body control with events assigned
        private Button CbKll                           // PROPERTY Private body property to assign the control and wite events
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return __cbKll;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (__cbKll != null)
                {
                    __cbKll.Click -= cbKll_Click;
                    __cbKll.KeyDown -= cbKll_KeyDown;
                    __cbKll.KeyPress -= cbKll_KeyPress;
                    __cbKll.KeyUp -= cbKll_KeyUp;
                }

                __cbKll = value;
                if (__cbKll != null)
                {
                    __cbKll.Click += cbKll_Click;
                    __cbKll.KeyDown += cbKll_KeyDown;
                    __cbKll.KeyPress += cbKll_KeyPress;
                    __cbKll.KeyUp += cbKll_KeyUp;
                }
            }
        }

        // Button to delete email
        private Button __cbDel;                         // CONTROL  Private body control with events assigned
        private Button CbDel    
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return __cbDel;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (__cbDel != null)
                {
                    __cbDel.Click -= cbDel_Click;
                    __cbDel.KeyDown -= cbDel_KeyDown;
                    __cbDel.KeyPress -= cbDel_KeyPress;
                    __cbDel.KeyUp -= cbDel_KeyUp;
                }

                __cbDel = value;
                if (__cbDel != null)
                {
                    __cbDel.Click += cbDel_Click;
                    __cbDel.KeyDown += cbDel_KeyDown;
                    __cbDel.KeyPress += cbDel_KeyPress;
                    __cbDel.KeyUp += cbDel_KeyUp;
                }
            }
        }

        private Button _flagTaskCb;
        private Button FlagTaskCb    // Flag as Task
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _flagTaskCb;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_flagTaskCb != null)
                {
                    _flagTaskCb.Click -= cbFlag_Click;
                    _flagTaskCb.KeyDown -= cbFlag_KeyDown;
                    _flagTaskCb.KeyPress -= cbFlag_KeyPress;
                    _flagTaskCb.KeyUp -= cbFlag_KeyUp;
                }

                _flagTaskCb = value;
                if (_flagTaskCb != null)
                {
                    _flagTaskCb.Click += cbFlag_Click;
                    _flagTaskCb.KeyDown += cbFlag_KeyDown;
                    _flagTaskCb.KeyPress += cbFlag_KeyPress;
                    _flagTaskCb.KeyUp += cbFlag_KeyUp;
                }
            }
        }

        private Button _cbTmp;
        private Button CbTmp
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cbTmp;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cbTmp != null)
                {
                    _cbTmp.KeyDown -= cbTmp_KeyDown;
                    _cbTmp.KeyUp -= cbTmp_KeyUp;
                }

                _cbTmp = value;
                if (_cbTmp != null)
                {
                    _cbTmp.KeyDown += cbTmp_KeyDown;
                    _cbTmp.KeyUp += cbTmp_KeyUp;
                }
            }
        }
        
        private Panel _frm;
        public virtual Panel Frm
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _frm;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_frm != null)
                {
                    _frm.KeyDown -= frm_KeyDown;
                    _frm.KeyPress -= frm_KeyPress;
                    _frm.KeyUp -= frm_KeyUp;
                }

                _frm = value;
                if (_frm != null)
                {
                    _frm.KeyDown += frm_KeyDown;
                    _frm.KeyPress += frm_KeyPress;
                    _frm.KeyUp += frm_KeyUp;
                }
            }
        }
        #endregion
        
        #region UI Controls - Others Without Events
        public Label lblConvCt;                   // Count of Conversation Members
        private Label _lblMyPosition;             // ACCELERATOR Email Position
        private Label _lbl1;                      // From:
        private Label _lbl2;                      // Subject:
        private Label _lbl3;                      // Body:
        private Label _lbl4;                      // Sent On:
        private Label _lbl5;                      // Folder:
        private Label _lblSender;                   // <SENDER>
        public Label LblSubject;                  // <SUBJECT>
        public string StrlblTo;                   // <TO>
        public Label lblSentOn;                   // <SENTON>
        public Label lblTriage;                   // X as Triage placeholder
        public Label lblActionable;               // <ACTIONABL>
        private Label lblAcF;                     // ACCELERATOR F for Folder Search
        private Label lblAcD;                     // ACCELERATOR D for Folder Dropdown
        private Label lblAcC;                     // ACCELERATOR C for Grouping Conversations
        private Label lblAcX;                     // ACCELERATOR X for Delete email
        private Label lblAcR;                     // ACCELERATOR R for remove item from list
        private Label lblAcT;                     // ACCELERATOR T for Task ... Flag item and make it a task
        private Label lblAcO;                     // ACCELERATOR O for Open Email
        private Label lblAcA;                     // ACCELERATOR A for Save Attachments
        private Label lblAcW;                     // ACCELERATOR W for Delete Flow
        private Label lblAcM;                     // ACCELERATOR M for Save Mail
        private Label _lblTmp;

        // QUESTION: Shouldn't these have events???
        private CheckBox _chbxSaveAttach;
        private CheckBox _chbxSaveMail;
        private CheckBox _chbxDelFlow;
        #endregion
        
        #region Outlook Variables
        public MailItem Mail;
        private Folder _fldrOriginal;
        private Folder _fldrTarget;
        #endregion

        #region Resizing Variables
        public struct ctrlPosition
        {
            public bool blInOrigPos;
            public int topOriginal;
            public int topNew;
            public int leftOriginal;
            public int leftNew;
            public int heightOriginal;
            public int heightNew;
            public int widthOriginal;
            public int widthNew;
        }

        private ctrlPosition pos_chbxSaveAttach;  // Checkbox Save Attachment X% Left Position
        private ctrlPosition pos_chbxSaveMail;    // Checkbox Save Mail X% Left Position
        private ctrlPosition pos_chbxDelFlow;     // Checkbox Delete Flow X% Left Position
        private ctrlPosition pos_lblAcA;          // A Accelerator X% Left Position
        private ctrlPosition pos_lblAcW;          // W Accelerator X% Left Position
        private ctrlPosition pos_lblAcM;          // M Accelerator X% Left Position
        private ctrlPosition pos_frm;
        private ctrlPosition pos_cbo;
        private ctrlPosition pos_chk;
        private ctrlPosition pos_body;
        private ctrlPosition pos_lblAcC;
        private ctrlPosition pos_lblAcD;
        private ctrlPosition pos_lblAcO;


        private int lblSubject_Width;
        private int lblBody_Width;               // Body Width
        private int cbFlag_Left;                 // Task button X% left position
        private int lblAcT_Left;                 // Task accelerator X% left position
        private int lbl5_Left;                   // Folder label X% left position
        private int txt_Left;                    // Folder search box X% left position Y% Width
        private int txt_Width;                   // Folder search box X% left position Y% Width
        private int lblAcF_Left;                 // F Accelerator X% left position
        private int lblAcD_Left;                 // D Accelerator X% left position
        private int cbo_Left;                    // Dropdown box X% Left position Y% Width
        private int cbo_Width;                   // Dropdown box X% Left position Y% Width
        private int cbDel_Left;                  // Delete button X+Y% Left position
        private int cbKll_Left;
        private int lblAcX_Left;
        private int lblAcR_Left;
        private int lblAcC_Left;                 // Conversation accelerator X% Left position
        private int chk_Left;                    // Conversation checkbox X% Left Position
        private int lblConvCt_Left;              // Conversation Count X% Left Position
        private int chbxSaveAttach_Left;         // Checkbox Save Attachment X% Left Position
        private int chbxSaveMail_Left;           // Checkbox Save Mail X% Left Position
        private int chbxDelFlow_Left;            // Checkbox Delete Flow X% Left Position
        private int lblAcA_Left;                 // A Accelerator X% Left Position
        private int lblAcW_Left;                 // W Accelerator X% Left Position
        private int lblAcM_Left;                 // M Accelerator X% Left Position
        private int lngBlock_Width;              // Width of block of controls that need to be right justified
        private int lblActionable_Left;
        private int lblActionable_Width;
        private int lblSentOn_Left;
        private int lblTriage_Left;
        private int lblTriage_Width;
        #endregion

        #region Notes
        // The following functions are needed that reside at a higher level in the process
        // due to the fact that they require interaction with other instances of this class
        // as well as the parent form and object data model
        // 
        // QFD_Minimize
        // KeyDownHandler
        // KeyboardHandler_KeyUp
        // KeyboardHandler_KeyPress
        // toggleAcceleratorDialogue
        // RemoveSpecificControlGroup
        // ExplConvView_ToggleOn
        // ConvToggle_Group
        // ConvToggle_UnGroup
        #endregion

        #region Main Class Functions
        internal QfcController(MailItem m_mail,
                               List<Control> controlList,
                               int intPositionArg,
                               bool BoolRemoteMouseApp,
                               IQfcControllerCallbacks CallbackFunctions,
                               IApplicationGlobals AppGlobals,
                               IntPtr hwnd = default,
                               Enums.InitTypeEnum InitTypeE = Enums.InitTypeEnum.InitSort)
        {
            // Wire global and delegate variables and handles
            _globals = AppGlobals;
            _callbacks = CallbackFunctions;
            _activeExplorer = AppGlobals.Ol.App.ActiveExplorer();
            _initType = InitTypeE;
            hWndCaller = hwnd;
            _intMyPosition = intPositionArg;
            Mail = m_mail;

            //Resolve control and wire handlers
            ResolveControlAssignments(controlList);
            WireEventHandlers();
            EmailFormatting();
            SetInitialControlSizePosition();

            

            StrlblTo = Mail.To;

            if (BoolRemoteMouseApp)
                ToggleRemoteMouseAppLabels();
        }

        //temporary for getting tests running
        // TODO: create interface for testing
        internal QfcController() { }

        private void ResolveControlAssignments(List<Control> controlList)
        {
            // Resolve controls in collection to their specific control
            // TODO: Simplify control resolution. It is overengineered
            _fldrOriginal = (Folder)Mail.Parent;
            string strBodyText;
            _colCtrls = controlList;
            foreach (Control ctlTmp in controlList)
            {
                Debug.WriteLine(ctlTmp.GetType().Name);
                switch (ctlTmp.GetType().Name ?? "")
                {
                    case "Panel":
                        {
                            Frm = (Panel)ctlTmp;
                            break;
                        }
                    case "CheckBox":
                        {
                            switch (ctlTmp.Text ?? "")
                            {
                                case "  Conversation":
                                    {
                                        ConversationCb = (CheckBox)ctlTmp;
                                        break;
                                    }
                                case " Attach":
                                    {
                                        _chbxSaveAttach = (CheckBox)ctlTmp;
                                        break;
                                    }
                                case " Flow":
                                    {
                                        _chbxDelFlow = (CheckBox)ctlTmp;
                                        break;
                                    }
                                case " Mail":
                                    {
                                        _chbxSaveMail = (CheckBox)ctlTmp;
                                        break;
                                    }
                            }

                            break;
                        }
                    case "ComboBox":
                        {
                            FolderCbo = (ComboBox)ctlTmp;
                            break;
                        }
                    case "TextBox":
                        {
                            if (ctlTmp.Text == "<BODY>")
                            {
                                strBodyText = Mail.Body.Replace(System.Environment.NewLine, " ");
                                strBodyText = strBodyText.Replace("  ", " ");
                                strBodyText = strBodyText.Replace("  ", " ") + "<EOM>";
                                ctlTmp.Text = strBodyText;
                                TxtBoxBody = (TextBox)ctlTmp;
                                TxtBoxBody = (TextBox)ctlTmp;
                            }
                            else
                            {
                                SearchTxt = (TextBox)ctlTmp;
                            }

                            break;
                        }

                    case "Label":
                        {
                            _lblTmp = (Label)ctlTmp;
                            switch (_lblTmp.Text ?? "")
                            {
                                case "From:":
                                    {
                                        _lbl1 = _lblTmp;
                                        break;
                                    }
                                case "Subject:":
                                    {
                                        _lbl2 = _lblTmp;
                                        break;
                                    }
                                case "Body:":
                                    {
                                        _lbl3 = _lblTmp;
                                        break;
                                    }
                                case "Sent On:":
                                    {
                                        _lbl4 = _lblTmp;
                                        break;
                                    }
                                case "Folder:":
                                    {
                                        _lbl5 = _lblTmp;
                                        break;
                                    }
                                case "<SENDER>":
                                    {
                                        _lblTmp.Text = Mail.Sent == true ? CaptureEmailDetailsModule.GetSenderAddress(Mail) : "Draft Message";
                                        _lblSender = _lblTmp;
                                        break;
                                    }
                                case "<SUBJECT>":
                                    {
                                        _lblTmp.Text = Mail.Subject;
                                        LblSubject = _lblTmp;
                                        break;
                                    }
                                case "ABC":
                                    {
                                        _lblTmp.Text = GetFields.CustomFieldID_GetValue(Mail, "Triage");
                                        lblTriage = _lblTmp;
                                        break;
                                    }
                                case "<ACTIONABL>":
                                    {
                                        _lblTmp.Text = GetFields.CustomFieldID_GetValue(Mail, "Actionable");
                                        lblActionable = _lblTmp;
                                        break;
                                    }
                                case "<#>":
                                    {
                                        lblConvCt = _lblTmp;
                                        break;
                                    }
                                case "<Pos#>":
                                    {
                                        _lblMyPosition = _lblTmp;
                                        break;
                                    }
                                case "<BODY>":
                                    {
                                        break;
                                    }

                                case "<SENTON>":
                                    {
                                        _lblTmp.Text = Mail.SentOn.ToString("MM/dd/yy HH:MM");
                                        lblSentOn = _lblTmp;
                                        break;
                                    }
                                case "F":
                                    {
                                        lblAcF = _lblTmp;
                                        break;
                                    }

                                case "D":
                                    {
                                        lblAcD = _lblTmp;
                                        break;
                                    }
                                case "C":
                                    {
                                        lblAcC = _lblTmp;
                                        break;
                                    }
                                case "X":
                                    {
                                        lblAcX = _lblTmp;
                                        break;
                                    }
                                case "R":
                                    {
                                        lblAcR = _lblTmp;
                                        break;
                                    }
                                case "T":
                                    {
                                        lblAcT = _lblTmp;
                                        break;
                                    }
                                case "O":
                                    {
                                        lblAcO = _lblTmp;
                                        break;
                                    }
                                case "A":
                                    {
                                        lblAcA = _lblTmp;
                                        break;
                                    }
                                case "W":
                                    {
                                        lblAcW = _lblTmp;
                                        break;
                                    }
                                case "M":
                                    {
                                        lblAcM = _lblTmp;
                                        break;
                                    }
                            }

                            break;
                        }
                    case "Button":
                        {
                            CbTmp = (Button)ctlTmp;
                            if (CbTmp.Text == "X")
                            {
                                CbDel = (Button)ctlTmp;
                            }
                            else if (CbTmp.Text == "-->")
                            {
                                CbKll = (Button)ctlTmp;
                            }
                            else if (CbTmp.Text == "|>")
                            {
                                FlagTaskCb = (Button)ctlTmp;
                            }

                            break;
                        }
                }

            }
        }

        internal void Init_FolderSuggestions(object varList = null)
        {

            int i;
            UserProperty objProperty;

            if (!(varList is Array))
            {
                objProperty = Mail.UserProperties.Find("FolderKey");
                if (objProperty is not null)
                    varList = objProperty.Value;
            }
            if (varList is Array)
            {
                Array varArray = varList as Array;
                if (ArrayIsAllocated.IsAllocated(ref varArray))
                {
                    // For i = LBound(varList) To UBound(varList)
                    FolderCbo.Items.AddRange((object[])varList);
                    FolderCbo.SelectedIndex = 0;
                    // Next i
                }
            }
            else
            {
                // TODO: cSuggestions and cFolderHandler are to mixed up with functionality. Need to clean up.
                _suggestions = FolderSuggestionsModule.Folder_Suggestions(Mail, _globals, false);

                if (_suggestions.Count > 0)
                {
                    Array.Resize(ref _strFolders, _suggestions.Count + 1);
                    var loopTo = _suggestions.Count;
                    for (i = 1; i <= loopTo; i++)
                        _strFolders[i] = _suggestions.FolderList[i];
                    FolderCbo.Items.AddRange(_strFolders);
                    FolderCbo.SelectedIndex = 1;
                }
                else
                {
                    _fldrHandler = new cFolderHandler(_globals);
                    FolderCbo.Items.AddRange(_fldrHandler.FindFolder("", true, ReCalcSuggestions: true, objItem: Mail));

                    if (FolderCbo.Items.Count >= 2)
                        FolderCbo.SelectedIndex = 2;
                }

            }

            // Set _fldrHandler = New cFolderHandler
            // cbo.List = _fldrHandler.FindFolder("", True, ReCalcSuggestions:=True, objItem:=mail)
            // If cbo.ListCount >= 2 Then cbo.Value = cbo.List(2)

            // Set objProperty = mail.UserProperties.FIND("AutoFile")
            // If Not objProperty Is Nothing Then _searchTxt.Value = objProperty.Value


            // Call Email_SortToExistingFolder.FindFolder("", True, objItem:=mail)




        }

        internal void CountMailsInConv(int ct = 0)
        {



            // Dim Sel As Collection

            if (ct != 0)
            {
                lblConvCt.Text = ct.ToString();
            }
            else
            {
                conv = new cConversation(_globals.Ol.App) { item = Mail };
                _selItemsInClass = conv.get_ToList(true, true);
                lblConvCt.Text = _selItemsInClass.Count.ToString();
            }



        }

        public void MoveMail()
        {
            IList selItems = new List<MailItem>();

            bool Attchments;
            bool blRepullConv;
            bool blDoMove;

            blRepullConv = false;

            if (Mail is not null)
            {
                if (ConversationCb.Checked == true)
                {
                    if (_selItemsInClass is not null)
                    {
                        if ((_selItemsInClass.Count == int.Parse(lblConvCt.Text)) & (_selItemsInClass.Count != 0))
                        {
                            selItems = _selItemsInClass;
                        }
                        else
                        {
                            blRepullConv = true;
                        }
                    }
                    else
                    {
                        blRepullConv = true;
                    }

                    if (blRepullConv)
                    {
                        // Set selItems = New Collection
                        // Set Sel = New Collection
                        // Sel.Add Mail
                        // Set selItems = Email_SortToExistingFolder.DemoConversation(selItems, Sel)

                        conv = new cConversation(_globals.Ol.App) { item = Mail };
                        selItems = conv.get_ToList(true, true);
                    }
                }
                else
                {
                    selItems.Add(Mail);
                }
                Attchments = (FolderCbo.SelectedItem as string != "Trash to Delete") ? false : _chbxSaveAttach.Checked;

                blDoMove = true;
                if (_fldrOriginal.FolderPath != Mail.Parent.FolderPath)
                    blDoMove = false;

                if (blDoMove)
                {
                    LoadCTFANDSubjectsANDRecents.Load_CTF_AND_Subjects_AND_Recents();
                    SortItemsToExistingFolder.MASTER_SortEmailsToExistingFolder(selItems: selItems,
                                                                                Pictures_Checkbox: false,
                                                                                SortFolder: FolderCbo.SelectedItem as string,
                                                                                Save_MSG: _chbxSaveMail.Checked,
                                                                                Attchments: Attchments,
                                                                                Remove_Flow_File: _chbxDelFlow.Checked,
                                                                                OlArchiveRootPath: _globals.Ol.ArchiveRootPath);
                    SortItemsToExistingFolder.Cleanup_Files();
                } // blDoMove

            }

        }

        public void ctrlsRemove()
        {



            while (_colCtrls.Count > 0)
            {
                Frm.Controls.Remove((Control)_colCtrls[_colCtrls.Count]);
                _colCtrls.RemoveAt(_colCtrls.Count - 1);
            }

            _fldrHandler = null;

        }

        public void kill()
        {
            _mPassedControl = null;
            ConversationCb = null;
            FolderCbo = null;
            SearchTxt = null;
            Frm = null;
            CbKll = null;
            Mail = null;
            _fldrTarget = null;
            _lblTmp = null;
            // Set _suggestions = Nothing
            // Set _strFolders = Nothing
            _colCtrls = null;
            _fldrHandler = null;
        }

        internal string Sender
        {
            get
            {
                return _lblSender.Text;
            }
        }

        public int Position
        {
            get
            {
                return _intMyPosition;
            }
            set
            {
                _intMyPosition = value;
            }
        }

        #endregion

        #region Formatting and Sizing Controls

        private void EmailFormatting()
        {
            if (Mail.UnRead == true)
            {
                LblSubject.ForeColor = Color.DarkBlue;
                LblSubject.Font = new Font(LblSubject.Font, FontStyle.Bold);
                _lblSender.ForeColor = Color.DarkBlue;
                _lblSender.Font = new Font(_lblSender.Font, FontStyle.Bold);
            }
        }

        private void SetInitialControlSizePosition()
        {
            lblSubject_Width = LblSubject.Width;
            lblBody_Width = TxtBoxBody.Width;
            cbFlag_Left = FlagTaskCb.Left;
            lblAcT_Left = lblAcT.Left;

            lblTriage_Width = lblTriage.Width;
            lblTriage_Left = lblTriage.Left;
            lblActionable_Left = lblActionable.Left;
            lblActionable_Width = lblActionable.Width;

            cbDel_Left = CbDel.Left;
            cbKll_Left = CbKll.Left;
            lblAcX_Left = lblAcX.Left;
            lblAcR_Left = lblAcR.Left;

            lblSentOn_Left = lblSentOn.Left;                 // SentOn X% Left Position

            if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
            {
                lbl5_Left = _lbl5.Left;
                lblAcF_Left = lblAcF.Left;
                lblAcD_Left = lblAcD.Left;
                cbo_Left = FolderCbo.Left;
                cbo_Width = FolderCbo.Width;
                lblAcC_Left = lblAcC.Left;                       // Conversation accelerator X% Left position
                chk_Left = ConversationCb.Left;                             // Conversation checkbox X% Left Position
                chbxSaveAttach_Left = _chbxSaveAttach.Left;       // Checkbox Save Attachment X% Left Position
                chbxSaveMail_Left = _chbxSaveMail.Left;           // Checkbox Save Mail X% Left Position
                chbxDelFlow_Left = _chbxDelFlow.Left;             // Checkbox Delete Flow X% Left Position
                lblAcA_Left = lblAcA.Left;                       // A Accelerator X% Left Position
                lblAcW_Left = lblAcW.Left;                       // W Accelerator X% Left Position
                lblAcM_Left = lblAcM.Left;                       // M Accelerator X% Left Position
                txt_Left = SearchTxt.Left;
                txt_Width = SearchTxt.Width;
                lblConvCt_Left = lblConvCt.Left;                 // Conversation Count X% Left Position
            }

            lngBlock_Width = Frm.Width - chbxSaveAttach_Left; // Width of block of right justified controls
        }

        public void Accel_Toggle()
        {
            if (_lblMyPosition.Enabled == true)
            {
                if (_blAccelFocusToggle)
                {
                    if (blExpanded == true)
                        ExpandCtrls1();
                    Accel_FocusToggle();
                }
                _lblMyPosition.Enabled = false;
                _lblMyPosition.Visible = false;
                _lblMyPosition.SendToBack();
            }
            else
            {
                _lblMyPosition.Text = _intMyPosition.ToString();
                _lblMyPosition.Enabled = true;
                _lblMyPosition.Visible = true;
                _lblMyPosition.BackColor = Color.Blue;
                _lblMyPosition.BringToFront();
            }
        }

        public void Accel_FocusToggle()
        {
            Control ctlTmp;

            if (_blAccelFocusToggle)
            {
                _blAccelFocusToggle = false;
                foreach (Control currentCtlTmp in _colCtrls)
                {
                    ctlTmp = currentCtlTmp;
                    //switch (Information.TypeName(ctlTmp) ?? "")
                    switch (ctlTmp)
                    {
                        case Panel:
                            {
                                ctlTmp.BackColor = SystemColors.Control;
                                break;
                            }
                        case CheckBox:
                            {
                                ctlTmp.BackColor = SystemColors.Control;
                                break;
                            }
                        case Label:
                            {
                                if (ctlTmp.Text.Length <= 2)
                                {
                                    ctlTmp.Visible = false;
                                    ctlTmp.SendToBack();
                                }
                                else
                                {
                                    ctlTmp.BackColor = SystemColors.Control;
                                }

                                break;
                            }
                        case TextBox:
                            {
                                ctlTmp.BackColor = SystemColors.Control;
                                break;
                            }
                    }
                }
                if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
                {
                    lblConvCt.Visible = true;
                    lblConvCt.BackColor = SystemColors.Control;
                    lblConvCt.BringToFront();
                    lblTriage.Visible = true;
                    lblTriage.BackColor = SystemColors.Control;
                    lblTriage.BringToFront();
                }
                _lblMyPosition.Visible = true;
                _lblMyPosition.BackColor = Color.Blue;
                _lblMyPosition.BringToFront();
            }

            else
            {
                _blAccelFocusToggle = true;
                foreach (Control currentCtlTmp1 in _colCtrls)
                {
                    ctlTmp = currentCtlTmp1;
                    switch (ctlTmp)
                    {
                        case Panel:
                            {
                                ctlTmp.BackColor = Color.PaleTurquoise;
                                break;
                            }
                        case CheckBox:
                            {
                                ctlTmp.BackColor = Color.PaleTurquoise;
                                break;
                            }
                        case Label:
                            {
                                if (ctlTmp.Text.Length <= 2)
                                {
                                    ctlTmp.Visible = true;
                                    ctlTmp.BringToFront();
                                }
                                else
                                {
                                    ctlTmp.BackColor = Color.PaleTurquoise;
                                }

                                break;
                            }
                        case TextBox:
                            {
                                ctlTmp.BackColor = Color.PaleTurquoise;
                                break;
                            }
                    }
                }
                if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
                {
                    lblConvCt.BackColor = Color.PaleTurquoise;
                    lblTriage.BackColor = Color.PaleTurquoise;
                }
                _lblMyPosition.BackColor = Color.DarkGreen;
                // Modal        With _activeExplorer
                // Modal            .ClearSelection
                // Modal            If .IsItemSelectableInView(mail) Then .AddToSelection mail
                // Modal            'DoEvents
                // Modal        End With
            }
        }

        public void Mail_Activate()
        {
            if (_activeExplorer.CurrentFolder.DefaultItemType != OlItemType.olMailItem)
            {
                _activeExplorer.NavigationPane.CurrentModule = _activeExplorer
                    .NavigationPane.Modules.GetNavigationModule(OlNavigationModuleType.olModuleMail);
            }
            if (_activeExplorer.CurrentView.Name != "tmpNoConversation")
            {
                _activeExplorer.CurrentView = "tmpNoConversation";
            }
            _activeExplorer.ClearSelection();
            try
            {
                if (_activeExplorer.IsItemSelectableInView(Mail))
                    _activeExplorer.AddToSelection(Mail);
            }
            catch (System.Exception e) { MessageBox.Show("Error", "Error in QF.Mail_Activate: " + e.Message); }            
        }

        public void ResizeCtrls(int intPxChg)
        {
            double X1pct;
            double X2pct;
            double X3pct;
            int X1px;
            int X2px;
            int X3px;
            int lngTmp;

            X1pct = 0.6d;
            X3pct = X1pct / 2d;
            X2pct = 1d - X1pct;

            X1pct *= intPxChg;
            X2pct *= intPxChg;
            X3pct *= intPxChg;
            X1px = (int)Math.Round(Math.Round(X1pct, 0));
            X2px = (int)Math.Round(Math.Round(X2pct, 0));
            X3px = (int)Math.Round(Math.Round(X3pct, 0));

            LblSubject.Width = (int)(lblSubject_Width + X1px);                      // Subject Width X%
            FlagTaskCb.Left = (int)(cbFlag_Left + X1px + X2px);                         // Task button X% + Y% left position
            lblAcT.Left = (int)(lblAcT_Left + X1px + X2px);                         // Task accelerator X% + Y% left position
            CbDel.Left = (int)(cbDel_Left + X1px + X2px);                           // Delete button X+Y% Left position
            CbKll.Left = (int)(cbKll_Left + X1px + X2px);                           // Kill button X+Y% Left position
            lblAcX.Left = (int)(lblAcX_Left + X1px + X2px);
            lblAcR.Left = (int)(lblAcR_Left + X1px + X2px);
            lblSentOn.Left = (int)(lblSentOn_Left + X1px);                          // SentOn X% Left Position
            lblActionable.Left = (int)(lblActionable_Left + X3px);                  // <ACTIONABL> left position + X3px
            lblTriage.Left = (int)(lblTriage_Left + X3px);                          // Triage left position + X3px


            if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
            {
                SearchTxt.Left = (int)(txt_Left + X1px);                                  // Folder search box X% left position Y% Width
                SearchTxt.Width = (int)(txt_Width + X2px);                                // Folder search box X% left position Y% Width
                _lbl5.Left = (int)(lbl5_Left + X1px);                                // Folder label X% left position
                lblAcF.Left = (int)(lblAcF_Left + X1px);                            // F Accelerator X% left position
                lblConvCt.Left = (int)(lblConvCt_Left + X1px);                      // Conversation Count X% Left Position
                _chbxSaveAttach.Left = (int)(chbxSaveAttach_Left + X1px + X2px);     // Checkbox Save Attachment X% Left Position
                _chbxSaveMail.Left = (int)(chbxSaveMail_Left + X1px + X2px);         // Checkbox Save Mail X% Left Position
                _chbxDelFlow.Left = (int)(chbxDelFlow_Left + X1px + X2px);           // Checkbox Delete Flow X% Left Position
                lblAcA.Left = (int)(lblAcA_Left + X1px + X2px);                     // A Accelerator X% Left Position
                lblAcW.Left = (int)(lblAcW_Left + X1px + X2px);                     // W Accelerator X% Left Position
                lblAcM.Left = (int)(lblAcM_Left + X1px + X2px);                     // M Accelerator X% Left Position

                if (blExpanded)
                {

                    FolderCbo.Width = (int)(Frm.Width - FolderCbo.Left - lngBlock_Width - 5L);
                    pos_cbo.leftOriginal = cbo_Left + X1px;                   // Dropdown box X% Left position Y% Width
                    pos_cbo.widthOriginal = cbo_Width + X2px;                 // Dropdown box X% Left position Y% Width
                    pos_lblAcD.leftOriginal = lblAcD_Left + X1px;             // D Accelerator X% left position
                    pos_lblAcC.leftOriginal = lblAcC_Left + X1px;             // Conversation accelerator X% Left position
                    pos_chk.leftOriginal = chk_Left + X1px;                   // Conversation checkbox X% Left Position
                    lngTmp = ConversationCb.Left;
                    ConversationCb.Left = lblConvCt.Left - 10;
                    lblAcC.Left = (int)(lblAcC.Left + ConversationCb.Left - lngTmp);
                    TxtBoxBody.Width = Frm.Width - TxtBoxBody.Left - 5;
                    pos_body.widthOriginal = lblBody_Width + X1px;            // Body Width X%
                }

                else
                {

                    FolderCbo.Left = (int)(cbo_Left + X1px);                               // Dropdown box X% Left position Y% Width
                    FolderCbo.Width = (int)(cbo_Width + X2px);                             // Dropdown box X% Left position Y% Width
                    lblAcD.Left = (int)(lblAcD_Left + X1px);                         // D Accelerator X% left position
                    lblAcC.Left = (int)(lblAcC_Left + X1px + X2px);                  // Conversation accelerator X% Left position
                    ConversationCb.Left = (int)(chk_Left + X1px + X2px);                        // Conversation checkbox X% Left Position
                    TxtBoxBody.Width = (int)(lblBody_Width + X1px);

                }                     // Body Width X%
            }

            else
            {
                TxtBoxBody.Width = (int)(lblBody_Width + X1px + X2px);
            }                   // Body Width X%

        }

        public virtual void ExpandCtrls1()
        {

            int lngShift;
            // Private pos_lblAcC          As ctrlPosition
            // Private pos_lblAcD          As ctrlPosition
            // Private pos_lblAcO          As ctrlPosition

            if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
            {
                if (blExpanded == false)
                {
                    blExpanded = true;
                    Frm.Height = Frm.Height * 2;
                    lngShift = LblSubject.Top + LblSubject.Height - FolderCbo.Top + 1;

                    pos_cbo.topOriginal = FolderCbo.Top;
                    pos_cbo.topNew = pos_cbo.topOriginal + lngShift;
                    FolderCbo.Top = (int)pos_cbo.topNew;

                    pos_lblAcD.topOriginal = lblAcD.Top;
                    lblAcD.Top = (int)(pos_lblAcD.topOriginal + lngShift);

                    pos_cbo.leftOriginal = FolderCbo.Left;
                    FolderCbo.Left = TxtBoxBody.Left;

                    pos_lblAcD.leftOriginal = lblAcD.Left;
                    lblAcD.Left = Math.Max(0, FolderCbo.Left - pos_cbo.leftOriginal + pos_lblAcD.leftOriginal);

                    pos_cbo.widthOriginal = FolderCbo.Width;
                    pos_cbo.widthNew = pos_cbo.leftOriginal - FolderCbo.Left + pos_cbo.widthOriginal - lngBlock_Width;
                    FolderCbo.Width = (int)pos_cbo.widthNew;

                    lngShift = FolderCbo.Top + FolderCbo.Height - TxtBoxBody.Top + 1;

                    
                        
                    pos_body.topOriginal = TxtBoxBody.Top;
                    pos_body.topNew = pos_body.topOriginal + lngShift;
                    TxtBoxBody.Top = pos_body.topNew;

                    pos_lblAcO.topOriginal = lblAcO.Top;
                    lblAcO.Top = (lblAcO.Top + lngShift);

                    pos_body.heightOriginal = TxtBoxBody.Height;
                    pos_body.heightNew = Frm.Height - pos_body.topNew - 5;
                    TxtBoxBody.Height = pos_body.heightNew;
                    pos_body.widthOriginal = TxtBoxBody.Width;
                    pos_body.widthNew = Frm.Width - TxtBoxBody.Left - 5;
                    TxtBoxBody.Width = pos_body.widthNew;
                   

                    ConversationCb.Text = "";
                    pos_chk.leftOriginal = ConversationCb.Left;
                    ConversationCb.Left = lblConvCt.Left - 10;
                    pos_lblAcC.leftOriginal = lblAcC.Left;
                    lblAcC.Left = (ConversationCb.Left - pos_chk.leftOriginal + pos_lblAcC.leftOriginal);

                    pos_chk.topOriginal = ConversationCb.Top;
                    ConversationCb.Top = lblConvCt.Top;

                    pos_lblAcC.topOriginal = lblAcC.Top;
                    lblAcC.Top = lblConvCt.Top;

                    pos_chk.widthOriginal = ConversationCb.Width;
                    ConversationCb.Width = 10;


                    pos_chbxSaveAttach.topOriginal = _chbxSaveAttach.Top;
                    _chbxSaveAttach.Top = (int)pos_cbo.topNew;

                    pos_chbxSaveMail.topOriginal = _chbxSaveMail.Top;
                    _chbxSaveMail.Top = (int)pos_cbo.topNew;

                    pos_chbxDelFlow.topOriginal = _chbxDelFlow.Top;
                    _chbxDelFlow.Top = (int)pos_cbo.topNew;

                    pos_lblAcA.topOriginal = lblAcA.Top;
                    lblAcA.Top = (int)pos_cbo.topNew;

                    pos_lblAcW.topOriginal = lblAcW.Top;
                    lblAcW.Top = (int)pos_cbo.topNew;

                    pos_lblAcM.topOriginal = lblAcM.Top;
                    lblAcM.Top = (int)pos_cbo.topNew;
                }






                else
                {
                    blExpanded = false;
                    Frm.Height = (int)Math.Round(Frm.Height / 2d);

                    FolderCbo.Top = (int)pos_cbo.topOriginal;
                    FolderCbo.Left = (int)pos_cbo.leftOriginal;
                    FolderCbo.Width = (int)pos_cbo.widthOriginal;

                    lblAcD.Top = (int)pos_lblAcD.topOriginal;
                    lblAcD.Left = (int)pos_lblAcD.leftOriginal;

                    TxtBoxBody.Top = (int)pos_body.topOriginal;
                    TxtBoxBody.Height = (int)pos_body.heightOriginal;
                    TxtBoxBody.Width = (int)pos_body.widthOriginal;
                    lblAcO.Top = (int)pos_lblAcO.topOriginal;

                    ConversationCb.Text = "  Conversation";
                    ConversationCb.Left = (int)pos_chk.leftOriginal;
                    ConversationCb.Top = (int)pos_chk.topOriginal;
                    ConversationCb.Width = (int)pos_chk.widthOriginal;
                    lblAcC.Left = (int)pos_lblAcC.leftOriginal;
                    lblAcC.Top = (int)pos_lblAcC.topOriginal;

                    _chbxSaveAttach.Top = (int)pos_chbxSaveAttach.topOriginal;
                    _chbxSaveMail.Top = (int)pos_chbxSaveMail.topOriginal;
                    _chbxDelFlow.Top = (int)pos_chbxDelFlow.topOriginal;
                    lblAcA.Top = (int)pos_lblAcA.topOriginal;
                    lblAcW.Top = (int)pos_lblAcW.topOriginal;
                    lblAcM.Top = (int)pos_lblAcM.topOriginal;


                }
            }
            else if (blExpanded == false)
            {
                blExpanded = true;
                Frm.Height = Frm.Height * 2;
                {
                    pos_body.topOriginal = TxtBoxBody.Top;
                    pos_lblAcO.topOriginal = lblAcO.Top;
                    pos_body.heightOriginal = TxtBoxBody.Height;
                    pos_body.heightNew = Frm.Height - pos_body.topOriginal - 5;
                    TxtBoxBody.Height = (int)pos_body.heightNew;
                }
            }
            else
            {
                blExpanded = false;
                Frm.Height = (int)Math.Round(Frm.Height / 2d);
                {
                    TxtBoxBody.Top = (int)pos_body.topOriginal;
                    TxtBoxBody.Height = (int)pos_body.heightOriginal;
                    lblAcO.Top = (int)pos_lblAcO.topOriginal;
                }


            }

        }

        internal void ToggleRemoteMouseAppLabels()
        {
            p_BoolRemoteMouseApp = !p_BoolRemoteMouseApp;
            if (p_BoolRemoteMouseApp)
            {

                lblAcX.Text = "^-";       // ACCELERATOR X for Delete email
                lblAcX.Width *= 2;
                lblAcR.Text = "F3";       // ACCELERATOR R for remove item from list
                lblAcT.Text = "F2";       // ACCELERATOR T for Task ... Flag item and make it a task
                lblAcO.Text = "^0";       // ACCELERATOR O for Open Email
                lblAcO.Width *= 2;
                lblAcM.Width *= 2;
                if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
                {
                    lblAcF.Text = "F1";   // ACCELERATOR F for Folder Search
                    lblAcD.Text = "F4";   // ACCELERATOR D for Folder Dropdown
                    lblAcC.Text = "F7";   // ACCELERATOR C for Grouping Conversations
                    lblAcA.Text = "F8";   // ACCELERATOR A for Save Attachments
                    lblAcW.Text = "F9";   // ACCELERATOR W for Delete Flow
                    lblAcM.Text = "^=";   // ACCELERATOR M for Save Mail
                }
            }
            else
            {
                lblAcX.Text = "X";        // ACCELERATOR X for Delete email
                lblAcX.Width = (int)Math.Round(lblAcX.Width / 2d);
                lblAcR.Text = "R";        // ACCELERATOR R for remove item from list
                lblAcT.Text = "T";        // ACCELERATOR T for Task ... Flag item and make it a task
                lblAcO.Text = "O";        // ACCELERATOR O for Open Email
                lblAcO.Width = (int)Math.Round(lblAcO.Width / 2d);
                lblAcM.Width = (int)Math.Round(lblAcM.Width / 2d);
                if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
                {
                    lblAcF.Text = "F";   // ACCELERATOR F for Folder Search
                    lblAcD.Text = "D";   // ACCELERATOR D for Folder Dropdown
                    lblAcC.Text = "C";   // ACCELERATOR C for Grouping Conversations
                    lblAcA.Text = "A";   // ACCELERATOR A for Save Attachments
                    lblAcW.Text = "W";   // ACCELERATOR W for Delete Flow
                    lblAcM.Text = "M";   // ACCELERATOR M for Save Mail
                }
            }
        }

        #endregion

        #region Event Handlers

        private void WireEventHandlers()
        {
            __cbDel.Click += cbDel_Click;
            __cbDel.KeyDown += cbDel_KeyDown;
            __cbDel.KeyPress += cbDel_KeyPress;
            __cbDel.KeyUp += cbDel_KeyUp;
            _flagTaskCb.Click += cbFlag_Click;
            _flagTaskCb.KeyDown += cbFlag_KeyDown;
            _flagTaskCb.KeyPress += cbFlag_KeyPress;
            _flagTaskCb.KeyUp += cbFlag_KeyUp;
            __cbKll.Click += cbKll_Click;
            __cbKll.KeyDown += cbKll_KeyDown;
            __cbKll.KeyPress += cbKll_KeyPress;
            __cbKll.KeyUp += cbKll_KeyUp;
            _folderCbo.KeyDown += cbo_KeyDown;
            _folderCbo.KeyUp += cbo_KeyUp;
            _cbTmp.KeyDown += cbTmp_KeyDown;
            _cbTmp.KeyUp += cbTmp_KeyUp;
            _conversationCb.CheckedChanged += chk_Click;
            _conversationCb.KeyDown += chk_KeyDown;
            _conversationCb.KeyUp += chk_KeyUp;
            _frm.KeyDown += frm_KeyDown;
            _frm.KeyPress += frm_KeyPress;
            _frm.KeyUp += frm_KeyUp;
            _searchTxt.TextChanged += txt_Change;
            _searchTxt.KeyDown += txt_KeyDown;
            _searchTxt.KeyPress += txt_KeyPress;
            _searchTxt.KeyUp += txt_KeyUp;
        }

        public virtual void KeyboardHandler(string AccelCode)
        {
            switch (AccelCode ?? "")
            {
                case "O":
                    {

                        LblSubject.ForeColor = Color.FromArgb(int.MinValue + 0x00000012);
                        _lblSender.ForeColor = Color.FromArgb(int.MinValue + 0x00000012);
                        LblSubject.Font = new Font(LblSubject.Font, FontStyle.Regular);
                        _lblSender.Font = new Font(_lblSender.Font, FontStyle.Regular);
                        break;
                    }


                case "C":
                    {
                        if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
                            ConversationCb.Checked = !ConversationCb.Checked;
                        break;
                    }
                case "A":
                    {
                        if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
                            _chbxSaveAttach.Checked = !_chbxSaveAttach.Checked;
                        break;
                    }
                case "W":
                    {
                        if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
                            _chbxDelFlow.Checked = !_chbxDelFlow.Checked;
                        break;
                    }
                case "M":
                    {
                        if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
                            _chbxSaveMail.Checked = !_chbxSaveMail.Checked;
                        break;
                    }
                case "T":
                    {
                        FlagAsTask();
                        break;
                    }
                case "F":
                    {
                        if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
                            SearchTxt.Focus();
                        break;
                    }
                case "D":
                    {
                        if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
                            FolderCbo.Focus();
                        break;
                    }
                case "X":
                    {
                        FolderCbo.SelectedItem = "Trash to Delete";
                        break;
                    }
                case "R":
                    {
                        _callbacks.RemoveSpecificControlGroup(Position);
                        break;
                    }
            }
        }

        private void bdy_Click(object sender, EventArgs e)
        {
            LblSubject.ForeColor = Color.FromArgb(int.MinValue + 0x00000012);
            LblSubject.Font = new Font(LblSubject.Font, FontStyle.Regular);
            _lblSender.ForeColor = Color.FromArgb(int.MinValue + 0x00000012);
            _lblSender.Font = new Font(_lblSender.Font, FontStyle.Regular);
            Mail.Display();
            _callbacks.QFD_Minimize();
            if (_callbacks.BlShowInConversations)
                _callbacks.ExplConvView_ToggleOn();
        }

        private void cbDel_Click(object sender, EventArgs e)
        {
            FolderCbo.SelectedItem = "Trash to Delete";
        }

        private void cbDel_KeyDown(object sender, KeyEventArgs e)
        {
            _callbacks.KeyboardHandler_KeyDown(sender, e);
        }

        private void cbDel_KeyPress(object sender, KeyPressEventArgs e)
        {
            KeyPressHandler_Class(sender, e);
        }

        private void cbDel_KeyUp(object sender, KeyEventArgs e)
        {
            // Select Case KeyCode
            // Case 18
            // _callbacks.toggleAcceleratorDialogue
            _callbacks.KeyboardHandler_KeyUp(sender, e);
            // Case Else
            // End Select
        }

        private void cbFlag_Click(object sender, EventArgs e)
        {
            FlagAsTask();
        }

        private void FlagAsTask()
        {
            List<MailItem> Sel = new() { Mail };
            var flagTask = new FlagTasks(AppGlobals: _globals, ItemList: Sel, blFile: false, hWndCaller: hWndCaller);
            flagTask.Run();
            FlagTaskCb.Text = "!";
        }

        private void cbFlag_KeyDown(object sender, KeyEventArgs e)
        {
            _callbacks.KeyboardHandler_KeyDown(sender, e);
        }

        private void cbFlag_KeyPress(object sender, KeyPressEventArgs e)
        {
            KeyPressHandler_Class(sender, e);
        }

        private void cbFlag_KeyUp(object sender, KeyEventArgs e)
        {
            // Select Case KeyCode
            // Case 18
            // _callbacks.toggleAcceleratorDialogue
            _callbacks.KeyboardHandler_KeyUp(sender, e);
            // Case Else
            // End Select
        }

        private void cbKll_Click(object sender, EventArgs e)
        {
            _callbacks.RemoveSpecificControlGroup(Position);
        }

        private void cbKll_KeyDown(object sender, KeyEventArgs e)
        {
            _callbacks.KeyboardHandler_KeyDown(sender, e);
        }

        private void cbKll_KeyPress(object sender, KeyPressEventArgs e)
        {
            KeyPressHandler_Class(sender, e);
        }

        private void cbKll_KeyUp(object sender, KeyEventArgs e)
        {
            // Select Case KeyCode
            // Case 18
            // _callbacks.toggleAcceleratorDialogue
            _callbacks.KeyboardHandler_KeyUp(sender, e);
            // Case Else
            // End Select
        }

        private void cbo_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Return:
                    {
                        if (_intEnterCounter == 1)
                        {
                            _intEnterCounter = 0;
                            _callbacks.KeyboardHandler_KeyDown(sender, e);
                        }
                        else
                        {
                            _intEnterCounter = 1;
                            _intComboRightCtr = 0;
                        }

                        break;
                    }

                default:
                    {
                        _callbacks.KeyboardHandler_KeyDown(sender, e);
                        break;
                    }
            }
        }

        private void cbo_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Alt:
                    {
                        _callbacks.KeyboardHandler_KeyUp(sender, e);
                        break;
                    }
                case Keys.Escape:
                    {
                        _intEnterCounter = 0;
                        _intComboRightCtr = 0;
                        break;
                    }
                case Keys.Right:
                    {
                        _intEnterCounter = 0;
                        if (_intComboRightCtr == 0)
                        {
                            FolderCbo.DroppedDown = true;
                            _intComboRightCtr = 1;
                        }
                        else if (_intComboRightCtr == 1)
                        {

                            SortItemsToExistingFolder.InitializeSortToExisting(InitType: "Sort", 
                                                                               QuickLoad: false, 
                                                                               WholeConversation: false, 
                                                                               strSeed: FolderCbo.SelectedItem as string, 
                                                                               objItem: Mail);
                            _callbacks.RemoveSpecificControlGroup(Position);
                        }
                        else
                        {
                            MessageBox.Show("Error","Error in intComboRightCtr ... setting to 0 and continuing");
                            _intComboRightCtr = 0;
                        }

                        break;
                    }
                case Keys.Left:
                    {
                        _intEnterCounter = 0;
                        _intComboRightCtr = 0;
                        break;
                    }
                case Keys.Down:
                    {
                        _intEnterCounter = 0;
                        break;
                    }
                case Keys.Up:
                    {
                        _intEnterCounter = 0;
                        break;
                    }
            }
        }

        private void cbTmp_KeyDown(object sender, KeyEventArgs e)
        {
            _callbacks.KeyboardHandler_KeyDown(sender, e);
        }

        private void cbTmp_KeyUp(object sender, KeyEventArgs e)
        {
            _callbacks.KeyboardHandler_KeyUp(sender, e);
        }

        private void chk_Click(object sender, EventArgs e)
        {

            List<MailItem> selItems = new();
            object objItem;
            MailItem objMail;
            int i;
            string[] varList;

            if (_selItemsInClass is null)
                CountMailsInConv();

            var loopTo = _selItemsInClass.Count;
            for (i = 1; i <= loopTo; i++)
            {
                objItem = _selItemsInClass[i];
                objMail = (MailItem)objItem;
                if ((objMail.EntryID ?? "") != (Mail.EntryID ?? ""))
                    selItems.Add(objMail);
            }


            if (ConversationCb.Checked == true)
            {
                _callbacks.ConvToggle_Group(selItems, _intMyPosition);
                lblConvCt.Enabled = true;
            }
            else
            {
                varList = FolderCbo.Items.Cast<object>().Select(item => item.ToString()).ToArray();
                _callbacks.ConvToggle_UnGroup(selItems, _intMyPosition, int.Parse(lblConvCt.Text), varList);
                lblConvCt.Enabled = false;
            }



        }

        private void chk_KeyDown(object sender, KeyEventArgs e)
        {
            _callbacks.KeyboardHandler_KeyDown(sender, e);
        }

        private void chk_KeyUp(object sender, KeyEventArgs e)
        {
            // Select Case KeyCode
            // Case 18
            // _callbacks.toggleAcceleratorDialogue
            _callbacks.KeyboardHandler_KeyUp(sender, e);
            // Case Else
            // End Select
        }

        private void frm_KeyDown(object sender, KeyEventArgs e)
        {
            _callbacks.KeyboardHandler_KeyDown(sender, e);
        }

        private void frm_KeyPress(object sender, KeyPressEventArgs e)
        {
            _callbacks.KeyboardHandler_KeyPress(sender, e);
        }

        private void frm_KeyUp(object sender, KeyEventArgs e)
        {
            // Select Case KeyCode
            // Case 18
            // _callbacks.toggleAcceleratorDialogue
            _callbacks.KeyboardHandler_KeyUp(sender, e);
            // Case Else
            // End Select
        }

        private void lst_KeyDown(object sender, KeyEventArgs e)
        {
            _callbacks.KeyboardHandler_KeyDown(sender, e);
        }

        private void lst_KeyUp(object sender, KeyEventArgs e)
        {
            // Select Case KeyCode
            // Case 18
            // _callbacks.toggleAcceleratorDialogue
            _callbacks.KeyboardHandler_KeyUp(sender, e);
            // Case Else
            // End Select
        }

        private void txt_Change(object sender, EventArgs e)
        {

            FolderCbo.Items.Clear();
            FolderCbo.Items.AddRange(_fldrHandler.FindFolder("*" + SearchTxt.Text + "*", true, ReCalcSuggestions: false, objItem: Mail));

            if (FolderCbo.Items.Count >= 2)
                FolderCbo.SelectedIndex = 1;

        }

        private void KeyPressHandler_Class(object sender, KeyPressEventArgs e)
        {

        }

        private void txt_KeyDown(object sender, KeyEventArgs e)
        {
            // Select Case KeyCode
            // Case 18
            _callbacks.KeyboardHandler_KeyDown(sender, e);
            // Case Else
            // End Select
        }

        private void txt_KeyPress(object sender, KeyPressEventArgs e)
        {
            _callbacks.KeyboardHandler_KeyPress(sender, e);
        }

        private void txt_KeyUp(object sender, KeyEventArgs e)
        {
            // Select Case KeyCode
            // Case 18
            // _callbacks.toggleAcceleratorDialogue
            _callbacks.KeyboardHandler_KeyUp(sender, e);
            // Case Else
            // End Select
        }

        #endregion
    }
}