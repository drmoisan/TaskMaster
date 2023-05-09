using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ToDoModel
{
    public class TreeNode<T>
    {
        // Public ID As String

        public TreeNode(T value)
        {
            Value = value;
        }

        public TreeNode<T> this[int i]
        {
            get
            {
                return Children[i];
            }
        }


        public TreeNode<T> Parent { get; set; }

        public T Value { get; private set; }

        public bool IsAncestor(TreeNode<T> model)
        {
            if (ReferenceEquals(this, model))
                return true;
            if (Parent is null)
                return false;
            return Parent.IsAncestor(model);
        }

        public int ChildCount
        {
            get
            {
                return Children.Count;
            }
        }


        public List<TreeNode<T>> Children { get; set; } = new List<TreeNode<T>>();


        public TreeNode<T> AddChild(T value)
        {
            var node = new TreeNode<T>(value) { Parent = this };
            // node.ID = NextChildID()
            Children.Add(node);
            return node;
        }
        public TreeNode<T> AddChild(TreeNode<T> node)
        {
            // node.Parent = Me
            // node.ID = NextChildID()
            Children.Add(node);
            return node;
        }

        public TreeNode<T> InsertChild(TreeNode<T> node)
        {
            node.Parent = this;
            // node.ID = strID
            Children.Insert(0, node);
            return node;
        }
        public TreeNode<T> AddChild(T value, string strID)
        {
            var node = new TreeNode<T>(value) { Parent = this };
            // node.ID = strID
            Children.Add(node);
            return node;
        }
        public TreeNode<T>[] AddChildren(params T[] values)
        {
            return values.Select(new Func<T, TreeNode<T>>(AddChild)).ToArray();
        }

        public bool RemoveChild(TreeNode<T> node)
        {
            return Children.Remove(node);
        }

        public void Traverse(Action<T> action)
        {
            action(Value);

            foreach (var child in Children)
                child.Traverse(action);
        }

        public void Traverse(Action<TreeNode<T>> action)
        {
            action(this);

            foreach (var child in Children)
                child.Traverse(action);
        }

        public object FindByDelegate(Func<T, string, bool> comparator, string StringToCompare)
        {

            foreach (var node in Children)
            {
                if (comparator(Value, StringToCompare))
                {
                    return node;
                }
            }
            return null;
        }

        public object FindByDelegate(Func<T, T, bool> comparator, T T2)
        {

            foreach (var node in Children)
            {
                if (comparator(Value, T2))
                {
                    return node;
                }
            }
            return null;
        }

        public IEnumerable<T> Flatten()
        {
            return new[] { Value }.Concat(Children.SelectMany(x => x.Flatten()));
        }
    }
}