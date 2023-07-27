using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickFiler.Interfaces
{
    public interface IQfcItemController
    {
        /// <summary>
        /// Toggles special formatting for one group to highlight the group of controls that is active
        /// </summary>
        void ToggleFocus(); // Turn on or off the formatting to highlight this QfcItem
        void ToggleFocus(Enums.ToggleState desiredState);
        int CounterEnter { get; set; }
        int CounterComboRight { get; set; }
        bool IsExpanded {  get; }
        bool IsChild { get; set; }
        bool IsActiveUI { get; set; }
        string ConvOriginID { get; set; }
        int Height { get; }
        MailItem Mail { get; set; }
        void ToggleExpansion(); 
        string SelectedFolder { get; }
        int ItemNumber { get; set; }
        int ItemIndex { get; set; }
        void PopulateFolderCombobox(object varList = null); // Handles just the UI aspect. Relies on FolderSuggestionsModule.Folder_Suggestions
        bool SuppressEvents { get; set; }
        void ApplyReadEmailFormat();
        void FlagAsTask();
        void MarkItemForDeletion();
        void JumpToSearchTextbox();
        void JumpToFolderDropDown();
        void ToggleSaveCopyOfMail();
        void ToggleSaveAttachments();
        void ToggleConversationCheckbox();
        void ToggleConversationCheckbox(Enums.ToggleState desiredState);
        IQfcCollectionController Parent { get; }
        void PopulateConversation();
        void PopulateConversation(DataFrame df);
        void PopulateConversation(int countOnly);
        void SetThemeDark(bool async);
        void SetThemeLight(bool async);
        void Cleanup();
        void MoveMail();
        DataFrame DfConversation { get; }
        string Subject { get; }
        string To { get; }
        string Sender { get; }
        string SentDate { get; }
        string SentTime { get; }
        IList<TableLayoutPanel> TableLayoutPanels { get; }
        IList<Button> Buttons { get; }
        IList<IQfcTipsDetails> ListTipsDetails { get; }
        void ToggleNavigation(bool async);
        void ToggleNavigation(bool async, Enums.ToggleState desiredState);
        void ToggleTips(bool async);
        void ToggleTips(bool async, Enums.ToggleState desiredState);
        public Dictionary<string, System.Action> RightKeyActions { get; }
    }
}
