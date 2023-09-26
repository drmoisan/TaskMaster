using Microsoft.Office.Interop.Outlook;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;
using UtilitiesCS;
using System;
using System.Threading.Tasks;
using QuickFiler.Helper_Classes;

namespace QuickFiler.Interfaces
{
    public interface IQfcCollectionController
    {
        // Public Properties
        

        // UI Add and Remove QfcItems
        void LoadControlsAndHandlers(IList<MailItem> listObjects, RowStyle template, RowStyle templateExpanded);
        Task LoadControlsAndHandlersAsync(IList<MailItem> listObjects, RowStyle template, RowStyle templateExpanded);
        ItemViewer LoadItemViewer(int intItemNumber, RowStyle template, bool blGroupConversation = true, int columnNumber = 0); 
        void PopOutControlGroup(int intPosition);
        void RemoveControls();
        Task RemoveControlsAsync();
        void RemoveSpaceToCollapseConversation();
        void RemoveSpecificControlGroup(int intPosition);
        Task MoveEmailsAsync(ScoStack<IMovedMailInfo> StackMovedItems);

        // UI Select QfcItems
        int ActivateBySelection(int intNewSelection, bool blExpanded);
        void ChangeByIndex(int idx);
        void SelectNextItem();
        void SelectPreviousItem();
        void ToggleOffNavigation(bool async);
        void ToggleOnNavigation(bool async);
        void ToggleExpansionStyle(int itemIndex, Enums.ToggleState desiredState);

        // UI Converations Expansion
        void ToggleGroupConv(int childCount, int indexOriginal);
        void ToggleGroupConv(string originalId);
        void ToggleUnGroupConv(ConversationResolver resolver, string entryID, int conversationCount, object folderList);
        void MakeSpaceToEnumerateConversation(int insertionIndex, int insertCount);                        

        // UI Light Dark
        void SetDarkMode(bool async);
        void SetLightMode(bool async);

        // Helper Functions
        int EmailsLoaded { get; }
        bool ReadyForMove { get; }
        void ResetPanelHeight();

        void Cleanup();

        string[] GetMoveDiagnostics(string durationText, string durationMinutesText, double Duration, string dataLineBeg, DateTime OlEndTime, ref AppointmentItem OlAppointment);

    }
}