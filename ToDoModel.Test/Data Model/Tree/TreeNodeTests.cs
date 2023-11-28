using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ToDoModel;
using UtilitiesCS;

namespace ToDoModel.Test
{
    [TestClass]
    public class TreeNodeTests
    {
        private MockRepository mockRepository;
        private DebugTextWriter tw;

        //[ClassInitialize]
        //public void ClassInitialize()
        //{
        //    tw = new DebugTextWriter();
        //    Console.SetOut(tw);
        //}
        
        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            tw = new DebugTextWriter();
            Console.SetOut(tw);

        }

        private char NextChar(char c)
        {
            if (c == 'z')
                currentChar = 'a';
            else
                currentChar = (char)(c + 1);
            return currentChar;
        }
        private char currentChar = 'z';

        private TreeNode<string> CreateTreeNode()
        { 
            return new TreeNode<string>("");
        }

        private TreeNode<string> CreateTreeNode(string text)
        {
            return new TreeNode<string>(text);
        }

        private TreeNode<string> CreateTreeNode(int levels)
        {
            return CreateChildren(null, levels).First();
        }

        private List<TreeNode<string>> CreateChildren (TreeNode<string> parent, int levels)
        {
            if (levels-- == 0) return null;

            var r = new Random();
            var childCount = r.Next(1, 5);
            
            List<TreeNode<string>> children = new List<TreeNode<string>>();
            if (parent != null)
            {
                children = Enumerable.Range(0, childCount)
                    .Select(i => parent
                    .AddChild($"{parent?.Value ?? ""}{NextChar(currentChar)}"))
                    .ToList();
            }
            else
            {
                children = Enumerable.Range(0, childCount)
                    .Select(i => CreateTreeNode($"{NextChar(currentChar)}")).ToList();
            }
            children.ForEach(child => CreateChildren(child, levels));
            return children;
        }

        private void PrintPretty(TreeNode<string> node, string indent, bool last)
        {
            Console.Write(indent);
            if (last)
            {
                Console.Write("\\-");
                indent += "  ";
            }
            else
            {
                Console.Write("|-");
                indent += "| ";
            }
            Console.WriteLine(node.Value);

            for (int i = 0; i < node.Children.Count; i++)
                PrintPretty(node.Children[i], indent, i == node.Children.Count - 1);
        }

        [TestMethod]
        public void IsAncestor_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var treeNode = this.CreateTreeNode();
            TreeNode<string> model = null;

            // Act
            var result = treeNode.IsAncestor(
                model);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void AddChild_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var treeNode = this.CreateTreeNode();
            string value = default(string);

            // Act
            var result = treeNode.AddChild(
                value);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void AddChild_StateUnderTest_ExpectedBehavior1()
        {
            // Arrange
            var treeNode = this.CreateTreeNode();
            string value = default(string);
            string strID = null;

            // Act
            var result = treeNode.AddChild(
                value,
                strID);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void AddChild_StateUnderTest_ExpectedBehavior2()
        {
            // Arrange
            var treeNode = this.CreateTreeNode();
            TreeNode<string> node = null;

            // Act
            var result = treeNode.AddChild(
                node);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void InsertChild_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var treeNode = this.CreateTreeNode();
            TreeNode<string> node = null;

            // Act
            var result = treeNode.InsertChild(
                node);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void AddChildren_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var treeNode = this.CreateTreeNode();
            string[] values = null;

            // Act
            var result = treeNode.AddChildren(
                values);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void RemoveChild_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var treeNode = this.CreateTreeNode();
            TreeNode<string> node = null;

            // Act
            var result = treeNode.RemoveChild(
                node);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Traverse_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var treeNode = this.CreateTreeNode();
            Action<string> action = null;

            // Act
            treeNode.Traverse(
                action);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Traverse_StateUnderTest_ExpectedBehavior1()
        {
            // Arrange
            var treeNode = this.CreateTreeNode();
            Action<string> action = null;

            // Act
            treeNode.Traverse(
                action);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void FindByDelegate_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var treeNode = this.CreateTreeNode();
            Func<string, string, bool> comparator = null;
            string StringToCompare = null;

            // Act
            var result = treeNode.FindByDelegate(
                comparator,
                StringToCompare);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void FindByDelegate_StateUnderTest_ExpectedBehavior1()
        {
            // Arrange
            var treeNode = this.CreateTreeNode();
            Func<string, string, bool> comparator = null;
            string T2 = default(string);

            // Act
            var result = treeNode.FindByDelegate(
                comparator,
                T2);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Descendents_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var treeNode = this.CreateTreeNode(5);
            
            

            // Act
            var result = treeNode.Descendents();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Descendents_StateUnderTest_ExpectedBehavior2()
        {
            // Arrange
            //var treeNode2 = this.CreateTreeNode(3);
            
            var treeNode = CreateTreeNode("a");
            var childB = CreateTreeNode("b");
            var childC = CreateTreeNode("c");
            var childD = CreateTreeNode("d");
            var childE = CreateTreeNode("e");
            var childF = CreateTreeNode("f");
            var childG = CreateTreeNode("g");
            treeNode.AddChild(childB);
            treeNode.AddChild(childC);
            childB.AddChild(childD);
            childB.AddChild(childE);
            childB.AddChild(childF);
            childC.AddChild(childG);
            //Original Tree
            //\-a
            //  | -b
            //  | | -d
            //  | | -e
            //  | \-f
            //  \-c
            //    \-g
            
            var expected = new List<TreeNode<string>> { childB, childD, childE, childF, childC, childG };

            // Act
            Console.WriteLine("Original Tree");
            PrintPretty(treeNode, "", true);
            
            Console.WriteLine("\nExpected Results");
            Console.WriteLine(string.Join(",",expected.Select(x => x.Value)));
            //treeNode.Traverse(x => Console.WriteLine(x.Value));

            //var expected = new List<TreeNode<string>> { };
            //treeNode.Traverse(x => expected.Add(x));

            var actual = treeNode.Descendents().ToList();
            Console.WriteLine("\nResults");
            Console.WriteLine(string.Join(",", actual.Select(x => x.Value)));
            //result.ForEach(x => Console.WriteLine(x.Value));

            // Assert
            Assert.IsTrue(actual.SequenceEqual(expected));
        }

        [TestMethod]
        public void FindAll_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var treeNode = this.CreateTreeNode();
            Func<TreeNode<string>, bool> comparator = null;

            // Act
            var result = treeNode.FindAll(
                comparator);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Flatten_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var treeNode = this.CreateTreeNode();

            // Act
            var result = treeNode.Flatten();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void FlattenIf_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var treeNode = this.CreateTreeNode();
            Func<string, bool> comparator = null;

            // Act
            var result = treeNode.FlattenIf(
                comparator);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }
    }
}
