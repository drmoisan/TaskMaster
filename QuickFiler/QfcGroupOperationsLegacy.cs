using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using ToDoModel;
using UtilitiesVB;

namespace QuickFiler
{

    /// <summary>
/// Class manages UI interactions with the collection of Qfc controllers and viewers
/// </summary>
    internal class QfcGroupOperationsLegacy
    {
        private readonly QuickFileViewer _viewer;
        private readonly Enums.InitTypeEnum _initType;
        private IApplicationGlobals _globals;
        private Collection _colQFClass;
        private Collection _colFrames;
        private int _intUniqueItemCounter;
        private int _intActiveSelection;
        private bool _boolRemoteMouseApp = false;
        private IntPtr _lFormHandle;
        private bool _suppressAcceleratorEvents = false;
        private QuickFileController _parent;

        public QfcGroupOperationsLegacy(QuickFileViewer viewerInstance, Enums.InitTypeEnum InitType, IApplicationGlobals AppGlobals, QuickFileController ParentObject)
        {

            _viewer = viewerInstance;
            _initType = InitType;
            _globals = AppGlobals;
            _parent = ParentObject;
        }

        #region Viewer Operations

        internal void LoadControlsAndHandlers(Collection colEmails)
        {
            MailItem Mail;
            QfcController QF;
            Collection colCtrls;
            bool blDebug;

            blDebug = false;

            _colQFClass = new Collection();
            _colFrames = new Collection();

            _intUniqueItemCounter = 0;

            foreach (var objItem in colEmails)
            {
                if (objItem is MailItem)
                {
                    _intUniqueItemCounter += 1;
                    Mail = (MailItem)objItem;
                    colCtrls = new Collection();
                    LoadGroupOfCtrls(ref colCtrls, _intUniqueItemCounter);

                    QF = new QfcController(Mail, colCtrls, _intUniqueItemCounter, _boolRemoteMouseApp, Caller: this, AppGlobals: _globals, hwnd: _lFormHandle, InitTypeE: _initType);
                    _colQFClass.Add(QF);
                }
            }

            _viewer.WindowState = FormWindowState.Maximized;
            // ShowWindow(_lFormHandle, SW_SHOWMAXIMIZED)

            if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
            {
                // ToggleOffline
                foreach (QfcController currentQF in _colQFClass)
                {
                    QF = currentQF;
                    QF.Init_FolderSuggestions();
                    QF.CountMailsInConv();
                    // DoEvents
                }
                // ToggleOffline
            }

            _intActiveSelection = 0;

            _parent.FormResize(true);
            _viewer.L1v1L2_PanelMain.Focus();
        }

        internal void LoadGroupOfCtrls(ref Collection colCtrls, int intItemNumber, int intPosition = 0, bool blGroupConversation = true, bool blWideView = false)
        {

            long lngTopOff;
            bool blDebug = false;

            lngTopOff = blWideView ? QuickFileControllerConstants.Top_Offset : QuickFileControllerConstants.Top_Offset_C;
            if (intPosition == 0)
                intPosition = intItemNumber;

            if (intItemNumber * (QuickFileControllerConstants.frmHt + QuickFileControllerConstants.frmSp) + QuickFileControllerConstants.frmSp > _viewer.L1v1L2_PanelMain.Height)      // Was _heightPanelMainMax but I replaced with Me.Height
            {
                _viewer.L1v1L2_PanelMain.AutoScroll = true;

            }

            // Min Me Size is frmSp * 2 + frmHt
            var Frm = new Panel();
            _viewer.L1v1L2_PanelMain.Controls.Add(Frm);
            Frm.Height = QuickFileControllerConstants.frmHt;
            Frm.Top = (QuickFileControllerConstants.frmSp + QuickFileControllerConstants.frmHt) * (intPosition - 1) + QuickFileControllerConstants.frmSp + 16;
            Frm.Left = QuickFileControllerConstants.frmLt;
            Frm.Width = QuickFileControllerConstants.frmWd;
            Frm.TabStop = false;

            Frm.BorderStyle = BorderStyle.FixedSingle;
            colCtrls.Add(Frm, "frm");

            if (blWideView)
            {
                var lbl1 = new Label();
                Frm.Controls.Add(lbl1);
                lbl1.Height = 16;
                lbl1.Top = (int)lngTopOff;
                lbl1.Left = 6;
                lbl1.Width = 54;
                lbl1.Text = "From:";
                lbl1.Font = new Font(lbl1.Font.FontFamily, 10f, FontStyle.Bold);
                colCtrls.Add(lbl1, "lbl1");
            }  // blWideView

            if (blWideView)
            {
                var lbl2 = new Label();
                Frm.Controls.Add(lbl2);
                lbl2.Height = 16;
                lbl2.Top = (int)(lngTopOff + 32L);
                lbl2.Left = 6;
                lbl2.Width = 54;
                lbl2.Text = "Subject:";
                lbl2.Font = new Font(lbl2.Font.FontFamily, 10f, FontStyle.Bold);
                colCtrls.Add(lbl2, "lbl2");
            }  // blWideView

            if (blWideView)
            {
                var lbl3 = new Label();
                Frm.Controls.Add(lbl3);
                lbl3.Height = 16;
                lbl3.Top = (int)(lngTopOff + 48L);
                lbl3.Left = 6;
                lbl3.Width = 54;
                lbl3.Text = "Body:";
                lbl3.Font = new Font(lbl3.Font.FontFamily, 10f, FontStyle.Bold);
                colCtrls.Add(lbl3, "lbl3");
            }

            if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
            {
                // TURN OFF IF CONDITIONAL REMINDER
                var lbl5 = new Label();
                Frm.Controls.Add(lbl5);

                lbl5.Height = 16;
                lbl5.Top = (int)lngTopOff;
                lbl5.Left = 372;
                lbl5.Width = 60;
                lbl5.Text = "Folder:";
                lbl5.Font = new Font(lbl5.Font.FontFamily, 10f, FontStyle.Bold);
                colCtrls.Add(lbl5, "lbl5");
            }

            var lblSender = new Label();
            Frm.Controls.Add(lblSender);

            lblSender.Height = 16;
            lblSender.Top = (int)lngTopOff;

            if (blWideView)
            {
                lblSender.Left = (int)QuickFileControllerConstants.Left_lblSender;
                lblSender.Width = (int)QuickFileControllerConstants.Width_lblSender;
            }
            else
            {
                lblSender.Left = (int)QuickFileControllerConstants.Left_lblSender_C;
                lblSender.Width = (int)QuickFileControllerConstants.Width_lblSender_C;
            }  // blWideView


            lblSender.Text = "<SENDER>";
            lblSender.Font = new Font(lblSender.Font.FontFamily, 10f);
            colCtrls.Add(lblSender, "lblSender");

            var lblTriage = new Label();
            Frm.Controls.Add(lblTriage);

            lblTriage.Height = 16;
            lblTriage.Top = (int)lngTopOff;

            if (blWideView)
            {
                lblTriage.Left = (int)QuickFileControllerConstants.Left_lblSender;
                lblTriage.Width = (int)QuickFileControllerConstants.Width_lblSender;
            }
            else
            {
                lblTriage.Left = (int)QuickFileControllerConstants.Left_lblTriage;
                lblTriage.Width = (int)QuickFileControllerConstants.Width_lblTriage;
            }  // blWideView

            lblTriage.Text = "ABC";
            lblTriage.Font = new Font(lblTriage.Font.FontFamily, 10f);
            colCtrls.Add(lblTriage, "lblTriage");

            var lblActionable = new Label();
            Frm.Controls.Add(lblActionable);

            lblActionable.Height = 16;
            lblActionable.Top = (int)lngTopOff;

            if (blWideView)
            {
                lblActionable.Left = (int)QuickFileControllerConstants.Left_lblSender;
                lblActionable.Width = (int)QuickFileControllerConstants.Width_lblSender;
            }
            else
            {
                lblActionable.Left = (int)QuickFileControllerConstants.Left_lblActionable;
                lblActionable.Width = (int)QuickFileControllerConstants.Width_lblActionable;
            }


            lblActionable.Text = "<ACTIONABL>";
            lblActionable.Font = new Font(lblActionable.Font.FontFamily, 10f);
            colCtrls.Add(lblActionable, "lblActionable");

            var lblSubject = new Label();
            Frm.Controls.Add(lblSubject);

            if (blWideView)
            {
                lblSubject.Height = 16;
                lblSubject.Top = (int)(lngTopOff + 32L);
                lblSubject.Left = (int)QuickFileControllerConstants.Left_lblSubject;
                lblSubject.Width = (int)QuickFileControllerConstants.Width_lblSubject;
                lblSubject.Font = new Font(lblSubject.Font.FontFamily, 10f);
            }
            else if (_initType.HasFlag(Enums.InitTypeEnum.InitConditionalReminder))
            {
                lblSubject.Height = 24;
                lblSubject.Top = (int)(lngTopOff + 16L);
                lblSubject.Left = (int)QuickFileControllerConstants.Left_lblSubject_C;
                lblSubject.Width = QuickFileControllerConstants.frmWd - lblSubject.Left - lblSubject.Left;
                lblSubject.Font = new Font(lblSubject.Font.FontFamily, 16f);
            }
            else
            {
                lblSubject.Height = 24;
                lblSubject.Top = (int)(lngTopOff + 16L);
                lblSubject.Left = (int)QuickFileControllerConstants.Left_lblSubject_C;
                lblSubject.Width = (int)QuickFileControllerConstants.Width_lblSubject_C;
                lblSubject.Font = new Font(lblSubject.Font.FontFamily, 16f);
            }

            lblSubject.Text = "<SUBJECT>";
            colCtrls.Add(lblSubject, "lblSubject");

            var txtboxBody = new TextBox();
            Frm.Controls.Add(txtboxBody);

            if (blWideView)
            {
                txtboxBody.Top = (int)(lngTopOff + 36L);
                txtboxBody.Left = (int)QuickFileControllerConstants.Left_lblBody;
                txtboxBody.Width = (int)QuickFileControllerConstants.Width_lblBody;
                txtboxBody.Height = (int)(40 + 8 - lngTopOff);
            }
            else if (_initType.HasFlag(Enums.InitTypeEnum.InitConditionalReminder))
            {
                txtboxBody.Top = (int)(lngTopOff + 40L);
                txtboxBody.Left = (int)QuickFileControllerConstants.Left_lblBody_C;
                txtboxBody.Width = QuickFileControllerConstants.frmWd - txtboxBody.Left - txtboxBody.Left;
                txtboxBody.Height = (int)(48 + 8 - lngTopOff);
            }
            else
            {
                txtboxBody.Top = (int)(lngTopOff + 40L);
                txtboxBody.Left = (int)QuickFileControllerConstants.Left_lblBody_C;
                txtboxBody.Width = (int)QuickFileControllerConstants.Width_lblBody_C;
                txtboxBody.Height = (int)(48 + 8 - lngTopOff);

            }

            txtboxBody.Text = "<BODY>";
            txtboxBody.Font = new Font(txtboxBody.Font.FontFamily, 10f);
            txtboxBody.WordWrap = true;
            txtboxBody.Multiline = true;
            txtboxBody.ReadOnly = true;
            txtboxBody.BorderStyle = BorderStyle.None;
            colCtrls.Add(txtboxBody, "lblBody");

            var lblSentOn = new Label();
            Frm.Controls.Add(lblSentOn);
            lblSentOn.Height = 16;
            if (blWideView)
            {
                lblSentOn.Top = (int)(lngTopOff + 16L);
                lblSentOn.Left = (int)QuickFileControllerConstants.Left_lblSentOn;
                lblSentOn.TextAlign = ContentAlignment.TopLeft; // fmTextAlignLeft
            }
            else
            {
                lblSentOn.Top = (int)lngTopOff;
                lblSentOn.Left = (int)QuickFileControllerConstants.Left_lblSentOn_C;
                lblSentOn.TextAlign = ContentAlignment.TopRight;
            } // fmTextAlignRight

            lblSentOn.Width = 156;
            lblSentOn.Text = "<SENTON>";
            lblSentOn.Font = new Font(lblSentOn.Font.FontFamily, 10f);
            colCtrls.Add(lblSentOn, "lblSentOn");

            if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
            {
                var cbxFolder = new ComboBox();
                Frm.Controls.Add(cbxFolder);
                cbxFolder.Height = 24;
                cbxFolder.Top = (int)(27L + lngTopOff);
                cbxFolder.Left = (int)QuickFileControllerConstants.Left_cbxFolder;
                cbxFolder.Width = (int)QuickFileControllerConstants.Width_cbxFolder;
                cbxFolder.Font = new Font(cbxFolder.Font.FontFamily, 8f);
                cbxFolder.TabStop = false;
                cbxFolder.DropDownStyle = ComboBoxStyle.DropDownList;
                colCtrls.Add(cbxFolder, "cbxFolder");
            }

            var chbxGPConv = new CheckBox();
            var chbxSaveAttach = new CheckBox();
            var chbxDelFlow = new CheckBox();
            var chbxSaveMail = new CheckBox();
            var inpt = new TextBox();
            if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
            {
                Frm.Controls.Add(inpt);
                inpt.Height = 24;
                inpt.Top = (int)lngTopOff;
                inpt.Left = (int)QuickFileControllerConstants.Left_inpt;
                inpt.Width = (int)QuickFileControllerConstants.Width_inpt;
                inpt.Font = new Font(inpt.Font.FontFamily, 10f);
                inpt.TabStop = false;

                inpt.BackColor = SystemColors.Control;
                colCtrls.Add(inpt, "inpt");

                Frm.Controls.Add(chbxSaveMail);

                chbxSaveMail.Height = 16;
                chbxSaveMail.Width = 37;
                chbxSaveMail.Font = new Font(chbxSaveMail.Font.FontFamily, 10f);
                chbxSaveMail.Text = " Mail";
                chbxSaveMail.Checked = false;
                chbxSaveMail.TabStop = false;
                if (blWideView)
                {
                }

                else
                {
                    chbxSaveMail.Top = (int)(47L + lngTopOff);
                    chbxSaveMail.Left = (int)(QuickFileControllerConstants.Right_Aligned - chbxSaveMail.Width);
                }
                colCtrls.Add(chbxSaveMail, "chbxSaveMail");

                Frm.Controls.Add(chbxDelFlow);

                chbxDelFlow.Height = 16;
                chbxDelFlow.Width = 45;
                chbxDelFlow.Font = new Font(chbxDelFlow.Font.FontFamily, 10f);
                chbxDelFlow.Text = " Flow";
                chbxDelFlow.Checked = false;
                chbxDelFlow.TabStop = false;

                if (blWideView)
                {
                }

                else
                {
                    chbxDelFlow.Top = (int)(47L + lngTopOff);
                    chbxDelFlow.Left = chbxSaveMail.Left - chbxDelFlow.Width - 1;

                }
                colCtrls.Add(chbxDelFlow, "chbxDelFlow");

                Frm.Controls.Add(chbxSaveAttach);

                chbxSaveAttach.Height = 16;
                chbxSaveAttach.Width = 50;
                chbxSaveAttach.Font = new Font(chbxSaveAttach.Font.FontFamily, 10f);
                chbxSaveAttach.Text = " Attach";
                chbxSaveAttach.Checked = true;
                chbxSaveAttach.TabStop = false;

                if (blWideView)
                {
                }

                else
                {
                    chbxSaveAttach.Top = (int)(47L + lngTopOff);
                    chbxSaveAttach.Left = chbxDelFlow.Left - chbxSaveAttach.Width - 1;

                }
                colCtrls.Add(chbxSaveAttach, "chbxSaveAttach");

                Frm.Controls.Add(chbxGPConv);
                chbxGPConv.Height = 16;
                chbxGPConv.Width = 81;
                chbxGPConv.Font = new Font(chbxGPConv.Font.FontFamily, 10f);
                chbxGPConv.Text = "  Conversation";
                chbxGPConv.Checked = blGroupConversation;
                chbxGPConv.TabStop = false;
                if (blWideView)
                {
                    chbxGPConv.Top = (int)lngTopOff;
                    chbxGPConv.Left = (int)QuickFileControllerConstants.Left_chbxGPConv;
                }
                else
                {
                    chbxGPConv.Top = (int)(47L + lngTopOff);
                    chbxGPConv.Left = chbxSaveAttach.Left - chbxGPConv.Width - 1;
                }
                colCtrls.Add(chbxGPConv, "chbxGPConv");
            }

            var cbFlagItem = new Button();
            Frm.Controls.Add(cbFlagItem);
            cbFlagItem.Height = 24;
            cbFlagItem.Top = (int)lngTopOff;
            cbFlagItem.Left = (int)QuickFileControllerConstants.Left_cbFlagItem;
            cbFlagItem.Width = (int)QuickFileControllerConstants.Width_cb;
            cbFlagItem.Font = new Font(cbFlagItem.Font.FontFamily, 8f);
            cbFlagItem.Text = "|>";
            cbFlagItem.BackColor = SystemColors.Control;
            cbFlagItem.ForeColor = SystemColors.ControlText;
            cbFlagItem.TabStop = false;
            colCtrls.Add(cbFlagItem, "cbFlagItem");

            var cbKllItem = new Button();
            Frm.Controls.Add(cbKllItem);
            cbKllItem.Height = 24;
            cbKllItem.Top = (int)lngTopOff;
            cbKllItem.Left = (int)(cbFlagItem.Left + QuickFileControllerConstants.Width_cb + 2L);
            cbKllItem.Width = (int)QuickFileControllerConstants.Width_cb;
            cbKllItem.Font = new Font(cbKllItem.Font.FontFamily, 8f);
            cbKllItem.Text = "-->";
            cbKllItem.BackColor = SystemColors.Control;
            cbKllItem.ForeColor = SystemColors.ControlText;
            cbKllItem.TabStop = false;
            colCtrls.Add(cbKllItem, "cbKllItem");

            var cbDelItem = new Button();
            Frm.Controls.Add(cbDelItem);
            cbDelItem.Height = 24;
            cbDelItem.Top = (int)lngTopOff;
            cbDelItem.Left = (int)(cbKllItem.Left + QuickFileControllerConstants.Width_cb + 2L);
            cbDelItem.Width = (int)QuickFileControllerConstants.Width_cb;
            cbDelItem.Font = new Font(cbDelItem.Font.FontFamily, 8f);
            cbDelItem.Text = "X";
            cbDelItem.BackColor = Color.Red;
            cbDelItem.ForeColor = Color.White;
            cbDelItem.TabStop = false;
            colCtrls.Add(cbDelItem, "cbDelItem");

            if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
            {
                var lblConvCt = new Label();
                Frm.Controls.Add(lblConvCt);
                lblConvCt.Height = 24;
                lblConvCt.TextAlign = ContentAlignment.TopRight; // fmTextAlignRight

                if (blWideView)
                {
                    lblConvCt.Left = (int)QuickFileControllerConstants.Left_lblConvCt;
                    lblConvCt.Top = (int)lngTopOff;
                }
                else
                {
                    lblConvCt.Left = (int)QuickFileControllerConstants.Left_lblConvCt_C;
                    lblConvCt.Top = (int)(lngTopOff + 16L);
                }
                lblConvCt.Width = 36;
                lblConvCt.Text = "<#>";
                if (blWideView)
                {
                    lblConvCt.Font = new Font(lblConvCt.Font.FontFamily, 12f);
                }
                else
                {
                    lblConvCt.Font = new Font(lblConvCt.Font.FontFamily, 16f);
                }


                lblConvCt.Enabled = blGroupConversation;
                colCtrls.Add(lblConvCt, "lblConvCt");
            }

            var lblPos = new Label();
            Frm.Controls.Add(lblPos);
            lblPos.Height = 20;
            lblPos.Top = (int)lngTopOff;

            lblPos.Left = blWideView ? 6 : 0;

            lblPos.Width = 20;
            lblPos.Text = "<Pos#>";
            lblPos.Font = new Font(lblPos.Font.FontFamily, 10f, FontStyle.Bold);
            lblPos.BackColor = SystemColors.ControlText;
            lblPos.ForeColor = SystemColors.Control;
            lblPos.Enabled = false;
            lblPos.Visible = blDebug;
            colCtrls.Add(lblPos, "lblPos");

            if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
            {
                var lblAcF = new Label();
                Frm.Controls.Add(lblAcF);
                lblAcF.Height = 14;
                lblAcF.Top = (int)Math.Max(lngTopOff - 2L, 0L);
                lblAcF.Left = 363;
                lblAcF.Width = 14;
                lblAcF.Text = "F";
                lblAcF.Font = new Font(lblAcF.Font.FontFamily, 10f, FontStyle.Bold);
                lblAcF.BorderStyle = BorderStyle.Fixed3D; // fmBorderStyleSingle
                lblAcF.TextAlign = ContentAlignment.TopCenter;  // fmTextAlignCenter
                // .SpecialEffect = fmSpecialEffectBump
                lblAcF.BackColor = SystemColors.ControlText;
                lblAcF.ForeColor = SystemColors.Control;

                lblAcF.Visible = blDebug;
                colCtrls.Add(lblAcF, "lblAcF");

                var lblAcD = new Label();
                Frm.Controls.Add(lblAcD);
                lblAcD.Height = 14;
                lblAcD.Top = (int)(20L + lngTopOff);
                lblAcD.Left = 363;
                lblAcD.Width = 14;
                lblAcD.Text = "D";
                lblAcD.Font = new Font(lblAcD.Font.FontFamily, 10f, FontStyle.Bold);
                lblAcD.BorderStyle = BorderStyle.Fixed3D; // fmBorderStyleSingle
                lblAcD.TextAlign = ContentAlignment.TopCenter;  // fmTextAlignCenter
                // .SpecialEffect = fmSpecialEffectBump
                lblAcD.BackColor = SystemColors.ControlText;
                lblAcD.ForeColor = SystemColors.Control;
                lblAcD.Visible = blDebug;
                colCtrls.Add(lblAcD, "lblAcD");

                var lblAcC = new Label();
                Frm.Controls.Add(lblAcC);
                lblAcC.Height = 14;
                lblAcC.Top = (int)(lngTopOff + 47L);
                lblAcC.Left = chbxGPConv.Left + 12;
                lblAcC.Width = 14;
                lblAcC.Text = "C";
                lblAcC.Font = new Font(lblAcC.Font.FontFamily, 10f, FontStyle.Bold);
                lblAcC.BorderStyle = BorderStyle.Fixed3D; // fmBorderStyleSingle
                lblAcC.TextAlign = ContentAlignment.TopCenter;  // fmTextAlignCenter
                // .SpecialEffect = fmSpecialEffectBump
                lblAcC.BackColor = SystemColors.ControlText;
                lblAcC.ForeColor = SystemColors.Control;
                lblAcC.Visible = blDebug;
                colCtrls.Add(lblAcC, "lblAcC");
            }

            var lblAcR = new Label();
            Frm.Controls.Add(lblAcR);
            lblAcR.Height = 14;
            lblAcR.Top = (int)(2L + lngTopOff);
            lblAcR.Left = cbKllItem.Left + 6;
            lblAcR.Width = 14;
            lblAcR.Text = "R";
            lblAcR.Font = new Font(lblAcR.Font.FontFamily, 10f, FontStyle.Bold);
            lblAcR.BorderStyle = BorderStyle.Fixed3D; // fmBorderStyleSingle
            lblAcR.TextAlign = ContentAlignment.TopCenter;  // fmTextAlignCenter
            // .SpecialEffect = fmSpecialEffectBump
            lblAcR.BackColor = SystemColors.ControlText;
            lblAcR.ForeColor = SystemColors.Control;
            lblAcR.Visible = blDebug;
            colCtrls.Add(lblAcR, "lblAcR");

            var lblAcX = new Label();
            Frm.Controls.Add(lblAcX);
            lblAcX.Height = 14;
            lblAcX.Top = (int)(2L + lngTopOff);
            lblAcX.Left = cbDelItem.Left + 6;
            lblAcX.Width = 14;
            lblAcX.Text = "X";
            lblAcX.Font = new Font(lblAcX.Font.FontFamily, 10f, FontStyle.Bold);
            lblAcX.BorderStyle = BorderStyle.Fixed3D; // fmBorderStyleSingle
            lblAcX.TextAlign = ContentAlignment.TopCenter;  // fmTextAlignCenter
            // .SpecialEffect = fmSpecialEffectBump
            lblAcX.BackColor = SystemColors.ControlText;
            lblAcX.ForeColor = SystemColors.Control;
            lblAcX.Visible = blDebug;
            colCtrls.Add(lblAcX, "lblAcX");

            var lblAcT = new Label();
            Frm.Controls.Add(lblAcT);
            lblAcT.Height = 14;
            lblAcT.Top = (int)(2L + lngTopOff);
            lblAcT.Left = cbFlagItem.Left + 6;
            lblAcT.Width = 14;
            lblAcT.Text = "T";
            lblAcT.Font = new Font(lblAcT.Font.FontFamily, 10f, FontStyle.Bold);
            lblAcT.BorderStyle = BorderStyle.Fixed3D; // fmBorderStyleSingle
            lblAcT.TextAlign = ContentAlignment.TopCenter;  // fmTextAlignCenter
            // .SpecialEffect = fmSpecialEffectBump
            lblAcT.BackColor = SystemColors.ControlText;
            lblAcT.ForeColor = SystemColors.Control;
            lblAcT.Visible = blDebug;
            colCtrls.Add(lblAcT, "lblAcT");

            var lblAcO = new Label();
            Frm.Controls.Add(lblAcO);
            lblAcO.Height = 14;

            if (blWideView)
            {
                lblAcO.Top = (int)(36L + lngTopOff);
                lblAcO.Left = (int)QuickFileControllerConstants.Left_lblAcO_C;
            }
            else
            {
                lblAcO.Top = txtboxBody.Top;
                lblAcO.Left = (int)QuickFileControllerConstants.Left_lblAcO_C;
            }
            lblAcO.Width = 14;
            lblAcO.Text = "O";
            lblAcO.Font = new Font(lblAcO.Font.FontFamily, 10f, FontStyle.Bold);
            lblAcO.BorderStyle = BorderStyle.Fixed3D; // fmBorderStyleSingle
            lblAcO.TextAlign = ContentAlignment.TopCenter;  // fmTextAlignCenter
            // .SpecialEffect = fmSpecialEffectBump
            lblAcO.BackColor = SystemColors.ControlText;
            lblAcO.ForeColor = SystemColors.Control;
            lblAcO.Visible = blDebug;
            colCtrls.Add(lblAcO, "lblAcO");

            if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
            {
                var lblAcA = new Label();
                Frm.Controls.Add(lblAcA);
                lblAcA.Height = 14;

                if (blWideView)
                {
                    lblAcA.Top = (int)(36L + lngTopOff);
                    lblAcA.Left = chbxSaveAttach.Left + 10;
                }
                else
                {
                    lblAcA.Top = chbxSaveAttach.Top;
                    lblAcA.Left = chbxSaveAttach.Left + 10;
                }
                lblAcA.Width = 14;
                lblAcA.Text = "A";
                lblAcA.Font = new Font(lblAcA.Font.FontFamily, 10f, FontStyle.Bold);
                lblAcA.BorderStyle = BorderStyle.Fixed3D; // fmBorderStyleSingle
                lblAcA.TextAlign = ContentAlignment.TopCenter;  // fmTextAlignCenter
                // .SpecialEffect = fmSpecialEffectBump
                lblAcA.BackColor = SystemColors.ControlText;
                lblAcA.ForeColor = SystemColors.Control;
                lblAcA.Visible = blDebug;
                colCtrls.Add(lblAcA, "lblAcA");

                var lblAcW = new Label();
                Frm.Controls.Add(lblAcW);
                lblAcW.Height = 14;

                if (blWideView)
                {
                    lblAcW.Top = (int)(36L + lngTopOff);
                    lblAcW.Left = chbxDelFlow.Left + 29;
                }
                else
                {
                    lblAcW.Top = chbxDelFlow.Top;
                    lblAcW.Left = chbxDelFlow.Left + 29;
                }
                lblAcW.Width = 14;
                lblAcW.Text = "W";
                lblAcW.Font = new Font(lblAcW.Font.FontFamily, 10f, FontStyle.Bold);
                lblAcW.BorderStyle = BorderStyle.Fixed3D; // fmBorderStyleSingle
                lblAcW.TextAlign = ContentAlignment.TopCenter;  // fmTextAlignCenter
                lblAcW.BackColor = SystemColors.ControlText;
                lblAcW.ForeColor = SystemColors.Control;
                lblAcW.Visible = blDebug;
                colCtrls.Add(lblAcW, "lblAcW");

                var lblAcM = new Label();
                Frm.Controls.Add(lblAcM);
                lblAcM.Height = 14;

                if (blWideView)
                {
                    lblAcM.Top = (int)(36L + lngTopOff);
                    lblAcM.Left = chbxSaveMail.Left + 10;
                }
                else
                {
                    lblAcM.Top = chbxSaveMail.Top;
                    lblAcM.Left = chbxSaveMail.Left + 10;
                }
                lblAcM.Width = 14;
                lblAcM.Text = "M";
                lblAcM.Font = new Font(lblAcM.Font.FontFamily, 10f, FontStyle.Bold);
                lblAcM.BorderStyle = BorderStyle.Fixed3D; // fmBorderStyleSingle
                lblAcM.TextAlign = ContentAlignment.TopCenter;  // fmTextAlignCenter
                lblAcM.BackColor = SystemColors.ControlText;
                lblAcM.ForeColor = SystemColors.Control;
                lblAcM.Visible = blDebug;
                colCtrls.Add(lblAcM, "lblAcM");
            }



        }

        internal void RemoveControls()
        {

            QfcController QF;
            int i;

            // max = _colQFClass.Count
            // For i = max To 1 Step -1
            if (_colQFClass is not null)
            {
                while (_colQFClass.Count > 0)
                {
                    i = _colQFClass.Count;
                    QF = (QfcController)_colQFClass[i];
                    QF.ctrlsRemove();                                  // Remove controls on the frame
                    _viewer.L1v1L2_PanelMain.Controls.Remove(QF.frm);           // Remove the frame
                    QF.kill();                                         // Remove the variables linking to events

                    // PanelMain.Controls.Remove _colFrames(i).Name
                    _colQFClass.Remove(i);
                }
            }

            // _viewer.L1v1L2_PanelMain.ScrollHeight = _heightPanelMainMax



        }

        internal void MoveDownControlGroups(int intPosition, int intMoves)
        {

            int i;
            QfcController QF;
            Panel ctlFrame;

            var loopTo = intPosition;
            for (i = _colQFClass.Count; i >= loopTo; i -= 1)
            {

                // Shift items downward if there are any
                QF = (QfcController)_colQFClass[i];
                QF.Position += intMoves;
                ctlFrame = QF.frm;
                ctlFrame.Top = ctlFrame.Top + intMoves * (QuickFileControllerConstants.frmHt + QuickFileControllerConstants.frmSp);
            }
            // PanelMain.ScrollHeight = max((intMoves + _colQFClass.Count) * (frmHt + frmSp), _heightPanelMainMax)


        }

        public void ToggleRemoteMouseLabels()
        {
            _boolRemoteMouseApp = !_boolRemoteMouseApp;

            foreach (QfcController QF in _colQFClass)
                QF.ToggleRemoteMouseAppLabels();

        }

        public void MoveDownPix(int intPosition, int intPix)
        {

            int i;
            QfcController QF;
            Panel ctlFrame;

            var loopTo = intPosition;
            for (i = _colQFClass.Count; i >= loopTo; i -= 1)
            {

                // Shift items downward if there are any
                QF = (QfcController)_colQFClass[i];
                ctlFrame = QF.frm;
                ctlFrame.Top += intPix;
            }

        }

        public void AddEmailControlGroup(object objItem, int posInsert = 0, bool blGroupConversation = true, int ConvCt = 0, object varList = null, bool blChild = false)
        {

            MailItem Mail;
            QfcController QF;
            Collection colCtrls;

            _intUniqueItemCounter += 1;
            if (posInsert == 0)
                posInsert = _colQFClass.Count + 1;
            if (objItem is MailItem)
            {
                Mail = (MailItem)objItem;
                colCtrls = new Collection();
                LoadGroupOfCtrls(ref colCtrls, _intUniqueItemCounter, posInsert, blGroupConversation);
                QF = new QfcController(Mail, colCtrls, posInsert, _boolRemoteMouseApp, this, _globals);
                if (blChild)
                    QF.blHasChild = true;
                if (varList is Array == true)
                {
                    if (Information.UBound((Array)varList) == 0)
                    {
                        QF.Init_FolderSuggestions();
                    }
                    else
                    {
                        QF.Init_FolderSuggestions(varList);
                    }
                }
                else
                {
                    QF.Init_FolderSuggestions(varList);
                }
                QF.CountMailsInConv(ConvCt);

                if (posInsert > _colQFClass.Count)
                {
                    _colQFClass.Add(QF);
                }
                else
                {
                    // _colQFClass.Add(QF, QF.Mail.Subject & QF.Mail.SentOn & QF.Mail.Sender, posInsert)
                    _colQFClass.Add(QF, Before: posInsert);
                }

                // For i = 1 To _colQFClass.Count
                // QF = _colQFClass(i)
                // Debug.WriteLine("_colQFClass(" & i & ")   MyPosition " & QF.intMyPosition & "   " & QF.Mail.Subject)
                // Next i

            }

        }

        internal void RemoveSpecificControlGroup(int intPosition)
        {

            bool blDebug;
            QfcController QF;
            int intItemCount;
            int i;
            Panel ctlFrame;
            string strDeletedSub;
            string strDeletedDte;
            int intDeletedMyPos;

            blDebug = false;

            intItemCount = _colQFClass.Count;

            QF = (QfcController)_colQFClass[intPosition];                // Set class equal to specific member of collection

            strDeletedSub = QF.Mail.Subject;
            strDeletedDte = Strings.Format(QF.Mail.SentOn, @"mm\\dd\\yyyy hh:mm");
            intDeletedMyPos = QF.Position;


            QF.ctrlsRemove();                                  // Run the method that removes controls from the frame
            _viewer.L1v1L2_PanelMain.Controls.Remove(QF.frm);           // Remove the specific frame
            QF.kill();                                         // Remove the variables linking to events

            if (blDebug)
            {
                // Print data before movement
                Debug.Print("DEBUG DATA BEFORE MOVEMENT");

                var loopTo = intItemCount;
                for (i = 1; i <= loopTo; i++)
                {
                    if (i == intPosition)
                    {
                        Debug.Print(i + "  " + intDeletedMyPos + "  " + strDeletedDte + "  " + strDeletedSub);
                    }
                    else
                    {
                        QF = (QfcController)_colQFClass[i];
                        Debug.Print(i + "  " + QF.Position + "  " + Strings.Format(QF.Mail.SentOn, @"MM\\DD\\YY HH:MM") + "  " + QF.Mail.Subject);
                    }
                }
            }

            // Shift items upward if there are any
            if (intPosition < intItemCount)
            {
                var loopTo1 = intItemCount;
                for (i = intPosition + 1; i <= loopTo1; i++)
                {
                    QF = (QfcController)_colQFClass[i];
                    QF.Position -= 1;
                    ctlFrame = QF.frm;
                    ctlFrame.Top = ctlFrame.Top - QuickFileControllerConstants.frmHt - QuickFileControllerConstants.frmSp;
                }
                // _viewer.L1v1L2_PanelMain.ScrollHeight = max(_viewer.L1v1L2_PanelMain.ScrollHeight - frmHt - frmSp, _heightPanelMainMax)
            }

            _colQFClass.Remove(intPosition);

            if (blDebug)
            {
                // Print data after movement
                Debug.Print("DEBUG DATA POST MOVEMENT");

                var loopTo2 = _colQFClass.Count;
                for (i = 1; i <= loopTo2; i++)
                {
                    QF = (QfcController)_colQFClass[i];
                    Debug.Print(i + "  " + QF.Position + "  " + Strings.Format(QF.Mail.SentOn, @"MM\\DD\\YY HH:MM") + "  " + QF.Mail.Subject);
                }
            }

            QF = null;
        }

        public void ConvToggle_Group(Collection selItems, int intOrigPosition)
        {

            MailItem objEmail;
            int i;
            QfcController QF;
            QfcController QF_Orig;
            int intPosition;
            bool blDebug;

            blDebug = true;

            QF_Orig = (QfcController)_colQFClass[intOrigPosition];

            if (blDebug)
            {
                var loopTo = _colQFClass.Count;
                for (i = 1; i <= loopTo; i++)
                    // Debug.Print "_colQFClass(" & i & ")   MyPosition " & QF.intMyPosition & "   " & QF.mail.Subject
                    QF = (QfcController)_colQFClass[i];
            }

            foreach (var objItem in selItems)
            {
                objEmail = (MailItem)objItem;
                intPosition = GetEmailPositionInCollection(objEmail);
                // If intPosition < intOrigPosition Then QF_Orig.intMyPosition = intPosition
                RemoveSpecificControlGroup(intPosition);
            }
        }

        public void ConvToggle_UnGroup(Collection selItems, int intPosition, int ConvCt, object varList)
        {

            int i;
            QfcController QF;
            bool blDebug;

            blDebug = false;

            if (blDebug)
            {
                // Print data after movement
                // Debug.Print "DEBUG DATA BEFORE UNGROUP"
                var loopTo = _colQFClass.Count;
                for (i = 1; i <= loopTo; i++)
                    // Debug.Print i & "  " & QF.intMyPosition & "  " & Format(QF.mail.SentOn, "MM\DD\YY HH:MM") & "  " & QF.mail.Subject
                    QF = (QfcController)_colQFClass[i];
            }

            MoveDownControlGroups(intPosition + 1, selItems.Count);

            var loopTo1 = selItems.Count;
            for (i = 1; i <= loopTo1; i++)
                AddEmailControlGroup(selItems[i], intPosition + i, false, ConvCt, varList, true);

            if (blDebug)
            {
                // Print data after movement
                // Debug.Print "DEBUG DATA AFTER UNGROUP"
                var loopTo2 = _colQFClass.Count;
                for (i = 1; i <= loopTo2; i++)
                    // Debug.Print i & "  " & QF.intMyPosition & "  " & Format(QF.mail.SentOn, "MM\DD\YY HH:MM") & "  " & QF.mail.Subject
                    QF = (QfcController)_colQFClass[i];
            }
            _parent.FormResize(false);


        }

        internal void ResizeChildren(int intDiffx)
        {
            if (_colQFClass is not null)
            {
                foreach (QfcController QF in _colQFClass)
                {
                    if (QF.blHasChild)
                    {
                        QF.frm.Left = QuickFileControllerConstants.frmLt * 2;
                        QF.frm.Width = (int)(QuickFileControllerConstants.Width_frm + intDiffx - QuickFileControllerConstants.frmLt);
                        QF.ResizeCtrls(intDiffx - QuickFileControllerConstants.frmLt);
                    }
                    else
                    {
                        QF.frm.Width = (int)(QuickFileControllerConstants.Width_frm + intDiffx);
                        QF.ResizeCtrls(intDiffx);
                    }
                }
            }
        }

        #endregion

        #region Keyboard UI
        public void toggleAcceleratorDialogue()
        {
            QfcController QF;
            int i;

            if (_colQFClass is not null)
            {
                var loopTo = _colQFClass.Count;
                for (i = 1; i <= loopTo; i++)
                {
                    QF = (QfcController)_colQFClass[i];
                    if (QF.blExpanded & i != _colQFClass.Count)
                        MoveDownPix(i + 1, (int)Math.Round(QF.frm.Height * -0.5d));
                    QF.Accel_Toggle();
                }
            }

            if (_viewer.AcceleratorDialogue.Visible == true)
            {
                _viewer.AcceleratorDialogue.Visible = false;
                _viewer.L1v1L2_PanelMain.Focus();
            }
            else
            {
                if (AutoFile.AreConversationsGrouped(_globals.Ol.App.ActiveExplorer()))
                {

                }
                _viewer.AcceleratorDialogue.Visible = true;
                if (_intActiveSelection != 0)
                {
                    _viewer.AcceleratorDialogue.Text = _intActiveSelection.ToString();
                    try
                    {
                        QF = (QfcController)_colQFClass[_intActiveSelection];
                    }
                    catch (System.Exception ex)
                    {
                        _intActiveSelection = 1;
                        QF = (QfcController)_colQFClass[_intActiveSelection];
                    }
                    QF.Accel_FocusToggle();
                }

                _viewer.AcceleratorDialogue.Focus();
                _viewer.AcceleratorDialogue.SelectionStart = _viewer.AcceleratorDialogue.TextLength;
            }

            QF = null;
        }

        internal void ParseAcceleratorText()
        {
            var parser = new AcceleratorParser(this);
            parser.ParseAndExecute(_viewer.AcceleratorDialogue.Text, _intActiveSelection);
        }

        internal void ResetAcceleratorSilently()
        {
            bool blTemp = _suppressAcceleratorEvents;
            _suppressAcceleratorEvents = true;
            if (_intActiveSelection > 0)
            {
                _viewer.AcceleratorDialogue.Text = _intActiveSelection.ToString();
            }
            else
            {
                _viewer.AcceleratorDialogue.Text = "";
            }
            _suppressAcceleratorEvents = blTemp;
        }

        internal int ActivateByIndex(int intNewSelection, bool blExpanded)
        {
            if (intNewSelection > 0 & intNewSelection <= _colQFClass.Count)
            {
                QfcController QF = (QfcController)_colQFClass[intNewSelection];
                QF.Accel_FocusToggle();
                if (blExpanded)
                {
                    MoveDownPix(intNewSelection + 1, QF.frm.Height);
                    QF.ExpandCtrls1();
                }
                _intActiveSelection = intNewSelection;
                _viewer.L1v1L2_PanelMain.ScrollControlIntoView(QF.frm);
                return _intActiveSelection;
            }
            else
            {
                // Procedure failed so return current selection unaltered
                return _intActiveSelection;
            }
        }

        internal bool ToggleOffActiveItem(bool parentBlExpanded)
        {
            bool blExpanded = parentBlExpanded;
            if (_intActiveSelection != 0)
            {

                QfcController QF = (QfcController)_colQFClass[_intActiveSelection];
                if (QF.blExpanded)
                {
                    MoveDownPix(_intActiveSelection + 1, (int)Math.Round(QF.frm.Height * -0.5d));
                    QF.ExpandCtrls1();
                    blExpanded = true;
                }
                QF.Accel_FocusToggle();


                _intActiveSelection = 0;
            }
            return blExpanded;
        }

        internal void SelectPreviousItem()
        {
            if (_intActiveSelection > 1)
            {
                _viewer.AcceleratorDialogue.Text = (_intActiveSelection - 1).ToString();
            }
            _viewer.AcceleratorDialogue.SelectionStart = _viewer.AcceleratorDialogue.TextLength;
        }

        internal void SelectNextItem()
        {
            if (_intActiveSelection < _colQFClass.Count)
            {
                _viewer.AcceleratorDialogue.Text = (_intActiveSelection + 1).ToString();
            }
            _viewer.AcceleratorDialogue.SelectionStart = _viewer.AcceleratorDialogue.TextLength;
        }

        internal void MakeSpaceToEnumerateConversation()
        {
            bool blExpanded = false;
            if (_intActiveSelection != 0)
            {
                QfcController QF = (QfcController)_colQFClass[_intActiveSelection];
                if (QF.lblConvCt.Text != "1" & QF.chk.Checked == true)
                {
                    if (QF.blExpanded)
                    {
                        blExpanded = true;
                        MoveDownPix(_intActiveSelection + 1, (int)Math.Round(QF.frm.Height * -0.5d));
                        QF.ExpandCtrls1();
                    }
                    toggleAcceleratorDialogue();
                    // QF.KB toggles the conversation checkbox which triggers enumeration of conversation
                    QF.KB("C");
                    toggleAcceleratorDialogue();

                    if (blExpanded)
                    {
                        MoveDownPix(_intActiveSelection + 1, QF.frm.Height);
                        QF.ExpandCtrls1();
                    }
                }
            }
        }

        internal void RemoveSpaceToCollapseConversation()
        {
            if (_intActiveSelection != 0)
            {
                bool blExpanded = false;
                QfcController QF = (QfcController)_colQFClass[_intActiveSelection];
                if (QF.lblConvCt.Text != "1" & QF.chk.Checked == false)
                {
                    if (QF.blExpanded)
                    {
                        blExpanded = true;
                        MoveDownPix(_intActiveSelection + 1, (int)Math.Round(QF.frm.Height * -0.5d));
                        QF.ExpandCtrls1();
                    }
                    toggleAcceleratorDialogue();
                    QF.KB("C");
                    toggleAcceleratorDialogue();

                    if (blExpanded)
                    {
                        MoveDownPix(_intActiveSelection + 1, QF.frm.Height);
                        QF.ExpandCtrls1();
                    }

                }
                _viewer.AcceleratorDialogue.SelectionStart = _viewer.AcceleratorDialogue.TextLength;
            }
        }

        internal bool IsSelectionBelowMax(int intNewSelection)
        {
            if (intNewSelection <= _colQFClass.Count)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Properties and Helper Functions
        internal QuickFileController Parent
        {
            get
            {
                return _parent;
            }
        }

        internal int EmailsLoaded
        {
            get
            {
                return _colQFClass.Count;
            }
        }

        private int DoesCollectionHaveConvID(object objItem, Collection col)
        {
            int DoesCollectionHaveConvIDRet = default;

            object objItemInCol;
            MailItem objMailInCol;
            MailItem objMail;
            int i;

            DoesCollectionHaveConvIDRet = 0;

            if (objItem is MailItem)
            {
                objMail = (MailItem)objItem;
                if (col is not null)
                {
                    var loopTo = col.Count;
                    for (i = 1; i <= loopTo; i++)
                    {
                        objItemInCol = col[i];
                        if (objItemInCol is MailItem)
                        {
                            objMailInCol = (MailItem)objItemInCol;
                            if ((objMailInCol.ConversationID ?? "") == (objMail.ConversationID ?? ""))
                                DoesCollectionHaveConvIDRet = i;
                        }
                    }
                }
            }

            return DoesCollectionHaveConvIDRet;



        }

        private int GetEmailPositionInCollection(MailItem objMail)
        {
            int GetEmailPositionInCollectionRet = default;



            QfcController QF;
            int i;

            GetEmailPositionInCollectionRet = 0;
            var loopTo = _colQFClass.Count;
            for (i = 1; i <= loopTo; i++)
            {
                QF = (QfcController)_colQFClass[i];
                if ((QF.Mail.EntryID ?? "") == (objMail.EntryID ?? ""))
                    GetEmailPositionInCollectionRet = i;
            }

            return GetEmailPositionInCollectionRet;



        }

        internal QfcController TryGetQfc(object index)
        {
            try
            {
                return (QfcController)_colQFClass[index];
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        #endregion

        #region Email Filing
        internal bool ReadyForMove
        {
            get
            {
                bool blReadyForMove = true;
                string strNotifications = "Can't complete actions! Not all emails assigned to folder" + System.Environment.NewLine;

                foreach (QfcController QF in _colQFClass)
                {
                    if (QF.cbo.SelectedValue as string != "")
                    {
                        blReadyForMove = false;
                        strNotifications = strNotifications + QF.Position + "  " + QF.Mail.SentOn.ToString("mm\\dd\\yyyy") + "  " + QF.Mail.Subject + System.Environment.NewLine;
                    }
                }
                strNotifications = Strings.Mid(strNotifications, 1, Strings.Len(strNotifications) - 1);
                if (!blReadyForMove)
                    MessageBox.Show("Error Notification", strNotifications, MessageBoxButtons.OK);
                return blReadyForMove;
            }
        }

        internal void MoveEmails(ref cStackObject MovedMails)
        {
            if (_viewer.AcceleratorDialogue.Visible == true)
            {
                _viewer.AcceleratorDialogue.Text = "";
                toggleAcceleratorDialogue();
            }
            else
            {
                _intActiveSelection = 0;
            }
            foreach (QfcController QF in _colQFClass)
            {
                QF.MoveMail();
                MovedMails.Push(QF.Mail);
            }
        }

        internal string[] GetMoveDiagnostics(string durationText, string durationMinutesText, double Duration, string dataLineBeg, DateTime OlEndTime, ref AppointmentItem OlAppointment)
        {
            int k;
            string[] strOutput = new string[EmailsLoaded + 1];
            var loopTo = Conversions.ToInteger(EmailsLoaded);
            for (k = 1; k <= loopTo; k++)
            {
                QfcController QF = (QfcController)_colQFClass[k];
                var infoMail = new cInfoMail();
                if (infoMail.Init_wMail(QF.Mail, OlEndTime: OlEndTime, lngDurationSec: (long)Math.Round(Duration)))
                {
                    if (string.IsNullOrEmpty(OlAppointment.Body))
                    {
                        OlAppointment.Body = infoMail.ToString;
                        OlAppointment.Save();
                    }
                    else
                    {
                        OlAppointment.Body = OlAppointment.Body + System.Environment.NewLine + infoMail.ToString;
                        OlAppointment.Save();
                    }
                }
                string dataLine = dataLineBeg + xComma(QF.LblSubject.Text);
                dataLine = dataLine + "," + "QuickFiled";
                dataLine = dataLine + "," + durationText;
                dataLine = dataLine + "," + durationMinutesText;
                dataLine = dataLine + "," + xComma(QF.StrlblTo);
                dataLine = dataLine + "," + xComma(QF.Sender);
                dataLine = dataLine + "," + "Email";
                dataLine = dataLine + "," + xComma(QF.cbo.SelectedItem.ToString());           // Target Folder
                dataLine = dataLine + "," + QF.lblSentOn.Text;
                dataLine = dataLine + "," + Strings.Format(QF.Mail.SentOn, "hh:mm");
                strOutput[k] = dataLine;
            }

            return default;
        }

        private string xComma(string str)
        {
            string xCommaRet = default;
            string strTmp;

            strTmp = Strings.Replace(str, ", ", "_");
            strTmp = Strings.Replace(strTmp, ",", "_");
            xCommaRet = StringManipulation.GetStrippedText(strTmp);
            return xCommaRet;
            // xComma = StripAccents(strTmp)
        }
        #endregion

    }
}