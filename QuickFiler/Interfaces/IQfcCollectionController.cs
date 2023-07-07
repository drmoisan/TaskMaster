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
        // UI Add and Remove QfcItems
        void LoadControlsAndHandlers(IList<MailItem> listObjects, RowStyle template);
        QfcItemViewer LoadItemViewer(int intItemNumber, RowStyle template, bool blGroupConversation = true, int columnNumber = 0); 
        void AddEmailControlGroup(MailItem mailItem, int posInsert = 0, bool blGroupConversation = true, int ConvCt = 0, object varList = null, bool blChild = false);
        void RemoveControls();
        void RemoveSpaceToCollapseConversation();
        void RemoveSpecificControlGroup(int intPosition);
        void MoveEmails(StackObjectCS<MailItem> StackMovedItems);

        // UI Select QfcItems
        int ActivateByIndex(int intNewSelection, bool blExpanded);
        void SelectNextItem();
        void SelectPreviousItem();
        void ToggleOffNavigation();
        void ToggleOnNavigation();

        // UI Move QfcItems
        void MoveDownControlGroups(int intPosition, int intMoves);		//Rewrite
        void MoveDownPix(int intPosition, int intPix);                  //Rewrite
        void ResizeChildren(int intDiffx);                              //possibly unneccessary with new control group

        // UI Converations Expansion
        void ConvToggle_Group(IList<MailItem> selItems, int intOrigPosition);
        void ConvToggle_UnGroup(IList<MailItem> mailItems, int baseEmailIndex, int conversationCount, object folderList);
        void MakeSpaceToEnumerateConversation();                        //Rewrite

        // UI Light Dark
        void SetDarkMode();
        void SetLightMode();

        // Helper Functions
        bool IsSelectionBelowMax(int intNewSelection);
        int EmailsLoaded { get; }
        bool ReadyForMove { get; }

        void Cleanup();

        string[] GetMoveDiagnostics(string durationText, string durationMinutesText, double Duration, string dataLineBeg, DateTime OlEndTime, ref AppointmentItem OlAppointment);

    }
}