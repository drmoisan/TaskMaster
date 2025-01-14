using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UtilitiesCS.Extensions;

namespace UtilitiesCS
{
    public class TreeNode<T>
    {
        public TreeNode(T value) => Value = value;
        public TreeNode(TreeNode<T> node)
        {
            Parent = node.Parent;
            Value = node.Value;
            Children = node.Children;
        }

        #region Public Properties

        private TreeNode<T> _parent;
        public TreeNode<T> Parent { get => _parent; set => _parent = value; }

        private T _value;
        public T Value { get => _value; private set => _value = value; }

        private List<TreeNode<T>> _children = new List<TreeNode<T>>();
        public List<TreeNode<T>> Children { get => _children; set => _children = value; }
        public TreeNode<T> this[int i]
        {
            get
            {
                return Children[i];
            }
        }

        public int ChildCount { get => Children.Count; }

        public int Depth
        {
            get
            {
                if (Parent is null)
                    return 0;
                return Parent.Depth + 1;
            }
        }

        #endregion Public Properties

        #region Change Structure

        public TreeNode<T> AddChild(T value)
        {
            var node = new TreeNode<T>(value) { Parent = this };
            Children.Add(node);
            return node;
        }
        
        public TreeNode<T> AddChild(T value, string strID)
        {
            var node = new TreeNode<T>(value) { Parent = this };
            // node.ID = strID
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
        
        public TreeNode<T>[] AddChildren(params T[] values)
        {
            return values.Select(new Func<T, TreeNode<T>>(AddChild)).ToArray();
        }

        public TreeNode<T> InsertChild(TreeNode<T> node)
        {
            node.Parent = this;
            Children.Insert(0, node);
            return node;
        }

        public bool RemoveChild(TreeNode<T> node)
        {
            return Children.Remove(node);
        }

        #endregion Change Structure

        #region Public Methods

        public IEnumerable<TreeNode<T>> Descendents(bool includeSelf = false)
        {
            TreeNode<T>[] nodes = includeSelf ?[this]: [];
            return nodes.Concat(Children.SelectMany(x => x.Descendents(true)));
        }

        public TreeNode<T> FirstAncestor(Func<T, bool> condition)
        {
            if (Parent is null)
                return default;
            if (condition(Parent.Value))
                return Parent;
            return Parent.FirstAncestor(condition);
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

        public TreeNode<T> FindSequentialNode<U>(Func<T, U, bool> comparator, Queue<U> sequence)
        {
            comparator.ThrowIfNull();
            var first = sequence.ThrowIfNullOrEmpty().Dequeue();
            var node = FindNode((current) => comparator(current, first), true);
            while (node is not null && !sequence.IsEmpty())
            {
                var next = sequence.Dequeue();
                node = node.Children?.Where(x => comparator(x.Value, next))?.FirstOrDefault();
            }
            return node;
        }

        public TreeNode<T> FindNode(Func<T, bool> comparator, bool descendByLevel = false) 
        { 
            if (!descendByLevel) 
            { 
                return FindAll(comparator).FirstOrDefault(); 
            }
            else
            {
                TreeNode<T>[] nodes = { this };
                while (!nodes.IsNullOrEmpty())
                {
                    var first = nodes.FirstOrDefault(node => comparator(node.Value));
                    if (first != default) { return first; }
                    else { nodes = GetNextLevel(nodes).Where(node => node is not null).ToArray(); }
                }
                return default;
            }
            
        }
        
        public TreeNode<T>[] GetNextLevel(TreeNode<T>[] nodes) 
        { 
            if (nodes is null) { return null; }
            return nodes.Where(x => !x.Children.IsNullOrEmpty()).SelectMany(x => x.Children).ToArray();
        }

        public IEnumerable<TreeNode<T>> FindAll(Func<T, bool> comparator)
        {
            if (comparator(this.Value))
                return new TreeNode<T>[] { this }.Concat(Children.SelectMany(x => x.FindAll(comparator)));
            else
                return new TreeNode<T>[] { }.Concat(Children.SelectMany(x => x.FindAll(comparator)));
        }

        public IEnumerable<TreeNode<T>> FindAll(Func<TreeNode<T>, bool> comparator) 
        {
            if (comparator(this))
                return new TreeNode<T>[] { this }.Concat(Children.SelectMany(x => x.FindAll(comparator)));
            else
                return new TreeNode<T>[] { }.Concat(Children.SelectMany(x => x.FindAll(comparator)));
        }
        
        public IEnumerable<T> Flatten()
        {
            return new[] { Value }.Concat(Children.SelectMany(x => x.Flatten()));
        }

        public IEnumerable<T> FlattenIf(Func<T, bool> comparator)
        {
            return Flatten().Where(comparator);
        }
        
        public bool IsAncestor(TreeNode<T> model)
        {
            if (ReferenceEquals(this, model))
                return true;
            if (Parent is null)
                return false;
            return Parent.IsAncestor(model);
        }
        
        public virtual IEnumerable<TreeNode<T>> Leaves()
        {
            TreeNode<T>[] nodes = [];
            return nodes.Concat(Children.SelectMany(x => 
            {
                if (x.Children.Count == 0)
                    return new[] { x };
                else
                    return x.Leaves();
            }));
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

        #endregion Public Methods
    }
}