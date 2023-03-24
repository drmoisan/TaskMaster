
namespace QuickFiler
{
    internal static class QuickFileControllerConstants
    {

        #region API Constants
        internal const long GWL_STYLE = -16; // Sets a new window style  As LongPtr in 64bit version
        internal const long WS_SYSMENU = 0x80000L; // Windows style   As LongPtr in 64bit version
        internal const int WS_THICKFRAME = 0x40000; // Style that is apparently resizble
        internal const long WS_MINIMIZEBOX = 0x20000L;   // As LongPtr in 64bit version
        internal const long WS_MAXIMIZEBOX = 0x10000L;   // As LongPtr in 64bit version
        internal const int SW_SHOWMAXIMIZED = 3;
        internal const int SW_FORCEMINIMIZE = 11;
        internal const int WM_SETFOCUS = 0x7;
        internal const int GWL_HWNDPARENT = -8;
        internal const string olAppCLSN = "rctrl_renwnd32";
        internal const int GA_PARENT = 1;
        internal const int GA_ROOTOWNER = 3;
        internal const int Modal = 0;
        internal const int Modeless = 1;
        #endregion

        #region Form and Control Constants
        internal const long Top_Offset = 16L;
        internal const long Top_Offset_C = 0L;
        internal const long Left_frm = 12L;
        internal const long Left_lbl1 = 6L;
        internal const long Left_lbl2 = 6L;
        internal const long Left_lbl3 = 6L;
        internal const long Left_lbl4 = 6L;
        internal const long Left_lbl5 = 372L;           // Folder:
        internal const long Left_lblSender = 66L;            // <SENDER>
        internal const long Left_lblSender_C = 6L;             // <SENDER> Compact view
        internal const long Right_Aligned = 648L;
        internal const long Left_lblTriage = 181L;           // X Triage placeholder
        internal const long Left_lblActionable = 198L;           // <ACTIONABL>
        internal const long Left_lblSubject = 66L;            // <SUBJECT>
        internal const long Left_lblSubject_C = 6L;             // <SUBJECT> Compact view
        internal const long Left_lblBody = 66L;            // <BODY>
        internal const long Left_lblBody_C = 6L;             // <BODY> Compact view
        internal const long Left_lblSentOn = 66L;            // <SENTON>
        internal const long Left_lblSentOn_C = 200L;           // <SENTON> Compact view
        internal const long Left_lblConvCt = 290L;           // Count of Conversation Members
        internal const long Left_lblConvCt_C = 320L;           // Count of Conversation Members Compact view
        internal const long Left_lblPos = 6L;             // ACCELERATOR Email Position
        internal const long Left_cbxFolder = 372L;           // Combo box containing Folder Suggestions
        internal const long Left_inpt = 438L;           // Input for folder search 408 to 438
        internal const long Left_chbxGPConv = 210L;           // Checkbox to Group Conversations
        internal const long Left_chbxGPConv_C = 372L;           // Checkbox to Group Conversations
        internal const long Left_cbDelItem = 588L;           // Delete email
        internal const long Left_cbKllItem = 618L;           // Remove mail from Processing
        internal const long Left_cbFlagItem = 569L;           // Flag as Task
        internal const long Left_lblAcF = 363L;           // ACCELERATOR F for Folder Search
        internal const long Left_lblAcD = 363L;           // ACCELERATOR D for Folder Dropdown
        internal const long Left_lblAcC = 384L;           // ACCELERATOR C for Grouping Conversations
        internal const long Left_lblAcC_C = 548L;           // ACCELERATOR C for Grouping Conversations
        internal const long Left_lblAcX = 594L;           // ACCELERATOR X for Delete email
        internal const long Left_lblAcR = 624L;           // ACCELERATOR R for remove item from list
        internal const long Left_lblAcT = 330L;           // ACCELERATOR T for Task ... Flag item and make it a task
        internal const long Left_lblAcO = 50L;            // ACCELERATOR O for Open Email
        internal const long Left_lblAcO_C = 0L;            // ACCELERATOR O for Open Email
        internal const long Width_frm = 655L;
        internal const long Width_lbl1 = 54L;
        internal const long Width_lbl2 = 54L;
        internal const long Width_lbl3 = 54L;
        internal const long Width_lbl4 = 52L;
        internal const long Width_lbl5 = 78L;            // Folder:
        internal const long Width_lblSender = 138L;           // <SENDER>
        internal const long Width_lblSender_C = 174L;           // <SENDER> Compact view
        internal const long Width_lblTriage = 11L;            // X Triage placeholder
        internal const long Width_lblActionable = 72L;            // <ACTIONABL>
        internal const long Width_lblSubject = 294L;           // <SUBJECT>
        internal const long Width_lblSubject_C = 354L;           // <SUBJECT> Compact view
        internal const long Width_lblBody = 294L;           // <BODY>
        internal const long Width_lblBody_C = 354L;           // <BODY> Compact view
        internal const long Width_lblSentOn = 80L;            // <SENTON>
        internal const long Width_lblConvCt = 30L;            // Count of Conversation Members
        internal const long Width_lblPos = 20L;            // ACCELERATOR Email Position
        internal const long Width_cbxFolder = 276L;           // Combo box containing Folder Suggestions 
        internal const long Width_inpt = 126L;           // Input for folder search 156 to 126
        internal const long Width_chbxGPConv = 96L;            // Checkbox to Group Conversations
        internal const long Width_cb = 25L;            // Command buttons for: Delete email, Remove mail from Processing, and Flag as Task
        internal const long Width_lblAc = 14L;            // ACCELERATOR Width
        internal const long Width_lblAcF = 14L;            // ACCELERATOR F for Folder Search
        internal const long Width_lblAcD = 14L;            // ACCELERATOR D for Folder Dropdown
        internal const long Width_lblAcC = 14L;            // ACCELERATOR C for Grouping Conversations
        internal const long Width_lblAcX = 14L;            // ACCELERATOR X for Delete email
        internal const long Width_lblAcR = 14L;            // ACCELERATOR R for remove item from list
        internal const long Width_lblAcT = 14L;            // ACCELERATOR T for Task ... Flag item and make it a task
        internal const long Width_lblAcO = 14L;            // ACCELERATOR O for Open Email
        internal const long Height_UserForm = 149L;          // Minimum height of Userform
        internal const long Width_UserForm = 700L;        // Minimum width of Userform
        internal const long Width_PanelMain = 683L;           // Minimum width of _viewer.L1v1L2_PanelMain
                                                              // Frame Design Constants
        internal const int frmHt = 96;
        internal const int frmWd = 655;
        internal const int frmLt = 12;
        internal const int frmSp = 6;
        internal const long OK_left = 216L;
        internal const long CANCEL_left = 354L;
        internal const long OK_width = 120L;
        internal const long UNDO_left = 480L;
        internal const long UNDO_width = 42L;
        internal const long spn_left = 606L;
        #endregion

    }
}