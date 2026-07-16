using System.Collections.Generic;
using System.Linq;

namespace UtilitiesCS
{
    /// <summary>
    /// Pure, host-neutral builder that turns the ordered <see cref="FolderRow"/> sequence produced by
    /// <see cref="FolderPredictor.FolderRowArray"/> / <see cref="FolderPredictor.FindFolderRows"/> into
    /// a forest of <see cref="TreeNode{T}"/> of <see cref="FolderNodeViewModel"/>. Scored
    /// <see cref="FolderRowKind.Suggestion"/> rows (non-null <see cref="FolderRow.Score"/>) are split on
    /// <c>\</c> with find-or-add ancestor synthesis, attaching the probability only at the full-folder
    /// leaf; synthesized ancestors carry no probability but are expandable. Every other row kind
    /// (<see cref="FolderRowKind.Separator"/>, <see cref="FolderRowKind.SearchResult"/>,
    /// <see cref="FolderRowKind.Recent"/>) is emitted as a depth-0 leaf preserving its
    /// <see cref="FolderRow.Text"/> verbatim and in input order. This seam is NOT coverage-exempt.
    /// </summary>
    public class FolderHierarchyBuilder
    {
        /// <summary>
        /// Builds the root forest from the ordered rows. The full folder path is retained on each node
        /// as the key/selection value; <see cref="FolderNodeViewModel.DisplayName"/> is the last path
        /// segment (or the verbatim text for non-suggestion rows).
        /// </summary>
        /// <param name="rows">The ordered rows from the predictor.</param>
        /// <returns>The forest of root nodes, preserving input order.</returns>
        public IReadOnlyList<TreeNode<FolderNodeViewModel>> Build(IReadOnlyList<FolderRow> rows)
        {
            var roots = new List<TreeNode<FolderNodeViewModel>>();
            if (rows == null)
            {
                return roots;
            }

            foreach (var row in rows)
            {
                if (row.Score.HasValue)
                {
                    AddSuggestion(roots, row.Score.Value);
                }
                else
                {
                    // Separator / SearchResult / Recent: a depth-0 leaf preserving Text verbatim.
                    var leaf = new FolderNodeViewModel(
                        row.Text,
                        row.Text,
                        probability: null,
                        depth: 0,
                        hasChildren: false
                    );
                    roots.Add(new TreeNode<FolderNodeViewModel>(leaf));
                }
            }

            return roots;
        }

        /// <summary>
        /// Splits the suggestion's folder path on <c>\</c> and walks/inserts nodes with find-or-add,
        /// synthesizing ancestors (no probability, expandable) and attaching the probability at the
        /// full-folder leaf.
        /// </summary>
        private static void AddSuggestion(
            List<TreeNode<FolderNodeViewModel>> roots,
            FolderScore score
        )
        {
            var segments = score.FolderPath.Split('\\');
            List<TreeNode<FolderNodeViewModel>> currentLevel = roots;
            TreeNode<FolderNodeViewModel> currentNode = null;
            string cumulative = null;

            for (int i = 0; i < segments.Length; i++)
            {
                cumulative = cumulative == null ? segments[i] : cumulative + "\\" + segments[i];
                bool isLeaf = i == segments.Length - 1;

                var existing = currentLevel.FirstOrDefault(n => n.Value.FolderPath == cumulative);
                if (existing == null)
                {
                    var vm = new FolderNodeViewModel(
                        cumulative,
                        segments[i],
                        isLeaf ? (double?)score.Probability : null,
                        depth: i,
                        hasChildren: !isLeaf
                    );

                    if (currentNode == null)
                    {
                        currentNode = new TreeNode<FolderNodeViewModel>(vm);
                        roots.Add(currentNode);
                    }
                    else
                    {
                        currentNode = currentNode.AddChild(vm);
                    }
                }
                else
                {
                    currentNode = existing;
                }

                currentLevel = currentNode.Children;
            }
        }
    }
}
