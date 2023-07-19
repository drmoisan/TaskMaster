using Microsoft.Office.Interop.Outlook;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;
using UtilitiesCS;
using System;

namespace QuickFiler.Interfaces
{
    public interface IQfcCollectionController
    {
        // Public Properties
        

        // UI Add and Remove QfcItems
        void LoadControlsAndHandlers(IList<MailItem> listObjects, RowStyle template);
        QfcItemViewer LoadItemViewer(int intItemNumber, RowStyle template, bool blGroupConversation = true, int columnNumber = 0); 
        void PopOutControlGroup(int intPosition);
        void RemoveControls();
        void RemoveSpaceToCollapseConversation();
        void RemoveSpecificControlGroup(int intPosition);
        void MoveEmails(StackObjectCS<MailItem> StackMovedItems);

        // UI Select QfcItems
        int ActivateBySelection(int intNewSelection, bool blExpanded);
        void ChangeByIndex(int idx);
        void SelectNextItem();
        void SelectPreviousItem();
        void ToggleOffNavigation(bool async);
        void ToggleOnNavigation(bool async);

        // UI Converations Expansion
        void ConvToggle_Group(int childCount, int indexOriginal);
        void ConvToggle_Group(string originalId);
        void ConvToggle_UnGroup(IList<MailItem> mailItems, string entryID, int conversationCount, object folderList);
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