using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;

namespace QuickFiler.Interfaces
{
    public interface IQfcCollectionController
    {
        // Public Properties
        List<QfcItemGroup> ItemGroups { get; set; }

        // UI Add and Remove QfcItems
        Task LoadSecondaryAsync();
        void LoadControlsAndHandlers_01(
            IList<MailItem> listObjects,
            RowStyle template,
            RowStyle templateExpanded
        );
        void LoadControlsAndHandlers_01(TableLayoutPanel tlp, List<QfcItemGroup> itemGroups);
        Task LoadControlsAndHandlers_01Async(
            IList<MailItem> listObjects,
            RowStyle template,
            RowStyle templateExpanded
        );
        Task LoadControlsAndHandlers_01Async(
            IList<QfcPreScoredItem> preScored,
            RowStyle template,
            RowStyle templateExpanded
        );
        ItemViewer LoadItemViewer_03(
            int intItemNumber,
            RowStyle template,
            bool blGroupConversation = true,
            int columnNumber = 0
        );
        void PopOutControlGroup(int intPosition);
        Task PopOutControlGroupAsync(int selection);
        void RemoveControls();
        Task RemoveControlsAsync();
        void EliminateSpaceForItems(int removalInex, int removalCount);
        void RemoveSpecificControlGroup(int intPosition);
        Task RemoveSpecificControlGroupAsync(int selection);
        Task MoveEmailsAsync(SloStack<IMovedMailInfo> StackMovedItems);
        void AddItemGroup(MailItem mailItem);

        /// <summary>
        /// Removes item groups whose <c>ItemController.TopFolderScore</c> is below the score
        /// cutoff derived from <paramref name="threshold"/> as
        /// <c>(long)Math.Round(threshold * 1000, 0)</c>. The comparison is inclusive of the
        /// boundary: a group whose score equals the cutoff is retained. Groups with no qualifying
        /// suggestion (score 0) are removed whenever the cutoff is greater than 0. Removal reuses
        /// the existing control-group removal path so the move monitor is unhooked and remaining
        /// groups are renumbered on the UI thread.
        ///
        /// This method is not the live issue #233 high-confidence enforcement gate. Issue #233
        /// applies threshold filtering in the datamodel dequeue layer before items are surfaced.
        /// </summary>
        /// <param name="threshold">A probability in the range [0.0, 1.0].</param>
        Task RemoveBelowThresholdAsync(double threshold);

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
        void ToggleUnGroupConv(
            ConversationResolver resolver,
            string entryID,
            int conversationCount,
            object folderList
        );
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

        string[] GetMoveDiagnostics(
            string durationText,
            string durationMinutesText,
            double Duration,
            string dataLineBeg,
            DateTime OlEndTime,
            ref AppointmentItem OlAppointment
        );
    }
}
