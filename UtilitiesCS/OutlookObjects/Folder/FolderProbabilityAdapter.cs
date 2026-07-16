#nullable enable
using System;
using System.Collections.Generic;

namespace UtilitiesCS
{
    /// <summary>
    /// Joins an <see cref="IFolderProbabilitySource"/> to the nodes of a
    /// <see cref="FolderSuggestionTree"/> by full-path string equality, assigning
    /// <see cref="FolderSuggestionNode.Probability"/> for matched folder rows. Banner rows and folder
    /// rows with no upstream probability keep <see cref="FolderSuggestionNode.Probability"/> null so
    /// their percentage cell renders blank. This adapter is the sole join point to the upstream
    /// contract and never recomputes scores.
    /// </summary>
    public sealed class FolderProbabilityAdapter
    {
        private readonly IFolderProbabilitySource _source;

        /// <summary>Creates an adapter over the given probability source.</summary>
        /// <param name="source">The upstream probability seam. Must not be null.</param>
        public FolderProbabilityAdapter(IFolderProbabilitySource source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        /// <summary>
        /// Applies probabilities to every folder node in <paramref name="tree"/> whose full path the
        /// source resolves. Banner nodes are skipped; unmatched folder nodes are left with a null
        /// probability.
        /// </summary>
        /// <param name="tree">The suggestion tree whose folder nodes receive probabilities. Must not be null.</param>
        public void Apply(FolderSuggestionTree tree)
        {
            if (tree == null)
            {
                throw new ArgumentNullException(nameof(tree));
            }

            foreach (var node in Flatten(tree.Roots))
            {
                if (node.Kind == FolderSuggestionNodeKind.Banner)
                {
                    continue;
                }

                if (_source.TryGetProbability(node.FullPath, out double probability))
                {
                    node.Probability = probability;
                }
            }
        }

        private static IEnumerable<FolderSuggestionNode> Flatten(
            IReadOnlyList<FolderSuggestionNode> nodes
        )
        {
            foreach (var node in nodes)
            {
                yield return node;
                foreach (var descendant in Flatten(node.Children))
                {
                    yield return descendant;
                }
            }
        }
    }
}
