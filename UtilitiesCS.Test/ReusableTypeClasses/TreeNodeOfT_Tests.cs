using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class TreeNodeOfT_Tests
    {
        [TestMethod]
        public void AddChildAndAddChildren_CreateParentChildRelationshipsAndDepths()
        {
            // Arrange
            var root = new UtilitiesCS.TreeNode<string>("root");

            // Act
            var first = root.AddChild("first");
            var added = root.AddChildren("second", "third");

            // Assert
            root.ChildCount.Should().Be(3);
            first.Parent.Should().BeSameAs(root);
            added.Should().HaveCount(2);
            added.All(child => child.Parent == root).Should().BeTrue();
            added.All(child => child.Depth == 1).Should().BeTrue();
            root.Depth.Should().Be(0);
        }

        [TestMethod]
        public void InsertChild_PlacesNodeAtFrontAndSetsParent()
        {
            // Arrange
            var root = new UtilitiesCS.TreeNode<int>(0);
            root.AddChild(2);
            var inserted = new UtilitiesCS.TreeNode<int>(1);

            // Act
            root.InsertChild(inserted);

            // Assert
            root[0].Should().BeSameAs(inserted);
            inserted.Parent.Should().BeSameAs(root);
        }

        [TestMethod]
        public void RemoveChild_RemovesExistingNode()
        {
            // Arrange
            var root = new UtilitiesCS.TreeNode<string>("root");
            var child = root.AddChild("child");

            // Act
            var removed = root.RemoveChild(child);

            // Assert
            removed.Should().BeTrue();
            root.ChildCount.Should().Be(0);
        }

        [TestMethod]
        public void DescendentsFlattenAndFlattenNodes_ReturnExpectedSequences()
        {
            // Arrange
            var root = new UtilitiesCS.TreeNode<string>("root");
            var left = root.AddChild("left");
            left.AddChild("leaf");
            root.AddChild("right");

            // Act
            var descendents = root.Descendents(includeSelf: true).Select(node => node.Value).ToArray();
            var flattenedValues = root.Flatten().ToArray();
            var flattenedNodes = root.FlattenNodes().Select(node => node.Value).ToArray();

            // Assert
            descendents.Should().Equal("root", "left", "leaf", "right");
            flattenedValues.Should().Equal("root", "left", "leaf", "right");
            flattenedNodes.Should().Equal("root", "left", "leaf", "right");
        }

        [TestMethod]
        public void LeavesGetLeavesAtMaxDepthAndIsAncestor_ReportTreeShape()
        {
            // Arrange
            var root = new UtilitiesCS.TreeNode<string>("root");
            var left = root.AddChild("left");
            var deepLeaf = left.AddChild("deep");
            var shallowLeaf = root.AddChild("shallow");

            // Act
            var leaves = root.Leaves().Select(node => node.Value).ToArray();
            var deepestLeaves = root.GetLeavesAtMaxDepth().Select(node => node.Value).ToArray();

            // Assert
            leaves.Should().Equal("deep", "shallow");
            deepestLeaves.Should().Equal("deep");
            deepLeaf.IsAncestor(root).Should().BeTrue();
            shallowLeaf.IsAncestor(left).Should().BeFalse();
        }

        [TestMethod]
        public void FindNodeFindAllFirstAncestorAndSequentialNode_SearchTreeSuccessfully()
        {
            // Arrange
            var root = new UtilitiesCS.TreeNode<string>("root");
            var branch = root.AddChild("branch");
            var leaf = branch.AddChild("leaf");
            root.AddChild("other");

            // Act
            var firstByDepth = root.FindNode(value => value.StartsWith("leaf"), descendByLevel: true);
            var allContainingO = root.FindAll(value => value.Contains('o')).Select(node => node.Value).ToArray();
            var ancestor = leaf.FirstAncestor(value => value == "root");
            var sequential = root.FindSequentialNode((current, expected) => current == expected, new Queue<string>(new[] { "root", "branch", "leaf" }));

            // Assert
            firstByDepth.Value.Should().Be("leaf");
            allContainingO.Should().Equal("root", "other");
            ancestor.Should().BeSameAs(root);
            sequential.Should().BeSameAs(leaf);
        }

        [TestMethod]
        public void TraverseAndTraverseAncestors_VisitNodesInExpectedOrder()
        {
            // Arrange
            var root = new UtilitiesCS.TreeNode<string>("root");
            var left = root.AddChild("left");
            var leaf = left.AddChild("leaf");
            root.AddChild("right");
            var traversed = new List<string>();
            var traversedNodes = new List<string>();
            var ancestorValues = new List<string>();
            var ancestorNodes = new List<string>();
            var breadthFirst = new List<string>();
            var upwardByLevel = new List<string>();

            // Act
            root.Traverse(value => traversed.Add(value));
            root.Traverse(node => traversedNodes.Add(node.Value));
            leaf.TraverseAncestors(value => ancestorValues.Add(value));
            leaf.TraverseAncestors(node => ancestorNodes.Add(node.Value));
            root.TraverseByLevel(down: true, node => breadthFirst.Add(node.Value));
            root.TraverseByLevel(down: false, node => upwardByLevel.Add(node.Value));

            // Assert
            traversed.Should().Equal("root", "left", "leaf", "right");
            traversedNodes.Should().Equal("root", "left", "leaf", "right");
            ancestorValues.Should().Equal("leaf", "left", "root");
            ancestorNodes.Should().Equal("leaf", "left", "root");
            breadthFirst.Should().Equal("root", "left", "right", "leaf");
            upwardByLevel.Should().Equal("leaf", "left", "root");
        }
    }
}
