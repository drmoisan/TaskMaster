#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    /// <summary>
    /// Hierarchy-aware folder predictor implementing the Local Classifier Per Parent Node
    /// (LCPPN) strategy. The folder tree is reconstructed from backslash-delimited relative
    /// paths; each internal node owns a <see cref="PerParentClassifier"/> that decides among its
    /// direct children. Prediction descends the tree with beam search and returns the leaf whose
    /// path-product probability is highest, or an empty result on abstention.
    /// </summary>
    /// <remarks>
    /// The per-parent shared token base is serialized inline (via <see cref="Corpus"/>), not as a
    /// separate <c>CorpusInherit</c> file, so the whole predictor is one JSON document distinct
    /// from <c>Folder.json</c>. All prediction logic is pure and testable without Outlook COM.
    /// </remarks>
    public sealed class LcppnFolderPredictor
        : SmartSerializable<LcppnFolderPredictor>,
            IFolderPredictor
    {
        private const char PathSeparator = '\\';

        /// <summary>Serialized schema version for forward migration. Defaults to 1.</summary>
        [JsonProperty]
        public int Version { get; set; } = 1;

        /// <summary>Beam width retained at each descent step. Mirrors the config value.</summary>
        [JsonProperty]
        public int BeamWidth { get; set; } = 3;

        /// <summary>Abstention threshold on the path-product probability. Mirrors the config value.</summary>
        [JsonProperty]
        public double MinimumPathProbability { get; set; } = 0.5;

        /// <summary>Shrinkage weight applied by each per-parent classifier.</summary>
        [JsonProperty]
        public double ShrinkageLambda { get; set; } = 0.7;

        /// <summary>Cold-start example threshold applied by each per-parent classifier.</summary>
        [JsonProperty]
        public int MinColdStartExamples { get; set; } = 5;

        /// <summary>
        /// Per-parent classifiers keyed by full parent path (empty string for the root). Serialized
        /// inline so the whole predictor is one document.
        /// </summary>
        [JsonProperty]
        public Dictionary<string, PerParentClassifier> Nodes { get; set; } =
            new Dictionary<string, PerParentClassifier>(StringComparer.Ordinal);

        /// <summary>
        /// The folder hierarchy reconstructed from the trained leaf paths. Not serialized: it is
        /// fully derivable from <see cref="Nodes"/> (each node's children are the child segments of
        /// its classifier) and is rebuilt on deserialization, avoiding a redundant second copy of
        /// the structure in the JSON document.
        /// </summary>
        [JsonIgnore]
        public FolderHierarchyTree Tree { get; set; } = new FolderHierarchyTree();

        /// <summary>Parameterless constructor required by <see cref="SmartSerializable{T}"/> and Newtonsoft.</summary>
        public LcppnFolderPredictor()
            : base()
        {
            // SmartSerializable serializes its _parent reference, so the predictor registers itself
            // as its own serialization root (the same pattern BayesianClassifierGroup uses).
            base._parent = this;
        }

        // Rebuilds the (un-serialized) hierarchy tree from the deserialized Nodes so descent works
        // immediately after a round-trip.
        [System.Runtime.Serialization.OnDeserialized]
        internal void OnDeserialized(System.Runtime.Serialization.StreamingContext context)
        {
            RebuildTree();
        }

        /// <summary>
        /// Rebuilds <see cref="Tree"/> from the current <see cref="Nodes"/>. Each parent key's
        /// classifier child segments become that node's edges.
        /// </summary>
        public void RebuildTree()
        {
            var tree = new FolderHierarchyTree();
            if (Nodes is not null)
            {
                foreach (var entry in Nodes)
                {
                    foreach (var child in entry.Value.ChildSegments)
                    {
                        tree.AddLeaf(entry.Key, child);
                    }
                }
            }

            Tree = tree;
        }

        /// <summary>
        /// Builds a predictor from a mined corpus using <c>FolderInfo.RelativePath</c> as the leaf
        /// label and <c>Tokens</c> as the features. Produces one <see cref="PerParentClassifier"/>
        /// per internal node, each keyed by its direct child segments.
        /// </summary>
        /// <param name="corpus">The mined mail corpus; must not be null. Entries with no relative path are skipped.</param>
        /// <param name="config">The validated predictor configuration; must not be null.</param>
        /// <returns>A populated predictor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="corpus"/> or <paramref name="config"/> is null.</exception>
        public static LcppnFolderPredictor Build(
            IEnumerable<MinedMailInfo> corpus,
            LcppnFolderPredictorConfig config
        )
        {
            if (corpus is null)
            {
                throw new ArgumentNullException(nameof(corpus));
            }

            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            config.Validate();
            var predictor = new LcppnFolderPredictor
            {
                BeamWidth = config.BeamWidth,
                MinimumPathProbability = config.MinimumPathProbability,
                ShrinkageLambda = config.ShrinkageLambda,
                MinColdStartExamples = config.MinColdStartExamples,
            };

            foreach (var mail in corpus)
            {
                var relativePath = mail?.FolderInfo?.RelativePath;
                if (string.IsNullOrEmpty(relativePath))
                {
                    continue;
                }

                // Reached only when relativePath (derived from mail) is non-empty, so mail is non-null.
                predictor.Train(relativePath!, mail!.Tokens ?? Array.Empty<string>(), 1);
            }

            return predictor;
        }

        /// <summary>
        /// Trains the predictor on one email filed to the leaf identified by <paramref name="tag"/>.
        /// Updates only the per-parent classifiers along the root-to-leaf path; nodes off the path
        /// are untouched. A previously unseen child registers on its parent only.
        /// </summary>
        /// <param name="tag">The leaf folder relative path (backslash-delimited).</param>
        /// <param name="matchTokens">The tokenized email content.</param>
        /// <param name="emailCount">The number of emails represented by the tokens.</param>
        public void Train(string tag, IEnumerable<string> matchTokens, int emailCount)
        {
            var tokens = matchTokens?.ToArray() ?? Array.Empty<string>();
            var segments = SplitPath(tag);
            if (segments.Length == 0)
            {
                return;
            }

            var parentKey = FolderHierarchyTree.RootKey;
            foreach (var segment in segments)
            {
                Tree.AddLeaf(parentKey, segment);
                GetOrAddNode(parentKey).Train(segment, tokens, emailCount);
                parentKey = Combine(parentKey, segment);
            }
        }

        /// <summary>
        /// Reverses prior training for the leaf identified by <paramref name="tag"/>, applying
        /// <c>UnTrain</c> along the root-to-leaf path only. Nodes off the path are untouched.
        /// </summary>
        /// <param name="tag">The leaf folder relative path (backslash-delimited).</param>
        /// <param name="matchTokens">The tokenized email content previously trained.</param>
        /// <param name="emailCount">The number of emails represented by the tokens.</param>
        public void UnTrain(string tag, IEnumerable<string> matchTokens, int emailCount)
        {
            var tokens = matchTokens?.ToArray() ?? Array.Empty<string>();
            var segments = SplitPath(tag);
            if (segments.Length == 0)
            {
                return;
            }

            var parentKey = FolderHierarchyTree.RootKey;
            foreach (var segment in segments)
            {
                if (Nodes.TryGetValue(parentKey, out var node))
                {
                    node.UnTrain(segment, tokens, emailCount);
                }

                parentKey = Combine(parentKey, segment);
            }
        }

        /// <summary>
        /// Classifies tokenized content by beam-search descent and returns leaf predictions ordered
        /// by descending path-product probability. Returns an empty result on abstention.
        /// </summary>
        /// <param name="tokens">The tokenized email content.</param>
        /// <returns>Ordered leaf predictions, or an empty ordered query when the predictor abstains.</returns>
        public OrderedParallelQuery<Prediction<string>> Classify(string[] tokens)
        {
            var leaves = DescendBeam(tokens ?? Array.Empty<string>());
            if (leaves.Count == 0)
            {
                return Empty();
            }

            leaves.Sort((a, b) => b.LogProbability.CompareTo(a.LogProbability));
            var top = leaves[0];
            if (Math.Exp(top.LogProbability) < MinimumPathProbability)
            {
                // Abstain: the best path product does not clear the threshold. This also covers the
                // root-abstention case, since a root child that never clears the threshold yields no
                // qualifying descent and the top leaf (if any) remains below the bar.
                return Empty();
            }

            return leaves
                .Select(leaf => new Prediction<string>(leaf.Path, Math.Exp(leaf.LogProbability)))
                .AsParallel()
                .OrderByDescending(prediction => prediction.Probability);
        }

        // Beam-search descent. Each frontier entry is a partial path with its cumulative log
        // probability. Entries whose node is a leaf are emitted as completed candidates.
        private List<LeafCandidate> DescendBeam(string[] tokens)
        {
            var completed = new List<LeafCandidate>();
            var frontier = new List<PartialPath>
            {
                new PartialPath(FolderHierarchyTree.RootKey, 0.0),
            };

            while (frontier.Count > 0)
            {
                var next = new List<PartialPath>();
                foreach (var partial in frontier)
                {
                    if (!Nodes.TryGetValue(partial.NodeKey, out var node))
                    {
                        // A node with no classifier is a terminal leaf reached by descent.
                        if (partial.NodeKey.Length > 0)
                        {
                            completed.Add(
                                new LeafCandidate(partial.NodeKey, partial.LogProbability)
                            );
                        }

                        continue;
                    }

                    var scores = node.ScoreChildren(tokens);
                    if (scores.Count == 0)
                    {
                        if (partial.NodeKey.Length > 0)
                        {
                            completed.Add(
                                new LeafCandidate(partial.NodeKey, partial.LogProbability)
                            );
                        }

                        continue;
                    }

                    foreach (var score in scores)
                    {
                        var childKey = Combine(partial.NodeKey, score.Key);
                        var childLog =
                            partial.LogProbability
                            + Math.Log(Math.Max(score.Value, double.Epsilon));
                        if (Tree.IsLeaf(childKey))
                        {
                            completed.Add(new LeafCandidate(childKey, childLog));
                        }
                        else
                        {
                            next.Add(new PartialPath(childKey, childLog));
                        }
                    }
                }

                // Retain only the top BeamWidth partial paths by cumulative log probability.
                next.Sort((a, b) => b.LogProbability.CompareTo(a.LogProbability));
                if (next.Count > BeamWidth)
                {
                    next = next.GetRange(0, BeamWidth);
                }

                frontier = next;
            }

            return completed;
        }

        private OrderedParallelQuery<Prediction<string>> Empty()
        {
            return Array
                .Empty<Prediction<string>>()
                .AsParallel()
                .OrderByDescending(prediction => prediction.Probability);
        }

        private PerParentClassifier GetOrAddNode(string parentKey)
        {
            if (!Nodes.TryGetValue(parentKey, out var node))
            {
                node = new PerParentClassifier(ShrinkageLambda, MinColdStartExamples);
                Nodes[parentKey] = node;
            }

            return node;
        }

        private static string[] SplitPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return Array.Empty<string>();
            }

            return path.Split(PathSeparator).Where(s => !string.IsNullOrEmpty(s)).ToArray();
        }

        private static string Combine(string parentKey, string childSegment)
        {
            return parentKey.Length == 0 ? childSegment : parentKey + PathSeparator + childSegment;
        }

        private readonly struct PartialPath
        {
            public PartialPath(string nodeKey, double logProbability)
            {
                NodeKey = nodeKey;
                LogProbability = logProbability;
            }

            public string NodeKey { get; }
            public double LogProbability { get; }
        }

        private readonly struct LeafCandidate
        {
            public LeafCandidate(string path, double logProbability)
            {
                Path = path;
                LogProbability = logProbability;
            }

            public string Path { get; }
            public double LogProbability { get; }
        }
    }
}
