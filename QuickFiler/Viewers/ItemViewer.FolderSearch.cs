using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using UtilitiesCS;

namespace QuickFiler
{
    // Forwarding implementations for the narrowed IItemViewer folder and search intent members
    // (#351). Every folder member is a thin delegation to the host-neutral, unit-tested
    // BreadcrumbBridgeCoordinator pipeline (see ItemViewer.Breadcrumb.cs); the legacy CboFolders
    // owner-draw machinery and the FolderHierarchyBuilder.Build call are decommissioned (AC-5).
    // Path A (FolderRow suggestions) and Path B (plain string[] search results, verbatim
    // including "Trash to Delete") semantics are preserved bit-for-bit through
    // BreadcrumbSelectionMap (G10). On a bare viewer (no pipeline yet) the members are inert:
    // setters no-op and getters return the legacy empty-combo values. The whole ItemViewer type
    // is [ExcludeFromCodeCoverage] via its primary partial in ItemViewer.cs.
    public partial class ItemViewer
    {
        public void AddFolderItems(string[] items) => BreadcrumbCoordinator?.AddItems(items);

        public void SetFolderSuggestions(IReadOnlyList<FolderRow> rows) =>
            BreadcrumbCoordinator?.SetSuggestions(rows);

        public string GetSelectedFolder() => BreadcrumbCoordinator?.GetSelectedFolder();

        public void SetFolderSelectedIndex(int index) => BreadcrumbCoordinator?.SelectRow(index);

        public void SetFolderSelectedItem(string item) => BreadcrumbCoordinator?.SelectItem(item);

        public void SetFolderDroppedDown(bool droppedDown) =>
            SetBreadcrumbDropDownState(droppedDown);

        // #438: the search-path presentation intent. Thin forwarding to the non-focusing breadcrumb
        // path (ItemViewer.Breadcrumb.cs), which latches the non-focusing open on the open
        // coordinator and delegates to BreadcrumbBridgeCoordinator.PresentSearchResults. Both of
        // those are host-neutral and unit-tested; this partial stays coverage-exempt forwarding.
        public void PresentFolderSearchResults(string[] items) =>
            PresentBreadcrumbSearchResults(items);

        // #677: the deactivate-cancel intent. Thin forwarding to the existing #488-era cancel path
        // so a breadcrumb popup cannot outlive the QuickFiler form losing activation.
        public void CancelBreadcrumbSelector() => BreadcrumbCoordinator?.CancelSelector();

        public void ClearFolderItems() => BreadcrumbCoordinator?.Clear();

        public void FocusFolderDropDown() => FocusBreadcrumb();

        public bool FolderContains(string item) =>
            BreadcrumbCoordinator != null && BreadcrumbCoordinator.Contains(item);

        public string[] GetFolderItems() =>
            BreadcrumbCoordinator?.GetFolderItems() ?? Array.Empty<string>();

        public event EventHandler FolderSelectionChanged
        {
            add => _folderSelectionChangedHandlers += value;
            remove => _folderSelectionChangedHandlers -= value;
        }

        // Raised synthetically from the bridge for Left/Right arrow messages (FR-6), preserving
        // the IItemViewer.FolderKeyDown seam for existing consumers.
        public event KeyEventHandler FolderKeyDown
        {
            add => _folderKeyDownHandlers += value;
            remove => _folderKeyDownHandlers -= value;
        }

        public string SearchText => TxtboxSearch.Text;

        public event EventHandler SearchTextChanged
        {
            add => TxtboxSearch.TextChanged += value;
            remove => TxtboxSearch.TextChanged -= value;
        }

        public event KeyEventHandler SearchKeyDown
        {
            add => TxtboxSearch.KeyDown += value;
            remove => TxtboxSearch.KeyDown -= value;
        }

        // #680: the search textbox's Leave event, forwarded verbatim. The controller owns the
        // dismissal that WinForms menu mode used to provide for a non-capturing popup.
        public event EventHandler SearchLeave
        {
            add => TxtboxSearch.Leave += value;
            remove => TxtboxSearch.Leave -= value;
        }

        // #680: the Escape/Leave dismissal guard. Thin forwarding to the host-neutral, unit-tested
        // bridge coordinator, exactly like every other member in this coverage-exempt partial.
        public bool IsFolderDropDownOpen => BreadcrumbCoordinator?.IsSelectorOpen == true;

        public void FocusSearch() => TxtboxSearch.Focus();
    }
}
