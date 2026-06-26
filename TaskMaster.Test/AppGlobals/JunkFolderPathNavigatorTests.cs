using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskMaster;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Deterministic, COM-free tests for <see cref="JunkFolderPathNavigator"/> (issue #211, AC10).
    /// These verify both the enumeration-bound performance invariant (the navigator must touch
    /// only the folders along the resolution path plus the first-segment breadth-first frontier,
    /// NOT the entire tree — the defect being fixed) and the correctness invariant (the navigator
    /// resolves the IDENTICAL folder as the legacy FolderTree + FindSequentialNode path for valid
    /// configured paths). No live COM, timers, network, filesystem, or temporary files are used.
    /// </summary>
    [TestClass]
    public class JunkFolderPathNavigatorTests
    {
        /// <summary>
        /// In-memory <see cref="IFolderNode"/> that records how many times its direct children are
        /// enumerated, so a test can assert the navigator's enumeration cost. Every access of
        /// <see cref="ChildFolders"/> increments the shared counter exactly once.
        /// </summary>
        private sealed class CountingFolderNode : IFolderNode
        {
            private readonly List<IFolderNode> _children;
            private readonly int[] _enumerationCounter;

            public CountingFolderNode(
                string name,
                int[] enumerationCounter,
                List<IFolderNode> children = null
            )
            {
                Name = name;
                _enumerationCounter = enumerationCounter;
                _children = children ?? new List<IFolderNode>();
            }

            public string Name { get; }

            public IReadOnlyList<IFolderNode> ChildFolders
            {
                get
                {
                    _enumerationCounter[0]++;
                    return _children;
                }
            }
        }

        /// <summary>
        /// Builds a wide-and-deep tree rooted at "Root". Each non-leaf node has
        /// <paramref name="breadth"/> children; the tree is <paramref name="depth"/> levels deep
        /// below the root. The path "Root\Child0\GrandChild0\..." is always resolvable. Returns the
        /// root and the shared enumeration counter (index 0).
        /// </summary>
        private static (IFolderNode Root, int[] Counter, int TotalNodes) BuildWideDeepTree(
            int breadth,
            int depth
        )
        {
            var counter = new int[1];
            var totalNodes = new int[1];
            var root = BuildSubtree("Root", breadth, depth, counter, totalNodes);
            return (root, counter, totalNodes[0]);
        }

        private static IFolderNode BuildSubtree(
            string name,
            int breadth,
            int remainingDepth,
            int[] counter,
            int[] totalNodes
        )
        {
            totalNodes[0]++;
            var children = new List<IFolderNode>();
            if (remainingDepth > 0)
            {
                for (var i = 0; i < breadth; i++)
                {
                    var childName = remainingDepth == 1 ? $"Leaf{i}" : $"Node{remainingDepth}_{i}";
                    children.Add(
                        BuildSubtree(childName, breadth, remainingDepth - 1, counter, totalNodes)
                    );
                }
            }

            return new CountingFolderNode(name, counter, children);
        }

        /// <summary>
        /// Returns the verbatim child-name sequence (excluding the root) for the always-resolvable
        /// path produced by <see cref="BuildSubtree"/>, so tests can build the matching relative
        /// path string.
        /// </summary>
        private static string FirstChildPath(int depth)
        {
            var segments = new List<string>();
            for (var d = depth; d >= 1; d--)
            {
                segments.Add(d == 1 ? "Leaf0" : $"Node{d}_0");
            }

            return string.Join("\\", segments);
        }

        // ---------------------------------------------------------------------
        // [P2-T3] Enumeration-bound regression test (the defect-encoding invariant).
        // Now exercises the production JunkFolderPathNavigator.ResolvePath (P3-T4).
        // ---------------------------------------------------------------------

        [TestMethod]
        public void ResolvePath_AccessesOnlyFoldersAlongThePath_NotEntireTree()
        {
            // Arrange: a wide+deep tree where full enumeration is far more expensive than the path.
            const int breadth = 5;
            const int depth = 4;
            var (root, counter, totalNodes) = BuildWideDeepTree(breadth, depth);
            var path = FirstChildPath(depth);

            // The path-bound enumeration budget: the first-segment BFS frontier (root + one level)
            // plus one child enumeration per subsequent segment. For this fixed tree the production
            // navigator enumerates the root's children once (matches at level 1) plus one child
            // enumeration per subsequent segment, i.e. exactly `depth` enumerations. A full-tree
            // walk (the legacy FolderTree behavior) enumerated every non-leaf node (785 on this
            // tree per the recorded red run), which is strictly larger. The bound is intentionally
            // strict so the prior eager path failed it; the production navigator satisfies it.
            var pathBoundBudget = depth;

            // Act: resolve the valid path through the PRODUCTION navigator.
            var resolved = JunkFolderPathNavigator.ResolvePath(root, path);

            // Assert: correct folder resolved, and enumeration count is path-bound, strictly less
            // than what a full-tree enumeration would require.
            resolved.Should().NotBeNull("the valid path must resolve a folder");
            resolved
                .Name.Should()
                .Be("Leaf0", "the navigator must resolve the deepest path segment");

            counter[0]
                .Should()
                .BeLessThanOrEqualTo(
                    pathBoundBudget,
                    "the navigator must touch only the resolution path plus the first-segment BFS "
                        + "frontier, not the entire tree (issue #211 AC10)"
                );

            _ = totalNodes;
        }

        // ---------------------------------------------------------------------
        // [P2-T5] Correctness tests — direct-navigation equivalence with the legacy comparator.
        // ---------------------------------------------------------------------

        [TestMethod]
        public void ResolvePath_ValidSingleSegment_ResolvesCorrectDirectChild()
        {
            // Arrange
            var counter = new int[1];
            var inbox = new CountingFolderNode("Inbox", counter);
            var sent = new CountingFolderNode("Sent", counter);
            var root = new CountingFolderNode(
                "Root",
                counter,
                new List<IFolderNode> { inbox, sent }
            );

            // Act
            var resolved = JunkFolderPathNavigator.ResolvePath(root, "Sent");

            // Assert
            resolved
                .Should()
                .BeSameAs(
                    sent,
                    "a single-segment relative path must resolve the matching direct child"
                );
        }

        [TestMethod]
        public void ResolvePath_ValidNestedMultiSegment_ResolvesCorrectDeepFolder()
        {
            // Arrange
            var counter = new int[1];
            var target = new CountingFolderNode("Spam", counter);
            var junk = new CountingFolderNode("Junk", counter, new List<IFolderNode> { target });
            var inbox = new CountingFolderNode("Inbox", counter, new List<IFolderNode> { junk });
            var root = new CountingFolderNode("Root", counter, new List<IFolderNode> { inbox });

            // Act
            var resolved = JunkFolderPathNavigator.ResolvePath(root, "Inbox\\Junk\\Spam");

            // Assert
            resolved
                .Should()
                .BeSameAs(
                    target,
                    "a nested multi-segment path must resolve the matching deep folder"
                );
        }

        [TestMethod]
        public void ResolvePath_PathDifferingOnlyInCase_DoesNotMatch()
        {
            // Arrange
            var counter = new int[1];
            var inbox = new CountingFolderNode("Inbox", counter);
            var root = new CountingFolderNode("Root", counter, new List<IFolderNode> { inbox });

            // Act — ordinal, case-sensitive comparison ("inbox" != "Inbox").
            var resolved = JunkFolderPathNavigator.ResolvePath(root, "inbox");

            // Assert
            resolved
                .Should()
                .BeNull(
                    "matching is ordinal and case-sensitive, so a case-mismatched segment must not match"
                );
        }

        [TestMethod]
        public void ResolvePath_FirstSegmentEqualsRootName_ResolvesRoot()
        {
            // Arrange
            var counter = new int[1];
            var inbox = new CountingFolderNode("Inbox", counter);
            var root = new CountingFolderNode("Root", counter, new List<IFolderNode> { inbox });

            // Act — BFS-from-root parity: the first segment is matched against the root node itself.
            var resolved = JunkFolderPathNavigator.ResolvePath(root, "Root");

            // Assert
            resolved
                .Should()
                .BeSameAs(
                    root,
                    "the first-segment BFS starts at the root node, so a segment equal to the root name resolves the root"
                );
        }

        [TestMethod]
        public void ResolvePath_UnmatchedSegment_ReturnsNull()
        {
            // Arrange
            var counter = new int[1];
            var inbox = new CountingFolderNode("Inbox", counter);
            var root = new CountingFolderNode("Root", counter, new List<IFolderNode> { inbox });

            // Act — "Inbox" matches at level 1, but "DoesNotExist" has no matching direct child.
            var resolved = JunkFolderPathNavigator.ResolvePath(root, "Inbox\\DoesNotExist");

            // Assert
            resolved.Should().BeNull("an unmatched segment must yield a not-found (null) result");
        }

        // ---------------------------------------------------------------------
        // Defensive negative/edge guards (>= 90% new-code coverage; UT2 scenario completeness).
        // ---------------------------------------------------------------------

        /// <summary>
        /// An <see cref="IFolderNode"/> whose <see cref="ChildFolders"/> is null, exercising the
        /// navigator's null-children guards on both the first-segment BFS frontier and the
        /// subsequent-segment direct-child walk.
        /// </summary>
        private sealed class NullChildrenFolderNode : IFolderNode
        {
            public NullChildrenFolderNode(string name) => Name = name;

            public string Name { get; }

            public IReadOnlyList<IFolderNode> ChildFolders => null;
        }

        [TestMethod]
        public void ResolvePath_NullRoot_ReturnsNull()
        {
            // Act — a null root cannot resolve any path.
            var resolved = JunkFolderPathNavigator.ResolvePath(null, "Inbox");

            // Assert
            resolved.Should().BeNull("a null root yields a not-found (null) result");
        }

        [TestMethod]
        public void ResolvePath_NullRelativePath_ReturnsNull()
        {
            // Arrange
            var counter = new int[1];
            var root = new CountingFolderNode("Root", counter);

            // Act — a null relative path cannot resolve any folder.
            var resolved = JunkFolderPathNavigator.ResolvePath(root, null);

            // Assert
            resolved.Should().BeNull("a null relative path yields a not-found (null) result");
        }

        [TestMethod]
        public void ResolvePath_SubsequentSegmentOnNullChildren_ReturnsNull()
        {
            // Arrange — "Inbox" matches at level 1, but its ChildFolders is null, so the second
            // segment match must safely return null (MatchChild null-children guard).
            var inbox = new NullChildrenFolderNode("Inbox");
            var counter = new int[1];
            var root = new CountingFolderNode("Root", counter, new List<IFolderNode> { inbox });

            // Act
            var resolved = JunkFolderPathNavigator.ResolvePath(root, "Inbox\\Anything");

            // Assert
            resolved
                .Should()
                .BeNull("a subsequent segment against a node with null children must yield null");
        }

        [TestMethod]
        public void ResolvePath_FirstSegmentBfsAcrossNullChildrenFrontier_ResolvesDeeperMatch()
        {
            // Arrange — the root does NOT match the first segment, forcing the BFS to expand the
            // frontier. One frontier node has null children (exercising the NextLevel null-children
            // guard); the match lives on a sibling subtree at a deeper level.
            var counter = new int[1];
            var deadEnd = new NullChildrenFolderNode("DeadEnd");
            var target = new CountingFolderNode("Target", counter);
            var branch = new CountingFolderNode(
                "Branch",
                counter,
                new List<IFolderNode> { target }
            );
            var root = new CountingFolderNode(
                "Root",
                counter,
                new List<IFolderNode> { deadEnd, branch }
            );

            // Act — BFS: level 0 = Root (no match), level 1 = [DeadEnd, Branch] (no match),
            // level 2 = [Target] (match); DeadEnd's null children are skipped by NextLevel.
            var resolved = JunkFolderPathNavigator.ResolvePath(root, "Target");

            // Assert
            resolved
                .Should()
                .BeSameAs(
                    target,
                    "the first-segment BFS must skip null-children frontier nodes and still find a deeper match"
                );
        }
    }
}
