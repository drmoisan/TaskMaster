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

        private static int _rightAligned = 648;
        internal static int RightAligned { get => (int)Math.Round(Multiplier * _rightAligned, 0); }

        internal static int ScaledInt(int value)
        {
            return (int)Math.Round(value * Multiplier, 0);
        }

        //private static int frmHt = 96;
        //private static int frmWd = 655;
        //private static int frmLt = 12;

        internal static ConstantGroup Panel { get => new ConstantGroup(width: 655, height: 96, left: 12); }
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
                var cg = new ConstantGroup();
                cg.Height = ScaledInt(16);
                cg.Left = ScaledInt(6);
                cg.Top = TopOffset + ScaledInt(16);
                cg.Width = ScaledInt(354);
                return cg;
            }
        }
        internal static ConstantGroup TxtBody
        {
            get
            {
                var cg = new ConstantGroup();
                cg.Height = ScaledInt(56) - TopOffset;
                cg.Left = ScaledInt(6);
                cg.Top = TopOffset + ScaledInt(40);
                cg.Width = ScaledInt(354);
                return cg;
            }
        }
        internal static ConstantGroup LblSentOn { get => new ConstantGroup(width: 156, height: 16, left: 200, top: _topOffsetC); }  
        internal static ConstantGroup ComboFolder 
        {
            get 
            { 
                var cg = new ConstantGroup(width: 276, height: 24, left: 372);
                cg.Top = TopOffset + ScaledInt(27);
                return cg;
            }
        }
        internal static ConstantGroup Inpt { get => new ConstantGroup(width: 126, height: 24, left: 438, top: _topOffsetC); }
        internal static ConstantGroup CheckboxSaveMail
        {
            get
            {
                var cg = new ConstantGroup();
                cg.Height = ScaledInt(16);
                cg.Width = ScaledInt(40); 
                cg.Left = RightAligned - cg.Width;
                cg.Top = TopOffset + ScaledInt(47);

                return cg;
            }
        }
        internal static ConstantGroup CheckboxDelFlow
        {
            get
            {
                var cg = new ConstantGroup();
                cg.Height = ScaledInt(16);
                cg.Width = ScaledInt(45);
                cg.Top = TopOffset + ScaledInt(47);

                return cg;
            }
        }
        internal static ConstantGroup CheckboxSaveAttachment
        {
            get
            {
                var cg = new ConstantGroup();
                cg.Height = ScaledInt(16);
                cg.Width = ScaledInt(50);
                cg.Top = TopOffset + ScaledInt(47);
                return cg;
            }
        }
        internal static ConstantGroup CheckboxGroupConversations
        {
            get
            {
                var cg = new ConstantGroup();
                cg.Height = ScaledInt(16);
                cg.Width = ScaledInt(90);
                cg.Top = TopOffset + ScaledInt(47);
                return cg;
            }
        }
        internal static ConstantGroup LblConversationCt 
        {
            get 
            { 
                var cg = new ConstantGroup(width: 36, height: 24, left: 320);
                cg.Top = TopOffset + ScaledInt(16);
                return cg;
            }
        }
        internal static ConstantGroup LblPos { get => new ConstantGroup(width: 20, height: 20, left: 0, top: _topOffsetC); }
        //internal static ConstantGroup LblAcF { get => new ConstantGroup(width: , height: , left: , top: _topOffsetC); }
        internal static ConstantGroup LblAcF { get => new ConstantGroup(width: 14, height: 14, left: 363, top: (int)Math.Max(_topOffsetC - 2,0)); }
        internal static ConstantGroup LblAcD { get => new ConstantGroup(width: 14, height: 14, left: 363, top: _topOffsetC + 20); }
        internal static ConstantGroup LblAcC
        {
            get
            {
                var cg = new ConstantGroup(width: 14, height: 14);
                cg.Top = TopOffset + ScaledInt(47);
                return cg;
            }
        }
        internal static ConstantGroup LblAcR
        {
            get
            {
                var cg = new ConstantGroup(width: 14, height: 14);
                cg.Top = TopOffset + ScaledInt(2);
                return cg;
            }
        }
        internal static ConstantGroup LblAcX
        {
            get
            {
                var cg = new ConstantGroup(width: 14, height: 14);
                cg.Top = TopOffset + ScaledInt(2);
                return cg;
            }
        }
        internal static ConstantGroup LblAcO 
        {
            get 
            { 
                var cg = new ConstantGroup(width: 14, height: 14, left: 0);
                cg.Top = TxtBody.Top;
                return cg; 
            }
        }
        internal static ConstantGroup LblAcA
        {
            get
            {
                var cg = new ConstantGroup(width: 14, height: 14);
                cg.Top = CheckboxSaveAttachment.Top;
                cg.Left = CheckboxSaveAttachment.Left + ScaledInt(10);
                return cg;
            }
        }
        internal static ConstantGroup LblAcW
        {
            get
            {
                var cg = new ConstantGroup(width: 14, height: 14);
                cg.Top = CheckboxDelFlow.Top;
                cg.Left = CheckboxDelFlow.Left + ScaledInt(29);
                return cg;
            }
        }
        internal static ConstantGroup LblAcM
        {
            get
            {
                var cg = new ConstantGroup(width: 14, height: 14);
                cg.Top = CheckboxSaveMail.Top;
                cg.Left = CheckboxSaveMail.Left + ScaledInt(10);
                return cg;
            }
        }



        #region left
        private static int left_frm = 12;        
        private static int left_lblPos = 6;             // ACCELERATOR Email Position
        //private static int left_chbxGPConv = 210;           // Checkbox to Group Conversations
        //private static int left_chbxGPConv_C = 372;           // Checkbox to Group Conversations
        private static int left_cbDelItem = 588;           // Delete email
        private static int left_cbKllItem = 618;           // Remove _mail from Processing
        private static int left_cbFlagItem = 569;           // Flag as Task
        #endregion
        #region width
        private static int width_frm = 655;
        //private static int width_lbl2 = 54;
        //private static int width_lbl3 = 54;
        private static int width_lblPos = 20;            // ACCELERATOR Email Position
        private static int width_chbxSaveMail = 37;
        private static int width_chbxGPConv = 96;            // Checkbox to Group Conversations
        private static int width_cb = 25;            // Command buttons for: Delete email, Remove _mail from Processing, and Flag as Task
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
        internal static int Left_lblPos { get => (int)Math.Round(Multiplier * left_lblPos, 0); }
        //internal static int Left_chbxGPConv { get => (int)Math.Round(Multiplier * left_chbxGPConv, 0); }
        //internal static int Left_chbxGPConv_C { get => (int)Math.Round(Multiplier * left_chbxGPConv_C, 0); }
        internal static int Left_cbDelItem { get => (int)Math.Round(Multiplier * left_cbDelItem, 0); }
        internal static int Left_cbKllItem { get => (int)Math.Round(Multiplier * left_cbKllItem, 0); }
        internal static int Left_cbFlagItem { get => (int)Math.Round(Multiplier * left_cbFlagItem, 0); }
        internal static int Width_frm { get => (int)Math.Round(Multiplier * width_frm, 0); }
        internal static int Width_lblPos { get => (int)Math.Round(Multiplier * width_lblPos, 0); }
        internal static int Width_chbxSaveMail { get => (int)Math.Round(Multiplier * width_chbxSaveMail, 0); }
        internal static int Width_chbxGPConv { get => (int)Math.Round(Multiplier * width_chbxGPConv, 0); }
        internal static int Width_cb { get => (int)Math.Round(Multiplier * width_cb, 0); }
        internal static int Height_UserForm { get => (int)Math.Round(Multiplier * height_UserForm, 0); }
        internal static int Width_UserForm { get => (int)Math.Round(Multiplier * width_UserForm, 0); }
        internal static int Width_PanelMain { get => (int)Math.Round(Multiplier * width_PanelMain, 0); }
        //internal static int FrmHt { get => (int)Math.Round(Multiplier* frmHt, 0); }
        //internal static int FrmWd { get => (int) Math.Round(Multiplier* frmWd, 0); }
        //internal static int FrmLt { get => (int) Math.Round(Multiplier* frmLt, 0); }
        internal static int FrmSp { get => (int) Math.Round(Multiplier* frmSp, 0); }
        internal static int OK_left { get => (int)Math.Round(Multiplier * oK_left, 0); }
        internal static int CANCEL_left { get => (int)Math.Round(Multiplier * cANCEL_left, 0); }
        internal static int OK_width { get => (int)Math.Round(Multiplier * oK_width, 0); }
        internal static int UNDO_left { get => (int)Math.Round(Multiplier * uNDO_left, 0); }
        internal static int UNDO_width { get => (int)Math.Round(Multiplier * uNDO_width, 0); }
        internal static int Spn_left { get => (int)Math.Round(Multiplier * spn_left, 0); }
        #endregion

    }
}