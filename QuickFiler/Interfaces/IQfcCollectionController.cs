using Microsoft.Office.Interop.Outlook;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;

namespace QuickFiler
{
    public interface IQfcCollectionController
    {
        // UI Add and Remove QfcItems
        void LoadControlsAndHandlers(IList<MailItem> colEmails);
        QfcItemViewerForm LoadItemViewer(int intItemNumber, bool blGroupConversation = true); 
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
        void MoveDownPix(int intPosition, int intPix);                  //Rewrite
        void ResizeChildren(int intDiffx);                              //possibly unneccessary with new control group

        // UI Converations Expansion
        void ConvToggle_Group(IList<MailItem> selItems, int intOrigPosition);
        void ConvToggle_UnGroup(IList<MailItem> selItems, int intPosition, int ConvCt, object varList);
        void MakeSpaceToEnumerateConversation();                        //Rewrite

        // Helper Functions
        bool IsSelectionBelowMax(int intNewSelection);
        int EmailsLoaded { get; }
        bool ReadyForMove { get; }
        

    }
}