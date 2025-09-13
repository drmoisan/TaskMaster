using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Helper_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS;

namespace QuickFiler.Interfaces
{
    public interface IQfcItemController
    {
        void AssignFolderComboBox();
        /// <summary>
        /// Toggles special formatting for one group to highlight the group of controls that is active
        /// </summary>        
        Task InitializeAsync();
        Task InitializeSequentialAsync();
        void Initialize(bool async);
        Task LoadConversationResolverAsync(CancellationTokenSource tokenSource, CancellationToken token, bool loadAll);
        void ToggleFocus(); // Turn on or off the formatting to highlight this QfcItem
        void ToggleFocus(Enums.ToggleState desiredState);
        Task ToggleFocusAsync();
        int CounterEnter { get; set; }
        int CounterComboRight { get; set; }
        bool IsExpanded {  get; }
        bool IsChild { get; set; }
        bool IsActiveUI { get; set; }
        string ConvOriginID { get; set; }
        int Height { get; }
        MailItemHelper ItemHelper { get; set; }
        MailItem Mail { get; set; }
        void ToggleExpansion(); 
        string SelectedFolder { get; }
        int ItemNumber { get; set; }
        int ItemIndex { get; set; }
        int ItemNumberDigits { get; set; }
        void PopulateFolderComboBox(object varList = null); // Handles just the UI aspect. Relies on FolderSuggestionsModule.Folder_Suggestions
        bool SuppressEvents { get; set; }
        void ApplyReadEmailFormat(object state);
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
        Task PopulateConversationAsync(CancellationTokenSource tokenSource, CancellationToken token, bool loadAll);
        //void PopulateConversation(DataFrame df);
        void PopulateConversation(int countOnly);
        void PopulateConversation(ConversationResolver resolver);
        Task PopulateFolderComboBoxAsync(CancellationToken token, object varList = null);
        Task LoadFolderHandlerAsync(CancellationToken cancel, object varList = null);
        void PopulateControls(MailItemHelper helper, int viewerPosition);
        void RenderConversationCount(int count);
        void RenderConversationCount();
        void SetThemeDark(bool async);
        void SetThemeLight(bool async);
        void Cleanup();
        Task MoveMailAsync();
        //string Subject { get; }
        //string To { get; }
        //string Sender { get; }
        //string SentDate { get; }
        //string SentTime { get; }
        IList<TableLayoutPanel> TableLayoutPanels { get; }
        IList<Button> Buttons { get; }
        IList<IQfcTipsDetails> ListTipsDetails { get; }
        IList<IQfcTipsDetails> ListTipsExpanded { get; }
        void ToggleNavigation(bool async);
        void ToggleNavigation(bool async, Enums.ToggleState desiredState);
        Task ToggleNavigationAsync(Enums.ToggleState desiredState);
        void ToggleTips(bool async, Enums.ToggleState desiredState);
        Task ToggleExpansionAsync();
        Task ToggleFocusAsync(Enums.ToggleState off);
        public CancellationToken Token { get; set; }

        public Dictionary<string, System.Action> RightKeyActions { get; }
        Task InitializeGraphicsAsync();
    }
}
