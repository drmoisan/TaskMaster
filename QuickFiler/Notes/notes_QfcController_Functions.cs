using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QuickFiler.Notes
{
    internal interface ITmp
    {
        Button CbDel { get; set; }
        Button CbKll { get; set; }
        Button CbTmp { get; set; }
        CheckBox ConversationCb { get; set; }
        Button FlagTaskCb { get; set; }
        ComboBox FolderCbo { get; set; }
        Panel Frm { get; set; }
        int Position { get; set; }
        TextBox SearchTxt { get; set; }
        string Sender { get; }
        TextBox TxtBoxBody { get; set; }

        //TODO: Create a function to set a few styles upfront so that we can just apply them
		void CreateStyles()
		
		//TODO: Functions to adapt to new QfcItemViewer
		void Accel_FocusToggle(); 
        void Accel_Toggle();
		
		void ApplyReadEmailFormat();
		void FlagAsTask();
		void JumpToSearchTextbox();
		void MarkItemForDeletion();
		void ToggleDeleteFlow();
		void ToggleSaveCopyOfMail();
		void ToggleConversationCheckbox();
		
        void bdy_Click(object sender, EventArgs e);
        void cbDel_Click(object sender, EventArgs e);
        void cbDel_KeyDown(object sender, KeyEventArgs e);
        void cbDel_KeyPress(object sender, KeyPressEventArgs e);
        void cbDel_KeyUp(object sender, KeyEventArgs e);
        void cbFlag_Click(object sender, EventArgs e);
        void cbFlag_KeyDown(object sender, KeyEventArgs e);
        void cbFlag_KeyPress(object sender, KeyPressEventArgs e);
        void cbFlag_KeyUp(object sender, KeyEventArgs e);
        void cbKll_Click(object sender, EventArgs e);
        void cbKll_KeyDown(object sender, KeyEventArgs e);
        void cbKll_KeyPress(object sender, KeyPressEventArgs e);
        void cbKll_KeyUp(object sender, KeyEventArgs e);
        void cbo_KeyDown(object sender, KeyEventArgs e);
        void cbo_KeyUp(object sender, KeyEventArgs e);
        void cbTmp_KeyDown(object sender, KeyEventArgs e);
        void cbTmp_KeyUp(object sender, KeyEventArgs e);
        void chk_Click(object sender, EventArgs e);
        void chk_KeyDown(object sender, KeyEventArgs e);
        void chk_KeyUp(object sender, KeyEventArgs e);
        void CountMailsInConv(int ct = 0);
        void ctrlsRemove();
        void EmailFormatting();
        void ExpandCtrls1();
        void FlagAsTask();
        void frm_KeyDown(object sender, KeyEventArgs e);
        void frm_KeyPress(object sender, KeyPressEventArgs e);
        void frm_KeyUp(object sender, KeyEventArgs e);
        void Init_FolderSuggestions(object varList = null);
        void KeyboardHandler(string AccelCode);
        void KeyPressHandler_Class(object sender, KeyPressEventArgs e);
        void kill();
        void lst_KeyDown(object sender, KeyEventArgs e);
        void lst_KeyUp(object sender, KeyEventArgs e);
        void Mail_Activate();
        void MoveMail();
        void ResizeCtrls(int intPxChg);
        void ResolveControlAssignments(List<Control> controlList);
        void SetInitialControlSizePosition();
        void ToggleRemoteMouseAppLabels();
        void txt_Change(object sender, EventArgs e);
        void txt_KeyDown(object sender, KeyEventArgs e);
        void txt_KeyPress(object sender, KeyPressEventArgs e);
        void txt_KeyUp(object sender, KeyEventArgs e);
        void WireEventHandlers();
    }
}