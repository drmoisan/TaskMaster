using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Stores caller-local selected folder paths without mutating shared snapshot nodes.
    /// </summary>
    public sealed class FolderTreeSelectionOverlay
    {
        private readonly HashSet<string> _selectedRelativePaths;

        public FolderTreeSelectionOverlay(IEnumerable<string> selectedRelativePaths)
        {
            _selectedRelativePaths = new HashSet<string>(
                selectedRelativePaths ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase
            );
        }

        public IReadOnlyCollection<string> SelectedRelativePaths =>
            new ReadOnlyCollection<string>(_selectedRelativePaths.ToArray());

        public bool IsSelected(FolderTreeSnapshotNode node)
        {
            if (node == null)
            {
                return false;
            }

            return _selectedRelativePaths.Contains(node.RelativePath);
        }

        public FolderTreeSelectionOverlay WithSelection(string relativePath, bool selected)
        {
            var copy = new HashSet<string>(
                _selectedRelativePaths,
                StringComparer.OrdinalIgnoreCase
            );
            if (selected)
            {
                copy.Add(relativePath);
            }
            else
            {
                copy.Remove(relativePath);
            }

            return new FolderTreeSelectionOverlay(copy);
        }
    }
}
