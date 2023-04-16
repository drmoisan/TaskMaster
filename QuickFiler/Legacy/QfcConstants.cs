using System;
using UtilitiesCS;
using Windows.Win32;

namespace QuickFiler
{
    internal static class QfcConstants
    {

        public class ConstantGroup
        {
            public ConstantGroup() { }
            public ConstantGroup(int width, int height, int left) 
            {
                _width = width;
                _height = height;
                _left = left;
            }

            public ConstantGroup(int width, int height, int left, int top)
            {
                _width = width;
                _height = height;
                _left = left;
                _top = top;
            }
            public ConstantGroup(int width, int height)
            {
                _width = width;
                _height = height;
            }

            private int _width;
            private int _height;
            private int _top;
            private int _left;
            internal static double Multiplier { get => (double)(PInvoke.ScreenHeight / (double)1080); }

            public int Width {
                get => (int)Math.Round(Multiplier * _width, 0);
                set => _width = (int)Math.Round(value/Multiplier,0); 
            }
            public int Height {
                get => (int)Math.Round(Multiplier * _height, 0);
                set => _height = (int)Math.Round(value / Multiplier, 0);
            }
            public int Top {
                get => (int)Math.Round(Multiplier * _top, 0);
                set => _top = (int)Math.Round(value / Multiplier, 0);
            }
            public int Left {
                get => (int)Math.Round(Multiplier * _left, 0);
                set => _left = (int)Math.Round(value / Multiplier, 0);
            }
        }
        
        internal static double Multiplier { get => (double)(PInvoke.ScreenHeight / (double)1080); }
        private static bool _wideView = false;
        internal static bool WideView { get => _wideView; set => _wideView = value; }

        private static int _topOffset = 16;
        private static int _topOffsetC = 1;
        internal static int TopOffset
        {
            get
            {
                if (WideView)
                { 
                    return (int)Math.Round(_topOffset * Multiplier, 0); 
                }
                else 
                { 
                    return (int)Math.Round(_topOffsetC * Multiplier, 0); 
                }
            }
        }
        
        internal static int ScaledInt(int value)
        {
            return (int)Math.Round(value * Multiplier, 0);
        }
        
        internal static ConstantGroup Lbl1 { get => new ConstantGroup(width: 54, height: 16, left: 6, top: _topOffsetC); }

        internal static ConstantGroup Lbl2
        {
            get
            {
                var cg = new ConstantGroup(width: 54, height: 16, left: 6);
                cg.Top = TopOffset + ScaledInt(32);
                return cg;
            }
        }

        internal static ConstantGroup Lbl3
        {
            get
            {
                var cg = new ConstantGroup(width: 54, height: 16, left: 6);
                cg.Top = TopOffset + ScaledInt(48);
                return cg;
            }
        }

        internal static ConstantGroup Lbl5 { get => new ConstantGroup(width: 60, height: 16, left: 372, top: _topOffsetC); }
        internal static ConstantGroup LblSender { get => new ConstantGroup(width: 174, height: 16, left: 6, top: _topOffsetC); }
        internal static ConstantGroup LblTriage { get => new ConstantGroup(width: 11, height: 16, left: 181, top: _topOffsetC); }
        internal static ConstantGroup LblActionable { get => new ConstantGroup(width: 72, height: 16, left: 198, top: _topOffsetC); }

        internal static ConstantGroup LblSubject
        {
            get
            {
                var cg = new ConstantGroup(width: 54, height: 16, left: 6);
                cg.Top = TopOffset + ScaledInt(32);
                return cg;
            }
        }

        private static int left_lblSubject = 66;            // <SUBJECT>
        private static int left_lblSubject_C = 6;             // <SUBJECT> Compact view
        private static int width_lblSubject = 294;           // <SUBJECT>
        private static int width_lblSubject_C = 354;           // <SUBJECT> Compact view


        #region left
        private static int left_frm = 12;
        //private static int left_lbl2 = 6;
        //private static int left_lbl3 = 6;
        private static int right_Aligned = 648;
        private static int left_lblBody = 66;            // <BODY>
        private static int left_lblBody_C = 6;             // <BODY> Compact view
        private static int left_lblSentOn = 66;            // <SENTON>
        private static int left_lblSentOn_C = 200;           // <SENTON> Compact view
        private static int left_lblConvCt = 290;           // Count of Conversation Members
        private static int left_lblConvCt_C = 320;           // Count of Conversation Members Compact view
        private static int left_lblPos = 6;             // ACCELERATOR Email Position
        private static int left_cbxFolder = 372;           // Combo box containing Folder Suggestions
        private static int left_inpt = 438;           // Input for folder search 408 to 438
        private static int left_chbxGPConv = 210;           // Checkbox to Group Conversations
        private static int left_chbxGPConv_C = 372;           // Checkbox to Group Conversations
        private static int left_cbDelItem = 588;           // Delete email
        private static int left_cbKllItem = 618;           // Remove _mail from Processing
        private static int left_cbFlagItem = 569;           // Flag as Task
        private static int left_lblAcF = 363;           // ACCELERATOR F for Folder Search
        private static int left_lblAcD = 363;           // ACCELERATOR D for Folder Dropdown
        private static int left_lblAcC = 384;           // ACCELERATOR C for Grouping Conversations
        private static int left_lblAcC_C = 548;           // ACCELERATOR C for Grouping Conversations
        private static int left_lblAcX = 594;           // ACCELERATOR X for Delete email
        private static int left_lblAcR = 624;           // ACCELERATOR R for remove item from list
        private static int left_lblAcT = 330;           // ACCELERATOR T for Task ... Flag item and make it a task
        private static int left_lblAcO = 50;            // ACCELERATOR O for Open Email
        private static int left_lblAcO_C = 0;            // ACCELERATOR O for Open Email
        #endregion
        #region width
        private static int width_frm = 655;
        //private static int width_lbl2 = 54;
        //private static int width_lbl3 = 54;
        private static int width_lblBody = 294;           // <BODY>
        private static int width_lblBody_C = 354;           // <BODY> Compact view
        private static int width_lblSentOn = 156;            // <SENTON>
        private static int width_lblConvCt = 30;            // Count of Conversation Members
        private static int width_lblPos = 20;            // ACCELERATOR Email Position
        private static int width_cbxFolder = 276;           // Combo box containing Folder Suggestions 
        private static int width_chbxSaveMail = 37;
        private static int width_inpt = 126;           // Input for folder search 156 to 126
        private static int width_chbxGPConv = 96;            // Checkbox to Group Conversations
        private static int width_cb = 25;            // Command buttons for: Delete email, Remove _mail from Processing, and Flag as Task
        private static int width_lblAc = 14;            // ACCELERATOR Width
        private static int width_lblAcF = 14;            // ACCELERATOR F for Folder Search
        private static int width_lblAcD = 14;            // ACCELERATOR D for Folder Dropdown
        private static int width_lblAcC = 14;            // ACCELERATOR C for Grouping Conversations
        private static int width_lblAcX = 14;            // ACCELERATOR X for Delete email
        private static int width_lblAcR = 14;            // ACCELERATOR R for remove item from list
        private static int width_lblAcT = 14;            // ACCELERATOR T for Task ... Flag item and make it a task
        private static int width_lblAcO = 14;            // ACCELERATOR O for Open Email
        private static int width_UserForm = 700;        // Minimum _width of Userform
        private static int width_PanelMain = 683;           // Minimum _width of _viewer.L1v1L2_PanelMain

        #endregion
        #region height

        private static int height_UserForm = 149;          // Minimum _height of Userform
        private static int _lbl_height16 = 16;
        private static int _lbl_height24 = 24;
        private static int _space16 = 16;
        private static int _space32 = 32;
        private static int _space36 = 36;
        private static int _space40 = 40;
        private static int _space48 = 48;
        private static int _space56 = 56;
        #endregion
        #region frame
        // Frame Design Constants
        private static int frmHt = 96;
        private static int frmWd = 655;
        private static int frmLt = 12;
        private static int frmSp = 6;
        private static int oK_left = 216;
        private static int cANCEL_left = 354;
        private static int oK_width = 120;
        private static int uNDO_left = 480;
        private static int uNDO_width = 42;
        private static int spn_left = 606;
        #endregion

        #region fields
        internal static int Top_Offset { get => (int)Math.Round(Multiplier * _topOffset, 0); }
        internal static int Lbl_height16 { get => (int)Math.Round(Multiplier * _lbl_height16, 0); }
        internal static int Lbl_height24 { get => (int)Math.Round(Multiplier * _lbl_height24, 0); }
        internal static int Space16 { get => (int)Math.Round(Multiplier * _space16, 0); }
        internal static int Space32 { get => (int)Math.Round(Multiplier * _space32, 0); }
        internal static int Space36 { get => (int)Math.Round(Multiplier * _space36, 0); }
        internal static int Space40 { get => (int)Math.Round(Multiplier * _space40, 0); }
        internal static int Space48 { get => (int)Math.Round(Multiplier * _space48, 0); }
        internal static int Space56 { get => (int)Math.Round(Multiplier * _space56, 0); }

        internal static int Top_Offset_C { get => (int)Math.Round(Multiplier * _topOffsetC, 0); }
        internal static int Left_frm { get => (int)Math.Round(Multiplier * left_frm, 0); }
        //internal static int Left_lbl3 { get => (int)Math.Round(Multiplier * left_lbl3, 0); }
        internal static int Right_Aligned { get => (int)Math.Round(Multiplier * right_Aligned, 0); }
        internal static int Left_lblSubject { get => (int)Math.Round(Multiplier * left_lblSubject, 0); }
        internal static int Left_lblSubject_C { get => (int)Math.Round(Multiplier * left_lblSubject_C, 0); }
        internal static int Left_lblBody { get => (int)Math.Round(Multiplier * left_lblBody, 0); }
        internal static int Left_lblBody_C { get => (int)Math.Round(Multiplier * left_lblBody_C, 0); }
        internal static int Left_lblSentOn { get => (int)Math.Round(Multiplier * left_lblSentOn, 0); }
        internal static int Left_lblSentOn_C { get => (int)Math.Round(Multiplier * left_lblSentOn_C, 0); }
        internal static int Left_lblConvCt { get => (int)Math.Round(Multiplier * left_lblConvCt, 0); }
        internal static int Left_lblConvCt_C { get => (int)Math.Round(Multiplier * left_lblConvCt_C, 0); }
        internal static int Left_lblPos { get => (int)Math.Round(Multiplier * left_lblPos, 0); }
        internal static int Left_cbxFolder { get => (int)Math.Round(Multiplier * left_cbxFolder, 0); }
        internal static int Left_inpt { get => (int)Math.Round(Multiplier * left_inpt, 0); }
        internal static int Left_chbxGPConv { get => (int)Math.Round(Multiplier * left_chbxGPConv, 0); }
        internal static int Left_chbxGPConv_C { get => (int)Math.Round(Multiplier * left_chbxGPConv_C, 0); }
        internal static int Left_cbDelItem { get => (int)Math.Round(Multiplier * left_cbDelItem, 0); }
        internal static int Left_cbKllItem { get => (int)Math.Round(Multiplier * left_cbKllItem, 0); }
        internal static int Left_cbFlagItem { get => (int)Math.Round(Multiplier * left_cbFlagItem, 0); }
        internal static int Left_lblAcF { get => (int)Math.Round(Multiplier * left_lblAcF, 0); }
        internal static int Left_lblAcD { get => (int)Math.Round(Multiplier * left_lblAcD, 0); }
        internal static int Left_lblAcC { get => (int)Math.Round(Multiplier * left_lblAcC, 0); }
        internal static int Left_lblAcC_C { get => (int)Math.Round(Multiplier * left_lblAcC_C, 0); }
        internal static int Left_lblAcX { get => (int)Math.Round(Multiplier * left_lblAcX, 0); }
        internal static int Left_lblAcR { get => (int)Math.Round(Multiplier * left_lblAcR, 0); }
        internal static int Left_lblAcT { get => (int)Math.Round(Multiplier * left_lblAcT, 0); }
        internal static int Left_lblAcO { get => (int)Math.Round(Multiplier * left_lblAcO, 0); }
        internal static int Left_lblAcO_C { get => (int)Math.Round(Multiplier * left_lblAcO_C, 0); }
        internal static int Width_frm { get => (int)Math.Round(Multiplier * width_frm, 0); }

        //internal static int Width_lbl3 { get => (int)Math.Round(Multiplier * width_lbl3, 0); }

        internal static int Width_lblSubject { get => (int)Math.Round(Multiplier * width_lblSubject, 0); }
        internal static int Width_lblSubject_C { get => (int)Math.Round(Multiplier * width_lblSubject_C, 0); }
        internal static int Width_lblBody { get => (int)Math.Round(Multiplier * width_lblBody, 0); }
        internal static int Width_lblBody_C { get => (int)Math.Round(Multiplier * width_lblBody_C, 0); }
        internal static int Width_lblSentOn { get => (int)Math.Round(Multiplier * width_lblSentOn, 0); }
        internal static int Width_lblConvCt { get => (int)Math.Round(Multiplier * width_lblConvCt, 0); }
        internal static int Width_lblPos { get => (int)Math.Round(Multiplier * width_lblPos, 0); }
        internal static int Width_cbxFolder { get => (int)Math.Round(Multiplier * width_cbxFolder, 0); }
        internal static int Width_chbxSaveMail { get => (int)Math.Round(Multiplier * width_chbxSaveMail, 0); }
        internal static int Width_inpt { get => (int)Math.Round(Multiplier * width_inpt, 0); }
        internal static int Width_chbxGPConv { get => (int)Math.Round(Multiplier * width_chbxGPConv, 0); }
        internal static int Width_cb { get => (int)Math.Round(Multiplier * width_cb, 0); }
        internal static int Width_lblAc { get => (int)Math.Round(Multiplier * width_lblAc, 0); }
        internal static int Width_lblAcF { get => (int)Math.Round(Multiplier * width_lblAcF, 0); }
        internal static int Width_lblAcD { get => (int)Math.Round(Multiplier * width_lblAcD, 0); }
        internal static int Width_lblAcC { get => (int)Math.Round(Multiplier * width_lblAcC, 0); }
        internal static int Width_lblAcX { get => (int)Math.Round(Multiplier * width_lblAcX, 0); }
        internal static int Width_lblAcR { get => (int)Math.Round(Multiplier * width_lblAcR, 0); }
        internal static int Width_lblAcT { get => (int)Math.Round(Multiplier * width_lblAcT, 0); }
        internal static int Width_lblAcO { get => (int)Math.Round(Multiplier * width_lblAcO, 0); }
        internal static int Height_UserForm { get => (int)Math.Round(Multiplier * height_UserForm, 0); }
        internal static int Width_UserForm { get => (int)Math.Round(Multiplier * width_UserForm, 0); }
        internal static int Width_PanelMain { get => (int)Math.Round(Multiplier * width_PanelMain, 0); }
        internal static int FrmHt { get => (int)Math.Round(Multiplier* frmHt, 0); }
        internal static int FrmWd { get => (int) Math.Round(Multiplier* frmWd, 0); }
        internal static int FrmLt { get => (int) Math.Round(Multiplier* frmLt, 0); }
        internal static int FrmSp { get => (int) Math.Round(Multiplier* frmSp, 0); }
        internal static int OK_left { get => (int)Math.Round(Multiplier * oK_left, 0); }
        internal static int CANCEL_left { get => (int)Math.Round(Multiplier * cANCEL_left, 0); }
        internal static int OK_width { get => (int)Math.Round(Multiplier * oK_width, 0); }
        internal static int UNDO_left { get => (int)Math.Round(Multiplier * uNDO_left, 0); }
        internal static int UNDO_width { get => (int)Math.Round(Multiplier * uNDO_width, 0); }
        internal static int Spn_left { get => (int)Math.Round(Multiplier * spn_left, 0); }
        #endregion

        //private static int left_lbl1 = 6;
        //private static int width_lbl1 = 54;
        //internal static int Left_lbl1 { get => (int)Math.Round(Multiplier * left_lbl1, 0); }
        //internal static int Left_lbl2 { get => (int)Math.Round(Multiplier * left_lbl2, 0); }
        //internal static int Width_lbl1 { get => (int)Math.Round(Multiplier * width_lbl1, 0); }
        //internal static int Width_lbl2 { get => (int)Math.Round(Multiplier * width_lbl2, 0); }
    }
}