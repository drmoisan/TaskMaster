using Microsoft.Office.Interop.Outlook;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UtilitiesCS.HelperClasses;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS
{
    public class FolderTree: INotifyPropertyChanged
    {
        #region ctor

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FolderTree() { }

        public FolderTree(IEnumerable<MAPIFolder> olRoots)
        {
            _roots = olRoots.Select(RootFromFolder).ToList();
            DetangleRoots();            
            WireNotifications();
        }

        public FolderTree(MAPIFolder olRoot)
        {
            var root = RootFromFolder(olRoot);
            _roots = new List<TreeNode<FolderWrapper>>() { root };
            WireNotifications();
        }

        public FolderTree(MAPIFolder olRoot, IList<string> selections)
        {
            var root = RootFromFolder(olRoot);
            root.Traverse(node => node.Selected = selections.Contains(node.RelativePath));
            _roots = new List<TreeNode<FolderWrapper>>() { root };
            WireNotifications();
        }

        public FolderTree(MAPIFolder olRoot, IList<string> selections, ProgressTracker progress)
        {
            int folderCount = 0;
            var root = RootFromFolder(olRoot, progress.SpawnChild(95), ref folderCount);

            if (folderCount > 0)
            { 
                var traverseProgress = progress.SpawnChild(5);
                double increment = 100 / (double)folderCount;
                double rt = 0;

                root.Traverse(node => 
                { 
                    node.Selected = selections.Contains(node.RelativePath);
                    rt += increment;
                    traverseProgress.Report(rt);
                });
            }
            _roots = new List<TreeNode<FolderWrapper>>() { root };
            WireNotifications();

            progress.Report(100);
        }

        public async static Task<FolderTree> CreateAsync(MAPIFolder olRoot)
        {
            var tree = new FolderTree();
            await Task.Run(() =>
            {
                tree._roots = new List<TreeNode<FolderWrapper>>() { tree.RootFromFolder(olRoot) };
                tree.WireNotifications();
            });
            
            return tree;
        }

        public async static Task<FolderTree> CreateAsync(IEnumerable<MAPIFolder> olRoots)
        {
            var tree = new FolderTree();
            await Task.Run(() =>
            {
                tree._roots = olRoots.Select(tree.RootFromFolder).ToList();
                tree.DetangleRoots();
                tree.WireNotifications();
            });

            return tree;
        }

        #endregion ctor

        #region Initialization

        private void DetangleRoots()
        {
            if (_roots.Count > 1)
            {
                int i = 0, j = 0;
                var dict = _roots.Select(x => (Key: x, Value: x.Flatten().ToList())).ToDictionary(x => x.Key, x => x.Value);

                while (i < _roots.Count - 1)
                {
                    j = 0;
                    while (j < _roots.Count - 1)
                    {
                        if (i == j) { j++; continue; }
                        if (dict.ElementAt(i).Value.IsSubsetOf(dict.ElementAt(j).Value))
                        {
                            _roots.RemoveAt(i);
                            dict.Remove(dict.Keys.ElementAt(i));
                        }
                        j++;
                    }
                    i++;
                }
            }
        }

        public void LoadItemCounts()
        {
            Roots.ForEach(root => root.TraverseByLevel(false, node => 
            { 
                var descendantItemCount = node.ChildCount == 0 ? 0 : node.Children.Sum(child => child.Value.ItemCountSubFolders);
                node.Value.ItemCountSubFolders = node.Value.ItemCount + descendantItemCount;
            }));
        }
        
        private TreeNode<FolderWrapper> RootFromFolder(MAPIFolder olRoot)
        {
            var info = new FolderWrapper(olRoot, olRoot);
            var root = new TreeNode<FolderWrapper>(info);
            this.InitializeChildren(root, olRoot);
            return root;
        }

        private TreeNode<FolderWrapper> RootFromFolder(MAPIFolder olRoot, ProgressTracker progress, ref int runningCount)
        {
            var info = new FolderWrapper(olRoot, olRoot);
            var root = new TreeNode<FolderWrapper>(info);
            Interlocked.Increment(ref runningCount);

            if (olRoot.Folders.Count > 0)
            {
                this.InitializeChildren(root, olRoot, progress, ref runningCount);
            }
            else
            {
                progress.Report(100);
            }
            return root;
        }

        private List<TreeNode<FolderWrapper>> _roots;
        public List<TreeNode<FolderWrapper>> Roots { get => _roots; }

        private void InitializeChildren(TreeNode<FolderWrapper> node, MAPIFolder olRoot)
        {
            node.Value.OlFolder.Folders.Cast<MAPIFolder>()
                .ForEach(child =>
                {
                    var childNode = node.AddChild(new FolderWrapper(child, olRoot));
                    if (child.Folders.Count > 0)
                        InitializeChildren(childNode, olRoot);
                });
        }

        private void InitializeChildren(TreeNode<FolderWrapper> node, MAPIFolder olRoot, ProgressTracker progress, ref int runningTotal)
        {
            var children = node.Value.OlFolder.Folders.Cast<MAPIFolder>().ToArray();
            var count = children.Count();
            
            if (count > 0)
            {
                double increment = 100 / (double)count;
                double rt = 0;
                foreach (var child in children) 
                {
                    var childNode = node.AddChild(new FolderWrapper(child, olRoot));
                    Interlocked.Increment(ref runningTotal);
                    
                    rt += increment;
                    if (child.Folders.Count > 0)
                    {
                        var childProgress = progress.SpawnChild(increment);
                        InitializeChildren(childNode, olRoot, childProgress, ref runningTotal);
                    }
                    else
                    {
                        progress.Report(rt, $"Building Outlook Folder Tree ({runningTotal} completed)");
                        //progress.Increment(increment);
                    }
                }
                
                progress.Report(100);
            }                
        }

        #endregion Initialization

        #region Transformations and Comparisons

        public List<FolderWrapper> Flatten()
        {            
            return [.. _roots.SelectMany(root => root.Flatten())];
        }

        public List<TreeNode<FolderWrapper>> FlattenNodes()
        {
            return [.. _roots.SelectMany(root => root.FlattenNodes())];
        }

        public (List<TreeNode<FolderWrapper>> nodes, List<TreeNode<FolderWrapper>> contents, List<TreeNode<FolderWrapper>> currentOnly, List<TreeNode<FolderWrapper>> otherOnly) Compare(FolderTree other) 
        {
            var compareNodes = new FolderWrapperNodeComparer();
            var (nodes, onlyCurrentNodes, onlyOtherNodes) = CompareMembers(other, compareNodes);            
            var compareContents = new FolderWrapperNodeContentsComparer();
            //var (contents, onlyCurrentContents, onlyOtherContents) = CompareMembers(other, compareContents);
            var (contents, onlyCurrentContents, onlyOtherContents) = CompareMembers(onlyCurrentNodes, onlyOtherNodes, compareContents);
            return (nodes, contents, onlyCurrentContents, onlyOtherContents);
        }

        public (List<TreeNode<FolderWrapper>> same, List<TreeNode<FolderWrapper>> onlyCurrent, List<TreeNode<FolderWrapper>> onlyOther) CompareMembers(List<TreeNode<FolderWrapper>> current, List<TreeNode<FolderWrapper>> other, IEqualityComparer<TreeNode<FolderWrapper>> comparer)
        {
            var same = current.Intersect(other, comparer).ToList();
            var onlyCurrent = current.Except(other, comparer).ToList();
            var onlyOther = other.Except(current, comparer).ToList();
            return (same, onlyCurrent, onlyOther);
        }


        public (List<TreeNode<FolderWrapper>> same, List<TreeNode<FolderWrapper>> onlyCurrent, List<TreeNode<FolderWrapper>> onlyOther) CompareMembers(FolderTree other, IEqualityComparer<TreeNode<FolderWrapper>> comparer)
        {
            var currentFlat = this.FlattenNodes();
            var otherFlat = other.FlattenNodes();
            return CompareMembers(currentFlat, otherFlat, comparer);
        }

        public (List<FolderWrapper> same, List<FolderWrapper> onlyCurrent, List<FolderWrapper> onlyOther) CompareMembers(FolderTree other, IEqualityComparer<FolderWrapper> comparer)
        {
            var thisFlat = this.Flatten();
            var otherFlat = other.Flatten();            
            return CompareMembers(thisFlat, otherFlat, comparer);
        }

        public (List<FolderWrapper> same, List<FolderWrapper> onlyCurrent, List<FolderWrapper> onlyOther) CompareMembers(List<FolderWrapper> current, List<FolderWrapper> other, IEqualityComparer<FolderWrapper> comparer)
        {
            var same = current.Intersect(other, comparer).ToList();
            var onlyCurrent = current.Except(other, comparer).ToList();
            var onlyOther = other.Except(current, comparer).ToList();
            return (same, onlyCurrent, onlyOther);
        }

        #endregion Transformations and Comparisons

        #region Tree Filtering and Selection

        public void SetSelected(TreeNode<FolderWrapper> node, bool includeDescendents)
        {
            if (includeDescendents)
            {
                node.Traverse(node => node.Selected = true);
            }
            
        }

        public List<TreeNode<FolderWrapper>> FilterSelected(bool include)
        {
            var dummyRootOut = new TreeNode<FolderWrapper>(_roots[0].Value);
            var dummyRootIn = new TreeNode<FolderWrapper>(_roots[0].Value);
            dummyRootIn.Children = _roots;

            FilterChildren(dummyRootIn, dummyRootOut, include); 
            var selected = dummyRootOut.Children;
            return selected;
        }

        private void FilterChildren(TreeNode<FolderWrapper> source, TreeNode<FolderWrapper> destination, bool include)
        {
            foreach (var sourceChild in source.Children)
            {
                if (sourceChild.Value.Selected == include)
                {
                    var destinationChild = destination.AddChild(sourceChild.Value);
                    FilterChildren(sourceChild, destinationChild, include);
                }
                else
                {
                    FilterChildren(sourceChild, destination, include);
                }
            }
        }

        #endregion Tree Filtering and Selection

        #region INotifyPropertyChanged

        internal void WireNotifications()
        {
            _roots.ForEach(root => root.Traverse(node => node.Value.PropertyChanged += Child_PropertyChanged));
        }
        
        private TimedBatchAction _batchNotifier = new(TimeSpan.FromMilliseconds(50));

        private void Child_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _batchNotifier.RequestAction(() => PropertyChanged?.Invoke(sender, e));   
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged
    }


    
}