namespace QuickFiler.Notes
{
	public interface IQfcHomeController
	{
		IQfcExplorerController _expl;
		IQfcFormController _frm;
		IQfcCollectionController _grp;
		IKeyboardHandler _kbd;
		
		void Iterate();
	}
	
	
	public interface IQfcFormController
	{
		
		void FormResize(bool Force = false); // might not be necessary
		void ButtonCancel_Click();
		void ButtonOK_Click();
		void ButtonUndo_Click();
		void Cleanup();
		void QFD_Maximize();
        void QFD_Minimize();
        void SpnEmailPerLoad_Change();
        void Viewer_Activate();
	}
	
	public interface IQfcDatamodel
	{
		List<MailItem> DequeueNextEmailGroup;
		void UndoMove();
		cStackObject MovedMails;
		bool MoveEmails(ref cStackObject MovedMails);
		void CountMailsInConv(int ct = 0); //From item controller
	}
	
	public interface IKeyboardHandler
	{
		void ToggleKeyboardDialog(); // Need to rewrite
		void ToggleRemoteMouseLabels(); // Not supported yet
		bool ToggleOffActiveItem(bool parentBlExpanded);
		void KeyboardDialog_Change();
        void KeyboardDialog_KeyDown(object sender, KeyEventArgs e);
        void KeyboardDialog_KeyUp(object sender, KeyEventArgs e);
		void ResetAcceleratorSilently();
		void KeyboardHandler_KeyDown(object sender, KeyEventArgs e);
        void KeyboardHandler_KeyPress(object sender, KeyPressEventArgs e);
        void KeyboardHandler_KeyUp(object sender, KeyEventArgs e);
		void PanelMain_KeyDown(object sender, KeyEventArgs e);
        void PanelMain_KeyPress(object sender, KeyPressEventArgs e);
        void PanelMain_KeyUp(object sender, KeyEventArgs e);
	}
	
	public interface IQfcExplorerController
	{
		bool BlShowInConversations { get; set; }
		void OpenQFMail(MailItem OlMail);
		void ExplConvView_ToggleOff();
		void ExplConvView_ToggleOn();
		void ExplConvView_Cleanup();
		void ExplConvView_ReturnState();
	}
	
	public interface IQfcCollectionController
	{
		
				
		// UI Add and Remove QfcItems
		void LoadControlsAndHandlers(List<MailItem> colEmails);
		void LoadGroupOfCtrls(ref List<Control> colCtrls, int intItemNumber, int intPosition = 0, bool blGroupConversation = true, bool blWideView = false); // drastically simplify
		void AddEmailControlGroup(object objItem, int posInsert = 0, bool blGroupConversation = true, int ConvCt = 0, object varList = null, bool blChild = false);
		void RemoveControls();
        void RemoveSpaceToCollapseConversation();
        void RemoveSpecificControlGroup(int intPosition);
		
		// UI Select QfcItems
		int ActivateByIndex(int intNewSelection, bool blExpanded);
		void SelectNextItem();
        void SelectPreviousItem();
		
		// UI Move QfcItems
        void MoveDownControlGroups(int intPosition, int intMoves);		//Rewrite
        void MoveDownPix(int intPosition, int intPix);					//Rewrite
		void ResizeChildren(int intDiffx); 								//possibly unneccessary with new control group
		
		// UI Converations Expansion
		void ConvToggle_Group(List<MailItem> selItems, int intOrigPosition);
        void ConvToggle_UnGroup(List<MailItem> selItems, int intPosition, int ConvCt, object varList);
		void MakeSpaceToEnumerateConversation(); 						//Rewrite
		
		// Helper Functions
		bool IsSelectionBelowMax(int intNewSelection);
		int EmailsLoaded { get; }
		bool ReadyForMove { get; }
		bool ReadyForMove();
		
				
	}
		
	
	public interface IQfcItemController
	{
		void Accel_FocusToggle(); // Turn on or off the formatting to highlight this QfcItem
        void Accel_Toggle();
        void ctrlsRemove(); // May not be necessary
        bool BlExpanded {  get; }
        bool BlHasChild { get; set; }
        int Height { get; }
        MailItem Mail { get; set; }
        void ExpandCtrls1(); // Rewrite
        void PopulateFolderCombobox(object varList = null); // Handles just the UI aspect. Relies on FolderSuggestionsModule.Folder_Suggestions
        void ApplyReadEmailFormat();
        void FlagAsTask();
        void MarkItemForDeletion();
        void JumpToSearchTextbox();
        void JumpToFolderDropDown();
        void ToggleDeleteFlow();
        void ToggleSaveCopyOfMail();
        void ToggleSaveAttachments();
        void ToggleConversationCheckbox();
	}

}