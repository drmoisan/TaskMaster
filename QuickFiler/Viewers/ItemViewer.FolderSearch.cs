using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UtilitiesCS;

namespace QuickFiler
{
    // Forwarding implementations for the narrowed IItemViewer folder-combo and search intent members
    // (Seam C, Cluster 2c). Each member forwards to the underlying Designer-backed CboFolders /
    // TxtboxSearch controls. The whole ItemViewer type is [ExcludeFromCodeCoverage] via its primary
    // partial in ItemViewer.cs.
    public partial class ItemViewer
    {
        public void SetFolderItems(string[] items) => CboFolders.Items.AddRange(items);

        // #325 suggestion-tree state. The host-neutral seams own all correctness; this partial holds
        // only the WinForms glue (owner-draw paint, glyph hit-test, Items rebind) and is exempt.
        private FolderTreeStateModel _folderTreeState;
        private IReadOnlyList<TreeNode<FolderNodeViewModel>> _visibleFolderNodes =
            new List<TreeNode<FolderNodeViewModel>>();

        public void SetFolderSuggestions(IReadOnlyList<FolderRow> rows)
        {
            var forest = new FolderHierarchyBuilder().Build(rows);
            _folderTreeState = new FolderTreeStateModel(forest);
            RebindFolderTree();
        }

        // Rebuilds CboFolders.Items from the tree state's current visible-row projection. Each combo
        // item is a FolderNodeViewModel; DrawItem paints it and GetSelectedFolder maps it back to the
        // full folder path.
        private void RebindFolderTree()
        {
            if (_folderTreeState is null)
            {
                return;
            }

            _visibleFolderNodes = _folderTreeState.GetVisibleNodes();
            CboFolders.BeginUpdate();
            try
            {
                CboFolders.Items.Clear();
                CboFolders.Items.AddRange(
                    _visibleFolderNodes.Select(n => (object)n.Value).ToArray()
                );
            }
            finally
            {
                CboFolders.EndUpdate();
            }
        }

        // Expands or collapses the highlighted node in response to a keyboard arrow, then re-projects.
        // All transition and no-op logic lives in the unit-tested FolderTreeStateModel; this glue only
        // highlights the selected row, invokes the transition, and reports whether the expansion state
        // actually changed so the keyboard handler can fall through to legacy behavior on a no-op.
        internal bool FolderTreeRightArrow()
        {
            if (_folderTreeState is null)
            {
                return false;
            }
            HighlightSelectedFolderNode();
            var node = _folderTreeState.Highlighted;
            bool before = node != null && node.Value.Expanded;
            _folderTreeState.RightArrow();
            bool after = node != null && node.Value.Expanded;
            if (before != after)
            {
                RebindFolderTree();
                return true;
            }
            return false;
        }

        internal bool FolderTreeLeftArrow()
        {
            if (_folderTreeState is null)
            {
                return false;
            }
            HighlightSelectedFolderNode();
            var node = _folderTreeState.Highlighted;
            bool before = node != null && node.Value.Expanded;
            _folderTreeState.LeftArrow();
            bool after = node != null && node.Value.Expanded;
            if (before != after)
            {
                RebindFolderTree();
                return true;
            }
            return false;
        }

        private void HighlightSelectedFolderNode()
        {
            int index = CboFolders.SelectedIndex;
            if (_folderTreeState != null && index >= 0 && index < _visibleFolderNodes.Count)
            {
                _folderTreeState.Highlight(_visibleFolderNodes[index]);
            }
        }

        public string GetSelectedFolder()
        {
            if (CboFolders.SelectedItem is FolderNodeViewModel vm)
            {
                return vm.FolderPath;
            }
            return CboFolders.SelectedItem as string;
        }

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

        // Per-node horizontal budget (pixels): indent step per depth level, glyph column width, and
        // the reserved right column into which the percentage is right-aligned.
        private const int FolderIndentPerDepth = 14;
        private const int FolderGlyphWidth = 14;
        private const int FolderPercentColumnWidth = 46;

        // Owner-draw paint for CboFolders (DrawMode.OwnerDrawFixed). Per visible row it draws an indent
        // proportional to Depth, a +/- glyph when the node has children, the display name, and the
        // right-aligned whole-number percentage produced by the host-neutral FolderNodeViewModel. Non
        // FolderNodeViewModel items (for example the injected "Trash to Delete" string) are drawn as
        // plain left-aligned text so the retained SetFolderItems(string[]) path renders unchanged.
        internal void CboFolders_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0 || e.Index >= CboFolders.Items.Count)
            {
                e.DrawFocusRectangle();
                return;
            }

            using (var textBrush = new SolidBrush(e.ForeColor))
            {
                var item = CboFolders.Items[e.Index];
                if (item is FolderNodeViewModel vm)
                {
                    int indent = e.Bounds.Left + (vm.Depth * FolderIndentPerDepth);

                    if (vm.Glyph.HasValue)
                    {
                        var glyphRect = new Rectangle(
                            indent,
                            e.Bounds.Top,
                            FolderGlyphWidth,
                            e.Bounds.Height
                        );
                        TextRenderer.DrawText(
                            e.Graphics,
                            vm.Glyph.Value.ToString(),
                            e.Font,
                            glyphRect,
                            e.ForeColor,
                            TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                        );
                    }

                    int nameLeft = indent + FolderGlyphWidth;
                    var nameRect = new Rectangle(
                        nameLeft,
                        e.Bounds.Top,
                        Math.Max(0, e.Bounds.Right - FolderPercentColumnWidth - nameLeft),
                        e.Bounds.Height
                    );
                    TextRenderer.DrawText(
                        e.Graphics,
                        vm.DisplayName,
                        e.Font,
                        nameRect,
                        e.ForeColor,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                    );

                    // Right-aligned percentage into a fixed-width column anchored at e.Bounds.Right.
                    var percentRect = new Rectangle(
                        e.Bounds.Right - FolderPercentColumnWidth,
                        e.Bounds.Top,
                        FolderPercentColumnWidth,
                        e.Bounds.Height
                    );
                    TextRenderer.DrawText(
                        e.Graphics,
                        vm.FormattedPercentage,
                        e.Font,
                        percentRect,
                        e.ForeColor,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter
                    );
                }
                else
                {
                    e.Graphics.DrawString(
                        item?.ToString() ?? string.Empty,
                        e.Font,
                        textBrush,
                        e.Bounds,
                        StringFormat.GenericDefault
                    );
                }
            }

            e.DrawFocusRectangle();
        }

        // Glyph mouse hit-test: on a click within the glyph column of a parent row, toggle that node
        // in the tree state and re-project. The toggle correctness lives in FolderTreeStateModel; this
        // computes the drawn glyph rectangle for the row under the mouse and tests the X coordinate.
        internal void CboFolders_MouseDown(object sender, MouseEventArgs e)
        {
            if (_folderTreeState is null || CboFolders.ItemHeight <= 0)
            {
                return;
            }

            int index = e.Y / CboFolders.ItemHeight;
            if (index < 0 || index >= _visibleFolderNodes.Count)
            {
                return;
            }

            var node = _visibleFolderNodes[index];
            if (!node.Value.HasChildren)
            {
                return;
            }

            int glyphLeft = node.Value.Depth * FolderIndentPerDepth;
            int glyphRight = glyphLeft + FolderGlyphWidth;
            if (e.X >= glyphLeft && e.X <= glyphRight)
            {
                _folderTreeState.Highlight(node);
                _folderTreeState.Toggle(node);
                RebindFolderTree();
            }
        }
    }
}
