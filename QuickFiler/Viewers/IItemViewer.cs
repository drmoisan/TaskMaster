using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using BrightIdeasSoftware;
using Microsoft.Web.WebView2.WinForms;
using QuickFiler.Viewers;
using SVGControl;
using UtilitiesCS.Interfaces.IWinForm;

namespace QuickFiler
{
    public interface IItemViewer : IUserControl, IContainerControlLocal
    {
        IItemControler Controller { get; set; }
        IList<Label> ExpandedTipsLabels { get; }
        Label LblAcBody { get; set; }
        Label LblAcDelete { get; set; }
        Label LblAcFolder { get; set; }
        Label LblAcFwd { get; set; }
        Label LblAcMoveOptions { get; set; }
        Label LblAcOpen { get; set; }
        Label LblAcPopOut { get; set; }
        Label LblAcReply { get; set; }
        Label LblAcReplyAll { get; set; }
        Label LblAcSearch { get; set; }
        Label LblAcTask { get; set; }
        Label LblCaptionPredicted { get; set; }
        Label LblCaptionTriage { get; set; }
        Label LblSearch { get; set; }
        IList<Label> LeftTipsLabels { get; }
        List<Component> MenuItems { get; }
        IList<Label> TipsLabels { get; }
        Dispatcher UiDispatcher { get; }
        SynchronizationContext UiSyncContext { get; }

        // Display-state intent members (Seam B, Cluster 2a) replacing the raw text-bearing Labels
        // and TxtboxBody. The concrete ItemViewer keeps the underlying Labels/TextBox as public
        // members; only the interface surface is narrowed to intent.
        string SenderText { get; set; }
        string SubjectText { get; set; }
        string BodyText { get; set; }
        string TriageText { get; set; }
        string SentOnText { get; set; }
        string ActionableText { get; set; }
        string ItemNumberText { get; set; }
        string FolderText { get; set; }
        string ConversationCountText { get; set; }
        System.Drawing.Color ConversationCountBackColor { get; set; }
        event System.EventHandler BodyDoubleClick;
        void FocusSubject();

        // Button command events and menu intent members (Seam B, Cluster 2b) replacing the raw
        // ButtonSVG click events and ToolStripMenuItemCb check-state members. The concrete ItemViewer
        // keeps the underlying buttons/menu items as public members; only the interface is narrowed.
        event System.EventHandler DeleteItemClicked;
        event System.EventHandler FlagTaskClicked;
        event System.EventHandler PopOutClicked;
        event System.EventHandler ReplyClicked;
        event System.EventHandler ReplyAllClicked;
        event System.EventHandler ForwardClicked;
        event System.EventHandler ConversationModeChanged;
        bool ConversationModeChecked { get; set; }
        event System.EventHandler EmailCopyChanged;
        bool EmailCopyChecked { get; set; }
        event System.EventHandler AttachmentsChanged;
        bool AttachmentsChecked { get; set; }
        event System.EventHandler PicturesChanged;
        bool PicturesChecked { get; set; }
        DialogResult FlagTaskDialogResult { get; set; }
        System.Drawing.Color FlagTaskBackColor { get; set; }

        // Folder-combo and search intent members (Seam C, Cluster 2c) replacing the raw ComboBox
        // CboFolders and TextBox TxtboxSearch. GetFolderItems() exposes the current combo items as a
        // string[] so the controller can read the folder list (EnumerateConversation) without the raw
        // ComboBox; the underlying controls remain public on the concrete ItemViewer.
        void SetFolderItems(string[] items);

        // Additive intent member (#325): populates the folder dropdown from the ordered FolderRow
        // contract (FolderPredictor.FolderRowArray / FindFolderRows), building the expandable folder
        // tree and the right-aligned prediction percentages. Additive alongside — not a replacement
        // for — SetFolderItems(string[]), which live controller call sites still use.
        void SetFolderSuggestions(IReadOnlyList<UtilitiesCS.FolderRow> rows);
        string GetSelectedFolder();
        void SetFolderSelectedIndex(int index);
        void SetFolderSelectedItem(string item);
        void SetFolderDroppedDown(bool droppedDown);

        // Additive intent member (#438): presents one folder-search result set. Replaces the
        // controller's per-keystroke ClearFolderItems + SetFolderItems + SetFolderSelectedIndex +
        // SetFolderDroppedDown composition, which opened the drop-down (stealing keyboard focus from
        // the search textbox) and committed a mid-search folder selection on every keystroke. The
        // rows are replaced, the selector is opened only if closed, and the first selectable row is
        // highlighted pending-only — all without transferring focus. Additive alongside — not a
        // replacement for — SetFolderDroppedDown(bool), which every explicit gesture still uses.
        void PresentFolderSearchResults(string[] items);

        void ClearFolderItems();
        void FocusFolderDropDown();
        bool FolderContains(string item);
        string[] GetFolderItems();
        event System.EventHandler FolderSelectionChanged;
        event KeyEventHandler FolderKeyDown;
        string SearchText { get; }
        event System.EventHandler SearchTextChanged;
        event KeyEventHandler SearchKeyDown;
        void FocusSearch();

        // WebView and topic-thread intent members (Seam D, Cluster 2d) replacing the raw WebView2,
        // FastObjectListView, OLV columns, menu-strip/menu, and layout-panel members. The SentDate
        // sort dependency is encapsulated inside SortConversationByDate. MenuItems (IList<Component>)
        // is RETAINED above for the WireEvents hover loop. WebView core-init and expanded-action focus
        // targets stay concrete-bound (P2-T4 seam) and are not exposed here.
        void NavigateToString(string html);
        event System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs> WebViewInitializationCompleted;
        void SetConversationItems(System.Collections.IList items);
        void SortConversationByDate(SortOrder order);
        System.Collections.IList GetSelectedConversationItems();
        event ListViewItemSelectionChangedEventHandler ConversationItemSelectionChanged;
        void ShowMoveOptionsMenu();

        // System.Windows.Forms.Control dispatch and sizing members that QfcItemController accesses
        // through the field. The concrete ItemViewer satisfies all four via its UserControl base.
        // They are declared on the interface (not resolved by a concrete cast) so InvokeRequired-guarded
        // routing stays mockable for the dispatch-routing unit tests.
        // CS0108 (member hides an inherited ISynchronizeInvoke/IControl member) is suppressed
        // narrowly here: these four members are a deliberate, pre-existing re-declaration for
        // mockability, and adding the `new` keyword or otherwise restructuring the interface
        // hierarchy is an actual API-shape change, not an annotation-only fix (out of scope per
        // AC7 no-behavior-change).
#pragma warning disable CS0108
        bool InvokeRequired { get; }
        object Invoke(System.Delegate method);
        System.IAsyncResult BeginInvoke(System.Delegate method);
        int Height { get; }
#pragma warning restore CS0108

        void RemoveControlsColsRightOf(Control furthestRight);
    }
}
