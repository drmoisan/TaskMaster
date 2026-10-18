#nullable enable
namespace UtilitiesCS.OutlookObjects.Folder
{
    internal sealed partial class BreadcrumbSelectionSession
    {
        private BreadcrumbDescendantNode? _originalDescendant;
        private int _originalSubfolderIndex;

        /// <summary>Commits an immediate child using the compatible indexed address.</summary>
        public BreadcrumbSelectionEffects ActivateSubfolder(string rowIdentity, int subfolderIndex)
        {
            int rowIndex = IndexOfSelectable(rowIdentity);
            if (!IsOpen || rowIndex < 0 || subfolderIndex < 0)
            {
                return BreadcrumbSelectionEffects.None;
            }
            var row = _model.Rows[rowIndex];
            if (!row.IsSuggestion || !row.LeafExpanded || subfolderIndex >= row.Subfolders.Count)
            {
                return BreadcrumbSelectionEffects.None;
            }
            return ActivateDescendant(
                rowIdentity,
                row.DescendantRoot!.Children[subfolderIndex].Identity,
                subfolderIndex
            );
        }

        internal BreadcrumbSelectionEffects ActivateDescendant(
            string rowIdentity,
            string nodeIdentity,
            int legacyIndex = -1
        )
        {
            int rowIndex = IndexOfSelectable(rowIdentity);
            if (!IsOpen || rowIndex < 0)
            {
                return BreadcrumbSelectionEffects.None;
            }
            var root = _model.Rows[rowIndex].DescendantRoot;
            var node = root?.Find(nodeIdentity);
            if (
                node == null
                || node == root
                || !BreadcrumbSelectionMap.TryDescendantValue(_model, node, out _)
            )
            {
                return BreadcrumbSelectionEffects.None;
            }
            _model.SelectDescendant(rowIndex, node, legacyIndex);
            CommittedIdentity = rowIdentity;
            EndOpenSession();
            return BreadcrumbSelectionEffects.Handled
                | BreadcrumbSelectionEffects.SelectionChanged
                | BreadcrumbSelectionEffects.OpenStateChanged
                | BreadcrumbSelectionEffects.RenderRequired;
        }

        private void CaptureDescendantSelection()
        {
            _originalDescendant = _model.SelectedDescendant;
            _originalSubfolderIndex = _model.SelectedSubfolderIndex;
        }

        private void RestoreDescendantSelection(int rowIndex)
        {
            if (
                _originalDescendant != null
                && _model.Rows[rowIndex].DescendantRoot?.Find(_originalDescendant.Identity)
                    == _originalDescendant
            )
            {
                _model.SelectDescendant(rowIndex, _originalDescendant, _originalSubfolderIndex);
            }
        }
    }
}
