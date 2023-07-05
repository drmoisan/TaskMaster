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
        void Accel_FocusToggle(); // Turn on or off the formatting to highlight this QfcItem
        void Accel_Toggle();
        void ctrlsRemove(); // May not be necessary
        bool BlExpanded {  get; }
        bool BlHasChild { get; set; }
        int Height { get; }
        MailItem Mail { get; set; }
        void ExpandCtrls1(); // Rewrite
        string SelectedFolder { get; }
        int Position { get; set; }
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
        void PopulateConversation();
        void PopulateConversation(DataFrame df);
        void PopulateConversation(int countOnly);
        void SetThemeDark();
        void SetThemeLight();
        void Cleanup();
        void MoveMail();
        DataFrame DfConversation { get; }
        string Subject { get; }
        string To { get; }
        string Sender { get; }
        string SentDate { get; }
        string SentTime { get; }
        IList<TableLayoutPanel> Tlps { get; }
        IList<Button> Buttons { get; }
        IList<IQfcTipsDetails> ListTipsDetails { get; }
    }
}
