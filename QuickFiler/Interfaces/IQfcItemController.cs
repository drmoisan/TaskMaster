using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        object ObjItem { get; set; }
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
        void PopulateConversation();
        void PopulateConversation(DataFrame df);
        void PopulateConversation(int countOnly);
        void SetThemeDark();
        void SetThemeLight();
        void Cleanup();
        DataFrame DfConversation { get; }
    }
}
