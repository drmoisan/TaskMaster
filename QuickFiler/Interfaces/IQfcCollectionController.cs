using Microsoft.Office.Interop.Outlook;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;
using UtilitiesCS;
using System;
using System.Threading.Tasks;
using QuickFiler.Helper_Classes;
using QuickFiler.Controllers;

namespace QuickFiler.Interfaces
{
    public interface IQfcCollectionController
    {
        // Public Properties
        List<QfcItemGroup> ItemGroups { get; set; }

        // UI Add and Remove QfcItems
        void LoadControlsAndHandlers_01(IList<MailItem> listObjects, RowStyle template, RowStyle templateExpanded);
        void LoadControlsAndHandlers_01(TableLayoutPanel tlp, List<QfcItemGroup> itemGroups);
        Task LoadControlsAndHandlers_01Async(IList<MailItem> listObjects, RowStyle template, RowStyle templateExpanded);
        ItemViewer LoadItemViewer_03(int intItemNumber, RowStyle template, bool blGroupConversation = true, int columnNumber = 0); 
        void PopOutControlGroup(int intPosition);
        Task PopOutControlGroupAsync(int selection);
        void RemoveControls();
        Task RemoveControlsAsync();
        void EliminateSpaceForItems(int removalInex, int removalCount);
        void RemoveSpecificControlGroup(int intPosition);
        Task RemoveSpecificControlGroupAsync(int selection);
        Task MoveEmailsAsync(ScoStack<IMovedMailInfo> StackMovedItems);
        void AddItemGroup(MailItem mailItem);

        // UI Select QfcItems
        int ActivateBySelection(int intNewSelection, bool blExpanded);
        void ChangeByIndex(int idx);
        void SelectNextItem();
        void SelectPreviousItem();
        void ToggleOffNavigation(bool async);
        void ToggleOnNavigation(bool async);
        void ToggleExpansionStyle(int itemIndex, Enums.ToggleState desiredState);
        Task ToggleExpansionStyleAsync(int itemIndex, Enums.ToggleState desiredState);

        // UI Converations Expansion
        void ToggleGroupConv(int childCount, int indexOriginal);
        void ToggleGroupConv(string originalId);
        void ToggleUnGroupConv(ConversationResolver resolver, string entryID, int conversationCount, object folderList);
        void MakeSpaceForItems(int insertionIndex, int insertCount);                        

        // UI Light Dark
        void SetDarkMode(bool async);
        void SetLightMode(bool async);

        // Helper Functions
        int EmailsLoaded { get; }
        int EmailsToMove { get; }
        bool ReadyForMove { get; }
        void ResetPanelHeight();

        // Navigation
        void UnregisterNavigation();
        void RegisterNavigation();
        Task ToggleOffNavigationAsync();
        Task ToggleOnNavigationAsync();
        void CacheMoveObjects();
        void CleanupBackground();

        void Cleanup();

        string[] GetMoveDiagnostics(string durationText, string durationMinutesText, double Duration, string dataLineBeg, DateTime OlEndTime, ref AppointmentItem OlAppointment);
        
    }
}