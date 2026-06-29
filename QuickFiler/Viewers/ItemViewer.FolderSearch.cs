using System;
using System.Linq;
using System.Windows.Forms;

namespace QuickFiler
{
    // Forwarding implementations for the narrowed IItemViewer folder-combo and search intent members
    // (Seam C, Cluster 2c). Each member forwards to the underlying Designer-backed CboFolders /
    // TxtboxSearch controls. The whole ItemViewer type is [ExcludeFromCodeCoverage] via its primary
    // partial in ItemViewer.cs.
    public partial class ItemViewer
    {
        public void SetFolderItems(string[] items) => CboFolders.Items.AddRange(items);

        public string GetSelectedFolder() => CboFolders.SelectedItem as string;

        public void SetFolderSelectedIndex(int index) => CboFolders.SelectedIndex = index;

        public void SetFolderSelectedItem(string item) => CboFolders.SelectedItem = item;

        public void SetFolderDroppedDown(bool droppedDown) => CboFolders.DroppedDown = droppedDown;

        public void ClearFolderItems() => CboFolders.Items.Clear();

        public void FocusFolderDropDown() => CboFolders.Focus();

        public bool FolderContains(string item) => CboFolders.Items.Contains(item);

        public string[] GetFolderItems() =>
            CboFolders.Items.Cast<object>().Select(item => item.ToString()).ToArray();

        public event EventHandler FolderSelectionChanged
        {
            add => CboFolders.SelectedIndexChanged += value;
            remove => CboFolders.SelectedIndexChanged -= value;
        }

        public event KeyEventHandler FolderKeyDown
        {
            add => CboFolders.KeyDown += value;
            remove => CboFolders.KeyDown -= value;
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

        public void FocusSearch() => TxtboxSearch.Invoke(new Action(() => TxtboxSearch.Focus()));
    }
}
